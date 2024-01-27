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
                Blob �ڻ꿡�� �Ϲ� �迭, ���ڿ� �Ǵ� ��Ÿ ���� ��ü�� ���� ���� �����Ͱ� ���ԵǾ�� �� �˴ϴ�.
                Blob �ڻ� �����ʹ� �б� �����Դϴ�. ��, ��Ÿ�� �� ������� �ʽ��ϴ�.
                Blob �ڻ��� ������ �ε��Ϸ��� ���Ե� �����Ͱ� �� ���� �̾�� �մϴ� .  
                �����޴� �����ʹ� ����Ҽ� ���� (ex : String, GameObject �� ���� class��)

                ������ ���濡�� �������� �ʿ��� �Ӹ� �ƴ϶� ���� ��� �����Ϳ� ���� ��� ���� ������ ��ȿȭ�˴ϴ�.
                ���⿡�� DynamicBuffer �ν��Ͻ� �� ComponentSystemBase.GetComponentDataFromEntity �� ���� ���� ��ҿ� ���� ���� �׼����� �����ϴ� �޼����� ����� ���Ե˴ϴ� .
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
