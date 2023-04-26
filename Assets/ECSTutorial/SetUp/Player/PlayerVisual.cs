using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

public class PlayerVisual : MonoBehaviour
{
    [Header("Get Entity form MonoBehaviour")]
    private Entity targetEntity;

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            targetEntity = GetRandomEntity();//랜덤 선택
        }
        if (targetEntity != Entity.Null)
        {
            Vector3 followPosition =
                World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalTransform>(targetEntity).Position;//World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<WorldTransform>(targetEntity)._Position;
            transform.position = followPosition;
        }//선택된 엔티티 따라가게
    }
    private Entity GetRandomEntity()
    {
        EntityQuery playerTagEntityQuery = 
         World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PlayerTag));

         NativeArray<Entity> entityNativeArray = playerTagEntityQuery.ToEntityArray(Allocator.Temp);
         if (entityNativeArray.Length > 0)
         {
            return entityNativeArray[Random.Range(0, entityNativeArray.Length)];
         }else
         {
            return Entity.Null;
         }
    }
}
