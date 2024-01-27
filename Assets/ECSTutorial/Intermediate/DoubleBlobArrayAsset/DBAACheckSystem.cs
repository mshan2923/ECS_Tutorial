using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Tutorial.DBAA;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/blob-assets-concept.html
/// https://docs.unity3d.com/Packages/com.unity.entities@0.51/manual/sync_points.html#avoiding-sync-points
/// </summary>
public partial class DBAACheckSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        if (Entities.WithAll<DBAAComponent>().ToQuery().CalculateEntityCount() <= 0)
            this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        if (SystemAPI.HasSingleton<DBAAComponent>())
        {
            var target = SystemAPI.GetSingletonEntity<DBAAComponent>();
            var data = SystemAPI.GetSingleton<DBAAComponent>();

            Debug.Log($"[0][0] -> {data.assetReference.Value.firstBlobArray[0].secondBlobArray[0].randomValue}");

            /*
                Blob 자산에는 일반 배열, 문자열 또는 기타 관리 개체와 같은 관리 데이터가 포함되어서는 안 됩니다.
                Blob 자산 데이터는 읽기 전용입니다. 즉, 런타임 시 변경되지 않습니다.
                Blob 자산을 빠르게 로드하려면 포함된 데이터가 값 유형 이어야 합니다 .  
                관리받는 데이터는 사용할수 없음 (ex : String, GameObject 와 같은 class형)

                구조적 변경에는 동기점이 필요할 뿐만 아니라 구성 요소 데이터에 대한 모든 직접 참조도 무효화됩니다.
                여기에는 DynamicBuffer 인스턴스 와 ComponentSystemBase.GetComponentDataFromEntity 와 같은 구성 요소에 대한 직접 액세스를 제공하는 메서드의 결과가 포함됩니다 .
             */


            var checkHandle = new CheckJob().Schedule(Dependency);
            checkHandle.Complete();

            var aspect = SystemAPI.GetAspect<DBAATestAspect>(target);
            //Debug.Log($"--> {SystemAPI.GetAspect<DBAATestAspect>(target).DBAA.ValueRO.assetReference.Value.firstBlobArray.Length}");

            if (aspect.firsts.Length > 0)
            {
                Debug.Log($"first (Ref) : {aspect.firsts[0].secondBlobArray[0].randomValue} / first  : {aspect.firsts_NotRef[0].secondBlobArray[0].randomValue}");

            }
        }
    }

    public partial struct CheckJob : IJobEntity
    {
        public void Execute([EntityIndexInQuery]int index, Entity entity, DBAAComponent dbaa)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"Result [{index}] - {entity.Index}\n");

            ref BlobArray<first> first = ref dbaa.assetReference.Value.firstBlobArray;
            for (int i = 0; i < first.Length; i++)
            {
                for (int j = 0; j < first[i].secondBlobArray.Length; j++)
                {
                    sb.Append(first[i].secondBlobArray[j].randomValue);
                    sb.Append(" ,");
                }
                sb.Append('\n');
            }

            Debug.Log(sb.ToString());
        }
    }
}
