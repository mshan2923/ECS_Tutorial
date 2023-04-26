using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

//[DisallowMultipleComponent]//중복된 컨포넌트 방지
public class RotationSpeedAuthoring_IJobEntityBatch : MonoBehaviour
{
    public float DegreesPerSecond = 360.0F;
    //외부에 보여질 변수
}
public class RotationSpeedBaker_IJobEntityBatch : Baker<RotationSpeedAuthoring_IJobEntityBatch>
{
    public override void Bake(RotationSpeedAuthoring_IJobEntityBatch authoring)
    {
        AddComponent(new RotationSpeed_IJobEntityBatch{RadiansPerSecond = math.radians(authoring.DegreesPerSecond)});
        //this.GetComponent<Unity.Transforms.LocalTransform>()
        //AddComponent(new RotationComponentData {rotation = Quaternion.identity});
    }

    //EntityManager : World에는 하나의 EntityManager가 존재 , Entity와 Component를 관리(생성,읽기,업데이트,파괴)
    //  기본 스레드에서만 작동

    //버전이 바뀌며 IConvertGameObjectToEntity 이 Baker으로 바뀐걸로 추정 
}

