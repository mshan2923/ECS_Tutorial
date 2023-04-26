using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Tutorial.EntityManager
{
    public partial class OscillatingSystem : SystemBase
    {
    protected override void OnUpdate()
    {
        var time = SystemAPI.Time.ElapsedTime;
        var delta = SystemAPI.Time.DeltaTime;

        Entities.WithAll<OscillatingTag>().ForEach((ref LocalToWorld localToWorld) =>
        {
            //모든 엔티티중 OscillatingTag를 가진걸 LocalToWorld를 참조해서 ForEach

            var newPosition = localToWorld.Position;
            newPosition.y += math.sin((float) time * 2f) * delta;
            localToWorld.Value = float4x4.Translate(newPosition);
        }).ScheduleParallel();

    }
    }

}