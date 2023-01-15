using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity;
using Unity.Transforms;
using UnityEngine;
using Unity.Entities;

public partial class TutorialTagSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .ForEach((Entity entity, int entityInQueryIndex, ref LocalTransform trans, in RedTag tag) =>
            {
                //trans.Position += new float3(SystemAPI.Time.DeltaTime, 0, 0);
                trans.Rotation *= Quaternion.AngleAxis(10 * SystemAPI.Time.DeltaTime, Vector3.right);
            }).ScheduleParallel();

        Entities
            .ForEach((Entity entity, int entityInQueryIndex, ref LocalTransform trans, in GreenTag tag) =>
            {
                //trans.Position += new float3(SystemAPI.Time.DeltaTime, 0, 0);
                trans.Rotation *= Quaternion.AngleAxis(10 * SystemAPI.Time.DeltaTime, Vector3.up);
            }).ScheduleParallel();

        Entities
            .ForEach((Entity entity, int entityInQueryIndex, ref LocalTransform trans, in BlueTag tag) =>
            {
                //trans.Position += new float3(SystemAPI.Time.DeltaTime, 0, 0);
                trans.Rotation *= Quaternion.AngleAxis(10 * SystemAPI.Time.DeltaTime, Vector3.forward);
            }).ScheduleParallel();
    }
}
