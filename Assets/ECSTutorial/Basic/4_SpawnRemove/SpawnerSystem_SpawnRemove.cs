using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

public partial class SpawnerSystem_SpawnRemove : SystemBase
{
    BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        //SystemHandle temp = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        //EntityCommandBuffer temp = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        entityCommandBufferSystem = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>();
        //** Managed -> Class

    }
    protected override void OnUpdate()
    {
        // 병렬 쓰기 작업에서 명령을 예약할 예정이므로 AsParallelWriter();를 설정한다.
        // 예약된 명령을 BeginInitializationEntityCommandBufferSystem가 호출될때 실행되도록 한 것이다. 
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithName("SpawnerSystem_SpawnAndRemove")
            // 1, 2번 매개변수는 기본값이다. 3번째는 동기식으로 처리한다는 의미이다. 
            // 대충 이런게 있다 정도로만 이해하자.
            .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
            .ForEach((Entity entity, int entityInQueryIndex, in Spawner_SpawnRemover spawner, in LocalToWorld location) =>
            {
                // ref 쓰기허용 , in 읽기전용 
                var random = new Random(1);
                Debug.Log("Spawning");
                for (var x = 0; x < spawner.CountX; x++)
                {
                    for (var y = 0; y < spawner.CountY; y++)
                    {
                        // entity 생성 예약
                        var instance = commandBuffer.Instantiate(entityInQueryIndex, spawner.Prefab);

                        // 엔티티가 생성될 위치를 결정한다.
                        var position = math.transform(location.Value, new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) * 2, y * 1.3F));

                        // 생성한 Entity의 위치를 position위치로 변경하는 명령을 예약한다. 
                        commandBuffer
                        .SetComponent(entityInQueryIndex, instance, new LocalTransform 
                            { Position = position , Rotation = quaternion.identity, Scale = 1});

                        // 랜덤 회전속도
                        commandBuffer
                        .SetComponent(entityInQueryIndex, instance,
                         new RotationSpeed_SpawnRemove{RadiansPerSecond = math.radians(random.NextFloat(30f, 90f))});

                        commandBuffer
                        .SetComponent(entityInQueryIndex, instance,
                         new LifeTime { Value = random.NextFloat(10f, 20f)});
                    }
                }                

                // 스폰 Entity를 제거한다. 제거하지 않으면 onUpdate에 의해 Cube가 계속 생성된다.
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }).ScheduleParallel();

        // 위의 작업이 완료되면
        // 예약한 명령을 다음 프레임에서 BeginInitializationEntityCommandBufferSystem 호출될때 처리하라고 등록한다.
        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
