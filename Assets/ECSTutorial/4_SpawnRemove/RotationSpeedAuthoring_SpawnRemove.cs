using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class RotationSpeedAuthoring_SpawnRemove : MonoBehaviour
{
    public float DegreesPerSecond = 60;
}

public class RotationSpeedBaker_SpawnRemove : Baker<RotationSpeedAuthoring_SpawnRemove>
{
    public override void Bake(RotationSpeedAuthoring_SpawnRemove authoring)
    {
        AddComponent(new RotationSpeed_SpawnRemove{ RadiansPerSecond = math.radians(authoring.DegreesPerSecond)});
        AddComponent(new LifeTime{Value = 0f});
    }
}
