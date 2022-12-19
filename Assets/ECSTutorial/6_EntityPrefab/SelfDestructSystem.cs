using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public partial class SelfDestructSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem m_EndSimECBSystem;

    protected override void OnCreate()
    {
        m_EndSimECBSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
        
    }
    protected override void OnUpdate()
    {
        // 멀티스레드에서 병렬로 처리되는 쓰기 명령을 기록하는 AsParallelWriter()
        var ecb = m_EndSimECBSystem.CreateCommandBuffer().AsParallelWriter();
        var dt = SystemAPI.Time.DeltaTime;
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref SelfDestruct spawner) =>
        {
            // 생존 시간이 0 이하가 되면 파괴한다.
            if((spawner.TimeToLive -= dt) < 0)
                ecb.DestroyEntity(entityInQueryIndex, entity);
        }).ScheduleParallel();
        
        // 명령버퍼를 예약하고 종속성을 전달한다. 
        m_EndSimECBSystem.AddJobHandleForProducer(Dependency);
    }
}
