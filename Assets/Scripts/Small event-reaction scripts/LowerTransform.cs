using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowerTransform : MonoBehaviour
{
    [SerializeField]
    private float moveAmount, loweringSpeed;

    [SerializeField]
    private Vector3 moveDirection;
    private bool isActivated = false;

    private void Update()
    {
        if (isActivated && moveAmount > 0)
        {
            transform.Translate(moveDirection * Time.deltaTime * loweringSpeed);
            moveAmount = moveAmount - (Time.deltaTime * loweringSpeed);
        }
    }

    public void ActivateLowering()
    {
        isActivated = true;
    }
}
