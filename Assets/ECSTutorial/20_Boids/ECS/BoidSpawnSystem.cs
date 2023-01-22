using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public partial class BoidSpawnSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        if (SystemAPI.HasSingleton<BoidControllerECSConponent>() == false)
        {
            Enabled = false;
            return;
        }

        BoidControllerECSConponent ControllerData = SystemAPI.GetSingleton<BoidControllerECSConponent>();
        NativeArray<Entity> boidArray = new NativeArray<Entity>(ControllerData.boidAmount, Allocator.TempJob);
        var ecb = World.GetExistingSystemManaged<BeginInitializationEntityCommandBufferSystem>().CreateCommandBuffer();

        ecb.Instantiate(ControllerData.prefab, boidArray);
        

        for (int i = 0; i < boidArray.Length; i++) 
        {
            Unity.Mathematics.Random rand = new Unity.Mathematics.Random((uint)i + 1);

            ecb.SetComponent<LocalTransform>(boidArray[i], new LocalTransform
            {
                Position = RandomPosition(ControllerData),
                Rotation = RandomRotation(),
                Scale = ControllerData.offsetScale
            });

            //ecb.SetSharedComponentManaged(boidArray[i], new Unity.Rendering.RenderMeshArray
            //(
            //    new Material[] {BoidControllerECS.Instance.sharedMaterial},
            //    new Mesh[] {BoidControllerECS.Instance.sharedMesh}
            //));//!SECTION Registering mesh null at index 0 inside a RenderMeshArray failed.
        }

        boidArray.Dispose();
        
    }
    protected override void OnUpdate()
    {
        
    }

    
        private float3 RandomPosition(BoidControllerECSConponent ControllerData) {
        return new float3(
            UnityEngine.Random.Range(-ControllerData.cageSize / 2f, ControllerData.cageSize / 2f),
            UnityEngine.Random.Range(-ControllerData.cageSize / 2f, ControllerData.cageSize / 2f),
            UnityEngine.Random.Range(-ControllerData.cageSize / 2f, ControllerData.cageSize / 2f)
        );
    }
    private quaternion RandomRotation() {
        return quaternion.Euler(
            UnityEngine.Random.Range(-360f, 360f),
            UnityEngine.Random.Range(-360f, 360f),
            UnityEngine.Random.Range(-360f, 360f)
        );
    }
}
