using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class RotationSpeedSystem_SpawnRemove : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        Entities
            .WithName("RotationSpeedSystem_SpawnAndRemove")
            .ForEach((ref LocalTransform rotation, in RotationSpeed_SpawnRemove rotSpeedSpawnAndRemove) =>
            {
             
                rotation.Rotation = math.mul(math.normalize(rotation.Rotation), 
                    quaternion.AxisAngle(math.up(), rotSpeedSpawnAndRemove.RadiansPerSecond * deltaTime));

                    Debug.Log("rotation" );

            }).ScheduleParallel();
    }
}
