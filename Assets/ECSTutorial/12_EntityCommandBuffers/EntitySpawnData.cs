using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.ECB
{
    public class EntitySpawnData : MonoBehaviour
    {
        public GameObject PrefabToSpawn;
        public float SpawnDelay;
        public float Timer;
    }
    
    public struct CapsuleTag : IComponentData {}
    public struct ShouldSpawnTag : IComponentData {}
    public struct EntitySpawnComponent : IComponentData
    {
        public Entity prefabToSpawn;
        public float spawnDelay;
        public float timer;
    }

    public class EntitySpawnDataBaker : Baker<EntitySpawnData>
    {
        public override void Bake(EntitySpawnData authoring)
        {
            AddComponent(new EntitySpawnComponent            
            {
                prefabToSpawn = GetEntity(authoring.PrefabToSpawn),
                spawnDelay = authoring.SpawnDelay,
                timer = authoring.Timer
            });
        }
    }
}