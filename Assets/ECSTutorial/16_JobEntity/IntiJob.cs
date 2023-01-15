using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;

namespace Tutorial.JobEntity
{
    public partial class IntiJob : SystemBase
    {
        [BurstCompile]
        public partial struct SetupJob : IJobEntity
        {
            public Unity.Mathematics.Random random;
            public EntityCommandBuffer.ParallelWriter ECB;
            public void Execute(Entity e, [EntityIndexInQuery] int sortKey)
            {
                ECB.AddComponent<RotationSpeedComponent>(sortKey, e, new RotationSpeedComponent{speed = random.NextFloat(-1, 1)});
            }
        }

        protected override void OnStartRunning()
        {
            {
                this.Enabled = false;
                return;
            }

            var ecbSystem = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>();

            var setup = new SetupJob
            {
                random = new Unity.Mathematics.Random(64562),
                ECB = ecbSystem.CreateCommandBuffer().AsParallelWriter()
            };
            setup.ScheduleParallel();
        }
        protected override void OnUpdate()
        {
        
        }
    }

}