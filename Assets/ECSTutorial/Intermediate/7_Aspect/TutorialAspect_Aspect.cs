using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public readonly partial struct TutorialAspect_Aspect : IAspect
{
    //public readonly Entity entity;

    //private readonly TransformAspect _transformAspect;
    private readonly RefRW<TutorialAspectComponent> CAspect;
    //private readonly RefRO<TutorialAspectComponent> CAspect;// 읽기전용 일때

    // 참조된 컨포넌트가 있으면 자동으로 추가

    public float Value
    {
        get => CAspect.ValueRO.value;
        set => CAspect.ValueRW.value = value;
    }

    public string Combine()
    {
        return CAspect.ValueRO.value + " : " + CAspect.ValueRO.text.ToString();
    }
}
