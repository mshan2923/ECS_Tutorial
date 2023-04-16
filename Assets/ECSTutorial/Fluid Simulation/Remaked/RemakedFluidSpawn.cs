using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace FluidSimulate
{
    public class RemakedFluidSpawn : MonoBehaviour
    {
        public GameObject particleObj;
        public int Amount;
        public float Between;
    }
    public struct RemakedFluidSpawnComponent : IComponentData
    {
        public Entity particle;
        public int Amount;
        public float Between;
    }
    public class RemakedFluidSpawnBake : Baker<RemakedFluidSpawn>
    {
        public override void Bake(RemakedFluidSpawn authoring)
        {
            AddComponent(new RemakedFluidSpawnComponent
            {
                particle = GetEntity(authoring.gameObject),
                Amount = authoring.Amount,
                Between = authoring.Between
            });
        }
    }

}