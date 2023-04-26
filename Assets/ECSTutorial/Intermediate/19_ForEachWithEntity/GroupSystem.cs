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

        float3 Target;
        int seed = 0;
        protected override void OnCreate()
        {
            //Enabled = false;            
        }
        protected override void OnStartRunning()
        {
            //NOTE - ECB으로 GetComponent X , SharedComponent는 수정X , IJob안에 ECB 왜..안되지?
            //NOTE - 시간 조금 지나면 자기 위치로 안가는데...
                
                if (SystemAPI.HasSingleton<GroupComponent>() == false)
                {
                    Enabled = false;
                    return;
                }

                Cgroup = SystemAPI.GetSingleton<GroupComponent>();
                var Temp = Cgroup.size / Cgroup.betweenSpace;
                resized = new int2(Mathf.FloorToInt(Temp.x), Mathf.FloorToInt(Temp.y));
                Amount = resized.x * resized.y;
        
                ecb = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>()
                    .CreateCommandBuffer();

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

            ecbParallel = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>()
                            .CreateCommandBuffer().AsParallelWriter();
                            //NOTE - 미리 할당 X

            MovedTime += SystemAPI.Time.DeltaTime;
            moveJob = new MoveJob
            {
                ECB = ecbParallel,
                movedTime = MovedTime,
                moveTime = Cgroup.moveTime,
                amount = Amount,
                delta = SystemAPI.Time.DeltaTime,
                speed = Cgroup.speed,

                resizedMap = this.resized,
                betweenSpace = Cgroup.betweenSpace,
                bPoint = Target
            };
            

            if (ArriveUnit < Amount)
            {
                moveJob.ScheduleParallel();

                ArriveUnit = 0;
                Entities
                .WithSharedComponentFilter<ArriveData>(new ArriveData{Arrive = true})
                .ForEach((Entity e) => 
                {
                    ArriveUnit++;
                }).WithoutBurst().Run();
                //NOTE - WithSharedComponentFilter는 ToQuery() 으로 EntityQuery로 변환 불가

            }else
            {
                Debug.Log("All Unit Arrive - " +  ArriveUnit + " / " + Amount);
                //ToAPoint = !ToAPoint;
                ArriveUnit = 0;
                MovedTime = 0;

                seed = new Unity.Mathematics.Random((uint)(seed + 1)).NextInt();
                Target = new Unity.Mathematics.Random((uint)(seed + 1)).NextFloat3
                (
                    new float3(Cgroup.size.x, 0, Cgroup.size.y) * -0.5f,
                    new float3(Cgroup.size.x, 0, Cgroup.size.y) * 0.5f
                );
            }
        
        }

        [BurstCompile]
        partial struct MoveJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            public float movedTime;
            public float moveTime;
            public int amount;
            public float delta;
            public float speed;

            public int2 resizedMap;
            public float betweenSpace;
            public float3 bPoint;

            void Execute(Entity e, [EntityIndexInQuery] int sortKey, ref UnitData data, ref LocalTransform trans)
            {
                float timeRate = (movedTime / moveTime) * 2;
                float amountRate = (float)data.index / amount;
                //float amountRate = (float)sortKey / amount;//이걸 써도 잘 작동

                //역순으로 이동 시작하고 , 목표지점에 도착하면 도착

                if ((1 - timeRate) <= amountRate)
                {
                    trans.Position += math.normalizesafe((bPoint + data.offset) - trans.Position) * speed * delta;
                    //math.normalize를 쓰게되면 float3.zero 일때 (Nan, Nan, Nan) 이 되어 버림
                }

                bool arrive = math.distancesq(trans.Position , bPoint + data.offset) <= (speed * speed  * delta * delta);
                data.Arrive = arrive;

                ECB.SetSharedComponent<ArriveData>(sortKey, e, new ArriveData{Arrive = arrive});
            }
        }
    }

}
