using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayingClockUI : MonoBehaviour
{
    [SerializeField] private Transform clockHandTransform;
    [SerializeField] private TextMeshProUGUI timerText;
    private void Update()
    {
        // Lấy thời gian chơi game hiện tại
        float gameTime = KitchenGameManager.Instance.GetGamePlayingTime(); // phương thức này trả về thời gian còn lại
        float gameTimeMax = KitchenGameManager.Instance.GetGamePlayingTimeMax(); //phương thức này trả về thời gian tối đa

        // Cập nhật góc quay của kim đồng hồ
        float normalizedTime = gameTime / gameTimeMax;
        float rotationAngle = normalizedTime * 360f; // 360 độ cho một vòng đầy đủ
        clockHandTransform.eulerAngles = new Vector3(0, 0, rotationAngle); // Quay ngược chiều kim đồng hồ


        // Định dạng thời gian thành mm:ss
        int minutes = Mathf.FloorToInt(gameTime / 60);
        int seconds = Mathf.FloorToInt(gameTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
