using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace Tutorial.ConnectFour
{
    public class PieceMateriaData : MonoBehaviour
    {
        public Material Red;
        public Material Blue;
        public Material Yellow;
    }
    public struct PieceMateriaComponent : IComponentData
    {
        public Entity red;
        public Entity blue;
        public Entity yellow;

        //Class 인 Material 쓸수가 없는디
        
    }
    public class PieceMateriaBaker : Baker<PieceMateriaData>
    {
        public override void Bake(PieceMateriaData authoring)
        {
            /*
            AddComponent(new PieceMateriaComponent
            {
                red = authoring.Red,
                blue = authoring.Blue,
                yellow = authoring.Yellow
            });*/

            //AddComponent(new PieceMateriaComponent{});
        }
    }
}