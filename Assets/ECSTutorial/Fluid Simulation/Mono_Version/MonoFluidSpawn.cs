using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class MonoFluidSpawn : MonoBehaviour
{
    public int Amount = 1000;
    public GameObject prefab;

    void Start()
    {
        var random = new Unity.Mathematics.Random(1);

        for(int i = 0; i < Amount; i++)
        {
            var position = new float3(i % 16 + random.NextFloat(-0.1f, 0.1f),
                2 + (i / 16 / 16) * 1.1f,
                (i / 16) % 16) + random.NextFloat(-0.1f, 0.1f);

            GameObject.Instantiate(prefab, position, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
