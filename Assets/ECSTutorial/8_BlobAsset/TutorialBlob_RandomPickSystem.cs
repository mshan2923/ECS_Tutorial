using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

//ISystem에선 Class 사용불가여서 싱글톤 못씀
public partial class TutorialBlob_RandomPickSystem : SystemBase
{
    protected override void OnUpdate()
    {

        var random = new Unity.Mathematics.Random((uint)Mathf.RoundToInt(SystemAPI.Time.DeltaTime * 1000));
        
        {
            //var data = SystemAPI.GetSingleton<BlobData>();

            //Debug.Log(data.BlobRef.Value.Array[random.NextInt(0, int.MaxValue) % 3]);
        }//싱글톤 버전


        {
            int i = 0;
            foreach(var temp in SystemAPI.Query<BlobData>())
            {
                if (temp.BlobRef.IsCreated)
                {
                    Debug.Log(temp.BlobRef.Value.Array[random.NextInt(0, int.MaxValue) % 3]  + " / " + i);
                    i++;
                }
                else
                    Debug.Log("BlobRef is Null");

                //쓰기 불가
            }
        }// 일반버전
    }
}
