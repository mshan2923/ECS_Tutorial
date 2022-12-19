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

    public int Amount;

    private void Start() 
    {   
        Amount = (SpawnSize.x * 2 + (SpawnSize.y * 2 - 2)) * SpawnSize.y;

        Debug.Log("---");
    }
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
        owner.Amount = (owner.SpawnSize.x * 2 + (owner.SpawnSize.y * 2 - 2)) * owner.SpawnSize.y;
    }
}
#endif

public struct CPhysicsObjSpawner : IComponentData
{
    public Entity prefab;
    public Vector3Int spawnSize;
    public float3 offset;
}
public class PhysicsObjSpawnerBaker : Baker<PhysicsObjSpawner>
{
    public override void Bake(PhysicsObjSpawner authoring)
    {
        AddComponent(new CPhysicsObjSpawner
        {
            prefab = GetEntity(authoring.Prefab),
             spawnSize = authoring.SpawnSize,
             offset = authoring.Offset
        });
    }
}
