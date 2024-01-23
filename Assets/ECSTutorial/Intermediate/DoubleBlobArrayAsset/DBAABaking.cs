using System.Collections;
using System.Collections.Generic;
using Tutorial.DBAA;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Tutorial.DBAA
{
    public class DBAABaking : Baker<DBAA_Authoring>
    {
        public override void Bake(DBAA_Authoring authoring)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref DoubleArray doubleArray = ref builder.ConstructRoot<DoubleArray>();
            var FirstArrayBuilder = builder.Allocate(ref doubleArray.firstBlobArray, authoring.firstArraySize);


            for (int i = 0; i < authoring.firstArraySize; i++)
            {
                var secondArrayBuilder = builder.Allocate(
                    ref FirstArrayBuilder[i].secondBlobArray,
                    authoring.secondArraySize);

                for (int j = 0; j < authoring.secondArraySize; j++)
                {
                    secondArrayBuilder[j].randomValue = Random.Range(0, 99);
                }
            }


            //AddBlobAsset<DoubleArray>(ref builder.CreateBlobAssetReference<DoubleArray>(Allocator.Persistent));

            AddComponent(GetEntity(authoring, TransformUsageFlags.None),
                new DBAAComponent
                {
                    assetReference = builder.CreateBlobAssetReference<DoubleArray>(Allocator.Persistent)
                });
        }
    }

}