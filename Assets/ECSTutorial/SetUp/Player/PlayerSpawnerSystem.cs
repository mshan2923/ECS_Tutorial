using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial class PlayerSpawnerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityQuery playerEntityQuery = EntityManager.CreateEntityQuery(typeof(PlayerTag));

        if (SystemAPI.HasSingleton<PlayerSpawnerComponent>())//직접추가
        {
            PlayerSpawnerComponent playerSpawnerComponent  = SystemAPI.GetSingleton<PlayerSpawnerComponent>();
            RefRW<RandomComponent> randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();

            var entityCommandBuffer =
            SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(World.Unmanaged);
                //https://youtu.be/H7zAORa3Ux0?t=3095

            //int spawnAmount = 20;
            if (playerEntityQuery.CalculateEntityCount() < playerSpawnerComponent.Amount)
            {
                Entity spawnedEntity = EntityManager.Instantiate(playerSpawnerComponent.playerPrefab);
                entityCommandBuffer.SetComponent(spawnedEntity, new Speed {value = randomComponent.ValueRW.random.NextFloat(1f, 5f)});
            }
        }
    }
}
