using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Tutorial.EntityManager
{
    public partial class SpawnEntitySystem : SystemBase
    {
    Vector2 spacing;
    Vector2 gridSize;
    protected override void OnStartRunning()
    {
        // 4번 튜토는  10 + 11 튜토리얼 합한거
        //  생성할 Entity를 예약해둬 병렬로 처리하는게 4번 튜토리얼

        if (! SystemAPI.HasSingleton<EntitySpawnComponent>())
        {
            this.Enabled = false;
            return;
        }
        var spawnData = SystemAPI.GetSingleton<EntitySpawnComponent>();

        gridSize = spawnData.spawnGrid;
        spacing = spawnData.spacing;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                var newEntity = EntityManager.Instantiate(spawnData.prefab);

                //var newPosition = new LocalToWorld {Value = CalculateTransform(x, y)};
                var newPosition = new LocalTransform {Position = CalculatePosition(x, y),
                     Rotation = quaternion.identity, Scale = 1};

                EntityManager.SetComponentData(newEntity, newPosition);

                if ((x + y) % 2 == 0)
                {
                    EntityManager.AddComponent<OscillatingTag>(newEntity);
                }
            }
        }
    }
    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            var query = EntityManager.CreateEntityQuery(typeof(OscillatingTag));
            EntityManager.DestroyEntity(query);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            EntityManager.DestroyAndResetAllEntities();
        }
    }

    float4x4 CalculateTransform (int x, int y)
    {
        return float4x4.Translate(new float3(x * spacing.x, 0, y * spacing.y));
        
    }

    float3 CalculatePosition(int x, int y)
    {
        return new float3(((x * spacing.x) - (gridSize.x * 0.5f)),
             0, (y * spacing.y) - (gridSize.y * 0.5f));
    }
    }

}
