using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public partial struct MovingISystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<RandomComponent>())//직접추가
        {
            RefRW<RandomComponent> randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();
            //SystemAPI.GetSingleton<>을 쓰면 값변경 X

            float deltaTime = SystemAPI.Time.DeltaTime;
            JobHandle jobHandle = new MoveJob 
            {
                deltaTime = deltaTime
            }.ScheduleParallel(state.Dependency);//MoveJob에 partial 이 있어야 ScheduleParallel 사용가능

            jobHandle.Complete();

            new TestReachedTargetPositionJob 
            {
                randomComponent = randomComponent
            }.Run();//.ScheduleParallel();

            Debug.LogWarning("Error is Here");
            //================================== ScheduleParallel , Run하며 에러하나씩 발생 
        }
    }
}
[BurstCompile]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]//에러메세지 제거 (partial 순서)
public partial struct MoveJob : IJobEntity
{
    public float deltaTime;

    [BurstCompile]
    public void Execute(MoveToPositionAspect moveToPositionAspect)
    {
        moveToPositionAspect.Move(SystemAPI.Time.DeltaTime);
    }
}
[BurstCompile]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]//에러메세지 제거 (partial 순서)
public partial struct TestReachedTargetPositionJob : IJobEntity
{
    [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
     public RefRW<RandomComponent> randomComponent;

     [BurstCompile]
    public void Execute(MoveToPositionAspect moveToPositionAspect)
    {
        moveToPositionAspect.TestReachedTargetPosition(randomComponent);
    }
}