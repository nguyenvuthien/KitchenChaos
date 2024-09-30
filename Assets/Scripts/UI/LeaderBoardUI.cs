using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private Transform leaderBoardPanel; // Panel chứa các mục bảng xếp hạng
    [SerializeField] private Transform entryTemplate; // Mẫu mục bảng xếp hạng

    private void Awake()
    {
        if (leaderBoardPanel == null)
        {
            Debug.LogError("Leaderboard Panel chưa được gán trong LeaderboardUI.");
        }

        if (entryTemplate == null)
        {
            Debug.LogError("Entry Template chưa được gán trong LeaderboardUI.");
        }
        else
        {
            entryTemplate.gameObject.SetActive(false); // Đảm bảo mẫu không active
        }
    }

    private void Start()
    {
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.OnLeaderboardUpdated += HandleLeaderboardUpdated;
            UpdateLeaderboardUI(LeaderboardManager.Instance.GetLeaderboard());
        }
        else
        {
            Debug.LogError("LeaderboardManager Instance không tồn tại.");
        }
    }

    private void OnDisable()
    {
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.OnLeaderboardUpdated -= HandleLeaderboardUpdated;
        }
    }

    // Xử lý khi bảng xếp hạng được cập nhật
    private void HandleLeaderboardUpdated(object sender, System.EventArgs e)
    {
        UpdateLeaderboardUI(LeaderboardManager.Instance.GetLeaderboard());
    }

    // Hàm cập nhật UI bảng xếp hạng
    public void UpdateLeaderboardUI(List<LeaderboardEntry> leaderboardEntries)
    {
        // Xóa các mục cũ trong bảng xếp hạng (ngoại trừ mẫu)
        foreach (Transform child in leaderBoardPanel)
        {
            if (child != entryTemplate)
            {
                Destroy(child.gameObject);
            }
        }

        // Tạo các mục mới dựa trên dữ liệu bảng xếp hạng
        foreach (LeaderboardEntry entry in leaderboardEntries)
        {
            Transform entryTransform = Instantiate(entryTemplate, leaderBoardPanel);
            entryTransform.gameObject.SetActive(true); // Kích hoạt mục mới

            // Giả sử entryTemplate có hai con: PlayerNameText và ScoreText
            // Thứ tự này phụ thuộc vào cấu trúc của bạn
            TextMeshProUGUI playerNameText = entryTransform.GetChild(0).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = entryTransform.GetChild(1).GetComponent<TextMeshProUGUI>();

            if (playerNameText != null)
            {
                playerNameText.text = entry.playerName;
            }
            else
            {
                Debug.LogError("Không tìm thấy TextMeshProUGUI cho PlayerName trong EntryTemplate.");
            }

            if (scoreText != null)
            {
                scoreText.text = entry.score.ToString();
            }
            else
            {
                Debug.LogError("Không tìm thấy TextMeshProUGUI cho Score trong EntryTemplate.");
            }
        }
    }
}
