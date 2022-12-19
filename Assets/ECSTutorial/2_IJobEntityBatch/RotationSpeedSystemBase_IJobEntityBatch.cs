using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//SystemBase은 partial 키워드 필수
//SystemBase : Class - Managed / Simpler , runs on main thread , Cannot use Burst
public partial class RotationSpeedSystemBase_IJobEntityBatch : SystemBase
{
    EntityQuery m_Query ;
    protected override void OnCreate()
    {
        base.OnCreate();

                // 시스템이 생성되면 조회할 Entity의 Query를 만듭니다.
        m_Query = GetEntityQuery(typeof(LocalTransform), ComponentType.ReadOnly<RotationSpeed_IJobEntityBatch>());
        //m_Query = World.EntityManager.CreateEntityQuery(typeof(LocalTransform), typeof(RotationSpeed_IJobEntityBatch));
        // ----------- 방법이 2개

        //SystemAPI.Query<LocalTransform>() // ForEach로
        //World.EntityManager.CreateEntityQuery() // 으로도찾고

        // 추가되었는데 HasSingleton<T>()이 인식을 못함 ....?

    }

    [Unity.Burst.BurstCompile]
    struct RotationSpeedJob : IJobChunk//Renamed IJobEntityBatch >> IJobChunk
    {
        public float DeltaTime;

        // Handle 필드
        public ComponentTypeHandle<LocalTransform> RotationTypeHandle;
        [ReadOnly] public ComponentTypeHandle<RotationSpeed_IJobEntityBatch> RotationSpeedTypeHandle;

        public void Execute(in ArchetypeChunk batchInChunk, int batchIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            //if ((batchInChunk.Has(ref RotationSpeedTypeHandle) && batchInChunk.Has(ref RotationTypeHandle)))
            {
                                        //batchInChunk 에서 아래 Handle과 같은 타입의 데이터를 NativeArray로 얻어온다.(참조 -> ref)
                var chunkRotations = batchInChunk.GetNativeArray(ref RotationTypeHandle);
                var chunkRotationSpeeds = batchInChunk.GetNativeArray(ref RotationSpeedTypeHandle);

                
                for (int i = 0; i < batchInChunk.Count; i++)
                {

                    var rotation = chunkRotations[i];
                    var rotationSpeed = chunkRotationSpeeds[i];//------ 여기까진 문제 X

                    rotation.Rotation = math.mul
                        (
                            math.normalize(rotation.Rotation),
                            quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * DeltaTime)
                        );

                    chunkRotations[i] = rotation;
                    //batchInChunk.SetChunkComponentData<LocalTransform>(ref RotationTypeHandle, chunkRotations[i]);//에러발생

                    /*
                    chunkRotations[i] = new LocalTransform
                    {   
                        Rotation = math.mul
                        (
                            math.normalize(rotation.Rotation),
                            quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * DeltaTime)
                        )
                    };
                    
                    */// 원본이 회전값만 넣어서....
                }
            }

            //정의한 구조체를 시스템에서 생성하고 Job을 예약할 때, 위에서 작성한 EntityQuery를 전달하면
            // Excute의 ArchtypeChunk에 EntityQuery에서 조회한 batchInChunk그룹이 전달된다.
            //  ComponentTypeHandle로 batchInChunk 안의 어떤 데이터를 사용할지 설정할 수 있다. 
        }
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
            var l_Query = World.EntityManager.CreateEntityQuery(typeof(LocalTransform), typeof(RotationSpeed_IJobEntityBatch));         
            Debug.Log("Find Amount : " + l_Query.CalculateEntityCount() + " / m_query : " + m_Query.CalculateEntityCount());

        //핸들 얻어오기
        //GetComponentTypeHandle 이 없는데? / SystemBase 에 있음
        var rotationType = GetComponentTypeHandle<LocalTransform>();
        var rotationSpeedType = GetComponentTypeHandle<RotationSpeed_IJobEntityBatch>(true);

        var job = new RotationSpeedJob()
        {
            RotationTypeHandle = rotationType,
            RotationSpeedTypeHandle = rotationSpeedType,
            DeltaTime = SystemAPI.Time.DeltaTime
        };

        // 작업 예약. Dependency(의존성)을 전달하여 여러 스레드에서 쓰고, 읽기로 발생하는 경쟁 문제를 해결한다.
        Dependency = job.ScheduleParallel(m_Query, Dependency);
        Dependency.Complete();

        //job.Run(m_Query);
        //job.Schedule(m_Query, Dependency);


        /*
        foreach(var temp in SystemAPI.Query<RefRW<LocalTransform>>())
        {
            temp.ValueRW.Position += new float3(0, 1, 0) * SystemAPI.Time.DeltaTime;
            temp.ValueRW.Rotation *= Quaternion.AngleAxis(45 * SystemAPI.Time.DeltaTime, Vector3.up);
            //temp.ValueRW.RotateY(45);//NotWork
        }

        //SystemAPI.Query<LocalTransform>().GetEnumerator()
        *///foreach 으로 하는 방법
        

        //NativeArray<LocalTransform> list_Lt = new NativeArray<LocalTransform>(m_Query.CalculateEntityCount(), Allocator.TempJob);
        //m_Query.CopyFromComponentDataArray<LocalTransform>(list_Lt);//------------이걸쓰니 사라지넼ㅋㅋㅋ 시발 
    }
}
