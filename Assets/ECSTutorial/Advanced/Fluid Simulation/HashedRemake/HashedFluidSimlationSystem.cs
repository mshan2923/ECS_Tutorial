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
        }//처음에 스폰된 위치 적용

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

            //Merge : 병합 
            // 키가 생길때
            public void ExecuteFirst(int index)
            {
                particleIndices[index] = index;
            }
            // 키가 있을때 , cellIndex == firstIndex
            public void ExecuteNext(int cellIndex, int index)
            {
                particleIndices[index] = cellIndex;
            }

            //#pragma warning restore 0649
            //FIXME - 
        }//딕셔러리에 처음 삽입 OR 삽입 될때

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
                //FluidSimlationComponent값을 계산 끝나고 적용되니
            }
        }//Acc 초기화

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

            public ParticleParameterComponent parameter;

            public NativeArray<LocalTransform> collisionTransform;
            public NativeArray<CollisionComponent> collisions;

            public void Execute(int index)
            {
                var particle = particleData[index];//

                switch (parameter.floorType)
                {
                    case FloorType.None:
                        return;
                    case FloorType.Collision:
                        {
                            //var particle = particleData[index];//

                            if (particleData[index].position.y <= parameter.floorHeight + parameter.ParticleRadius)
                            {
                                if (particleData[index].isGround == false)
                                {
                                    particle.velocity = Vector3.Reflect(particle.velocity, Vector3.up)
                                        * (1 - parameter.ParticleViscosity);
                                    //----------------------------------------------- 반사된 방향을 다시 반사해서 바로 멈추나?
                                }
                                var AccSpeed = particle.acc.magnitude;
                                particle.acc.y = 0;
                                particle.acc = particle.acc.normalized * AccSpeed;

                                particle.isGround = true;
                                // 아래로 내려가지 않도록 y 방향 제거
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
                            break;
                        }
                    case FloorType.Kill:
                        {
                            break;
                        }
                }
                
            }
        }

        //[BurstCompile]
        struct ComputeObstacleCollision : IJobParallelFor
        {
            public NativeArray<FluidSimlationComponent> particleData;

            [ReadOnly]  public NativeArray<LocalTransform> collisionTransform;
            [ReadOnly]  public NativeArray<CollisionComponent> collisions;

            public ParticleParameterComponent parameter;

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

                    switch (collisions[i].colliderType)
                    {
                        case ColliderType.Box:
                            {
                                // OBB 축기준 Project 해서 거리 비교
                                // 모든 축이 충돌이여야 충돌

                                var maxPoint = collisionTransform[i].Right() * collisions[i].WorldSize.x
                                    + collisionTransform[i].Up() * collisions[i].WorldSize.y
                                    + collisionTransform[i].Forward() * collisions[i].WorldSize.z;
                                maxPoint *= 0.5f;

                                var ostProjectX = math.project(collisionTransform[i].Position, collisionTransform[i].Right());
                                var ostAreaProjectX = math.project(collisionTransform[i].Position + maxPoint, collisionTransform[i].Right());
                                var particleProjectX = math.project(particle.position, collisionTransform[i].Right());
                                var projectXDis = math.distance(particleProjectX, ostProjectX);
                                if (projectXDis - parameter.ParticleRadius >= math.distance(ostAreaProjectX, ostProjectX))
                                {
                                    continue;
                                }

                                var ostProjectZ = math.project(collisionTransform[i].Position, collisionTransform[i].Forward());
                                var ostAreaProjectZ = math.project(collisionTransform[i].Position + maxPoint, collisionTransform[i].Forward());
                                var particleProjectZ = math.project(particle.position, collisionTransform[i].Forward());
                                var projectZDis = math.distance(particleProjectZ, ostProjectZ);
                                if (projectZDis - parameter.ParticleRadius >= math.distance(ostAreaProjectZ, ostProjectZ))
                                {
                                    continue;
                                }

                                var ostProjectY = math.project(collisionTransform[i].Position, collisionTransform[i].Up());
                                var ostAreaProjectY = math.project(collisionTransform[i].Position + maxPoint, collisionTransform[i].Up());
                                var particleProjectY = math.project(particle.position, collisionTransform[i].Up());
                                var projectYDis = math.distance(particleProjectY, ostProjectY);

                                //=================== 충돌되는 box 면이....

                                if (projectYDis - parameter.ParticleRadius < math.distance(ostAreaProjectY, ostProjectY))
                                {
                                    float3 dir = float3.zero;
                                    {
                                        var disToMaxX = math.distancesq(particleProjectX, ostAreaProjectX);
                                        var disToMaxY = math.distancesq(particleProjectY, ostAreaProjectY);
                                        var disToMaxZ = math.distancesq(particleProjectZ, ostAreaProjectZ);

                                        if (disToMaxX < disToMaxY && disToMaxX < disToMaxZ)
                                        {
                                            if (math.dot(collisionTransform[i].Right(), math.normalize(particleProjectX - ostAreaProjectX)) > 0)
                                            {
                                                dir = collisionTransform[i].Right();
                                            }
                                            else
                                            {
                                                dir = -collisionTransform[i].Right();
                                            }
                                        }
                                        else if (disToMaxY < disToMaxX && disToMaxY < disToMaxZ)
                                        {
                                            if (math.dot(collisionTransform[i].Up(), math.normalize(particleProjectY - ostAreaProjectY)) > 0)
                                            {
                                                dir = collisionTransform[i].Up();
                                            }
                                            else
                                            {
                                                dir = -collisionTransform[i].Up();
                                            }
                                        }
                                        else
                                        {
                                            if (math.dot(collisionTransform[i].Forward(), math.normalize(particleProjectZ - ostAreaProjectZ)) > 0)
                                            {
                                                dir = collisionTransform[i].Forward();
                                            }
                                            else
                                            {
                                                dir = -collisionTransform[i].Forward();
                                            }
                                        }
                                    }//Calculate Box Normal

                                    particle.position += Vector3.Normalize(dir);//---- 적용이 안되나??
                                    particle.velocity += Vector3.Normalize(dir);
                                }

                                break;
                            }
                        case ColliderType.Plane:
                            {
                                var ostProjectX = math.project(collisionTransform[i].Position, collisionTransform[i].Right());
                                var ostAreaProjectX = math.project(collisionTransform[i].Position + collisions[i].WorldSize * 0.5f, collisionTransform[i].Right());
                                var particleProjectX = math.project(particle.position, collisionTransform[i].Right());
                                if (math.distancesq(particleProjectX, ostProjectX) >= math.distancesq(ostAreaProjectX, ostProjectX))
                                {
                                    continue;
                                }

                                var ostProjectZ = math.project(collisionTransform[i].Position, collisionTransform[i].Forward());
                                var ostAreaProjectZ = math.project(collisionTransform[i].Position + collisions[i].WorldSize * 0.5f, collisionTransform[i].Forward());
                                var particleProjectZ = math.project(particle.position, collisionTransform[i].Forward());
                                if (math.distancesq(particleProjectZ, ostProjectZ) >= math.distancesq(ostAreaProjectZ, ostProjectZ))
                                {
                                    continue;
                                }


                                var ostProjectY = math.project(collisionTransform[i].Position, collisionTransform[i].Up());
                                var particleProjectY = math.project(particle.position, collisionTransform[i].Up());

                                if (math.distancesq(ostProjectY, particleProjectY) <= parameter.ParticleRadius)
                                {
                                    float3 dir = float3.zero;

                                    if (math.dot(collisionTransform[i].Up(), math.normalize(particleProjectY - ostProjectY)) > 0)
                                    {
                                        dir = collisionTransform[i].Up();
                                    }
                                    else
                                    {
                                        dir = -collisionTransform[i].Up();
                                    }

                                    particle.position += Vector3.Normalize(dir);//---- 적용이 안되나??
                                    particle.velocity += Vector3.Normalize(dir);
                                    //=============== 
                                }


                                break;
                            }
                        case ColliderType.Sphere:
                        default:
                            {
                                particle.velocity = math.reflect(particle.velocity, collisionTransform[i].Up())//Vector3.Reflect(temp.velocity, collisionTransform[i].Up)
                                        * (1 - parameter.ParticleViscosity);
                                //collisionTransform[i].Up 가 반사축

                                particle.position += Vector3.Normalize(offset);//---- 적용이 안되나??
                                particle.velocity += Vector3.Normalize(offset);

                                //particleData[index] = particle;
                                break;
                            }
                    }
                }

                particleData[index] = particle;
                
            }
        }//================= 평면, 오브젝트 충돌 처리 , 위치값 변경 적용이 안되는듯?
        //=================++++++ Box 회전시 충돌 적용 안되는데??
        //=====loop , Job 병렬처리시 여러 요소 변경이 안되는건가?

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
                            temp.velocity *= 1 - (parameter.ParticleDrag * parameter.DT);
                        }
                    }
                    else
                    {
                        float CollisionAcc = Mathf.Max(1 - parameter.ParticleViscosity, 0);
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
                                var reflectVel = temp.velocity * (1 - parameter.ParticleViscosity);
                                //(parameter.DT * CollisionAcc * temp.velocity);

                                if (moveRes[index] >= 0)
                                {
                                    temp.velocity = Vector3.Reflect((reflectVel + temp.acc * parameter.DT), pressureDir[index].normalized);
                                }
                                else
                                {
                                    temp.velocity = Vector3.Reflect((-reflectVel + temp.acc * parameter.DT), pressureDir[index].normalized);
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
                    //data.acc -= parameter.Gravity; //=========== 계속 Acc가 쌓임
                    acc -= parameter.Gravity;
                }
                data.velocity = particleData[index].velocity + acc * parameter.DT;
                //if ()

                if (float.IsNaN(particleData[index].velocity.x) || float.IsNaN(particleData[index].velocity.y) || float.IsNaN(particleData[index].velocity.z))
                {
                    //이동을 파업했어....
                }
                else
                {
                    data.position += particleData[index].velocity * parameter.DT;
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
            }// 스폰된 위치 정보를 FluidSimlationComponent 에게 줌

            if (timer > Parameter.DT)
            {
                timer = 0;
                return;
            }
            else
            {
                timer += SystemAPI.Time.DeltaTime;
            }

            #region 초기화

            NativeArray<FluidSimlationComponent> particleData =
                ParticleGroup.ToComponentDataArray<FluidSimlationComponent>(Allocator.TempJob);

            int particleCount = particleData.Length;

            NativeParallelMultiHashMap<int, int> hashMap = new NativeParallelMultiHashMap<int, int>(particleCount, Allocator.TempJob);

            var particleDir = new NativeArray<Vector3>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var particleMoveRes = new NativeArray<float>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            ///NativeArray<int> particleIndices = new NativeArray<int>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<int> cellOffsetTableNative = new NativeArray<int>(cellOffsetTable, Allocator.TempJob);

            var obstacleTransform = ObstacleGroup.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var obstacleTypeData = ObstacleGroup.ToComponentDataArray<CollisionComponent>(Allocator.TempJob);
            #endregion

            #region 설정

            var particleDirJob = new MemsetNativeArray<Vector3> { Source = particleDir, Value = Vector3.zero };
            JobHandle particleDirJobHandle = particleDirJob.Schedule(particleCount, 64);
            var particleMoveResJob = new MemsetNativeArray<float> { Source = particleMoveRes, Value = 0 };
            JobHandle particleMoveResJobHandle = particleMoveResJob.Schedule(particleCount, 64);

            JobHandle SetupMergedHandle = JobHandle.CombineDependencies(PositionSetupHandle, particleDirJobHandle, particleMoveResJobHandle);

            ///MemsetNativeArray<int> particleIndicesJob = new MemsetNativeArray<int> { Source = particleIndices, Value = 0 };
            ///JobHandle particleIndicesJobHandle = particleIndicesJob.Schedule(particleCount, 64);
            //----------> particleIndices : 해당영역에 첫번째 파티클 / 딱히 쓰는데 없는데

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

            //particlePosition 이 완료되고 실행
            JobHandle hashPositionsJobHandle = hashPositionsJob.Schedule(particleCount, 64, ResetAccHandle);
            //이걸쓰는 job이 hashPositionJob 과 particleIndicesJob 끝나야 실행되게
            ///JobHandle mergedPositionIndicesJobHandle = JobHandle.CombineDependencies(hashPositionsJobHandle, particleIndicesJobHandle);

            ///MergeParticles mergeParticlesJob = new MergeParticles
            ///{
            ///     particleIndices = particleIndices
            ///};

            //이 작업의 목적은 각 입자에 hashMap 버킷의 ID를 부여하는 것입니다.
            ///JobHandle mergeParticlesJobHandle = mergeParticlesJob.Schedule(hashMap, 64, mergedPositionIndicesJobHandle);
            ///mergeParticlesJobHandle.Complete();

            // 입자간 충돌 사전 작업완료
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
                collisionTransform = obstacleTransform
            };
            JobHandle FloorCollisionHandle = FloorCollisionJob.Schedule(particleCount, 64, computePressureJobHandle);

            if (Parameter.floorType == FloorType.Disable || Parameter.floorType == FloorType.Kill)
            {
                //===== ECB 으로 처리
            }
            //================= 평면, 오브젝트 충돌 처리

            //Debug.Log($"Obstacle : {ObstacleGroup.CalculateEntityCount()}");
            var groundCollision = new ComputeObstacleCollision
            {
                parameter = Parameter,
                particleData = particleData,

                collisions = obstacleTypeData,
                collisionTransform = obstacleTransform
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
            AddPositionHandle.Complete();// ------ 없으면 에러

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

