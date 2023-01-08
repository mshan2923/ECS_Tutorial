using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Tutorial.ECB
{
    public partial class SpawnEntitySystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem endSimulECB;
        protected override void OnCreate()
        {
            endSimulECB = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {
            var daltaTime = SystemAPI.Time.DeltaTime;
            var ecb = endSimulECB.CreateCommandBuffer().AsParallelWriter();// 병렬작업
            var random = new Unity.Mathematics.Random(35165);

            Entities.WithAll<ShouldSpawnTag>().ForEach((Entity e, int entityInQueryIndex,
                 ref EntitySpawnComponent spawnData, in LocalTransform trans) =>
                 {
                    //entityInQueryIndex은 사용할려면 이름이 같아야함
                    spawnData.timer -= daltaTime;
                    if (spawnData.timer <= 0)
                    {
                        spawnData.timer = spawnData.spawnDelay;
                        var newEntity = ecb.Instantiate(entityInQueryIndex, spawnData.prefabToSpawn);
                        //병렬작업 이여서  entityInQueryIndex 가 필요

                        ecb.AddComponent<CapsuleTag>(entityInQueryIndex, newEntity);
                        
                        var Ltrans = trans;
                        Ltrans.Position += random.NextFloat3(new float3(-1,0,-1), new float3(1,1,1));

                        ecb.SetComponent(entityInQueryIndex, newEntity, Ltrans);
                    }
                 }).ScheduleParallel();

                //Dependency.Complete();// 병렬 작업이 끝났다는걸 수동으로 알림
                endSimulECB.AddJobHandleForProducer(this.Dependency);
                //EndSimulationEntityCommandBufferSystem 변경사항 적용전 작업을 완료해야한다고 알림
        }
    }

}