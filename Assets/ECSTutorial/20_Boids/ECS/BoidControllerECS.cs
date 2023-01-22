using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class BoidControllerECS : MonoBehaviour
{
    public static BoidControllerECS Instance;
    public int boidAmount = 1000;
    public Mesh sharedMesh;
    public Material sharedMaterial;

    public GameObject Prefab;
    public float OffsetScale = 1f;
    public float boidSpeed;
    public float boidPerceptionRadius;
    public float cageSize;

    public float separationWeight;
    public float cohesionWeight;
    public float alignmentWeight;

    public float avoidWallsWeight;
    public float avoidWallsTurnDist;

    //FIXME - 컴포넌트로 바꾸고 , 싱글톤으로 값 접근하게 , boid를 EntityArchetype으로 샌성X

    //https://github.com/BadGraphixD/How-many-Boids-can-Unity-handle/blob/master/Assets/Scenes/2)%20ECS/BoidControllerECS.cs

    void Start()
    {
        
        if (Instance == null)
            Instance = this;
        /*
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityArchetype boidArchetype = entityManager.CreateArchetype(
            typeof(BoidECS), typeof(RenderMesh),
            typeof(RenderBounds), typeof(LocalToWorld)
        );

        if (SystemAPI.TryGetSingleton<BoidControllerECSConponent>(out var ControllerData) == false)
        {
            Debug.Log("Can't Get Singleton");
        }
        //entityManager.GetComponentData<BoidControllerECSConponent>();

        NativeArray<Entity> boidArray = new NativeArray<Entity>(boidAmount, Allocator.TempJob);
        //entityManager.CreateEntity(boidArchetype, boidArray);
        entityManager.Instantiate(ControllerData.prefab, boidArray);

        for (int i = 0; i < boidArray.Length; i++) 
        {
            Unity.Mathematics.Random rand = new Unity.Mathematics.Random((uint)i + 1);
            entityManager.SetComponentData(boidArray[i], new LocalToWorld {
                Value = float4x4.TRS(
                    RandomPosition(),
                    RandomRotation(),
                    new float3(1f))
            });
            
            entityManager.SetSharedComponentManaged(boidArray[i], new RenderMesh {
                mesh = sharedMesh,
                material = sharedMaterial,
            });


        }

        boidArray.Dispose();
                    Debug.Log("--");
        */
    }

        private float3 RandomPosition() {
        return new float3(
            UnityEngine.Random.Range(-cageSize / 2f, cageSize / 2f),
            UnityEngine.Random.Range(-cageSize / 2f, cageSize / 2f),
            UnityEngine.Random.Range(-cageSize / 2f, cageSize / 2f)
        );
    }
    private quaternion RandomRotation() {
        return quaternion.Euler(
            UnityEngine.Random.Range(-360f, 360f),
            UnityEngine.Random.Range(-360f, 360f),
            UnityEngine.Random.Range(-360f, 360f)
        );
    }
        private void OnDrawGizmos() {

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(
                cageSize,
                cageSize,
                cageSize
            )
        );
    }
}

public struct BoidControllerECSConponent : IComponentData
{
    public Entity prefab;
    public float offsetScale;
    public int boidAmount;
    public float boidSpeed;
    public float boidPerceptionRadius;
    public float cageSize;

    public float separationWeight;
    public float cohesionWeight;
    public float alignmentWeight;

    public float avoidWallsWeight;
    public float avoidWallsTurnDist;
}

public class BoidControllerECSBaker : Baker<BoidControllerECS>
{
    public override void Bake(BoidControllerECS authoring)
    {
        AddComponent(new BoidControllerECSConponent
        {
            prefab = GetEntity(authoring.Prefab),
            offsetScale = authoring.OffsetScale,
            boidAmount = authoring.boidAmount,
            boidSpeed = authoring.boidSpeed,
            boidPerceptionRadius = authoring.boidPerceptionRadius,
            cageSize = authoring.cageSize,

            separationWeight = authoring.separationWeight,
            cohesionWeight = authoring.cohesionWeight,
            alignmentWeight = authoring.alignmentWeight,

            avoidWallsWeight = authoring.avoidWallsWeight,
            avoidWallsTurnDist = authoring.avoidWallsTurnDist
        });
    }
}