using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


public struct RotationSpeed_SpawnRemove : IComponentData
{
    public float RadiansPerSecond;    
    //RotationSpeed_ForEach 를 써도 되지만 , 다른 System끼리 간섭때문에 새로 생성
}
