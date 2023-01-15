using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.ECB
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class SpawnControlSystem : SystemBase
    {
        EndInitializationEntityCommandBufferSystem endIntiECB;
        protected override void OnCreate()
        {
            endIntiECB = World.GetOrCreateSystemManaged<EndInitializationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {
            var spawnerQuery = EntityManager.CreateEntityQuery(typeof(EntitySpawnComponent));
            var ecb = endIntiECB.CreateCommandBuffer();
            
            if (Input.GetKeyDown(KeyCode.Y))
            {
                ecb.AddComponent<ShouldSpawnTag>(spawnerQuery);
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                ecb.RemoveComponent<ShouldSpawnTag>(spawnerQuery);
            }

            //EndInitializationEntityCommandBufferSystem 때 spawnerQuery에 있는 엔티티에 
            //  ShouldSpawnTag를 추가하거나 제거함
        }
    }

}
