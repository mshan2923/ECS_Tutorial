using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Tutorial.DBAA;

public readonly partial struct DBAATestAspect : IAspect
{
    public readonly Entity self;
    public readonly RefRW<DBAAComponent> DBAA;

    public ref BlobArray<first> firsts
    {
        get => ref DBAA.ValueRO.assetReference.Value.firstBlobArray;
    }

    public BlobArray<first> firsts_NotRef
    {
        get => DBAA.ValueRO.assetReference.Value.firstBlobArray;
    }
}
