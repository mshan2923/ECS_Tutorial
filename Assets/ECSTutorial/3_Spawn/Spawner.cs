using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace tutorial
{
public struct Spawner : IComponentData
{
    public Entity Prefab;// 주의! GameObject를 쓸경우 유니티 크래시
    public int CountX;
    public int CountY;
}

}