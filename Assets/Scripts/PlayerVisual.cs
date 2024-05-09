using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer headMeshRerender;
    [SerializeField] private MeshRenderer bodyMeshRerender;

    private Material material;

    private void Awake()
    {
        material = new Material(headMeshRerender.material);
        headMeshRerender.material = material;
        bodyMeshRerender.material = material;
    }

    public void SetPlayerColor(Color color)
    {
        material.color = color;
    }
}
