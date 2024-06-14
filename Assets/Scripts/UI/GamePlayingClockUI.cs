using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayingClockUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void Update()
    {
        // Lấy thời gian chơi game hiện tại
        float gameTime = KitchenGameManager.Instance.GetGamePlayingTime();

        // Định dạng thời gian thành mm:ss
        int minutes = Mathf.FloorToInt(gameTime / 60);
        int seconds = Mathf.FloorToInt(gameTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
