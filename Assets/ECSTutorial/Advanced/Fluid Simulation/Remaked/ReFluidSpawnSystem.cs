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
                        // entity ���� ����
                        var instance = commandBuffer.Instantiate(entityInQueryIndex, manager.particle);

                        var position = new float3((i % 16) * 1.2f + random.NextFloat(-0.1f, 0.1f) * manager.Between,
                         (i / 16 / 16) * 1.1f * manager.Between,
                          ((i / 16) % 16) * 1.2f + random.NextFloat(-0.1f, 0.1f)) * manager.Between + trans.Position;


                        commandBuffer
                        .SetComponent(entityInQueryIndex, instance,
                            new LocalTransform { Position = position, Rotation = quaternion.identity, Scale = 1 });

                        //========= (SHPManager.AddCollider�κ�)���忡 ��� SPHCollider�� ���� ������ ������Ʈ �߰�
                        //========= �ϴ°ɷ� ������ , ���� ��ǻƮ ���̴��� ����ȿ�� ���°� ó��
                        //  ==> ��ƼŬ�� �����Ǳ����� ���忡 �ִ� SPHCollider ��ũ�� ���� ������Ʈ�� ��Ƽ�
                        //   => SPHColliderComponent�� �߰� , �� �ڼ��Ѱ� SHPSystem �����ϰ�
                    }

                // ���� Entity�� �����Ѵ�. �������� ������ onUpdate�� ���� Cube�� ��� �����ȴ�.
                    commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                }).ScheduleParallel();

            // ���� �۾��� �Ϸ�Ǹ�
            // ������ ����� ���� �����ӿ��� BeginInitializationEntityCommandBufferSystem ȣ��ɶ� ó���϶�� ����Ѵ�.
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

                    //DoOnceEnable = true;//�۾��� ������ ����
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
                    //========================= ��Ȱ��ȭ�� ��ƼŬ ����
                    var particle = GetEntityQuery(typeof(FluidSimlationComponent), typeof(LocalTransform)).ToEntityArray(Allocator.TempJob);

                    //var amount = Entities.WithDisabled<FluidSimlationComponent>().ToQuery().CalculateEntityCount();
                    //EntityManager.SetEnabled();

                    //-------- FluidSimlationComponent�� ��Ȱ��ȭ �Ǹ�  ���� �����ӿ� �����ؼ� ��ƼƼ ��Ȱ��ȭ (�ȵǳ�)

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

                    }//=============== ������ ������ ��Ȱ��ȭ �Ǹ�  ��ƼŬ�� ������ ��������... --> �Ѱ��� �����ؼ� �׷�

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

                    var ecb = IntiECB.CreateCommandBuffer().AsParallelWriter();//�����۾�


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

                //SpawnerTrans.Position += randomPos;//���� �ȵ� ������ �� ����Ȱ� �����Ǽ�??
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