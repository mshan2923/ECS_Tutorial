using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct MoveToPositionAspect : IAspect
{
private readonly Entity entity;

    private readonly TransformAspect transformAspect;
    private readonly RefRO<Speed> speed;
    private readonly RefRW<TargetPosition> targetPosition;

    //public void Move(float delta, RefRW<RandomComponent> randomComponent)//Legacy - Covert ISystem
    public void Move(float delta)
    {
            //Calculate dir
            float3 direction = math.normalize(targetPosition.ValueRW.value - transformAspect.WorldPosition);
            //Move
            transformAspect.WorldPosition += direction * delta * speed.ValueRO.value;

            /*
            float reachedTargetDistance = .5f;
            if (math.distance(transformAspect.WorldPosition, targetPosition.ValueRW.value) < reachedTargetDistance)
            {
                //Generate new random target position
                //https://youtu.be/H7zAORa3Ux0?t=1715
                targetPosition.ValueRW.value = GetRandomPosition(randomComponent);
                Debug.Log(targetPosition.ValueRW.value);
            }
            *///Legacy - Covert ISystem / MoveTo 'TestReachedTargetPosition'
    }
    public void TestReachedTargetPosition(RefRW<RandomComponent> randomComponent)
    {
            float reachedTargetDistance = .5f;
            if (math.distance(transformAspect.WorldPosition, targetPosition.ValueRW.value) < reachedTargetDistance)
            {
                targetPosition.ValueRW.value = GetRandomPosition(randomComponent);
            }
    }
    private float3 GetRandomPosition(RefRW<RandomComponent> randomComponent)
    {
        return new float3
        (
            randomComponent.ValueRW.random.NextFloat(0f, 15f),
            0,
            randomComponent.ValueRW.random.NextFloat(0f, 15f)
        );
    }
}
