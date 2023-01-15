using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tutorial.ClickToSelect
{
    public partial class ChoosedSystem : SystemBase
    {
        protected override void OnUpdate()
        {
             Entities
                .WithNone<SelectedEntityTag>()
                .ForEach((Entity e, int entityInQueryIndex, ref LocalTransform trans, in SelectableUnitTag tag) => 
                {
                    //trans.Position.y = 0;
                    trans.Scale = 1;
                }).Schedule();

                //ref 가 뒤에 있으니 오류 뜨는데?

            Entities
                .ForEach((Entity e, ref LocalTransform trans, in SelectedEntityTag tag) =>
                {
                    //trans.Position.y = 1;
                    trans.Scale = 0.5f;
                }).Schedule();

                //Unity.Physics.ConvexCollider
        
        }
    }
}
