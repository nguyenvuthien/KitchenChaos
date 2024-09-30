using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.IO;
using System.Linq;

public class LeaderboardManager : NetworkBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    // Thêm sự kiện để thông báo khi bảng xếp hạng được cập nhật
    public event EventHandler OnLeaderboardUpdated;

    private List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>();
    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            saveFilePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");
            DontDestroyOnLoad(gameObject);
            LoadLeaderboard();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Gọi khi có điểm cao mới
    public void AddNewScore(string playerName, int score)
    {
        AddNewScoreServerRpc(playerName, score);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddNewScoreServerRpc(string playerName, int score)
    {
        // Thêm điểm mới vào danh sách
        leaderboardEntries.Add(new LeaderboardEntry(playerName, score));
        // Sắp xếp danh sách theo điểm giảm dần
        leaderboardEntries = leaderboardEntries.OrderByDescending(entry => entry.score).ToList();
        // Giữ lại chỉ top 10
        if (leaderboardEntries.Count > 10)
        {
            leaderboardEntries.RemoveAt(leaderboardEntries.Count - 1);
        }
        // Lưu danh sách vào file
        SaveLeaderboard();
        // Gửi cập nhật đến tất cả client
        UpdateLeaderboardClientRpc(leaderboardEntries.Select(entry => new SerializableLeaderboardEntry(entry)).ToArray());

        // Kích hoạt sự kiện
        OnLeaderboardUpdated?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void UpdateLeaderboardClientRpc(SerializableLeaderboardEntry[] updatedEntries)
    {
        leaderboardEntries = updatedEntries.Select(se => se.ToLeaderboardEntry()).ToList();
        // Kích hoạt sự kiện
        OnLeaderboardUpdated?.Invoke(this, EventArgs.Empty);
    }

    // Gửi bảng xếp hạng hiện tại đến client mới kết nối
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UpdateLeaderboardClientRpc(leaderboardEntries.Select(entry => new SerializableLeaderboardEntry(entry)).ToArray());
        }
    }

    // Lưu bảng xếp hạng vào file
    private void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(new LeaderboardWrapper { entries = leaderboardEntries });
        File.WriteAllText(saveFilePath, json);
    }

    // Tải bảng xếp hạng từ file
    private void LoadLeaderboard()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            LeaderboardWrapper wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
            if (wrapper != null && wrapper.entries != null)
            {
                leaderboardEntries = wrapper.entries;
            }
        }
    }

    // Lớp để tiện lưu trữ
    [System.Serializable]
    private class LeaderboardWrapper
    {
        public List<LeaderboardEntry> entries;
    }

    // Để truyền qua ClientRpc
    [System.Serializable]
    private class SerializableLeaderboardEntry : INetworkSerializable
    {
        public string playerName;
        public int score;

        public SerializableLeaderboardEntry() { }

        public SerializableLeaderboardEntry(LeaderboardEntry entry)
        {
            playerName = entry.playerName;
            score = entry.score;
        }

        public LeaderboardEntry ToLeaderboardEntry()
        {
            return new LeaderboardEntry(playerName, score);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref score);
        }
    }

    // Cung cấp phương thức để lấy bảng xếp hạng
    public List<LeaderboardEntry> GetLeaderboard()
    {
        return leaderboardEntries;
    }
}
