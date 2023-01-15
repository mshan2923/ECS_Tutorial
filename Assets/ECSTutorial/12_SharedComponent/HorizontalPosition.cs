using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.ConnectFour
{
    public struct HorizontalPosition : ISharedComponentData
    {
        public int Value; 
    }
    public struct VerticalPosition : ISharedComponentData
    {
        public int Value; 
    }            

}