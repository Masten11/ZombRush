using UnityEngine;
using UnityEngine.UI; // Change to 'using TMPro;' if you use TextMeshPro
using System.Collections.Generic;
using TMPro;

public class LeaderboardDisplay : MonoBehaviour
{
    [Header("Top 5 Text UI")]
    // We use an array so you can drag exactly 5 text elements into the Inspector
public TextMeshProUGUI[] topFiveTexts; 

    void Start()
    {
        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        // 1. Clear out the text first (in case there are fewer than 5 scores saved)
        foreach (TextMeshProUGUI txt in topFiveTexts)
        {
            txt.text = "---"; 
        }

        // 2. Make sure the Manager exists in the scene
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogWarning("LeaderboardManager not found in scene!");
            return;
        }

        // 3. Get the sorted entries from your existing script
        IReadOnlyList<LeaderboardEntry> entries = LeaderboardManager.Instance.GetEntries();

        // 4. Figure out how many to show (Max 5, or however many UI slots we have)
        int maxToShow = Mathf.Min(5, entries.Count);
        maxToShow = Mathf.Min(maxToShow, topFiveTexts.Length);

        // 5. Loop through and update the text
        for (int i = 0; i < maxToShow; i++)
        {
            LeaderboardEntry entry = entries[i];
            
            // Use the handy FormatTime method you already wrote!
            string formattedTime = LeaderboardManager.FormatTime(entry.timeSeconds);

            // Output looks like: "1. PlayerName - 02:45"
            topFiveTexts[i].text = $"{i + 1}. {entry.playerName} - {formattedTime}";
        }
    }
}