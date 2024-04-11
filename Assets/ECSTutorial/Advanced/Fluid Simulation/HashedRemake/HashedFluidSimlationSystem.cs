using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Samples.Boids;
using Unity.Physics;
using static UnityEngine.ParticleSystem;

namespace FluidSimulate
{
    [UpdateAfter(typeof(SPHManagerSystem))]
    public partial class HashedFluidSimlationSystem : SystemBase
    {
        private static readonly int[] cellOffsetTable =
        {
            1, 1, 1, 1, 1, 0, 1, 1, -1, 1, 0, 1, 1, 0, 0, 1, 0, -1, 1, -1, 1, 1, -1, 0, 1, -1, -1,
            0, 1, 1, 0, 1, 0, 0, 1, -1, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, -1, 1, 0, -1, 0, 0, -1, -1,
            -1, 1, 1, -1, 1, 0, -1, 1, -1, -1, 0, 1, -1, 0, 0, -1, 0, -1, -1, -1, 1, -1, -1, 0, -1, -1, -1
        };

        #region Job

        [BurstCompile]
        partial struct PositionSetup : IJobEntity
        {
            public void Execute([EntityIndexInQuery] int index, in LocalTransform transform, ref FluidSimlationComponent data)
            {
                data.position = transform.Position;
            }
        }//ó���� ������ ��ġ ����

        [BurstCompile]
        private struct HashPositions : IJobParallelFor
        {
            //#pragma warning disable 0649
            [ReadOnly] public float cellRadius;

            //public NativeArray<LocalTransform> positions;
            [ReadOnly] public NativeArray<FluidSimlationComponent> particleData;

            public NativeParallelMultiHashMap<int, int>.ParallelWriter hashMap;
            //#pragma warning restore 0649

            public void Execute(int index)
            {
                float3 position = particleData[index].position;
                    //positions[index].Position;

                int hash = GridHash.Hash(position, cellRadius);
                hashMap.Add(hash, index);

                //positions[index] = new LocalTransform { Position = position, Rotation = quaternion.identity, Scale = 1 };
            }
        }
        [BurstCompile]
        private struct MergeParticles : Samples.Boids.IJobNativeMultiHashMapMergedSharedKeyIndices
        {
            public NativeArray<int> particleIndices;

            //Merge : ���� 
            // Ű�� ���涧
            public void ExecuteFirst(int index)
            {
                particleIndices[index] = index;
            }
            // Ű�� ������ , cellIndex == firstIndex
            public void ExecuteNext(int cellIndex, int index)
            {
                particleIndices[index] = cellIndex;
            }

            //#pragma warning restore 0649
            //FIXME - 
        }//��ŷ����� ó�� ���� OR ���� �ɶ�

        [BurstCompile]
        partial struct ResetAcc : IJobEntity
        {
            [WriteOnly] public NativeArray<FluidSimlationComponent> particleData;
            public ParticleParameterComponent parameter;
            public Vector3 AccVaule;

            public void Execute([EntityIndexInQuery] int index, in FluidSimlationComponent data)
            {
                var temp = data;
                temp.acc = parameter.Gravity + AccVaule;

                particleData[index] = temp;
                //FluidSimlationComponent���� ��� ������ ����Ǵ�
            }
        }//Acc �ʱ�ȭ

        [BurstCompile]
        private struct ComputePressure : IJobParallelFor
        {
            [ReadOnly] public NativeParallelMultiHashMap<int, int> hashMap;
            [ReadOnly] public NativeArray<int> cellOffsetTable;
            [ReadOnly] public NativeArray<FluidSimlationComponent> particleData;

            [ReadOnly] public ParticleParameterComponent parameter;


            public NativeArray<Vector3> pressureDir;
            public NativeArray<float> moveRes;

            public void Execute(int index)
            {
                // Cache
                //int particleCount = particlesPosition.Length;
                var position = particleData[index].position;
                //float density = 0.0f;
                int i, hash, j;
                int3 gridOffset;
                int3 gridPosition = GridHash.Quantize(position, parameter.ParticleRadius);
                bool found;

                // Find neighbors
                for (int oi = 0; oi < 27; oi++)
                {
                    i = oi * 3;
                    gridOffset = new int3(cellOffsetTable[i], cellOffsetTable[i + 1], cellOffsetTable[i + 2]);
                    hash = GridHash.Hash(gridPosition + gridOffset);
                    NativeParallelMultiHashMapIterator<int> iterator;
                    found = hashMap.TryGetFirstValue(hash, out j, out iterator);
                    while (found)
                    {
                        // Neighbor found, get density
                        var rij = particleData[j].position - position;//position - particleData[j].position;
                        float r2 = math.lengthsq(rij);

                        float r = parameter.ParticleRadius + parameter.SmoothRadius;
                        if (r2 < 2 * r)
                        {
                            //density += settings.mass * (315.0f / (64.0f * PI * math.pow(settings.smoothingRadius, 9.0f)))
                            //  * math.pow(settings.smoothingRadiusSq - r2, 3.0f);

                            pressureDir[index] += rij;
                            moveRes[index] += Mathf.Clamp01(Vector3.Dot(-rij.normalized, -(particleData[index].velocity + particleData[index].acc * parameter.DT)));
                        }

                        // Next neighbor
                        found = hashMap.TryGetNextValue(out j, ref iterator);
                    }
                }

            }
        }

        [BurstCompile]
        struct ComputeFloorCollision : IJobParallelFor
        {
            public NativeArray<FluidSimlationComponent> particleData;
            public NativeArray<Entity> particleEntity;

            public ParticleParameterComponent parameter;

            public NativeArray<LocalTransform> collisionTransform;
            public NativeArray<CollisionComponent> collisions;

            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute(int index)
            {
                var particle = particleData[index];//

                switch (parameter.floorType)
                {
                    case FloorType.Collision:
                        {
                            //var particle = particleData[index];//

                            if (particleData[index].position.y <= parameter.floorHeight + parameter.ParticleRadius)
                            {
                                if (particleData[index].isGround == false)
                                {
                                    particle.velocity = Vector3.Reflect(particle.velocity, Vector3.up)
                                        * (1 - parameter.ParticleViscosity);
                                    //----------------------------------------------- �ݻ�� ������ �ٽ� �ݻ��ؼ� �ٷ� ���߳�?
                                }
                                var AccSpeed = particle.acc.magnitude;
                                particle.acc.y = 0;
                                particle.acc = particle.acc.normalized * AccSpeed;

                                particle.isGround = true;
                                // �Ʒ��� �������� �ʵ��� y ���� ����
                            }
                            else
                            {
                                particle.isGround = false;
                            }

                            particleData[index] = particle;
                            break;
                        }
                    case FloorType.Disable:
                        {
                            if (particleData[index].position.y <= parameter.floorHeight + parameter.ParticleRadius)
                            {
                                //ecb.SetEnabled(index, particleEntity[index], false);
                                ecb.SetComponentEnabled<FluidSimlationComponent>(index, particleEntity[index], false);
                            }
                            break;
                        }
                    case FloorType.Kill:
                        {
                            if (particleData[index].position.y <= parameter.floorHeight + parameter.ParticleRadius)
                            {
                                ecb.DestroyEntity(index, particleEntity[index]);
                            }
                            break;
                        }
                    case FloorType.None:
                    default:
                        return;
                }
                
            }
        }

        //[BurstCompile]
        struct ComputeObstacleCollision : IJobParallelFor
        {
            public NativeArray<FluidSimlationComponent> particleData;
            public NativeArray<Entity> particleEntity;

            [ReadOnly]  public NativeArray<LocalTransform> collisionTransform;
            [ReadOnly]  public NativeArray<CollisionComponent> collisions;

            public ParticleParameterComponent parameter;

            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute(int index)
            {
                var particle = particleData[index];

                for (int i = 0; i < collisions.Length; i++)
                {
                    float3 offset = particle.position;
                    offset -= collisionTransform[i].Position;
                    offset += parameter.ParticleRadius;

                    if (Vector3.SqrMagnitude(offset) > Vector3.SqrMagnitude(collisions[i].WorldSize * 0.5f))
                    {
                        continue;
                    }

                    bool IsCollision = collisions[i].IsCollisionSphere(collisionTransform[i], parameter.ParticleRadius,
                        particle.position, out var dir, out var dis);

                    if (IsCollision)
                    {
                        particle.position += dis * parameter.ParticlePush * Vector3.Normalize(dir);
                        particle.velocity += dis * parameter.ParticlePush * Vector3.Normalize(dir);

                        switch (collisions[i].colliderEvent)
                        {
                            case ColliderEvent.Collision:
                                break;
                            case ColliderEvent.DisableTrigger:
                                ecb.SetEnabled(index, particleEntity[index], false);
                                ecb.SetComponentEnabled<FluidSimlationComponent>(index, particleEntity[index], false);
                                break;
                            case ColliderEvent.KillTrigger:
                                ecb.DestroyEntity(index, particleEntity[index]);
                                break;
                        }
                    }
                }

                particleData[index] = particle;

            }
        }

        [BurstCompile]
        struct ComputeCollision : IJobParallelFor
        {
            public NativeArray<FluidSimlationComponent> particleData;
            public NativeArray<Vector3> pressureDir;
            public NativeArray<float> moveRes;
            public float Amount;

            public ParticleParameterComponent parameter;

            public void Execute(int index)
            {
                {
                    var temp = particleData[index];

                    if (Mathf.Approximately(pressureDir[index].sqrMagnitude, 0))
                    {
                        if (particleData[index].isGround)
                        {
                            temp.velocity *= 1 - (parameter.ParticleViscosity * parameter.DT);
                        }
                    }
                    else
                    {
                        float CollisionAcc = Mathf.Max(1 - parameter.ParticleDrag, 0);
                        if (particleData[index].isGround)
                        {
                            if (moveRes[index] >= 0)
                            {
                                var reflected = Vector3.Reflect(temp.velocity, pressureDir[index].normalized) * CollisionAcc;
                                reflected.y = Mathf.Max(reflected.y, 0);

                                temp.velocity = reflected.normalized * reflected.magnitude;
                            }
                            else
                            {
                                var reflected = Vector3.Reflect(-temp.velocity, pressureDir[index].normalized) * CollisionAcc;
                                reflected.y = Mathf.Max(reflected.y, 0);

                                temp.velocity = reflected.normalized * reflected.magnitude;
                            }
                        }
                        else
                        {
                            var CollisionRate = (parameter.ParticleRadius - pressureDir[index].magnitude) / parameter.ParticleRadius;
                            temp.velocity += parameter.Evaluate(CollisionRate) *
                                 parameter.CollisionPush * pressureDir[index].normalized;

                            if (Mathf.Abs(moveRes[index]) > 0.1f)
                            {
                                var reflectVel = temp.velocity * (1 - parameter.ParticleDrag);
                                //(parameter.DT * CollisionAcc * temp.velocity);

                                if (moveRes[index] >= 0)
                                {
                                    var result = Vector3.Reflect((reflectVel + temp.acc * parameter.DT), pressureDir[index].normalized);
                                    temp.velocity = result;
                                    temp.position -= pressureDir[index] * parameter.DT * parameter.ParticlePush;
                                }
                                else
                                {
                                    temp.velocity = Vector3.Reflect((-reflectVel + temp.acc * parameter.DT), pressureDir[index].normalized);
                                    temp.position += pressureDir[index] * parameter.DT * parameter.ParticlePush;
                                }
                            }
                        }
                    }

                    particleData[index] = temp;

                }//
            }
        }

        [BurstCompile]
        partial struct AddPosition : IJobEntity
        {
            public NativeArray<FluidSimlationComponent> particleData;
            public ParticleParameterComponent parameter;

            public void Execute([EntityIndexInQuery] int index, ref FluidSimlationComponent data)//, in LocalTransform transform
            {
                var acc = particleData[index].acc;

                if (particleData[index].isGround)
                {
                    //data.acc -= parameter.Gravity; //=========== ��� Acc�� ����
                    acc -= parameter.Gravity;
                }
                data.velocity = particleData[index].velocity + acc * parameter.DT;
                //if ()

                if (float.IsNaN(particleData[index].velocity.x) || float.IsNaN(particleData[index].velocity.y) || float.IsNaN(particleData[index].velocity.z))
                {
                    //�̵��� �ľ��߾�....
                }
                else
                {
                    data.position = particleData[index].position + particleData[index].velocity * parameter.DT;
                }

                data.acc = Vector3.zero;
                data.isGround = particleData[index].isGround;
            }
        }
        [BurstCompile]
        partial struct ApplyPosition : IJobEntity
        {

            public void Execute([EntityIndexInQuery] int index, ref LocalTransform transform, in FluidSimlationComponent data)
            {
                transform.Position = data.position;
            }
        }

        #endregion

        private EntityQuery ParticleGroup;
        private EntityQuery ObstacleGroup;

        ParticleParameterComponent Parameter;
        JobHandle PositionSetupHandle;
        bool isReady = false;
        float timer = 0;

        int DebuggingIndex = 1;

        protected override void OnCreate()
        {
            ParticleGroup = GetEntityQuery(typeof(FluidSimlationComponent), typeof(LocalTransform));
            ObstacleGroup = GetEntityQuery(typeof(CollisionComponent), typeof(LocalTransform));
        }
        protected override void OnStartRunning()
        {
            if (SystemAPI.HasSingleton<ParticleParameterComponent>())
                Parameter = SystemAPI.GetSingleton<ParticleParameterComponent>();
            else
                Enabled = false;

            isReady = false;
            

            if (Parameter.simulationType != SimulationType.HashedECS) 
            {
                Enabled = false;
            }
        }

        protected override void OnUpdate()
        {
            if (isReady == false)
            {
                PositionSetup PositionSetupJob = new PositionSetup
                {
                    //particleData = ParticleGroup.ToComponentDataArray<FluidSimlationComponent>(Allocator.TempJob)
                };
                PositionSetupHandle = PositionSetupJob.ScheduleParallel(ParticleGroup, Dependency);
                PositionSetupHandle.Complete();

                var tempData = ParticleGroup.ToComponentDataArray<FluidSimlationComponent>(Allocator.Temp);
                if (tempData.Length <= 0)
                {
                    tempData.Dispose();
                    return;
                }

                isReady = true;


                tempData.Dispose();
                return;
            }// ������ ��ġ ������ FluidSimlationComponent ���� ��

            if (timer > Parameter.DT)
            {
                timer = 0;
                return;
            }
            else
            {
                timer += SystemAPI.Time.DeltaTime;
            }

            #region �ʱ�ȭ

            NativeArray<FluidSimlationComponent> particleData =
                ParticleGroup.ToComponentDataArray<FluidSimlationComponent>(Allocator.TempJob);
            var particleEntity = ParticleGroup.ToEntityArray(Allocator.TempJob);

            int particleCount = particleData.Length;

            NativeParallelMultiHashMap<int, int> hashMap = new NativeParallelMultiHashMap<int, int>(particleCount, Allocator.TempJob);

            var particleDir = new NativeArray<Vector3>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var particleMoveRes = new NativeArray<float>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            ///NativeArray<int> particleIndices = new NativeArray<int>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<int> cellOffsetTableNative = new NativeArray<int>(cellOffsetTable, Allocator.TempJob);

            var obstacleTransform = ObstacleGroup.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var obstacleTypeData = ObstacleGroup.ToComponentDataArray<CollisionComponent>(Allocator.TempJob);

            
            var floorECB = new EntityCommandBuffer(Allocator.TempJob);
            var collisionECB = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(EntityManager.WorldUnmanaged);//new EntityCommandBuffer(Allocator.TempJob);

            #endregion

            #region ����

            var particleDirJob = new MemsetNativeArray<Vector3> { Source = particleDir, Value = Vector3.zero };
            JobHandle particleDirJobHandle = particleDirJob.Schedule(particleCount, 64);
            var particleMoveResJob = new MemsetNativeArray<float> { Source = particleMoveRes, Value = 0 };
            JobHandle particleMoveResJobHandle = particleMoveResJob.Schedule(particleCount, 64);

            JobHandle SetupMergedHandle = JobHandle.CombineDependencies(PositionSetupHandle, particleDirJobHandle, particleMoveResJobHandle);

            ///MemsetNativeArray<int> particleIndicesJob = new MemsetNativeArray<int> { Source = particleIndices, Value = 0 };
            ///JobHandle particleIndicesJobHandle = particleIndicesJob.Schedule(particleCount, 64);
            //----------> particleIndices : �ش翵���� ù��° ��ƼŬ / ���� ���µ� ���µ�

            //-----

            ResetAcc ResetAccJob = new ResetAcc
            {
                particleData = particleData,
                parameter = Parameter,
                AccVaule = Vector3.zero
            };
            JobHandle ResetAccHandle = ResetAccJob.ScheduleParallel(ParticleGroup, SetupMergedHandle);

            // Put positions into a hashMap
            HashPositions hashPositionsJob = new HashPositions
            {
                //positions = particlesPosition,
                particleData = particleData,
                hashMap = hashMap.AsParallelWriter(),
                cellRadius = Parameter.ParticleRadius
            };

            //particlePosition �� �Ϸ�ǰ� ����
            JobHandle hashPositionsJobHandle = hashPositionsJob.Schedule(particleCount, 64, ResetAccHandle);
            //�̰ɾ��� job�� hashPositionJob �� particleIndicesJob ������ ����ǰ�
            ///JobHandle mergedPositionIndicesJobHandle = JobHandle.CombineDependencies(hashPositionsJobHandle, particleIndicesJobHandle);

            ///MergeParticles mergeParticlesJob = new MergeParticles
            ///{
            ///     particleIndices = particleIndices
            ///};

            //�� �۾��� ������ �� ���ڿ� hashMap ��Ŷ�� ID�� �ο��ϴ� ���Դϴ�.
            ///JobHandle mergeParticlesJobHandle = mergeParticlesJob.Schedule(hashMap, 64, mergedPositionIndicesJobHandle);
            ///mergeParticlesJobHandle.Complete();

            // ���ڰ� �浹 ���� �۾��Ϸ�
            #endregion

            #region Calculation Job
            //computePressureJob + computeDensityPressureJob

            ComputePressure computePressureJob = new ComputePressure
            {
                hashMap = hashMap,
                cellOffsetTable = cellOffsetTableNative,
                particleData = particleData,
                parameter = Parameter,
                pressureDir = particleDir,
                moveRes = particleMoveRes
            };
            JobHandle computePressureJobHandle = computePressureJob.Schedule(particleCount, 64, hashPositionsJobHandle);// mergeParticlesJobHandle);

            ComputeFloorCollision FloorCollisionJob = new ComputeFloorCollision
            {
                particleData = particleData,
                parameter = Parameter,

                collisions = obstacleTypeData,
                collisionTransform = obstacleTransform,

                particleEntity = particleEntity,
                ecb = floorECB.AsParallelWriter()
            };
            JobHandle FloorCollisionHandle = FloorCollisionJob.Schedule(particleCount, 64, computePressureJobHandle);

            var groundCollision = new ComputeObstacleCollision
            {
                parameter = Parameter,
                particleData = particleData,

                collisions = obstacleTypeData,
                collisionTransform = obstacleTransform,

                particleEntity = particleEntity,
                ecb = collisionECB.AsParallelWriter()
            };
            JobHandle ObstacleCollisionHandle = groundCollision.Schedule(particleCount, 64, FloorCollisionHandle);


            ComputeCollision ComputeCollisionJob = new ComputeCollision
            {
                particleData = particleData,
                pressureDir = particleDir,
                moveRes = particleMoveRes,
                Amount = particleCount,
                parameter = Parameter
            };
            JobHandle ComputeCollisionHandle = ComputeCollisionJob.Schedule(particleCount, 64, ObstacleCollisionHandle);
            ComputeCollisionHandle.Complete();

            Debugging(particleData, "ComputeCollisionJob");//=================

            AddPosition AddPositionJob = new AddPosition
            {
                particleData = particleData,
                parameter = Parameter
            };
            JobHandle AddPositionHandle = AddPositionJob.ScheduleParallel(ParticleGroup, ComputeCollisionHandle);
            AddPositionHandle.Complete();// ------ ������ ����

            Debugging(particleData, "AddPositionJob");

            ApplyPosition ApplyPositionJob = new() { };
            //JobHandle ApplyPositionHandle = ApplyPositionJob.ScheduleParallel(ParticleGroup, AddPositionHandle);
            ApplyPositionJob.ScheduleParallel(ParticleGroup);

            #endregion

            //Dependency = AddPositionHandle;

            {
                particleData.Dispose();
                particleDir.Dispose();
                particleMoveRes.Dispose();

                obstacleTransform.Dispose();
                obstacleTypeData.Dispose();

                hashMap.Dispose();
                //particleIndices.Dispose();
                cellOffsetTableNative.Dispose();

                floorECB.Playback(EntityManager);
                floorECB.Dispose();
                //collisionECB.Playback(EntityManager);
                //collisionECB.Dispose();
            }
        }

        public void Debugging(NativeArray<FluidSimlationComponent> ParameterData , string comment)
        {
            //DebuggingIndex
            //ParameterData[0].position
            if (DebuggingIndex < ParameterData.Length)
            {
                //Debug.Log(DebuggingIndex + " | " + comment + " : Pos :" + ParameterData[DebuggingIndex].position
                //    + " / velo :" +  ParameterData[DebuggingIndex].velocity + " / Is Ground : " + ParameterData[DebuggingIndex].isGround
                //     + " / velo sqrLength : " + ParameterData[DebuggingIndex].velocity.sqrMagnitude);
            }
        }
    }
}

