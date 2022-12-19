using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class RotationSpeed_ForEachAuthoring : MonoBehaviour
{
    public float RadiusPerSpeed;
}
public class RotationSpeed_ForEachBake : Baker<RotationSpeed_ForEachAuthoring>
{
    public override void Bake(RotationSpeed_ForEachAuthoring authoring)
    {
        AddComponent(new RotationSpeed_ForEach {RadiusPerSpeed = authoring.RadiusPerSpeed});
    }
}
