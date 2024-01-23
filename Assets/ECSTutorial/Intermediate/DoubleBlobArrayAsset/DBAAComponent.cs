using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.DBAA
{
    public struct DBAAComponent : IComponentData
    {
        public BlobAssetReference<DoubleArray> assetReference;
    }

    public struct DoubleArray
    {
        public BlobArray<first> firstBlobArray;
    }
    public struct first
    {
        public BlobArray<second> secondBlobArray;
    }
    public struct second 
    {
        public int randomValue;
    }
}