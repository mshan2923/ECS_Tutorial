using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;

public class TutorialBlobAsset : MonoBehaviour
{
    public string[] Texts;
}
public struct BlobAsset
{
    public BlobArray<FixedString32Bytes> Array;
}
public struct BlobData : IComponentData
{
    public BlobAssetReference<BlobAsset> BlobRef;
}
public class TutorialBlobAssetBaker : Baker<TutorialBlobAsset>
{
    public override void Bake(TutorialBlobAsset authoring)
    {
        AddComponent(new BlobData{});
    }
}
