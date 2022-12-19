using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;

public struct PrefabSpawner : IComponentData
{
    public float SpawnsRemaining;
    public float SpawnsPerSecond;
}

// 이번 글에서 주요 주제인 EntityPrefabReference 이다.
public struct PrefabSpawnerBufferElement : IBufferElementData
{
    public EntityPrefabReference Prefab;

    /*
    PrefabSpawnerBufferElement은 IBufferElementData를 상속받았다는 것이다. 
    IBufferElementData는 간단하게 말해서 동적버퍼인데 지정한 크기 내에서 데이터를 넣다 뺏다 할 수 있는 타입이다. 
    설명링크 :  https://mrbinggrae.tistory.com/276
    */
}
