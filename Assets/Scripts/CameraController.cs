using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerController player;

    public Vector3 offset;

    private Vector3 currentVel;
    [SerializeField]
    private float smoothVel;

    [Range(1, 10)]
    [Tooltip("How much is the mouse-direction vector scaled?")]
    [SerializeField]
    private float mouseFactor;

    private void Awake()
    {
        Time.fixedDeltaTime = 0.02f * 0.33f;
        
        FindPlayer();
    }

    //Should be LOCAL player.
    private void FindPlayer()
    {
        player = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
        }
        if (player == null)
        {
            return;
        }


        Vector3 directionToMouse = (player.mousePos - player.transform.position);
        directionToMouse = new Vector3(directionToMouse.x, 0, directionToMouse.z).normalized;
        float lookFactor = Mathf.Min(Vector3.Distance(directionToMouse, player.transform.position), mouseFactor);
        Vector3 dirOffset = offset + (directionToMouse * lookFactor);
        Vector3 target = dirOffset + player.transform.position;

        transform.position = Vector3.SmoothDamp(transform.position, target, ref currentVel, smoothVel, 20f, Time.deltaTime);

        //transform.position = dirOffset + player.transform.position;
    }
}
