using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

/// <summary>
/// SystemBase : Class - Managed / Simpler , runs on main thread , Cannot use Burst
/// </summary>
public partial class MovingSystemBase : SystemBase
{
    //All SystemBase-derived classes must be defined with the `partial` keyword,
    // so that source generators can emit additional code into these classes.
    // Please add the `partial` keyword to MovingSystemBase, as well as all the classes it is nested within.

    protected override void OnUpdate()
    {
        // 구현부분 , MoveToPositionAspect 에서 미리 선언해놓음

        /*
        foreach ((TransformAspect transformAspect , RefRO<Speed> speed, RefRW<TargetPosition> targetPosition) in
         SystemAPI.Query<TransformAspect, RefRO<Speed>, RefRW<TargetPosition>>())
        {
            //Calculate dir
            float3 direction = math.normalize(targetPosition.ValueRW.value - transformAspect.WorldPosition);
            //Move
            transformAspect.WorldPosition += direction * SystemAPI.Time.DeltaTime * speed.ValueRO.value;
        }
        */// Legacy

        if (SystemAPI.HasSingleton<RandomComponent>())
        {
            RefRW<RandomComponent> randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();
            //SystemAPI.GetSingleton<>을 쓰면 값변경 X

            foreach (MoveToPositionAspect moveToPositionAspect in SystemAPI.Query<MoveToPositionAspect>())
            {
               //moveToPositionAspect.Move(SystemAPI.Time.DeltaTime, randomComponent);//Legacy - Convert ISystem'
               moveToPositionAspect.Move(SystemAPI.Time.DeltaTime);
               moveToPositionAspect.TestReachedTargetPosition(randomComponent);
            }
        
        }
    }
}
