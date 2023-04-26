using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public struct LifeTime : IComponentData
{
    public float Value;
}
public partial class LifeTimeSystem : SystemBase
{
    EntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        /*
        Cube를 생성할때 BeginInitializationEntityCommandBufferSystem를 사용했다면 이번에는 Cube를 삭제하는 기능이므로
         Simulation이 종료될때 호출되는 EndSimulationEntityCommandBufferSystem를 가져왔습니다. 
         (생성이 되어야 삭제가 될테니 생성되는 CommandBuffer보다 삭제되는 CommandBuffer가 늦게 처리되도록 함)
        */
    }
    protected override void OnUpdate()
    {
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var deltaTime = SystemAPI.Time.DeltaTime;

                // entityInQueryIndex 는 Query로 조회된 Entities의 식별코드이다.
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref LifeTime lifetime) =>
        {
            lifetime.Value -= deltaTime;
            //ForEach에서 조회한 Entities를 처리할때 각 entity마다 DeltaTime이 다르면 안되므로
            // OnUpdate에서 ForEach가 실행되기 전에 참조시킵니다.

            if (lifetime.Value < 0.0f)
            {
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }
        }).ScheduleParallel();

                // 명령 실행 예약하기
        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
