using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.ClickToSelect
{
    public class SelectableUnit : MonoBehaviour {}
    public struct SelectableUnitTag : IComponentData{}
    public class SelectableUnitBaker : Baker<SelectableUnit>
    {
        public override void Bake(SelectableUnit authoring)
        {
            AddComponent(new SelectableUnitTag{});
        }
    }

}