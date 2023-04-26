using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Entities.Serialization;

#if UNITY_EDITOR
public class PrefabSpawnerAuthoring : MonoBehaviour
{
    public GameObject[] Prefabs;
    public int SpawnCount;
    public float SpawnsPerSecond;
}

public class PrefabSpawnerBaker : Baker<PrefabSpawnerAuthoring>
{
    public override void Bake(PrefabSpawnerAuthoring authoring)
    {
        // Spawner 에게 스폰 간격과 생성할 개수가 존재하는 PrefabSpawner 컴포넌트를 추가한다.
        AddComponent(new PrefabSpawner{SpawnsRemaining = authoring.SpawnCount, SpawnsPerSecond = authoring.SpawnsPerSecond});

        // Spawner 에게 PrefabSpawnerBufferElement buffer 추가
        //var buffer = dstManager.AddBuffer<PrefabSpawnerBufferElement>(entity);
        var buffer = AddBuffer<PrefabSpawnerBufferElement>();

        foreach (var prefab in authoring.Prefabs)
        {
            buffer.Add(new PrefabSpawnerBufferElement {Prefab = new EntityPrefabReference(prefab)});
        }
    }
}
#endif
