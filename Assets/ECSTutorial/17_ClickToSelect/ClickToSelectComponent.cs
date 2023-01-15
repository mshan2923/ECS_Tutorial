using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

namespace Tutorial.ClickToSelect
{
    [Flags]
    public enum CollisionLayers
    {
        Selection = 1 << 0,
        Ground = 1 << 1,
        Units = 1 << 2
    }
    //
    public struct SelectedEntityTag : IComponentData{}
    public struct SelectionColliderTag : IComponentData{}
    
    public struct StepToLiveData : IComponentData
    {
        public int Value;
    }
}