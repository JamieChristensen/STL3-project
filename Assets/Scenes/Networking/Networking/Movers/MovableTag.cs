using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableTag : MonoBehaviour
{
    public bool isMoving;
    public Vector3 moveDir;
    private void LateUpdate()
    {
        if (isMoving)
        {
            isMoving = false;
            moveDir = Vector3.zero;
        }
    }
}
