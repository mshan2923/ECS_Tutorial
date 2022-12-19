using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SpawnerAuthoring_SpawnRemove : MonoBehaviour
{
    public GameObject Prefab;
    public int CountX;
    public int CountY;
}
public struct Spawner_SpawnRemover : IComponentData
{
    public Entity Prefab;// 주의! GameObject를 쓸경우 유니티 크래시
    public int CountX;
    public int CountY;

    //Spawner를 써도 되지만 다른 System의 간섭때문에
}
public class SpawnerBaker_SpawnRemove : Baker<SpawnerAuthoring_SpawnRemove>
{
    public override void Bake(SpawnerAuthoring_SpawnRemove authoring)
    {
        AddComponent(new Spawner_SpawnRemover
            {
                Prefab = GetEntity(authoring.Prefab),
                CountX = authoring.CountX,
                CountY = authoring.CountY
            });
            
    }
}