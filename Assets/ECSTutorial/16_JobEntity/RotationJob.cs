using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Tutorial.JobEntity
{
    public partial class RotationJob : SystemBase
    {
        public partial struct RotationEntityJob : IJobEntity
        {
            public void Execute(Entity e, [EntityIndexInQuery] int sortKey, ref LocalTransform trans, in RotationSpeedComponent Rot)
            {
                trans.Rotation *= Quaternion.AngleAxis(Rot.speed, Vector3.one);
            }
        }
        protected override void OnUpdate()
        {
            new RotationEntityJob{}.ScheduleParallel();
        }
    }

}