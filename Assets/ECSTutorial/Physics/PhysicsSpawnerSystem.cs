using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

//UpdateInGroup은 해당 시스템이 어떤그룹에 속하는지 설정 , 기본값  SimulationSystemGroup
//확인은 Systems 탭에서 / UpdateInGroup, UpdateBefore, UpdateAfter, DisableAutoCreation 존재
//[UpdateInGroup(typeof(SimulationSystemGroup))]
//[UpdateBefore(typeof(InitializationSystemGroup))]
public partial class PhysicsSpawnerSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    FixedStepSimulationSystemGroup FixedSystem;

    protected override void OnStartRunning()
    {

        entityCommandBufferSystem = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>();
        FixedSystem = World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();

        //      FixedStepSimulationSystemGroup 만 RateManager 유효한가?

        //Debug.Log("Fixed Time : " + World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>().RateManager.Timestep);
        //World.GetExistingSystemManaged<FixedStepSimulationSystemGroup>().RateManager.Timestep = 0.05f;

        //NOTE - 어째서 Update로 한 방식이 훨씬 빠르지?

        {
            
            //var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            var ECB = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>()
                .CreateCommandBuffer();

            

            Entities
            .WithName("PhysicsSpawnerSystem")
            //.WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
            .ForEach((Entity entity, int entityInQueryIndex, in CPhysicsObjSpawner Cspanwer, in LocalTransform trans) =>
            {
                float3 sphereMin = new float3(Cspanwer.spawnSize.x,0, Cspanwer.spawnSize.z) * -Cspanwer.betweenOffset;
                float3 sphereMax = new float3(Cspanwer.spawnSize.x, Cspanwer.spawnSize.y, Cspanwer.spawnSize.z) * Cspanwer.betweenOffset;

                Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)entityInQueryIndex + 1);

                for (int i = 0, h = 0; h < Cspanwer.spawnSize.z; h++)
                {
                    for (int w = 0; w < Cspanwer.spawnSize.x; w++)
                    {
                        for (int l = 0; l < Cspanwer.spawnSize.y; l++, i++)
                        {
                            if (Cspanwer.isSphere)
                            {
                                
                                var instance = ECB.Instantiate(Cspanwer.prefab);
                                ECB.SetComponent
                                    (instance, new LocalTransform
                                    {
                                        Position = random.NextFloat3(sphereMin, sphereMax) + Cspanwer.offset 
                                            + new float3(random.NextFloat(-0.1f, 0.1f), 0.5f, random.NextFloat(-0.1f, 0.1f))
                                            + trans.Position,
                                        Rotation = quaternion.identity,
                                        Scale = 1
                                    });

                            }else
                            {
                                if (Cspanwer.ishollow)
                                {
                                    if (l != 0 && w != 0 && l != Cspanwer.spawnSize.y - 1 && w != Cspanwer.spawnSize.x - 1)
                                    {
                                        //Skip to the next values
                                        continue;
                                    }
                                }

                                var instance = ECB.Instantiate(Cspanwer.prefab);

                                ECB.SetComponent
                                    (instance, new LocalTransform
                                    {
                                        Position = new float3(h, l, w) * Cspanwer.betweenOffset + Cspanwer.offset + new float3(0, 0.5f, 0),
                                        Rotation = quaternion.identity,
                                        Scale = 1
                                    });
                            }
                        }
                    }
                }
            }).Schedule();

            entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
    protected override void OnUpdate()
    {
        //Debug.Log("Delta : " + SystemAPI.Time.DeltaTime + " / ECS FixedTime : " + Mathf.Max(SystemAPI.Time.DeltaTime * 5,  0.01666667f));

        //FixedSystem.RateManager.Timestep = Mathf.Max(SystemAPI.Time.DeltaTime * 5,  0.01666667f);
        //--------------- PhysicsStep.SolverIterationCont 가 1이면 오히려 겹침상태가 오래 지속되 성능저하

        {
            /*
            var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
            .WithName("PhysicsSpawnerSystem")
            .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
            .ForEach((Entity entity, int entityInQueryIndex, in CPhysicsObjSpawner Cspanwer) =>
            {
                for (int h = 0; h < Cspanwer.spawnSize.z; h++)
                {
                    for (int w = 0; w < Cspanwer.spawnSize.x; w++)
                    {
                        for (int l = 0; l < Cspanwer.spawnSize.y; l++)
                        {
                            if (Cspanwer.ishollow)
                            {
                                if (l != 0 && w != 0 && l != Cspanwer.spawnSize.y - 1 && w != Cspanwer.spawnSize.x - 1)
                                {
                                    //Skip to the next values
                                    continue;
                                }
                            }

                            //var instance = commandBuffer.Instantiate(entityInQueryIndex, spawner.Prefab);
                            var instance = commandBuffer.Instantiate(entityInQueryIndex, Cspanwer.prefab);

                            commandBuffer.SetComponent
                                (entityInQueryIndex, instance, new LocalTransform
                                {
                                    Position = new float3(h, l, w) * 1.1f + Cspanwer.offset + new float3(0, 0.5f, 0),
                                    Rotation = quaternion.identity,
                                    Scale = 1
                                }
                                );
                        }
                    }
                }

                // 스폰 Entity를 제거한다. 제거하지 않으면 onUpdate에 의해 Cube가 계속 생성된다.
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();

            entityCommandBufferSystem.AddJobHandleForProducer(Dependency);*/
        }//Disable
    }
}
