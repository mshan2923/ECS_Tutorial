using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

//[SerializeField]
public struct RotationSpeed_IJobEntityBatch : IComponentData
{
    public float RadiansPerSecond;
}
