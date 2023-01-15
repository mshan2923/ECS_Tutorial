using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Tutorial.ClickToSelect
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    //[UpdateAfter(typeof(PhysicsSimulationGroup))] 
    public partial class SelectMultipleSystem : SystemBase
    {   
        //private StepPhysicsWorld _stepPhysicsWorld;
        private BuildPhysicsWorld _buildPhysicsWorld;
        private EndFixedStepSimulationEntityCommandBufferSystem _endFixedECBSystem;

        protected override void OnCreate()
        {
            RequireForUpdate<SelectionColliderTag>();
            
            //EntityManager.Asp<PhysicsWorld>();
            
            _endFixedECBSystem = World.GetOrCreateSystemManaged<EndFixedStepSimulationEntityCommandBufferSystem>();
            
        }
        protected override void OnUpdate()
        {
            //https://docs.unity3d.com/Packages/com.unity.physics@1.0/manual/simulation-results.html
            //유니티 공홈 코드가 잘못되었는데?

            var ecb = _endFixedECBSystem.CreateCommandBuffer();

            var jobHandle = new SelectionJob
            {
                SelectionVolumes = GetComponentLookup<SelectionColliderTag>(),
                Units = GetComponentLookup<SelectableUnitTag>(),
                ECB = ecb
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);//=============== 작동X

            jobHandle.Complete();
            /*
            var selectionEntity = SystemAPI.GetSingletonEntity<SelectionColliderTag>();//======================== 오류발샐ㅇ - 1개만이 아니니까

            if (SystemAPI.HasComponent<StepToLiveData>(selectionEntity))
            {
                var stepToLive = SystemAPI.GetComponent<StepToLiveData>(selectionEntity);
                stepToLive.Value--;
                ecb.SetComponent(selectionEntity, stepToLive);
                if (stepToLive.Value <= 0)
                {
                    ecb.DestroyEntity(selectionEntity);
                }
            }else
            {
                ecb.AddComponent<StepToLiveData>(selectionEntity);
                ecb.SetComponent(selectionEntity, new StepToLiveData{Value = 1});
            }
            */
        }

        public struct SelectionJob : ITriggerEventsJob
        {
            public ComponentLookup<SelectionColliderTag> SelectionVolumes;
            public ComponentLookup<SelectableUnitTag> Units;
            public EntityCommandBuffer ECB;

            public void Execute(TriggerEvent triggerEvent)
            {
                var entityA = triggerEvent.EntityA;
                var entityB = triggerEvent.EntityB;

                
                var isBodyASelection = SelectionVolumes.HasComponent(entityA);
                var isBodyBSelection = SelectionVolumes.HasComponent(entityB);
                /*
                if (isBodyASelection && isBodyBSelection)
                {
                    //return;
                }

                var isBodyAUnit = Units.HasComponent(entityA);
                var isBodyBUnit = Units.HasComponent(entityB);//여기서 뭔가 문제

                if ((isBodyASelection && !isBodyBUnit) || (isBodyBSelection && !isBodyAUnit))
                {
                    //return;
                }
                */
                
                //!SECTION 지금 충돌일어난거 전부 실행하다보니

                var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                if (manager.HasComponent<SelectionColliderTag>(entityA) || manager.HasComponent<SelectionColliderTag>(entityB))
                {
                    Debug.Log("SelectionJob - SelectionColliderTag Collision");

                    if (manager.HasComponent<SelectableUnitTag>(entityA) || manager.HasComponent<SelectableUnitTag>(entityB))
                    {
                        var selcted = isBodyASelection ? entityA : entityB;
                        ECB.AddComponent<SelectedEntityTag>(selcted);

                        Debug.Log("SelectionJob - Collision : " + selcted.ToString()); 
                    }else
                    {
                        Debug.Log("SelectionJob - Not Include SelectableUnitTag \n " + entityA.ToFixedString() + " << " + entityB.ToFixedString());
                    }

                    //음.. 뭐랑 상호작용? , 너무 많이 로그가
                }
            }
        }
    }

}