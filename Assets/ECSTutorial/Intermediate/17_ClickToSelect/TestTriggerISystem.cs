using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Tutorial.ClickToSelect
{
    public partial struct TestTriggerISystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        
        }

        public void OnDestroy(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new TriggerJob()
            {

            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        }

        public struct TriggerJob : Unity.Physics.ITriggerEventsJob
        {
            public void Execute(TriggerEvent triggerEvent)
            {
                var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                //triggerEvent.EntityA
                //World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<SelectableUnitTag>(triggerEvent.EntityA);

                if (manager.HasComponent<SelectableUnitTag>(triggerEvent.EntityA) || manager.HasComponent<SelectableUnitTag>(triggerEvent.EntityB))
                {
                    Debug.Log("TriggerJob - SelectableUnitTag Overlap");
                }else
                {
                    Debug.Log("TriggerJob - Overlap");//너무 많이 떠서... TestIntiPhysicsBodySystem.IntiJob가 실행되야 뜸, 그런데 오브젝트를 통과 하는디
                }
                if (manager.HasComponent<SelectionColliderTag>(triggerEvent.EntityA) || manager.HasComponent<SelectionColliderTag>(triggerEvent.EntityB))
                {
                    Debug.Log("TriggerJob - SelectionColliderTag Overlap");
                }
                //SystemBase.ITriggerEventsJob(selectMultipleSystem)는 안되고 ISystem.ITriggerEventsJob인 이거는 되는데...
            }
        }
    }

}
