using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    //[SerializeField] private MeshRenderer headMeshRerender;
    //[SerializeField] private MeshRenderer bodyMeshRerender;
    [SerializeField] private MeshRenderer apronMeshRerender;
    [SerializeField] private MeshRenderer pocketMeshRerender;

    private Material material;

    private void Awake()
    {
        material = new Material(apronMeshRerender.material);
        //headMeshRerender.material = material;
        //bodyMeshRerender.material = material;
        apronMeshRerender.material = material;
        pocketMeshRerender.material = material;

    }

    public void SetPlayerColor(Color color)
    {
        material.color = color;
    }
}
