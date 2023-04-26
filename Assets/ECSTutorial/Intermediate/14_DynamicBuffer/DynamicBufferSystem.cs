using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


public partial class Tutorial_DynamicBufferSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        
    }
    protected override void OnUpdate()
    {
        var random = new Unity.Mathematics.Random(19842);

        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);
        Entities
            .ForEach((Entity e , in Tutorial14Tag tag) =>
            {
                //var buffer = ecb.AddBuffer<DynamicBufferData>(e);
                var buffer =  new DynamicBuffer<DynamicBufferData>();
                if (EntityManager.HasBuffer<DynamicBufferData>(e))
                {
                    buffer =  EntityManager.GetBuffer<DynamicBufferData>(e, false);
                }else
                {
                    buffer = ecb.AddBuffer<DynamicBufferData>(e);
                }

                //ecb.SetBuffer<DynamicBufferData>(e);//SetBuffer는 AddBuffer 와 비슷함 , DynamicBufferData 버퍼가 있어야함

                buffer.Length = 20;
                for(int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = new DynamicBufferData {Value = random.NextInt()};
                }

                buffer.Add(new DynamicBufferData {Value = 0});
            }).WithoutBurst().Run();

            ecb.Dispose();

            //!SECTION 이것때문에 씨발 튜토 20때 알고리즘 문제 인줄알고....

            //동적 버퍼의 길이, 용량 및 내용을 설정하면 ECS는 이러한 변경 내용을 엔티티 명령 버퍼에 기록합니다.
            // EntityCommandBuffer를 재생하면 ECS가 동적 버퍼를 변경합니다.

            //https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/components-buffer-command-buffer.html

    }
}
