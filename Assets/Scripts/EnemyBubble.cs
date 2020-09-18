using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class EnemyBubble : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;
    [SerializeField]
    private Sprite[] sprites;

    private float timer = 0;
    private float maxDuration = 0;

    [SerializeField]
    private Image image;


    void Start()
    {
        if (sceneCamera == null)
        {
            sceneCamera = FindObjectOfType<Camera>();
        }
    }
    void Update()
    {
        transform.LookAt(sceneCamera.transform, Vector3.up); //
        if (timer < maxDuration)
        {
            timer += Time.deltaTime;
            image.enabled = true;
        }
        else
        {
            image.enabled = false;
        }
    }

    public void ActivateBubble(EnemyController.EnemyState state, float duration)
    {
        switch (state)
        {
            case EnemyController.EnemyState.Patrolling:
                maxDuration = duration;
                timer = 0;
                image.sprite = sprites[0];
                break;
            case EnemyController.EnemyState.Seeking:
                maxDuration = duration;
                timer = 0;
                image.sprite = sprites[1];
                break;
            case EnemyController.EnemyState.FightingPlayer:
                maxDuration = duration;
                timer = 0;
                image.sprite = sprites[2];
                break;

        }
    }
}
