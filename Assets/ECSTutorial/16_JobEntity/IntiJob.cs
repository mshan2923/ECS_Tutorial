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
            public void Execute(Entity e, [EntityIndexInQuery] int sortKey, ref RotationSpeedComponent rotSpeed)
            {
                //random = new Unity.Mathematics.Random((uint)sortKey + 15446);
                //rotSpeed.speed = random.NextFloat(-1, 1);//NOTE - 유니티 크래쉬
                ECB.SetComponent<RotationSpeedComponent>(sortKey, e, new RotationSpeedComponent{speed = random.NextFloat(-1, 1)});
                //NOTE - SetComponent 하고 ref 하면 크래쉬?
            }
        }

        protected override void OnStartRunning()
        {

            var setup = new SetupJob
            {
                random = new Unity.Mathematics.Random(64562),
                ECB = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>().CreateCommandBuffer().AsParallelWriter()
            };
            setup.ScheduleParallel();
        }
        protected override void OnUpdate()
        {
        
        }
    }

}