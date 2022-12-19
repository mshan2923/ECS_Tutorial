using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

#if UNITY_EDITOR
public class SelfDestructAuthoring : MonoBehaviour
{
    public float TimeToLive;
}
public class SelfDestructBaker : Baker<SelfDestructAuthoring>
{
    public override void Bake(SelfDestructAuthoring authoring)
    {
        AddComponent(new SelfDestruct{TimeToLive = authoring.TimeToLive});
    }
}
#endif