using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;

public partial class BlobSettingSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        using var blobBuilder = new BlobBuilder(Allocator.Temp);
        ref var blobAsset = ref blobBuilder.ConstructRoot<BlobAsset>();
        var TempArray = blobBuilder.Allocate(ref blobAsset.Array, 3);
        TempArray[0] = "첫번째";
        TempArray[1] = "두번째";
        TempArray[2] = "세번째";

        //BlobData를 사용 하고 있는 대상찾고 넣기 , 전부 넣어줘야하나?

        {
            //var data = SystemAPI.GetSingletonRW<BlobData>();
            //data.ValueRW.BlobRef = blobBuilder.CreateBlobAssetReference<BlobAsset>(Allocator.Persistent);
        }//싱글톤 버전 *** 컴포넌트가 1개만 있는경우 자동으로 싱글톤화
        

        {
            
            var data = new BlobData{BlobRef = blobBuilder.CreateBlobAssetReference<BlobAsset>(Allocator.Persistent)};
        
            Entities
            .WithName("BlobSetting")
            .WithBurst(Unity.Burst.FloatMode.Default, Unity.Burst.FloatPrecision.Standard, true)
            .ForEach ((Entity entity, int entityInQueryIndex, ref BlobData blobData) =>
            {
                blobData = data;
            }).ScheduleParallel();
            
        }// 일반버전 

    }
    protected override void OnUpdate()
    {
        
    }
}
