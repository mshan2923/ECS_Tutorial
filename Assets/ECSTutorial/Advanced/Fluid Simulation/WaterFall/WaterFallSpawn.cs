using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace FluidSimulate.WaterFall
{
    public class WaterFallSpawn : MonoBehaviour
    {
        public GameObject particleObj;
        public int Amount;
        public float RandomPower;

        public float SpawnRadius = 1f;
        public float SpawnInterval = 0.25f;
        public int SpawnAmountToOnece = 1;

        void Start()
        {

        }
    }
    public struct WaterFallSpawnComponent : IComponentData
    {
        public Entity particle;
        public int Amount;
        public float RandomPower;

        public float SpawnRadius;
        public float SpawnInterval;
        public int SpawnAmountToOnece;
    }
    public struct SpawnedTag : IComponentData { }

    public class WaterFallSpawnBake : Baker<WaterFallSpawn>
    {
        public override void Bake(WaterFallSpawn authoring)
        {
            AddComponent(new WaterFallSpawnComponent
            {
                particle = GetEntity(authoring.particleObj),
                Amount = authoring.Amount,
                RandomPower = authoring.RandomPower,

                SpawnRadius = authoring.SpawnRadius,
                SpawnInterval = authoring.SpawnInterval,
                SpawnAmountToOnece = authoring.SpawnAmountToOnece
            });
        }
    }
}