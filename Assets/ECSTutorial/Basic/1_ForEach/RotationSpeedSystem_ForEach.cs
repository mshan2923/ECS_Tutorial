using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class RotationSpeedSystem_ForEach : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        Entities
            .WithName("RotationSpeedSystem_ForEach") // 디버그용 이라함
            .ForEach // 괄호안의 Component를가지는  Entities를 Entity별로 반복문 돌림
            (
                (ref LocalTransform transform, in RotationSpeed_ForEach rotationSpeed) =>
                {// 항상 ref 다음에 in 이 있어야함 , ref는 쓸 대상 , in은 읽을대상 / 스레드에서 동시에 실행시키기위해 
                    transform.Rotation = math.mul
                    (
                        math.normalize(transform.Rotation),//math.normalize(transform._Rotation.value),
                        quaternion.AxisAngle(math.up(), rotationSpeed.RadiusPerSpeed * deltaTime)
                    );
                }
            ).ScheduleParallel();
            //ScheduleParallel() 은 반복문을병렬처리,
            // Run(메인스레드에서 실행), Schedule(멀티스레드 하나에서 실행)를 대신 입력 가능

    }
}
