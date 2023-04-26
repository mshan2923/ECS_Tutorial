using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public partial class TutorialAspectSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        foreach(var asp  in SystemAPI.Query<TutorialAspect_Aspect>())
        {
            Debug.Log(asp.Combine());
        }
    }
    protected override void OnUpdate()
    {
        
        Enabled = false;
    }
}
