using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct Speed : IComponentData
{
    public float value;
    // 외부와 정보교환 , export는 아직 모르겠음
}
