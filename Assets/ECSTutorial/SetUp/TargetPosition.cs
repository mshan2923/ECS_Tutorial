using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct TargetPosition : IComponentData
{
    public float3 value;
    //Entity에서 쓸 데이터
}
