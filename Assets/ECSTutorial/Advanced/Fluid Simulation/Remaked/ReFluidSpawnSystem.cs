using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;
using Unity.VisualScripting;
using static UnityEngine.EventSystems.EventTrigger;
using System.Linq;
using System.Text;
using static UnityEngine.ParticleSystem;

namespace FluidSimulate
{
    public partial class ReFluidSpawnSystem : SystemBase
    {
        #region Legacy(Disable)
        /*
        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithName("RemakedFluidSimlation")
                .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
                .ForEach((Entity entity, int entityInQueryIndex, in RemakedFluidSpawnComponent manager, in LocalTransform trans) =>
                {
                    Debug.Log("Spawning");

                    var random = new Random(1);
                    for (int i = 0; i < manager.Amount; i++)
                    {
                        // entity 생성 예약
                        var instance = commandBuffer.Instantiate(entityInQueryIndex, manager.particle);

                        var position = new float3((i % 16) * 1.2f + random.NextFloat(-0.1f, 0.1f) * manager.Between,
                         (i / 16 / 16) * 1.1f * manager.Between,
                          ((i / 16) % 16) * 1.2f + random.NextFloat(-0.1f, 0.1f)) * manager.Between + trans.Position;


                        commandBuffer
                        .SetComponent(entityInQueryIndex, instance,
                            new LocalTransform { Position = position, Rotation = quaternion.identity, Scale = 1 });

                        //========= (SHPManager.AddCollider부분)월드에 모든 SPHCollider를 가진 블럭마다 컨포넌트 추가
                        //========= 하는걸로 추정중 , 전에 컴퓨트 셰이더로 물리효과 내는것 처럼
                        //  ==> 파티클이 스폰되기전에 월드에 있는 SPHCollider 태크를 가진 오브젝트를 모아서
                        //   => SPHColliderComponent를 추가 , 더 자세한건 SHPSystem 연구하고
                    }

                // 스폰 Entity를 제거한다. 제거하지 않으면 onUpdate에 의해 Cube가 계속 생성된다.
                    commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                }).ScheduleParallel();

            // 위의 작업이 완료되면
            // 예약한 명령을 다음 프레임에서 BeginInitializationEntityCommandBufferSystem 호출될때 처리하라고 등록한다.
            entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
        */
        #endregion

        BeginInitializationEntityCommandBufferSystem IntiECB;
        //BeginInitializationEntityCommandBufferSystem
        //EndInitializationEntityCommandBufferSystem
        bool IsSpawn = false;
        bool DoOnceEnable = false;

        //NativeParallelHashMap<int, Entity> particles;

        NativeList<Entity> SpawnedParticle;
        //NativeList<Entity> DisabledParticle;

        SpawnerAspect spawnerAspect;

        protected override void OnCreate()
        {
            base.OnCreate();
            IntiECB = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>();
            

        }
        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            if (SpawnedParticle.IsCreated)
                SpawnedParticle.Dispose();
        }
        protected override void OnUpdate()
        {
            if (SystemAPI.TryGetSingletonEntity<RemakedFluidSpawnComponent>(out var spanwerEntity))
            {
                var manager = SystemAPI.GetSingleton<RemakedFluidSpawnComponent>();
                spawnerAspect = SystemAPI.GetAspect<SpawnerAspect>(spanwerEntity);

                if (!IsSpawn)
                {
                    spawnerAspect.SpawnParticle(IntiECB.CreateCommandBuffer(), 4632u)
                        .Schedule(Dependency).Complete();
                    IntiECB.AddJobHandleForProducer(Dependency);
                    IsSpawn = true;
                    return;
                }

                var particle = GetEntityQuery(typeof(FluidSimlationComponent), typeof(LocalTransform)).ToEntityArray(Allocator.TempJob);
                //spawnerAspect.GetActiveParticle(this);

                if (!SpawnedParticle.IsCreated)
                {
                    SpawnedParticle = new NativeList<Entity>(manager.Amount, Allocator.Persistent);
                    SpawnedParticle.AddRange(particle);
                }

                if (Input.GetMouseButton(0))
                {
                    if (!DoOnceEnable)
                    {
                        var ecb = new EntityCommandBuffer(Allocator.TempJob);
                        DoOnceEnable = true;

                        spawnerAspect.EnableParticles(this, ecb, SpawnedParticle, (uint)World.Time.ElapsedTime, -1, 8);
                        ecb.Playback(EntityManager);
                        ecb.Dispose();
                    }

                    //DoOnceEnable = true;//작업이 끝난후 적용
                }else
                {
                    DoOnceEnable = false;
                }
            }


            return;

            {
                //IntiECB = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>();

                if (IsSpawn)
                {
                    //Enabled = false;
                    //========================= 비활성화된 파티클 접근
                    var particle = GetEntityQuery(typeof(FluidSimlationComponent), typeof(LocalTransform)).ToEntityArray(Allocator.TempJob);

                    //var amount = Entities.WithDisabled<FluidSimlationComponent>().ToQuery().CalculateEntityCount();
                    //EntityManager.SetEnabled();

                    //-------- FluidSimlationComponent가 비활성화 되면  다음 프레임에 취합해서 엔티티 비활성화 (안되나)

                    //Debug.Log($"--> {particle.CalculateEntityCount()} / Disable Amount : {amount}");

                    if (!SpawnedParticle.IsCreated)
                    {
                        var manager = SystemAPI.GetSingleton<RemakedFluidSpawnComponent>();
                        SpawnedParticle = new NativeList<Entity>(manager.Amount, Allocator.Persistent);
                        SpawnedParticle.AddRange(particle);
                    }


                    var ecb = new EntityCommandBuffer(Allocator.TempJob);

                    //var disableArray = Entities.WithDisabled<FluidSimlationComponent>().ToQuery().ToEntityArray(Allocator.TempJob);

                    var spawnedArray = SpawnedParticle.ToArray(Allocator.TempJob);


                    bool cliked = Input.GetMouseButton(0);
                    if (cliked)
                    {

                        //spawnedArray.Except(particle).ToArray();
                        var Except = new NativeArray<Entity>(spawnedArray.Except(particle).ToArray(), Allocator.TempJob);
                        //Debug.Log($"Except : {Except.Length}");


                        var spawnerPos = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<RemakedFluidSpawnComponent>());

                        new AllEnable()
                        {
                            ecb = ecb.AsParallelWriter(),
                            amount = Except.Length,
                            disabled = Except,
                            SpawnerTrans = spawnerPos
                        }.Schedule(Except.Length, 8, Dependency).Complete();

                    }//=============== 누르고 있을때 비활성화 되면  파티클이 한점에 뭉쳐저서... --> 한개씩 스폰해서 그럼

                    ecb.Playback(EntityManager);
                    ecb.Dispose();

                    return;
                }

                if (!SystemAPI.HasSingleton<ParticleParameterComponent>())
                {
                    Enabled = false;
                    return;
                }

                {
                    ParticleParameterComponent Parameter = SystemAPI.GetSingleton<ParticleParameterComponent>();
                    float particleSize = Parameter.ParticleRadius / 0.5f;

                    var ecb = IntiECB.CreateCommandBuffer().AsParallelWriter();//병렬작업


                    new SpawnParticle()
                    {
                        ecb = ecb,
                        particleSize = particleSize
                    }.ScheduleParallel(Dependency).Complete();

                    IntiECB.AddJobHandleForProducer(Dependency);


                    IsSpawn = true;
                }//spawn

            }

        }
        public partial struct SpawnParticle : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            public float particleSize;

            public void Execute(Entity e, [EntityIndexInQuery] int index, RemakedFluidSpawnComponent manager, LocalTransform transform)
            {
                var random = new Random(24825);
                int size = Mathf.FloorToInt(Mathf.Pow(manager.Amount, 1 / 3f));

                for (int i = 0; i < manager.Amount; i++)
                {
                    var instance = ecb.Instantiate(index, manager.particle);

                    var position = new float3((i % size) * 1.2f + random.NextFloat(-0.1f, 0.1f) * manager.RandomPower,
                        0 + (i / size / size) * 1.2f,
                        ((i / size) % size) * 1.2f + random.NextFloat(-0.1f, 0.1f) * manager.RandomPower) + transform.Position;

                    var Ltrans = new LocalTransform
                    {
                        Position = position,
                        Rotation = quaternion.identity,
                        Scale = particleSize
                    };

                    ecb.SetComponent(index, instance, Ltrans);
                }
            }
        }
        public partial struct CountDisabledParticle : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ecb;

            [ReadOnly] public NativeArray<Entity> entities;

            public void Execute(int index)
            {
                ecb.SetEnabled(index, entities[index], false);
            }
        }

        public partial struct AllEnable : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            public float amount;

            [ReadOnly] public NativeArray<Entity> disabled;

            [ReadOnly] public LocalTransform SpawnerTrans;

            public void Execute(int index)
            {

                var random = new Random((uint)(24825 + index));
                int size = Mathf.FloorToInt(Mathf.Pow(amount, 1 / 3f));
                var randomPos = new float3((index % size) * 1.2f + random.NextFloat(-0.1f, 0.1f),
                        0 + (index / size / size) * 1.2f,
                        ((index / size) % size) * 1.2f + random.NextFloat(-0.1f, 0.1f));

                //Debug.Log($"size : {size} / {index} : {index % size} , {index /size / size} , {index / size % size}");
                //Debug.Log("Re Active : " + index + " / " + disabled[index].Index + " / " + randomPos);

                //SpawnerTrans.Position += randomPos;//설마 안된 이유가 값 변경된게 누적되서??
                var spawnTrans = SpawnerTrans;
                spawnTrans.Position += randomPos;

                ecb.SetEnabled(index, disabled[index], true);
                ecb.SetComponentEnabled<FluidSimlationComponent>(index, disabled[index], true);
                ecb.SetComponent(index, disabled[index], spawnTrans);
                ecb.SetComponent(index, disabled[index], new FluidSimlationComponent() { position = spawnTrans.Position});
            }
        }
    }

}