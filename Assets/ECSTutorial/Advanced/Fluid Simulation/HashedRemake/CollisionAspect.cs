using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

namespace FluidSimulate
{
    public enum ColliderType { Sphere, Box, Plane};
    public class CollisionAspect : MonoBehaviour
    {
        public ColliderType colliderType;
    }
    public struct CollisionComponent : IComponentData
    {
        public ColliderType colliderType;
        public float3 WorldSize;
    }
    public class CollisionBaker : Baker<CollisionAspect>
    {
        public override void Bake(CollisionAspect authoring)
        {
            if (authoring.TryGetComponent<MeshCollider>(out var meshCollider))
            {
                var size = meshCollider.sharedMesh.bounds.size;
                size.x *= authoring.transform.localScale.x;
                size.y *= authoring.transform.localScale.y;
                size.z *= authoring.transform.localScale.z;

                bool IsEllipse = false;
                if (authoring.colliderType == ColliderType.Sphere)
                {
                    IsEllipse = !Mathf.Approximately(size.x, size.y) || !Mathf.Approximately(size.x, size.z);
                }

                AddComponent(GetEntity(authoring, TransformUsageFlags.Dynamic),
                    new CollisionComponent
                    {
                        colliderType = IsEllipse? ColliderType.Box : authoring.colliderType,
                        WorldSize = size
                    });
            }
            else if(authoring.TryGetComponent<SphereCollider>(out var sphereCollider))
            {
                var size = sphereCollider.radius * 2f * Mathf.Max(authoring.transform.localScale.x, authoring.transform.localScale.y, authoring.transform.localScale.z);

                AddComponent(GetEntity(authoring, TransformUsageFlags.Dynamic),
                    new CollisionComponent
                    {
                        colliderType = ColliderType.Sphere,
                        WorldSize = size
                    });
            }else if (authoring.TryGetComponent<BoxCollider>(out var boxCollider))
            {
                var size = boxCollider.size;
                size.x *= authoring.transform.localScale.x;
                size.y *= authoring.transform.localScale.y;
                size.z *= authoring.transform.localScale.z;

                AddComponent(GetEntity(authoring, TransformUsageFlags.Dynamic),
                    new CollisionComponent
                    {
                        colliderType = ColliderType.Box,
                        WorldSize = size
                    });
            }



        }
    }

}
