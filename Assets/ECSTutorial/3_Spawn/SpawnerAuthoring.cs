using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public int CountX;
    public int CountY;
}
public class SpawnerBaker : Baker<SpawnerAuthoring>
{
    public override void Bake(SpawnerAuthoring authoring)
    {
        //Baker.GetEntity 으로 Prefab을 Entity으로 변환
        AddComponent(new Spawner{ Prefab = GetEntity(authoring.Prefab),
             CountX = authoring.CountX, CountY = authoring.CountY});
    }
}
