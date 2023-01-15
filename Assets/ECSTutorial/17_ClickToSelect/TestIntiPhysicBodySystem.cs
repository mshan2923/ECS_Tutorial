using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using RaycastHit = Unity.Physics.RaycastHit;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;

namespace Tutorial.ClickToSelect
{
    public partial class TestIntiPhysicBodySystem : SystemBase
    {
        BeginInitializationEntityCommandBufferSystem intiECB;
        protected override void OnStartRunning()
        {
            intiECB = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>();

            var ecb = intiECB.CreateCommandBuffer();
            var jobHandle = new IntiJob
            {
                ecb = ecb
            };
            //jobHandle.Schedule();
        }
        protected override void OnUpdate()
        {
            
            Dependency.Complete();
            /*
            Entities
                .WithAll<SelectableUnitTag>()
                .ForEach((Entity e) =>
                {
                    ecb.AddComponent<PhysicsVelocity>(e, new PhysicsVelocity{});
                    ecb.AddComponent<PhysicsMass>(e, PhysicsMass.CreateDynamic(new MassProperties{},1));
                    ecb.AddComponent<PhysicsMassOverride>(e, new PhysicsMassOverride{IsKinematic = 1});
                    ecb.AddComponent<PhysicsDamping>(e, new PhysicsDamping{});
                    ecb.AddComponent<PhysicsGravityFactor>(e, new PhysicsGravityFactor{});
                    ecb.AddComponent<PhysicsCollider>(e, new PhysicsCollider{});
                }).Schedule();*/         



        }

        public partial struct IntiJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public void Execute(Entity e, in SelectableUnitTag tag)
            {
                var physicsMat = Unity.Physics.Material.Default;
                physicsMat.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;

                var collider = new PhysicsCollider
                {
                    Value = Unity.Physics.BoxCollider.Create(new BoxGeometry
                    {
                        Center = new float3(0, 0.5f, 0),
                        Orientation = quaternion.identity,
                        Size = new float3(1,1,1),
                        BevelRadius = 0.05f                    
                    },
                    new CollisionFilter
                    {
                        BelongsTo = (uint) uint.MaxValue,//CollisionLayers.Selection,
                        CollidesWith = (uint) uint.MaxValue//(CollisionLayers.Units | CollisionLayers.Ground)
                        //----------- 충돌레이어 설정 문제
                    },
                    physicsMat
                    )
                };

                    
                    //ecb.AddComponent<PhysicsVelocity>(e, new PhysicsVelocity{});
                    //!SECTION------------- 이것때문에 'The UNKNOWN_OBJECT_TYPE has been deallocated' 발생 , 근대 이거 없으면 PhysicsBodyAspect가 없음...
                    ecb.AddComponent<PhysicsMass>(e, PhysicsMass.CreateDynamic(collider.MassProperties, 1));
                        ecb.AddComponent<PhysicsMassOverride>(e, new PhysicsMassOverride{IsKinematic = 0});//굳이
                    ecb.AddComponent<PhysicsDamping>(e, new PhysicsDamping{Linear = 0.01f, Angular = 0.05f});
                        //ecb.AddComponent<PhysicsGravityFactor>(e, new PhysicsGravityFactor{Value = -9.8f});//굳이
                            //PhysicsWorld.raycast 에서'The UNKNOWN_OBJECT_TYPE has been deallocated' 발생
                    ecb.AddComponent<PhysicsStep>(e, new PhysicsStep
                    {
                        SimulationType = SimulationType.UnityPhysics,
                        
                        Gravity = new float3(0, -9.81f, 0),
                        SolverIterationCount = 2,
                        MultiThreaded = 1
                    });                    
                    ecb.AddComponent<PhysicsCustomTags>(e, new PhysicsCustomTags{Value = 255});
                    
                    
                    ecb.AddComponent<PhysicsCollider>(e, collider);//----- 되는것도 안되게함 + Raycast 무시됨 (충돌레이어 설정 문제 , 근대 왜 레이케스트는 되는데 충돌은?)

                    //이미 있는걸 수정 => CollisionResponse, ShapeType ===> 메쉬를 ConvexCollider으로 만든 메쉬로 대체

                    //Unity.Physics.Authoring.PhysicsBodyAuthoring
            }
        }
    }
}
