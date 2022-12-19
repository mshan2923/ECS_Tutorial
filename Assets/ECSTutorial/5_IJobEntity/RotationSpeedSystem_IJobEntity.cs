using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

//주의 메세지 제거 (partial 순서)//여러 선언에서 필드간 순서가 정의가 되어있지 않습니다.
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
partial struct RotateEntityJob : IJobEntity
{
    public float DeltaTime;

    public void Execute(ref LocalTransform trans, in RotationSpeed_IJobEntity speed)
    {
        trans.Rotation =
            math.mul(
                math.normalize(trans.Rotation),
                quaternion.AxisAngle(math.up(), speed.RadiansPerSecond * DeltaTime));
    }
    /*
    IJobEntity를 상속받으면 Excute 함수를 구현해야 합니다.
     Excute함수의 매개변수로 어떤 ComponentData를 반복실행할 것인지 명시해야 합니다.
      위 예제에서는 LocalTransform(쓰기가능), RotationSpeed_IJobEntity(읽기)로 명시하여
       모든 Entity 중에 명시한 ComponentData타입을 가지고 있는 Entitiy를 조회하고 
       명시한 ComponentData를 가져옵니다. 
    ForEach의 람다함수로 Query를 작성한 것과 비슷합니다.
    */
}
public partial class RotationSpeedSystem_IJobEntity : SystemBase
{
    protected override void OnUpdate()
    {
    	// NativeArray 초기화 배열크기, 메모리 할당
        //NativeArray<LocalTransform> test = new NativeArray<LocalTransform>(3, Allocator.TempJob);
        
        var query = GetEntityQuery(typeof(LocalTransform) , typeof(RotationSpeed_IJobEntity));
        //  LocalTransform + RotationSpeed_IJobEntity 를 가진 모든 대상

        // 구조체 생성 후 실행 예약
        new RotateEntityJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel(query);

        // 메모리 할당 해제
        //test.Dispose();
    }
}
