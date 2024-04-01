using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[ExecuteAlways]
public class BoxCollisionTest : MonoBehaviour
{
    public UnityEngine.SphereCollider particle;
    public UnityEngine.BoxCollider Box;

    public bool isCollision;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 worldSize = new Vector3(
            Box.size.x * Box.transform.localScale.x,
            Box.size.y * Box.transform.localScale.y,
            Box.size.z * Box.transform.localScale.z
            );
        float radius = Mathf.Max(particle.transform.localScale.x, particle.transform.localScale.y, particle.transform.localScale.z) * 0.5f;
        isCollision = true;

        var maxPoint = Box.transform.up * worldSize.y 
            + Box.transform.right * worldSize.x
            + Box.transform.forward * worldSize.z;
        maxPoint *= 0.5f;

        Debug.DrawLine(Box.transform.position, maxPoint, Color.black, Time.deltaTime);

        var ostProjectY = math.project(Box.transform.position, Box.transform.up);
        var ostAreaProjectY = math.project(Box.transform.position + maxPoint, Box.transform.up);
        var particleProjectY = math.project(particle.transform.position, Box.transform.up);
        var projectXDis = math.distance(particleProjectY, ostProjectY);


        Debug.DrawLine(particleProjectY, ostProjectY, Color.green, Time.deltaTime);
        Debug.DrawLine(ostAreaProjectY + new float3(1, 0, 0) * 0.05f, ostProjectY + new float3(1, 0, 0) * 0.05f, Color.red, Time.deltaTime);

        Debug.Log($"{projectXDis} - {radius} >= {math.distance(ostAreaProjectY, ostProjectY)}");
        if (projectXDis - radius >= math.distance(ostAreaProjectY, ostProjectY))
        {
            isCollision = false;
            return;
        }else
        {

        }


    }
}
