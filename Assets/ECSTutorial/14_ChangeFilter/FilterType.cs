using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.ChangeFilter
{
    public enum FilterData 
    {
        A, B, C
    }
    public class FilterType : MonoBehaviour
    {
        public FilterData filterData;
    }
    public struct FilterTypeComponent : IComponentData
    {
        public FilterData filterData;
    }
    public class FillterTypeBaker : Baker<FilterType>
    {
        public override void Bake(FilterType authoring)
        {
            AddComponent(new FilterTypeComponent{ filterData = authoring.filterData});
        }
    }
}
