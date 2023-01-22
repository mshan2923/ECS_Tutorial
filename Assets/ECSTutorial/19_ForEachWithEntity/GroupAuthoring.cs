using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Tutorial.GroupMovement
{
    public class GroupAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public float UnitSize = 0.5f;
        public Vector2 Size = Vector2.one * 10;
        public float BetweenSpace = 0.5f;
        
        public float3 APoint;
        public float3 BPoint;
        public float MoveTime = 1f;
        public float Speed = 1f;
    }

    public struct GroupComponent : IComponentData
    {
        public Entity prefab;
        public float unitSize;
        public Vector2 size;
        public float betweenSpace;
        public float3 aPoint;
        public float3 bPoint;
        public float moveTime;
        public float speed;
    }

    public class GroupBaker : Baker<GroupAuthoring>
    {
        public override void Bake(GroupAuthoring authoring)
        {
            AddComponent(new GroupComponent
            {
                prefab = GetEntity(authoring.Prefab),
                unitSize = authoring.UnitSize,
                size = authoring.Size,
                betweenSpace = authoring.BetweenSpace,
                aPoint = authoring.APoint,
                bPoint = authoring.BPoint,
                moveTime = authoring.MoveTime,
                speed = authoring.Speed
            });
        }
    }

}