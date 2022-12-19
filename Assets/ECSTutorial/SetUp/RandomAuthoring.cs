using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RandomAuthoring : MonoBehaviour
{
    // 외부와 정보교환 , export는 아직 모르겠음
}
public class RandomBaker : Baker<RandomAuthoring>
{
    public override void Bake(RandomAuthoring authoring)
    {
        AddComponent( new RandomComponent { random = new Unity.Mathematics.Random(1)});
    }
}
