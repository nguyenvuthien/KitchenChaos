using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public LayerMask wallLayer; // Layer chứa các bức tường
    public float checkInterval = 0.1f; // Thời gian giữa mỗi lần kiểm tra

    private Transform playerTransform;
    private List<Renderer> hiddenWalls = new List<Renderer>();

    private void Start()
    {
        StartCoroutine(FindAndAssignPlayer());
    }

    private IEnumerator FindAndAssignPlayer()
    {
        // Đợi cho đến khi Player được tạo
        yield return new WaitForSeconds(1.0f);

        // Chỉ thực hiện logic này trên client cục bộ
        if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClient != null)
        {
            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (localPlayer != null)
            {
                SetFollowTarget(localPlayer.transform);
                playerTransform = localPlayer.transform;
                StartCoroutine(CheckWalls());
            }
        }
    }

    private void SetFollowTarget(Transform target)
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = target;
        }
    }

    private IEnumerator CheckWalls()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            HideWallsBetweenCameraAndPlayer();
        }
    }

    private void HideWallsBetweenCameraAndPlayer()
    {
        if (playerTransform == null || virtualCamera == null) return;

        Vector3 cameraPosition = virtualCamera.transform.position;
        Vector3 playerPosition = playerTransform.position;
        Vector3 direction = playerPosition - cameraPosition;
        float distance = direction.magnitude;

        // Bỏ ẩn các tường đã ẩn trước đó
        foreach (var wall in hiddenWalls)
        {
            wall.enabled = true;
        }
        hiddenWalls.Clear();

        // Raycast để kiểm tra tường giữa camera và player
        RaycastHit[] hits = Physics.RaycastAll(cameraPosition, direction, distance, wallLayer);
        foreach (var hit in hits)
        {
            Renderer wallRenderer = hit.collider.GetComponent<Renderer>();
            if (wallRenderer != null)
            {
                wallRenderer.enabled = false;
                hiddenWalls.Add(wallRenderer);
            }
        }
    }
}
