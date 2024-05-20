using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;

    private void Start()
    {
        // Đợi một khoảng thời gian để đảm bảo Player đã được tạo
        StartCoroutine(FindAndAssignPlayer());
    }

    private IEnumerator FindAndAssignPlayer()
    {
        // Đợi cho đến khi Player được tạo
        yield return new WaitForSeconds(1.0f);

        // Tìm tất cả các đối tượng Player
        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            var networkObject = player.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsLocalPlayer)
            {
                SetFollowTarget(player.transform);
                break;
            }
        }
    }

    private void SetFollowTarget(Transform target)
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = target;
            //virtualCamera.LookAt = target;
        }
    }
}
