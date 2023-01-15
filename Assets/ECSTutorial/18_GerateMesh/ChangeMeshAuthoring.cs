using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.GerateMesh
{
    public class ChangeMeshAuthoring : MonoBehaviour
    {
    }

    public struct ChangeMeshTag : IComponentData {}

    public class ChangeMeshBaker : Baker<ChangeMeshAuthoring>
    {
        public override void Bake(ChangeMeshAuthoring authoring)
        {
            //AddComponent(new ChangeMeshTag{});//==========Error
        }
    }
}