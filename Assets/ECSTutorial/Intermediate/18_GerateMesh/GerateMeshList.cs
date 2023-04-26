using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Tutorial.GerateMesh
{
    public class GerateMeshList : MonoBehaviour
    {
        public static GerateMeshList instance {get; private set;}
        public Mesh[] mesh;
        public Material DefaultMat;
        

        public void OnEnable()
        {
            if (instance == null)
                instance = this;
        }
    }

}
