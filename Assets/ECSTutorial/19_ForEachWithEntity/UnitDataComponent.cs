using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Tutorial.GroupMovement
{
    public struct UnitData : IComponentData
    {
        public int index;
        public float3 offset;
        public bool Arrive;
        
    }
    public struct ArriveData : ISharedComponentData
    {
        public bool Arrive;
    }


}
