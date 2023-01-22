using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;

namespace Tutorial.GerateMesh
{
    //[UpdateBefore(typeof(BeginSimulationEntityCommandBufferSystem))]
    public partial class ChangeMeshSystem : SystemBase
    {
        EntityCommandBuffer ecb;
        protected override void OnStartRunning()
        {
            Entity entity = EntityManager.CreateEntity();
            ecb = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>().CreateCommandBuffer();
        }
        protected override void OnUpdate()
        {
            Entities
                .WithAll<ChangeMeshTag>()
                .WithStructuralChanges()
                .ForEach((Entity e, in Unity.Rendering.RenderMeshArray render) => 
                {
                    //메쉬를 바꾸기 - 안되네 참조 안됨
                    //엔티티 생성후 랜더메쉬 추가
                    //render.Meshes[0] = GerateMeshList.instance.mesh[0];
                    {
                        var desc = new RenderMeshDescription(shadowCastingMode: UnityEngine.Rendering.ShadowCastingMode.On, receiveShadows: true);
                        var renderMeshArray = new RenderMeshArray
                        (
                            new Material[] {GerateMeshList.instance.DefaultMat},
                            new Mesh[] {GerateMeshList.instance.mesh[0]}
                        );

                        EntityManager.AddComponent(e, typeof(RenderMeshArray));
                        RenderMeshUtility.AddComponents(e, EntityManager, desc, renderMeshArray);//MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
                        //EntityManager.AddComponent<Unity.Transforms.LocalToWorld>(e);
                        //EntityManager.AddComponent<EditorRenderData>
                        EntityManager.SetComponentData<RenderBounds>(e, new RenderBounds{Value = new Unity.Mathematics.AABB
                        {
                            Center = Unity.Mathematics.float3.zero,
                            Extents = new Unity.Mathematics.float3(1, 1, 1) * 10  
                        }});//이거는 변경됨

                        //ecb.AddSharedComponent<RenderMeshArray>(e, renderMeshArray);//!SECTION  RenderMeshArray이 null 이될수있다며 오류
                    }

                    Debug.Log("Has RenderMeshArray : " + EntityManager.HasComponent<RenderMeshArray>(e) + "\n =>"
                        + EntityManager.GetSharedComponentManaged<RenderMeshArray>(e).Meshes[0].name);
                }).Run();
                

                Dependency.Complete();

            

                //!SECTION============= ....RenderMeshArray 가 ㅊ추가 + 변경 되었지만 적용이 안됨
            
        }
    }

}