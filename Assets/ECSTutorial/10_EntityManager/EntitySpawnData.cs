using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Tutorial.EntityManager
{
    public class EntitySpawnData : MonoBehaviour
    {
    public GameObject Prefab;
    public Vector2 SpawnGrid;
    public Vector2 Spacing;
    }
    public struct OscillatingTag : IComponentData {}
    public struct EntitySpawnComponent : IComponentData
    {
    public Entity prefab;
    public Vector2 spawnGrid;
    public Vector2 spacing;
    }
    public class EntitySpawnDataBaker : Baker<EntitySpawnData>
    {
    public override void Bake(EntitySpawnData authoring)
    {
        AddComponent(new EntitySpawnComponent
        {
            prefab = GetEntity(authoring.Prefab),
            spawnGrid = authoring.SpawnGrid,
            spacing = authoring.Spacing
        });
    }
    }
}
