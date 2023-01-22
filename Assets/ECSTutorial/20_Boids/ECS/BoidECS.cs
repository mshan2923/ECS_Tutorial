using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct BoidECS : IComponentData
{
}
//원래 ISharedComponentData 쓰는데 필드값이 없으면 오류뜸
