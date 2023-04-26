using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using RaycastHit = Unity.Physics.RaycastHit;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;
using Unity.Transforms;

namespace Tutorial.ClickToSelect
{
    //[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    //[UpdateAfter(typeof(PhysicsSystemGroup))]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsInitializeGroup))]
    public partial class SelectSystem : SystemBase
    {
        Camera camera;
        private PhysicsWorld physicsWorld;
        private CollisionWorld _collisionWorld;
        EntityArchetype selectionArchetype;

        Entity SelecterEntity;

        bool isDragging = false;
        float3 mouseStartPos;

        protected override void OnStartRunning()
        {
            this.Enabled = false;
            return;
            
            camera = Camera.main;
            selectionArchetype = EntityManager.CreateArchetype(typeof(PhysicsCollider), typeof(SelectionColliderTag), typeof(PhysicsWorldIndex), typeof(LocalTransform));// WorldTransform
            //LocalToWorld 때문에 'The UNKNOWN_OBJECT_TYPE has been deallocated' 발생
            if (SystemAPI.HasSingleton<PhysicsWorldSingleton>())
            {

            }else
            {
                var e = EntityManager.CreateEntity();
                //var entity = EntityManager.Instantiate(EntityManager.CreateEntity());
                EntityManager.AddComponent(e, typeof(PhysicsWorld));
            }
                physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        }
        protected override void OnUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseStartPos = Input.mousePosition;
            }
            if (Input.GetMouseButtonDown(0))
            {
                //EntityManager.DestroyEntity(SelecterEntity);
            }

            if (Input.GetMouseButton(0) && !isDragging)
            {
                if (math.distance(mouseStartPos, Input.mousePosition) > 25)
                {
                    //isDragging = true;
                }
            }

            isDragging = Input.GetKey(KeyCode.LeftControl);

            if (Input.GetMouseButton(0))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectUnit();
                }else
                {
                    if (isDragging)
                    {
                        //CreateTriggerToSelect();
                        CreateDebugCollider();
                    }else
                    {
                        var hit = SelectSingleUnit();
                        if (hit != Entity.Null)
                            Debug.Log(hit.Index + " : " + hit.ToString());
                    }
                }
            }
        }

        Entity SelectSingleUnit()
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            var rayStart = ray.origin;
            var rayEnd = ray.GetPoint(100f);

            Debug.DrawRay(rayStart, (rayEnd - rayStart), Color.red, 5f);

            if (Raycast(rayStart, rayEnd, out var raycastHit))            
            {
                var hitEntity = physicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
                if (EntityManager.HasComponent<SelectableUnitTag>(hitEntity))
                {
                    World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>()
                        .CreateCommandBuffer().AddComponent<SelectedEntityTag>(hitEntity);
                        //AddComponent 전에 .AsParallelWriter()하면 병렬로
                }

                return hitEntity;
            }

            return Entity.Null;
        }

        void CreateTriggerToSelect()//SelectMultipleUnits
        {
            //멀티선택은 Unity.Physics.ConvexCollider 를 사용해 , 트리거 볼륨을 만들어 트리거 이벤트를 받는 구조
            isDragging = false;

            var topLeft = math.min(mouseStartPos, Input.mousePosition);
            var botRight = math.max(mouseStartPos, Input.mousePosition);

            var rect = Rect.MinMaxRect(topLeft.x, topLeft.y, botRight.x, botRight.y);

            var cornerRays = new[]
            {
                Camera.main.ScreenPointToRay(rect.min),
                Camera.main.ScreenPointToRay(rect.max),
                Camera.main.ScreenPointToRay(new Vector2(rect.xMin, rect.yMax)),
                Camera.main.ScreenPointToRay(new Vector2(rect.xMax, rect.yMin))
            };

            {
                //Debug.DrawLine(cornerRays[0].GetPoint(1), cornerRays[2].GetPoint(1), Color.black, 5f);
                //Debug.DrawLine(cornerRays[0].GetPoint(1), cornerRays[3].GetPoint(1), Color.black, 5f);

                Physics.Raycast(cornerRays[0],out var hit0 , 50f);
                Physics.Raycast(cornerRays[1],out var hit1 , 50f);
                Physics.Raycast(cornerRays[2],out var hit2 , 50f);
                Physics.Raycast(cornerRays[3],out var hit3 , 50f);

                Debug.DrawLine(hit0.point, hit2.point, Color.black, 5f);
                Debug.DrawLine(hit0.point, hit3.point, Color.black, 5f);
                Debug.DrawLine(hit1.point, hit2.point, Color.black, 5f);
                Debug.DrawLine(hit1.point, hit3.point, Color.black, 5f);
            }

            var vertices = new NativeArray<float3>(5, Allocator.Temp);

            for (int i = 0; i < cornerRays.Length; i++)
            {
                vertices[i] = cornerRays[i].GetPoint(50f);
            }
            vertices[4] = Camera.main.transform.position;

            var collisionFilter = new CollisionFilter
            {
                BelongsTo = (uint) uint.MaxValue,//CollisionLayers.Selection,
                CollidesWith = (uint) uint.MaxValue//CollisionLayers.Units
            };

            //PhysicsMass.CreateKinematic(new MassProperties{});//======================= PhysicsBodyAspect 필요 => PhysicsMass 필요
            // 이후 참고 : https://forum.unity.com/threads/how-to-create-a-physicsmass-component-entirely-in-code.862624/

            var physicsMat = Unity.Physics.Material.Default;
            physicsMat.CollisionResponse = CollisionResponsePolicy.CollideRaiseCollisionEvents;

            var selectionCollider = ConvexCollider.Create(vertices, ConvexHullGenerationParameters.Default,
                 collisionFilter, physicsMat);
            
            
            var newSelectionEntity = EntityManager.CreateEntity(selectionArchetype);
            EntityManager.SetName(newSelectionEntity, new FixedString64Bytes("Multiple Select"));

                {
                    EntityManager.SetComponentData(newSelectionEntity, new PhysicsCollider{Value = selectionCollider});//selectionCollider

                        //PhysicsMass.CreateKinematic(selectionCollider.Value.MassProperties);//-------------------------- 이거면 될려나?
                    EntityManager.AddComponentData(newSelectionEntity, PhysicsMass.CreateKinematic(selectionCollider.Value.MassProperties));
                        //EntityManager.AddComponentData(newSelectionEntity, new PhysicsVelocity {});
                    EntityManager.AddComponentData(newSelectionEntity, new PhysicsDamping {});
                    EntityManager.AddComponentData(newSelectionEntity, new PhysicsStep
                    {
                        SimulationType = SimulationType.UnityPhysics,
                        SolverIterationCount = 2,
                        MultiThreaded = 1
                    });

                    EntityManager.AddComponentData(newSelectionEntity, new PhysicsCustomTags{Value = 255});
                    EntityManager.AddComponent<SelectionColliderTag>(newSelectionEntity);
                    //============================ PhysicsBodyBakingSystem.PhysicsBodyAuthoringBaker 참고해서.. 일반 오브젝트에 컴포넌트 추가해서
                    //=============================   충돌 이벤트되는지

                }

                
            //Unity.Rendering.RenderMeshUtility.AddComponents();


        }
        void CreateDebugCollider()
        {
            Debug.Log("Try CreateDebugCollider");

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray,out var hit , 50f);

            var physicsMat = Unity.Physics.Material.Default;
            physicsMat.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;

            var collider = new PhysicsCollider
            {
                Value = Unity.Physics.BoxCollider.Create(new BoxGeometry
            {
                Center = new float3(0, 0.5f, 0),
                BevelRadius = 0.05f,
                Orientation = quaternion.identity,
                Size = new float3(1,1,1) *  1000
            },
            new CollisionFilter
            {
                BelongsTo = (uint) uint.MaxValue,//CollisionLayers.Selection,
                CollidesWith = (uint) uint.MaxValue//(CollisionLayers.Units | CollisionLayers.Ground)
            },
            physicsMat
            )};

            if (SelecterEntity.Equals(Entity.Null))
            {
                SelecterEntity = EntityManager.CreateEntity();
            }
            EntityManager.AddComponent<SelectableUnitTag>(SelecterEntity);
            EntityManager.AddComponent<SelectionColliderTag>(SelecterEntity);

            EntityManager.AddComponentData(SelecterEntity, new LocalTransform //WorldTransform 
            {
                Position = hit.point,
                Rotation = quaternion.identity,
                Scale = 1000
            });
            EntityManager.AddComponentData<PhysicsCollider>(SelecterEntity, collider);
            
            EntityManager.AddComponentData<PhysicsStep>(SelecterEntity, new PhysicsStep
            {
                SimulationType = SimulationType.UnityPhysics,
                        
                Gravity = new float3(0, -9.81f, 0),
                SolverIterationCount = 2,
                MultiThreaded = 1
            });                    
            EntityManager.AddComponentData<PhysicsCustomTags>(SelecterEntity, new PhysicsCustomTags{Value = 255});
            EntityManager.AddComponentData<PhysicsMass>(SelecterEntity, PhysicsMass.CreateKinematic(collider.MassProperties));
            EntityManager.AddComponentData<Unity.Physics.Authoring.PhysicsBodyAuthoringData>(SelecterEntity,
             new Unity.Physics.Authoring.PhysicsBodyAuthoringData {IsDynamic = true, Mass = 1, OverrideDefaultMassDistribution = true
                , CustomMassDistribution = collider.MassProperties.MassDistribution});

             EntityManager.AddComponent<PhysicsVelocity>(SelecterEntity);
            
            //Unity.Physics.Aspects.RigidBodyAspect는 잘 추가 되었는데...
            //Debug.Log(" - " + EntityManager.GetAspectRO<Unity.Physics.Aspects.RigidBodyAspect>(SelecterEntity).Entity.Index);

            //Debug.Log("Pos : "  + EntityManager.GetComponentData<LocalTransform>(SelecterEntity).Position);
            //이것도 잘되는데? , 최소최댓값이 변하지 않음

            Debug.Log("Collider : "  + EntityManager.GetComponentData<PhysicsCollider>(SelecterEntity).Value.Value.CalculateAabb().Min + " ~ " 
            + EntityManager.GetComponentData<PhysicsCollider>(SelecterEntity).Value.Value.CalculateAabb().Max);

            Debug.Log("Shape :" + EntityManager.GetComponentData<PhysicsCollider>(SelecterEntity).Value.Value.GetCollisionResponse());
        }

        Entity DeselectUnit()
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            var rayStart = ray.origin;
            var rayEnd = ray.GetPoint(100f);

            Debug.DrawRay(rayStart, (rayEnd - rayStart), Color.red, 5f);

            if (Raycast(rayStart, rayEnd, out var raycastHit))            
            {
                var hitEntity = physicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
                if (EntityManager.HasComponent<SelectedEntityTag>(hitEntity))
                {
                    World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>()
                        .CreateCommandBuffer().RemoveComponent<SelectedEntityTag>(hitEntity);
                        //AddComponent 전에 .AsParallelWriter()하면 병렬로
                }

                return hitEntity;
            }

            return Entity.Null;
        }
        private bool Raycast(float3 rayStart, float3 rayEnd, out RaycastHit raycastHit)
        {
            var raycastInput = new RaycastInput
            {
                Start = rayStart,
                End = rayEnd,
                Filter = new CollisionFilter
                {
                    BelongsTo = (uint) CollisionLayers.Selection,
                    CollidesWith = (uint) (CollisionLayers.Ground | CollisionLayers.Units)
                }
            };

            return physicsWorld.CastRay(raycastInput, out raycastHit);//NOTE - 특정 컴포넌트를 인식을 못해서 'The UNKNOWN_OBJECT_TYPE' 오류 발생
        }
    }

}