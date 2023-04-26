

using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class RotationSpeed_IJobEntityAuthoring : MonoBehaviour
{  
    public float DegreesPerSecond = 60;
}

public class RotationSpeed_IJobEntityBaker : Baker<RotationSpeed_IJobEntityAuthoring>
{
    public override void Bake(RotationSpeed_IJobEntityAuthoring authoring)
    {
        AddComponent(new RotationSpeed_IJobEntity{RadiansPerSecond = math.radians(authoring.DegreesPerSecond)});
        //씨벌 소괄호 써서 오류 계속 떠서 시간만 낭비
    }
}