using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Tutorial.JobEntity
{
    public partial class RotationJob : SystemBase
    {
        // 메세지는 안뜨지만 partial 키워드 필요하고 , 지원되지않는 파라미터가 있으면 Schedule() 이 안뜸
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