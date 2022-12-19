using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

public partial class PhysicsSpawnerSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        entityCommandBufferSystem = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        {
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
                            if (l != 0 && w != 0 && l != Cspanwer.spawnSize.y - 1 && w != Cspanwer.spawnSize.x - 1)
                            {
                                //Skip to the next values
                                continue;
                            }

                            //var instance = commandBuffer.Instantiate(entityInQueryIndex, spawner.Prefab);
                            var instance = commandBuffer.Instantiate(entityInQueryIndex, Cspanwer.prefab);

                            commandBuffer.SetComponent
                                (entityInQueryIndex, instance, new LocalTransform
                                {
                                    Position = new float3(h, l + 0.5f, w) + Cspanwer.offset,
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

            entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
