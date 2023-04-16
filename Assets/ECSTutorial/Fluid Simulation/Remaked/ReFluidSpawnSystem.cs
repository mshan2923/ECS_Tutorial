using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace FluidSimulate
{
    public partial class ReFluidSpawnSystem : SystemBase
    {
        BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithName("RemakedFluidSimlation")
                .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
                .ForEach((Entity entity, int entityInQueryIndex, in RemakedFluidSpawnComponent manager, in LocalTransform trans) =>
                {
                    Debug.Log("Spawning");

                    var random = new Random(1);
                    for (int i = 0; i < manager.Amount; i++)
                    {
                    // entity ���� ����
                    var instance = commandBuffer.Instantiate(entityInQueryIndex, manager.particle);

                        var position = new float3((i % 16) * 1.2f + random.NextFloat(-0.1f, 0.1f) * manager.Between,
                         (i / 16 / 16) * 1.1f * manager.Between,
                          ((i / 16) % 16) * 1.2f + random.NextFloat(-0.1f, 0.1f)) * manager.Between + trans.Position;


                        commandBuffer
                    .SetComponent(entityInQueryIndex, instance,
                        new LocalTransform { Position = position, Rotation = quaternion.identity, Scale = 1 });

                    //========= (SHPManager.AddCollider�κ�)���忡 ��� SPHCollider�� ���� ������ ������Ʈ �߰�
                    //========= �ϴ°ɷ� ������ , ���� ��ǻƮ ���̴��� ����ȿ�� ���°� ó��
                    //  ==> ��ƼŬ�� �����Ǳ����� ���忡 �ִ� SPHCollider ��ũ�� ���� ������Ʈ�� ��Ƽ�
                    //   => SPHColliderComponent�� �߰� , �� �ڼ��Ѱ� SHPSystem �����ϰ�
                }

                // ���� Entity�� �����Ѵ�. �������� ������ onUpdate�� ���� Cube�� ��� �����ȴ�.
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                }).ScheduleParallel();

            // ���� �۾��� �Ϸ�Ǹ�
            // ������ ����� ���� �����ӿ��� BeginInitializationEntityCommandBufferSystem ȣ��ɶ� ó���϶�� ����Ѵ�.
            entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }

}