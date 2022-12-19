using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

public partial class PrefabSpawnerSystem : SystemBase
{
        private BeginSimulationEntityCommandBufferSystem m_BeginSimECBSystem;

    protected override void OnCreate()
    {
        m_BeginSimECBSystem = World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = m_BeginSimECBSystem.CreateCommandBuffer().AsParallelWriter();
        var rnd = new Random((uint)Environment.TickCount);

        Entities.WithNone<RequestEntityPrefabLoaded>().ForEach((Entity entity, int entityInQueryIndex, ref PrefabSpawner spawner, in DynamicBuffer<PrefabSpawnerBufferElement> prefabs) =>
        {

            // ecb에게 spawner 에게 RequestEntityPrefabLoaded를 추가하라고 명령내린다.
            ecb.AddComponent(entityInQueryIndex, entity, new RequestEntityPrefabLoaded {Prefab = prefabs[rnd.NextInt(prefabs.Length)].Prefab});
        }).ScheduleParallel();
        /*
        ecb.AddComponent에서 조회된 entity(Spawner)에게 RequestEntityPrefabLoaded를 Component로 추가하면서 우리가 위에서 연결한 두개의 Prefab 중에 하나를 선택하는 코드가 있다. 
        RequestEntityPrefabLoaded가 Entity에게 추가되면 유니티에서 만들어놓은 시스템이 EntityPrefabReference에 참조된 Prefab을 로드시킨다.
        */

        var dt = SystemAPI.Time.DeltaTime;

                // 위에서 생성한 entity가 로드가 완료되면 해당 entity에게 PrefabLoadResult 컴포넌트가 추가된다.
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref PrefabSpawner spawner, in PrefabLoadResult prefab) =>
        {
 
            var remaining = spawner.SpawnsRemaining;
            if (remaining < 0.0f)
            {
                // 더 이상 생성할 인스턴스 없음
                ecb.DestroyEntity(entityInQueryIndex, entity);
                return;
            }

            var newRemaining = remaining - dt * spawner.SpawnsPerSecond;
            var spawnCount = (int) remaining - (int) newRemaining;
            for (int i = 0; i < spawnCount; ++i)
            {
                var instance = ecb.Instantiate(entityInQueryIndex, prefab.PrefabRoot);
                int index = i + (int) remaining;
                ecb.SetComponent(entityInQueryIndex, instance, new LocalTransform
                {
                    Position = new float3(index*((index&1)*2-1), 0, 0),
                    Rotation = quaternion.identity,
                    Scale = 1
                });
            }
            spawner.SpawnsRemaining = newRemaining;
        }).ScheduleParallel();
        /*
        위에서 RequestEntityPrefabLoaded가 추가되면 유니티가 만들어놓은 System에서 Prefab을 Entity로 Create한다. 
        그리고 Create가 완료되면 Spawner에게 PrefabLoadResult가 자동으로 추가된다. (유니티에서 만들어놓은 System에서 저렇게 로직을 만들어놨다.)
         PrefabLoadResult에는 Create된 Entity가 전달되는데 이를 통해 Instantiate를 해서 Entity를 생성할 수 있다.
        */

        m_BeginSimECBSystem.AddJobHandleForProducer(Dependency);
    }
}
