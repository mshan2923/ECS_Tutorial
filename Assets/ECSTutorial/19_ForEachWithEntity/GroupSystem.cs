using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

namespace Tutorial.GroupMovement
{
    public partial class GroupSystem : SystemBase
    {
        EntityCommandBuffer ecb;
        EntityCommandBuffer.ParallelWriter ecbParallel;
        GroupComponent Cgroup;
        MoveJob moveJob;

        int ArriveUnit;
        int2 resized;
        int Amount;
        float MovedTime;
        bool ToAPoint = false;
        protected override void OnCreate()
        {
            //base.OnCreate();
            Enabled = false;
        }
        protected override void OnStartRunning()
        {
            //NOTE - ECB으로 GetComponent X , SharedComponent는 수정X , IJob안에 ECB 왜..안되지?
            //NOTE - 시간 조금 지나면 자기 위치로 안가는데...
                            
                Cgroup = SystemAPI.GetSingleton<GroupComponent>();
                var Temp = Cgroup.size / Cgroup.betweenSpace;
                resized = new int2(Mathf.FloorToInt(Temp.x), Mathf.FloorToInt(Temp.y));
                Amount = resized.x * resized.y;
        
                ecb = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>()
                    .CreateCommandBuffer();
                ecbParallel = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>()
                    .CreateCommandBuffer().AsParallelWriter();

                for (int i = 0; i < Amount; i++)
                {
                    var e = ecb.Instantiate(Cgroup.prefab);
                    float3 offsetPos = new float3((i % resized.x) * Cgroup.betweenSpace, 0,
                        (Mathf.Floor(i / resized.x) * Cgroup.betweenSpace));

                    ecb.AddComponent<UnitData>(e, new UnitData{index = i, offset = offsetPos});
                    ecb.AddSharedComponent<ArriveData>(e, new ArriveData{Arrive = false});

                    ecb.SetComponent<LocalTransform>(e, new LocalTransform
                    {
                        Position = Cgroup.aPoint + offsetPos,
                        Rotation = quaternion.identity,
                        Scale = Cgroup.unitSize
                    });
                }
        
        }

        protected override void OnUpdate()
        {
            //Movetime은 이동 딜레이 , 스폰의 역순으로 먼저 이동
            // 
            //if (MovedTime >= Cgroup.moveTime)
            //    MovedTime = 0;
            MovedTime += SystemAPI.Time.DeltaTime;
            moveJob = new MoveJob
            {
                movedTime = MovedTime,
                moveTime = Cgroup.moveTime,
                amount = Amount,
                delta = SystemAPI.Time.DeltaTime,
                speed = Cgroup.speed,

                resizedMap = this.resized,
                betweenSpace = Cgroup.betweenSpace,
                toAPoint = ToAPoint,
                aPoint = Cgroup.aPoint,
                bPoint = Cgroup.bPoint
            };//----   매번 초기화를 시켜줘야 잘작동... (안하면 처음 이동이후 돌아올때 제위치로 안감)
   

            if (ArriveUnit < Amount)
            {
                moveJob.ScheduleParallel();

                int Lamount = 0;
                Entities.ForEach((Entity e, in UnitData data) => 
                {
                    if (data.Arrive)
                        Lamount++;
                }).Run();
                ArriveUnit = Lamount;
            }else
            {
                //Debug.Log("All Unit Arrive - " +  ArriveUnit + " / " + Amount + "\n" + (ToAPoint ? "To A Point" : "To B Point"));
                ToAPoint = !ToAPoint;
                ArriveUnit = 0;
                MovedTime = 0;
            }

        }

        [BurstCompile]
        partial struct MoveJob : IJobEntity
        {
            public float movedTime;
            public float moveTime;
            public int amount;
            public float delta;
            public float speed;

            public int2 resizedMap;
            public float betweenSpace;
            public bool toAPoint;
            public float3 aPoint;
            public float3 bPoint;
            void Execute(Entity e, [EntityIndexInQuery] int sortKey, ref UnitData data, ref LocalTransform trans)
            {
                float timeRate = movedTime / moveTime;
                float amountRate = (float)data.index / amount;
                //float3 unitLocalPos = UnitLocalPos(data.index);

                //역순으로 이동 시작하고 , 목표지점에 도착하면 도착

                if ((1 - timeRate) <= amountRate)
                {
                    if (toAPoint)
                    {
                        //trans.Position += math.normalize((aPoint + data.offset - trans.Position)) * speed * delta;
                        trans.Position += math.normalize((aPoint)) * speed * delta;
                    }else
                    {
                        //trans.Position += math.normalize((bPoint + data.offset - trans.Position)) * speed * delta;
                        trans.Position += math.normalize((bPoint)) * speed * delta;
                    }
                }

                {
                    if (toAPoint)
                    {
                        data.Arrive = math.distance(trans.Position , aPoint + data.offset) <= (speed  * delta);
                    }else
                    {
                        data.Arrive = math.distance(trans.Position , bPoint + data.offset) <= (speed  * delta);
                    }

                    //SqrDistance 가 왜..?
                }
            }

            float SqrDistance(float3 value)
            {
                return value.x * value.x + value.y * value.y  + value.z * value.z;
            }
            float3 UnitLocalPos(int index)
            {
                return new float3((index % resizedMap.x) * betweenSpace, 0,
                        (Mathf.Floor((float)index / resizedMap.x) * betweenSpace));
            }
        }
    }

}
