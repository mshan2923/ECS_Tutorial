using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct PlayerSpawnerComponent : IComponentData
{
    public int Amount;//ECS 내부에서 쓸 변수
    public Entity playerPrefab;
}
