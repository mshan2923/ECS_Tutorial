using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlayerSpawnerAuthoring : MonoBehaviour
{
    public GameObject playerPrefab;
    public int Amount;//ECS 외부에서 값 가져올 변수
}

public class PlayerSpawnerBaker : Baker<PlayerSpawnerAuthoring>
{
    public override void Bake(PlayerSpawnerAuthoring authoring)
    {
        AddComponent(new PlayerSpawnerComponent { playerPrefab = GetEntity(authoring.playerPrefab), Amount = authoring.Amount});
        //ECS 내부로 전송
    }
}