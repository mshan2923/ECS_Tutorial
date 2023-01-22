using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

namespace Tutorial.GroupMovement
{
    [BurstCompile]
    public partial class TestGroupSystem : SystemBase
    {
        EntityCommandBuffer ecb;
        EntityCommandBuffer.ParallelWriter ecbParallel;
        GroupComponent Cgroup;

        int ArriveUnit;
        Vector2Int resized;
        int Amount;
        float MovedTime;
        protected override void OnCreate()
        {
            //base.OnCreate();
            //Enabled = false;
            Enabled = SystemAPI.HasSingleton<GroupComponent>();        
        }
        protected override void OnStartRunning()
        {
            //NOTE - ECB으로 GetComponent X , SharedComponent는 수정X , IJob안에 ECB 왜..안되지?
                            
                Cgroup = SystemAPI.GetSingleton<GroupComponent>();
                var Temp = Cgroup.size / Cgroup.betweenSpace;
                resized = new Vector2Int(Mathf.FloorToInt(Temp.x), Mathf.FloorToInt(Temp.y));
                Amount = resized.x * resized.y;

                ecb = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>()
                    .CreateCommandBuffer();
                ecbParallel = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>()
                    .CreateCommandBuffer().AsParallelWriter();

                for (int i = 0; i < Amount; i++)
                {
                    var e = ecb.Instantiate(Cgroup.prefab);
                    ecb.AddComponent<UnitData>(e, new UnitData{index = i});
                    ecb.AddSharedComponent<ArriveData>(e, new ArriveData{Arrive = false});

                    //var trans = EntityManager.GetComponentData<LocalTransform>(e);
                    //trans.Position = new float3((i % resized.x) * Cgroup.betweenSpace, 0,
                    //    (Mathf.Floor(i / resized.x) * Cgroup.betweenSpace));//NOTE - 오류뜸
                    ecb.SetComponent<LocalTransform>(e, new LocalTransform
                    {
                        Position = new float3((i % resized.x) * Cgroup.betweenSpace, 0,
                        (Mathf.Floor(i / resized.x) * Cgroup.betweenSpace)),
                        Rotation = quaternion.identity,
                        Scale = Cgroup.unitSize
                    });
                }            
                
        }
        protected override void OnUpdate()
        {
            if (MovedTime >= Cgroup.moveTime)
                MovedTime = 0;
            MovedTime += SystemAPI.Time.DeltaTime;

            var testHandle = new TestForEachEntity
            {
                //ecb = this.ecb,
                movedTime = MovedTime,
                moveTime = Cgroup.moveTime,
                amount = Amount,
                delta = SystemAPI.Time.DeltaTime,
                speed = Cgroup.speed,
                toAPoint = false,
                aPoint = Cgroup.aPoint,
                bPoint = Cgroup.bPoint
            };

            testHandle.ScheduleParallel();

            //DynamicBuffer 안되고.. sharedComponent는 Query로 못바꾸고


            //Entities.WithSharedComponentFilter<ArriveData>(new ArriveData{Arrive = false}).ForEach((Entity e) => 
            //    {
            //    }).Run();
            int amount = 0;
            Entities.ForEach((Entity e, in UnitData data) => 
            {
                if (data.Arrive)
                    amount++;
            }).Run();
            ArriveUnit = amount;
            if (amount == 0)
                Debug.Log("All Unit Arrive");
        }

        [BurstCompile]
        partial struct TestForEachEntity : IJobEntity
        {
            //ECB가 왜...
            //public EntityCommandBuffer ecb;
            public float movedTime;
            public float moveTime;
            public int amount;
            public float delta;
            public float speed;
            public bool toAPoint;
            public float3 aPoint;
            public float3 bPoint;
            void Execute(Entity e, [EntityIndexInQuery] int sortKey, ref UnitData data, ref LocalTransform trans)
            {
                float timeRate = movedTime / moveTime;
                float amountRate = (float)data.index / amount;
                if (amountRate > timeRate)
                {
                    if (toAPoint)
                    {
                        trans.Position += math.normalize((aPoint - bPoint)) * speed * delta;
                    }else
                    {
                        trans.Position += math.normalize((bPoint - aPoint)) * speed * delta;
                    }
                }
                
                data.Arrive = (amountRate > timeRate);
                //ecbParallel.SetSharedComponent<ArriveData>(sortKey, e, new ArriveData{Arrive = (amountRate > timeRate)});
                //ecb.SetSharedComponent<ArriveData>(e, new ArriveData{Arrive = (amountRate > timeRate)});
            }
        }
    }

}