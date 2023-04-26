using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Transforms;

//UpdateInGroup은 해당 시스템이 어떤그룹에 속하는지 설정 , 기본값  SimulationSystemGroup
//확인은 Systems 탭에서 / UpdateInGroup, UpdateBefore, UpdateAfter, DisableAutoCreation 존재
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class SpawnerSystem : SystemBase
{
    [BurstCompile]
    struct SetSpawnedTranslation : IJobParallelFor
    {
        // 보통은 병렬 작업에서 ComponentDataFromEntity에 쓰기 작업을 할 수 없다.
        // 병렬 쓰기를 가능하게 한다. 즉, IJobParallelFor는 인덱스 값으로 쓰기를 하므로 경쟁 문제가 발생하지 않으므로 사용한다.
        // 멀티쓰레드에서 쓰기 작업 중에 경쟁문제가 발생하지 않을 확신이 있을때만
        [NativeDisableParallelForRestriction]
        public ComponentLookup<LocalTransform> TranslationFromEntity;// ComponentDataFromEntity 에서 ComponentLookup 변경

        public NativeArray<Entity> Entities;

        // 로컬 좌표
        public float4x4 LocalToWorld;
        public int Stride;

        public void Execute(int index)
        {
            var entity = Entities[index];
            var y = index / Stride;
            var x = index - (y * Stride);

            // Entity의 좌표를 변경한다.
            TranslationFromEntity[entity] = new LocalTransform()
            {
                Position = math.transform(LocalToWorld, new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) * 2, y * 1.3F)),
                Rotation = quaternion.identity,
                Scale = 1
            };
        }
    }
    
    protected override void OnUpdate()
    {
        // WithStructuralChanges 버스트를 비활성하여 함수 내에서 엔티티 데이터를 구조적으로 변경할 수 있게 해준다.
        // WithStructuralChanges 보단 EntityCommandBuffer를 사용하는 것이 성능상 더 좋다. 
        Entities.WithStructuralChanges().ForEach((Entity entity, int entityInQueryIndex, in tutorial.Spawner spawnerFromEntity, in LocalToWorld spawnerLocalToWorld) =>
        {
                // Job 이 끝날때까지 메인쓰레드가 대기한다. (멀티스레드에서 작동하므로 비동기)
                Dependency.Complete();

                var spawnedCount = spawnerFromEntity.CountX * spawnerFromEntity.CountY;

                // NativeArray<Entity> 초기화
                var spawnedEntities =
                    new NativeArray<Entity>(spawnedCount, Allocator.TempJob); 

                // spawnedEntities 크기만큼 Entity를 생성하고 spawnedEntities에 생성한 Entity를 넣습니다.
                EntityManager.Instantiate(spawnerFromEntity.Prefab, spawnedEntities);
    
                // Spawner Entity를 제거합니다. (안 그러면 매프레임마다 Entity를 생성함)
                EntityManager.DestroyEntity(entity);

                var translationFromEntity = GetComponentLookup<LocalTransform>();
                // GetComponentDataFromEntity >> ComponentLookup
                var setSpawnedTranslationJob = new SetSpawnedTranslation
                {
                    TranslationFromEntity = translationFromEntity,
                    Entities = spawnedEntities,
                    LocalToWorld = spawnerLocalToWorld.Value,
                    Stride = spawnerFromEntity.CountX
                };

                // spawnedCount 수행할 반복횟수
                // 하나의 스레드에서 맻개의 Batch를 만들어서 처리할껀지
                // 두번째 매개변수는 배치 크기이다. 보통 32 또는 64를 사용하며, 매우 무거운 Job일 경우 1를 쓰는 것이 좋을 수 있다.
                Dependency = setSpawnedTranslationJob.Schedule(spawnedCount, 64, Dependency);
                Dependency = spawnedEntities.Dispose(Dependency);//메모리 할당 해제
        }).Run();

        /*
        int entityInQueryIndex :
            Entities.ForEach는 람다로 정의된 Query를 만족하는 모든 Entity를 하나씩 실행시키는 함수입니다.
             entityInQueryIndex는 조회된 Entity의 식별코드입니다.

        WithStructuralChanges() : 
            Entities.ForEach는 기본적으로 구조 변경(EntityManager로 Entity를 생성 및 제거 등)이 안 되도록 되어 있습니다.
            ( ForEach에서 람다로 정의한 쓰기 가능 타입은 구조 변경 가능) 그러나,
            메인스레드에서만 동작하게 하는 Run() 과 WithStructuralChanges() 으로 구조 변경을 할 수 있게 합니다.
            몰론 버스트컴파일을 끄고, 메인스레드에서 동작하므로 성능상 좋지 않습니다.
            WithStructuralChanges()보다는 EntityCommandBuffer를 사용하는 것이 좋습니다.
        */
    }
}
