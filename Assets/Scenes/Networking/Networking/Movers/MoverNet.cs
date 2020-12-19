using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MoverNet : NetworkBehaviour
{
    public float moveSpeed;
    public Vector3 direction;


    [Server]
    private void OnTriggerStay(Collider other)
    {
        if (!isServer)
        {
            return;
        }
        Move(other);
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
        {
            return;
        }
        Move(other);
    }

    [Server]
    private void Move(Collider other)
    {
        var move = direction;

        var root = other.transform.root;


        if (root.GetComponent<MovableTag>() == null)
        {
            return;
        }
        var movable = root.GetComponent<MovableTag>();

        if (movable.moveDir.magnitude > 1)
        {
            return;
        }

        if (movable.isMoving)
        {
            if (Vector3.Cross(movable.moveDir, direction).magnitude == 0)
            {
                var movX = Mathf.Abs(movable.moveDir.x);
                var movZ = Mathf.Abs(movable.moveDir.z);

                bool shouldReturn = movX >= 1 && movZ >= 1 ? true : false;

                if (shouldReturn)
                {
                    return;
                }
            }

        }

        root.GetComponent<MovableTag>().isMoving = true;

        //Check if root is 


        if (root.CompareTag("Player"))
        {
            if (other.transform.parent != null)
            {
                return;
            }
            root.GetComponent<PlayerNetworked>().RpcMove(move);
            movable.moveDir += direction;
            return;
        }



        if (other.GetComponent<Rigidbody>() != null)
        {
            var rb = other.GetComponent<Rigidbody>();
            //rb.velocity = rb.velocity - (rb.velocity * 0.9f * Time.deltaTime);
            movable.moveDir += direction;
            rb.AddForce(move*moveSpeed *0.02f, ForceMode.VelocityChange);
            return;
        }

        root.Translate(move * Time.deltaTime * moveSpeed, Space.World);

    }

}
