using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;

public class PhysicsObjSpawner : MonoBehaviour
{
    public GameObject Prefab;
    public Vector3Int SpawnSize;
    public Vector3 Offset;
    public float BetweenOffset = 1f;

    public bool IsSphere = false;
    public bool IsHollow = true;

    public int Amount;

}
#if UNITY_EDITOR
[CustomEditor(typeof(PhysicsObjSpawner))]
public class PhysicsObjSpawnerEditor : Editor
{
    PhysicsObjSpawner owner;
    private void OnEnable() 
    {
        owner = target as PhysicsObjSpawner;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (owner.IsHollow && owner.IsSphere == false)
            owner.Amount = (owner.SpawnSize.x * 2 + (owner.SpawnSize.y * 2 - 2)) * owner.SpawnSize.y;
        else
            owner.Amount = owner.SpawnSize.x * owner.SpawnSize.y * owner.SpawnSize.z;
    }
}
#endif

public struct CPhysicsObjSpawner : IComponentData
{
    public Entity prefab;
    public Vector3Int spawnSize;
    public float3 offset;
    public float betweenOffset;

    public bool isSphere;
    public bool ishollow;
}
public class PhysicsObjSpawnerBaker : Baker<PhysicsObjSpawner>
{
    public override void Bake(PhysicsObjSpawner authoring)
    {
        AddComponent(new CPhysicsObjSpawner
        {
            prefab = GetEntity(authoring.Prefab),
             spawnSize = authoring.SpawnSize,
             offset = authoring.Offset,
             betweenOffset = authoring.BetweenOffset,
             isSphere = authoring.IsSphere,
             ishollow = authoring.IsHollow
        });
    }
}
