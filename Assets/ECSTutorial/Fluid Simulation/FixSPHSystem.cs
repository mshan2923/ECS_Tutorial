using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
//using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst.Intrinsics;

partial struct TempJob : IJobEntity//IJobEntity 을 사용하면 참조 접근 
    {
        //[NativeDisableParallelForRestriction]
        
        //public NativeArray<Entity> Entities;


        //void IJobParallelFor.Execute(int index)
        //{
        //    transform[index].Translate(new float3(0.1f, 0, 0));
        //}

        void Execute(ref LocalTransform trans)
        {
            trans.Position += new float3(0.01f, 0, 0);
        }
    }
    struct Temp2Job : IJobParallelFor
    {
        public NativeArray<LocalTransform> transform;
            //[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
            //public RefRW<RandomComponent> randomComponent;

        public void Execute(int index)
        {
            //transform[index].Position+= new float3(0.01f, 0, 0);
            var trans = transform[index];
            trans.Position += new float3(0, 0.01f, 0);
            
            transform[index] = trans;
        }
    }
struct Temp3Job : IJob
{
    //=============  Temp2Job 에서 변경된 값을 적용시키기 위해 , 참조된 배열을 가져와야 되는데
    // ======= IJobEntity 이 되긴한데 index가 없으니... , 전엔 방법 있어는데 지금은 무조건 .. IJobEntity

    public void Execute()
    {
        throw new System.NotImplementedException();
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    [BurstCompile]
public partial class FixSPHSystem : SystemBase
{
    private EntityQuery SPHCharacterGroup;

    private static readonly int[] cellOffsetTable =
    {
        1, 1, 1, 1, 1, 0, 1, 1, -1, 1, 0, 1, 1, 0, 0, 1, 0, -1, 1, -1, 1, 1, -1, 0, 1, -1, -1,
        0, 1, 1, 0, 1, 0, 0, 1, -1, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, -1, 1, 0, -1, 0, 0, -1, -1,
        -1, 1, 1, -1, 1, 0, -1, 1, -1, -1, 0, 1, -1, 0, 0, -1, 0, -1, -1, -1, 1, -1, -1, 0, -1, -1, -1
    };

    protected override void OnCreate()
    {
        this.Enabled = false;
        //<SPHParticleComponent, SPHVelocityComponent>
        SPHCharacterGroup = GetEntityQuery
        (
            ComponentType.ReadOnly(typeof(SPHParticleComponent)),
            typeof(LocalTransform), typeof(SPHVelocityComponent)
        );
    }

    protected override void OnUpdate()
    {
        JobHandle particlesPositionJobHandle;
        //NativeArray<LocalTransform> particlePosition 
        //    = SPHCharacterGroup.ToComponentDataListAsync<LocalTransform>(Allocator.TempJob, out particlesPositionJobHandle).AsArray();
        NativeArray<LocalTransform> particlePosition = SPHCharacterGroup.ToComponentDataArray<LocalTransform>(Allocator.TempJob);//, out particlesPositionJobHandle
        var tempJob = new TempJob {        };
        tempJob.ScheduleParallel(SPHCharacterGroup);
        //var TempHandle = tempJob.ScheduleParallel(SPHCharacterGroup);//particlesPositionJobHandle
        //TempHandle.Complete();

        Debug.Log("Finded : " + SPHCharacterGroup.CalculateEntityCount() + " - " + particlePosition.Length);

        //particlePosition.Dispose();

        var temp2Job = new Temp2Job
        {
            transform = particlePosition
        };
        temp2Job.ScheduleByRef(particlePosition.Length, 1);
        //temp2Job.transform


        Job.WithCode(() =>
        {
            for(int i = 0; i < particlePosition.Length; i++)
            {
                
            }
        }).Schedule();

        Entities
            .WithName("SpawnerSystem_SpawnAndRemove")
            .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
            .ForEach((Entity entity, int entityInQueryIndex) => 
            {

            }).ScheduleParallel();
            

    }
}
