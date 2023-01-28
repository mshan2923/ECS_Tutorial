using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class DynamicBufferMono : MonoBehaviour
{
    public int value;
}
[InternalBufferCapacity(16)] // 초기용량
public struct DynamicBufferData : IBufferElementData
{
    public int Value;
}
public struct Tutorial14Tag : IComponentData {}
public class DynamicBufferBaker : Baker<DynamicBufferMono>
{
    public override void Bake(DynamicBufferMono authoring)
    {
        AddBuffer<DynamicBufferData>();
        AddComponent(new Tutorial14Tag{});
    }
}