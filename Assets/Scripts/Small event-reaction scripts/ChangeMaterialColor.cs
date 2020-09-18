using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaterialColor : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    [ColorUsage(true, true)]
    private Color colorToChangeTo;

    public void ChangeColor()
    {
        meshRenderer.material.color = colorToChangeTo;
    }
}
