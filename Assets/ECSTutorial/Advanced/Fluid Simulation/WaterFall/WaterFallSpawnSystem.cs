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

namespace FluidSimulate.WaterFall
{
    public partial class WaterFallSpawnSystem : SystemBase
    {
        BeginInitializationEntityCommandBufferSystem IntiECB;
        EntityQuery ParticlesQuery;

        bool IsSpawn = false;
        float Timer = 0f;
        NativeArray<Entity> entities;
        WaterFallSpawnComponent Manager;

        enum SpawnWorkType { Inti , Reset, Spawning};
        SpawnWorkType SpawnWork = SpawnWorkType.Inti;

        NativeParallelMultiHashMap<int, Entity> SpawnedEntity;

        #region Job
        partial struct Spawn : IJobEntity
        {
            public void Execute([EntityIndexInQuery] int index)
            {

            }
        }
        partial struct SetEnableAll : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public NativeArray<Entity> entities;
            public bool Vaule;
            public void Execute([EntityIndexInQuery] int index)
            {
                ECB.SetEnabled(index, entities[index], Vaule);
            }
        }
        partial struct TempTimer : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
            public EntityQuery entityQuery;

            public NativeArray<Entity> entities;
            public float Timer;
            public float SpawnInterval;

            public NativeParallelMultiHashMap<int, Entity>.ParallelWriter SpawnedEntity;// �б�� ���⸦ ���ÿ� ���� , ��� ����ó�� ����
            //NativeHashMap - �ȵ�

            //public EntityManager manager;//---------------------- �ȵ�
            // EntityManager, SystemAPI �� �ȵǳ�
            //------------------------------------ �׳� TAG �ٿ��� ���� ---> �±׸� ���̰� ��Ȱ��ȭ?

            public void Execute(int index)// , in FluidSimlationComponent manager)
            {
                //if ((Timer / 10f) > (index / entities.Length))
                
                {
                    if (entities.Length > index)
                    {
                        if (entityQuery.Matches(entities[index]))
                        {
                            //Debug.Log("--" + index);
                            //SpawnedEntity[index] = false;
                        }
                        else
                        {
                            if (Timer > index * SpawnInterval)
                            {
                                ECB.SetEnabled(index, entities[index], true);

                                var temp = new FluidSimlationComponent();
                                temp.velocity = Vector3.forward;
                                //ECB.SetComponent(index, entities[index], temp);// �̰ž��� �Ѱ��� ����

                                //SpawnedEntity[index] = true;
                                SpawnedEntity.Add(index, entities[index]);
                            }
                        }
                    }
                }
            }
        }
        partial struct StartVelocity : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public NativeArray<Entity> entities;
            public Vector3 velocity;
            [ReadOnly] public NativeParallelMultiHashMap<int, Entity> SpawnedEntity;

            public void Execute([EntityIndexInQuery] int index, in LocalTransform trans, in SpawnedTag tag)
            {
                //Debug.Log(index + " : " + SpawnedEntity[index]);
                
                if (SpawnedEntity.TryGetFirstValue(index, out Entity vaule, out var it))//(SpawnedEntity[index])
                {
                    var temp = new FluidSimlationComponent();
                    temp.velocity = velocity;
                    ECB.SetComponent(index, vaule, temp);// ����ȿ�� Ű�� ���� ����� , ���� �״�� �ְ�...

                    var tempPos = trans;
                    tempPos.Scale = 0.25f;
                    Debug.Log("new Spawn");
                    //ECB.SetComponent(index, vaule, tempPos);
                }else
                {
                    if (trans.Scale < 1)
                    {
                        var tempPos = trans;
                        tempPos.Scale = 1;

                        //ECB.SetComponent(index, entities[index], tempPos);
                    }
                }

                //SpawnedEntity.ContainsKey(index)
                //SpawnedEntity.TryGetValue(index, out Entity temp);
            }
        }
        #endregion

        protected override void OnCreate()
        {
            IntiECB = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>();
            RequireForUpdate<WaterFallSpawnComponent>();

            SpawnedEntity = new(entities.Length, Allocator.TempJob);
        }
        protected override void OnUpdate()
        {
            //------- ���������� ����
            var ecb = IntiECB.CreateCommandBuffer().AsParallelWriter();//�����۾�      
            //NativeArray<Entity> SpawnedEntity = new (entities.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            if (SpawnWork == SpawnWorkType.Spawning)
            {
                StartVelocity startVelocityJob = new StartVelocity
                {
                    ECB = ecb,
                    entities = entities,
                    velocity = Vector3.forward * 1,
                    //SpawnedEntity = CheckSpawnedEntity
                    SpawnedEntity = SpawnedEntity
                };
                var startVelocityHandle = startVelocityJob.ScheduleParallel(Dependency);//startVelocityJob.ScheduleParallel(ParticlesQuery, TimerHandle);
                startVelocityHandle.Complete();

                SpawnedEntity = new(entities.Length, Allocator.TempJob);//------ ���� ����� �̹� �����ӿ� �߰��Ȱɷ� �ϴٺ��� ������ ��Ͽ� �����
                // --------------- SpawnWork.Spawning �� �ٿ��� ���� �Ǵ��� Ȯ��
            }

            switch (SpawnWork)
            {
                case SpawnWorkType.Inti:
                    {
                        Entities
                            //.WithAll<RemakedFluidSpawnComponent>()
                            .WithName("WaterFall")
                            .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
                            .ForEach((Entity e, int entityInQueryIndex, in WaterFallSpawnComponent manager, in LocalTransform transform) =>
                            {

                                var random = new Random(24825);
                                int size = Mathf.FloorToInt(Mathf.Pow(manager.Amount, 1 / 3f));

                                for (int i = 0; i < manager.Amount; i++)
                                {
                                    var instance = ecb.Instantiate(entityInQueryIndex, manager.particle);

                                    var position = new float3((i % size) * 1.2f + random.NextFloat(-0.1f, 0.1f) * manager.RandomPower,
                                        0 + (i / size / size) * 1.2f,
                                        ((i / size) % size) * 1.2f + random.NextFloat(-0.1f, 0.1f) * manager.RandomPower) + transform.Position;

                                    var Ltrans = new LocalTransform
                                    {
                                        Position = position,
                                        Rotation = quaternion.identity,
                                        Scale = 1
                                    };

                                    ecb.SetComponent(entityInQueryIndex, instance, Ltrans);
                                    ecb.AddComponent(entityInQueryIndex, instance, new SpawnedTag());
                                    //ecb.SetComponent(entityInQueryIndex, instance, new FluidSimlationComponent());
                                    //ecb.SetEnabled(entityInQueryIndex, instance, false);
                                }
                            }).ScheduleParallel();

                        IntiECB.AddJobHandleForProducer(Dependency);
                        ParticlesQuery = GetEntityQuery(typeof(FluidSimlationComponent));

                        SpawnWork = SpawnWorkType.Reset;
                        break;
                    }
                case SpawnWorkType.Reset:
                    {

                        if (entities.Length == 0)
                        {
                            entities = ParticlesQuery.ToEntityArray(Allocator.Persistent);
                            Manager = SystemAPI.GetSingleton<WaterFallSpawnComponent>();
                        }

                        SetEnableAll setEnableJob = new SetEnableAll
                        {
                            ECB = ecb,
                            entities = ParticlesQuery.ToEntityArray(Allocator.TempJob),
                            Vaule = false
                        };
                        JobHandle SetEnableHandle = setEnableJob.ScheduleParallel(ParticlesQuery, Dependency);
                        SetEnableHandle.Complete();

                        SpawnWork = SpawnWorkType.Spawning;
                        break;
                    }
                case SpawnWorkType.Spawning:
                    {

                        Timer += SystemAPI.Time.DeltaTime;

                        //NativeArray<bool> CheckSpawnedEntity = new NativeArray<bool>(entities.Length, Allocator.TempJob);

                        TempTimer tempTimerJob = new TempTimer
                        {
                            ECB = ecb,
                            entities = entities,
                            Timer = Timer,
                            SpawnInterval = Manager.SpawnInterval,
                            entityQuery = ParticlesQuery,
                            SpawnedEntity = SpawnedEntity.AsParallelWriter()
                        };
                        var TimerHandle = tempTimerJob.Schedule(entities.Length, 64, Dependency);
                        TimerHandle.Complete();
                        Dependency = TimerHandle;

                        //-------------------------- �ʱ� �ӷ� - StartVelocity
                        /*
                        StartVelocity startVelocityJob = new StartVelocity
                        {
                            ECB = ecb,
                            entities = entities,
                            velocity = Vector3.forward * 100,
                            //SpawnedEntity = CheckSpawnedEntity
                            SpawnedEntity = SpawnedEntity
                        };
                        var startVelocityHandle = startVelocityJob.ScheduleParallel( TimerHandle);//startVelocityJob.ScheduleParallel(ParticlesQuery, TimerHandle);
                        startVelocityHandle.Complete();
                        Dependency = startVelocityHandle;
                        */
                        

                        //------------ ParticlesQuery�� Ȱ��ȭ�Ȱ͸� �ϴٺ��� �� �����Ӹ��� ��Ȱ��ȭ

                        for (int i = 0; i < entities.Length; i++)
                        {
                            //if (CheckSpawnedEntity[i])
                            {
                                //Debug.Log(" --- : " + i);
                                break;
                            }
                        }

                        //Debug.Log(GetEntityQuery(typeof(SpawnedTag)).CalculateEntityCount() + " / " + entities.Length + "\n" 
                        //    + SpawnedEntity.Count());
                        break;
                    }

            }//End Switch
        }
    }
}
