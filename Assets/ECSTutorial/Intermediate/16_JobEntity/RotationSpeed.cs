using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.JobEntity
{
    public class RotationSpeed : MonoBehaviour
    {
        public float Speed = 1;
    }
    public struct RotationSpeedComponent : IComponentData
    {
        public float speed;
    }
    public class RotationSpeedBaker : Baker<RotationSpeed>
    {
        public override void Bake(RotationSpeed authoring)
        {
            AddComponent(new RotationSpeedComponent{speed = authoring.Speed});
        }
    }
}