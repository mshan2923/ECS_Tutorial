using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;//필드 캡슐화하면 유니티 꺼짐...??
    
    void OnEnable()
    {
        if (instance == null)
            instance = this;
        
    }
}
