using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TutorialAspect : MonoBehaviour
{
    public float Value;
    public string Text;
}

public struct TutorialAspectComponent : IComponentData
{
    public float value;
    public Unity.Collections.FixedString32Bytes text;
}
public class TutorialAspectBaker : Baker<TutorialAspect>
{
    public override void Bake(TutorialAspect authoring)
    {
        AddComponent(new TutorialAspectComponent
        {
            value = authoring.Value,
             text = authoring.Text
        });
    }
}
