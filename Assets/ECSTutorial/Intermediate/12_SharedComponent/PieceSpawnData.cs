using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.ConnectFour
{
    public class PieceSpawnData : MonoBehaviour
    {
        public GameObject Prefab;
        public bool IsRedTurn;
    }   
    public struct PieceSpawnComponent : IComponentData
    {
        public Entity prefab;
        public bool isRedTurn;
    }
    public class PieceSpawnBaker : Baker<PieceSpawnData>
    {
        public override void Bake(PieceSpawnData authoring)
        {
            AddComponent(new PieceSpawnComponent
            {
                prefab = GetEntity(authoring.Prefab),
                isRedTurn = authoring.IsRedTurn
            });
        }
    }

}