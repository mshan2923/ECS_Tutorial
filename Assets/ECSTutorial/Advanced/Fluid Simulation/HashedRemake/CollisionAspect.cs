using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace FluidSimulate
{
    public enum ColliderShape { Sphere, Box, Plane};
    public enum ColliderEvent { Collision , DisableTrigger, KillTrigger};
    public class CollisionAspect : MonoBehaviour
    {
        public ColliderShape colliderType;
        public ColliderEvent colliderEvent = ColliderEvent.Collision;
    }



    public struct FluidCollider : IComponentData { }
    public struct FluidTrigger : IComponentData { }
    public struct CollisionComponent : IComponentData
    {
        public ColliderShape collidershape;
        public ColliderEvent colliderEvent;
        public float3 WorldSize;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform">Collider Transform</param>
        /// <param name="targetRadius">Particle Radius</param>
        /// <param name="targetPos">Particle Position</param>
        /// <param name="dir">Collision Normal Direction</param>
        /// <returns></returns>
        public bool IsCollisionSphere(LocalTransform transform, float targetRadius, float3 targetPos, out float3 dir, out float dis)
        {
            if (collidershape == ColliderShape.Plane || collidershape == ColliderShape.Box)
            {
                var maxPoint = transform.Right() * WorldSize.x
                                + transform.Forward() * WorldSize.z;
                if (collidershape != ColliderShape.Plane)
                    maxPoint += transform.Up() * WorldSize.y;
                maxPoint *= 0.5f;

                var ostProjectX = math.project(transform.Position, transform.Right());
                var ostAreaProjectX = math.project(transform.Position + maxPoint, transform.Right());
                var particleProjectX = math.project(targetPos, transform.Right());
                //var projectXDis = math.distance(particleProjectX, ostProjectX);
                var disX = math.distance(particleProjectX, ostProjectX) - math.distance(ostAreaProjectX, ostProjectX) - targetRadius;
                if (disX >= 0)//(projectXDis - targetRadius >= math.distance(ostAreaProjectX, ostProjectX))
                {
                    dis = disX;
                    dir = float3.zero;
                    return false;
                }

                var ostProjectZ = math.project(transform.Position, transform.Forward());
                var ostAreaProjectZ = math.project(transform.Position + maxPoint, transform.Forward());
                var particleProjectZ = math.project(targetPos, transform.Forward());
                //var projectZDis = math.distance(particleProjectZ, ostProjectZ);
                var disZ = math.distance(particleProjectZ, ostProjectZ) - math.distance(ostAreaProjectZ, ostProjectZ) - targetRadius;
                if (disZ >= 0)//(projectZDis - targetRadius >= math.distance(ostAreaProjectZ, ostProjectZ))
                {
                    dis = disZ;
                    dir = float3.zero;
                    return false;
                }

                if (collidershape == ColliderShape.Box)
                {
                    var ostProjectY = math.project(transform.Position, transform.Up());
                    var ostAreaProjectY = math.project(transform.Position + maxPoint, transform.Up());
                    var particleProjectY = math.project(targetPos, transform.Up());
                    //var projectYDis = math.distance(particleProjectY, ostProjectY);
                    var disY = math.distance(particleProjectY, ostProjectY) - math.distance(ostAreaProjectY, ostProjectY) - targetRadius;

                    if (disY >= 0)//(projectYDis - targetRadius >= math.distance(ostAreaProjectY, ostProjectY))
                    {
                        dis = disY;
                        dir = float3.zero;
                        return false;
                    }
                    else
                    {
                        var disToMaxX = math.distancesq(particleProjectX, ostAreaProjectX);
                        var disToMaxY = math.distancesq(particleProjectY, ostAreaProjectY);
                        var disToMaxZ = math.distancesq(particleProjectZ, ostAreaProjectZ);

                        if (disToMaxX < disToMaxY && disToMaxX < disToMaxZ)
                        {
                            dis = targetRadius - math.sqrt(disToMaxX);
                            dir = transform.Right() *
                                ((math.dot(transform.Right(), math.normalize(particleProjectX - ostAreaProjectX)) > 0)
                                ? 1 : -1);
                        }
                        else if (disToMaxY < disToMaxX && disToMaxY < disToMaxZ)
                        {
                            dis = targetRadius - math.sqrt(disToMaxY);
                            dir = transform.Up() *
                                ((math.dot(transform.Up(), math.normalize(particleProjectY - ostAreaProjectY)) > 0)
                                ? 1 : -1);
                        }
                        else
                        {
                            dis = targetRadius - math.sqrt(disToMaxZ);
                            dir = transform.Forward() *
                                ((math.dot(transform.Forward(), math.normalize(particleProjectZ - ostAreaProjectZ)) > 0)
                                ? 1 : -1);
                        }

                        return true;
                    }//Calculate Box Normal
                }
                else
                {
                    var ostProjectY = math.project(transform.Position, transform.Up());
                    var particleProjectY = math.project(targetPos, transform.Up());
                    var disPlane = math.distance(ostProjectY, particleProjectY);
                    if (disPlane > targetRadius)
                    {
                        dis = disPlane - targetRadius;
                        dir = float3.zero;
                        return false;
                    }
                    else
                    {
                        dis = targetRadius - disPlane;
                        dir = (math.dot(transform.Up(), math.normalize(particleProjectY - ostProjectY)) > 0)
                            ? transform.Up() : -transform.Up();
                        return true;
                    }
                }//Plane
            }
            else
            {
                float disTarget = math.distance(targetPos, transform.Position);
                float size = WorldSize.x * 0.5f;

                if (disTarget - targetRadius > size)
                {
                    dis = disTarget - (targetRadius + size);
                    dir = Vector3.zero;
                    return false;
                }
                else
                {
                    dis = (targetRadius + size) - disTarget;
                    dir = math.normalize(targetPos - transform.Position);
                    return true;
                }
            }
        }
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
                if (authoring.colliderType == ColliderShape.Sphere)
                {
                    IsEllipse = !Mathf.Approximately(size.x, size.y) || !Mathf.Approximately(size.x, size.z);
                }

                AddComponent(GetEntity(authoring, TransformUsageFlags.Dynamic),
                    new CollisionComponent
                    {
                        collidershape = IsEllipse? ColliderShape.Box : authoring.colliderType,
                        colliderEvent = authoring.colliderEvent,
                        WorldSize = size
                    });
            }
            else if(authoring.TryGetComponent<SphereCollider>(out var sphereCollider))
            {
                var size = sphereCollider.radius * 2f * Mathf.Max(authoring.transform.localScale.x, authoring.transform.localScale.y, authoring.transform.localScale.z);

                AddComponent(GetEntity(authoring, TransformUsageFlags.Dynamic),
                    new CollisionComponent
                    {
                        collidershape = ColliderShape.Sphere,
                        colliderEvent = authoring.colliderEvent,
                        WorldSize = new float3(1,1,1) * size
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
                        collidershape = ColliderShape.Box,
                        colliderEvent = authoring.colliderEvent,
                        WorldSize = size
                    });
            }


            if (authoring.colliderEvent == ColliderEvent.Collision)
            {
                AddComponent<FluidCollider>(GetEntity(authoring, TransformUsageFlags.Dynamic));
            }else
            {
                AddComponent<FluidTrigger>(GetEntity(authoring, TransformUsageFlags.Dynamic));
            }

        }
    }

}
