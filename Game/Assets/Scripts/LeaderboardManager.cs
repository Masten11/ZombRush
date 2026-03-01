using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public float timeSeconds;     // score: time survived
    public string dateIso;        // optional

    public LeaderboardEntry(string name, float timeSeconds)
    {
        this.playerName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
        this.timeSeconds = timeSeconds;
        this.dateIso = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}

[Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Settings")]
    public int maxEntries = 10;

    private LeaderboardData data = new LeaderboardData();
    private string FilePath => Path.Combine(Application.persistentDataPath, "leaderboard.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();

        Debug.Log("LeaderboardManager is running. Path: " + Application.persistentDataPath);
    }

    public IReadOnlyList<LeaderboardEntry> GetEntries()
    {
        return data.entries;
    }

    public void SubmitScore(string playerName, float timeSeconds)
    {
        Load(); // keep safe if multiple scenes touch it

        data.entries.Add(new LeaderboardEntry(playerName, timeSeconds));

        // Sort: highest time first
        data.entries.Sort((a, b) => b.timeSeconds.CompareTo(a.timeSeconds));

        // Trim to top N
        if (data.entries.Count > maxEntries)
            data.entries.RemoveRange(maxEntries, data.entries.Count - maxEntries);

        Save();
    }

    public bool WouldEnterTop10(float timeSeconds)
    {
        Load();
        if (data.entries.Count < maxEntries) return true;
        return timeSeconds > data.entries[data.entries.Count - 1].timeSeconds;
    }

    public void ClearAll()
    {
        data = new LeaderboardData();
        Save();
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save leaderboard: {e}");
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                data = new LeaderboardData();
                return;
            }

            string json = File.ReadAllText(FilePath);
            data = JsonUtility.FromJson<LeaderboardData>(json) ?? new LeaderboardData();

            // Safety: ensure sorted/trimmed even if file edited
            data.entries.Sort((a, b) => b.timeSeconds.CompareTo(a.timeSeconds));
            if (data.entries.Count > maxEntries)
                data.entries.RemoveRange(maxEntries, data.entries.Count - maxEntries);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load leaderboard (will reset): {e}");
            data = new LeaderboardData();
        }
    }

    // Utility formatting
    public static string FormatTime(float t)
    {
        int minutes = (int)(t / 60f);
        int seconds = (int)(t % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}