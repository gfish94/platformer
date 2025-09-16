using UnityEngine;
using System.Collections.Generic;

public class HighscoreManager : MonoBehaviour
{
    private const int MaxHighscores = 5;
    private const string HighscoreKey = "Highscore";

    public static void AddScore(int score)
    {
        List<int> scores = GetHighscores();
        scores.Add(score);
        scores.Sort((a, b) => b.CompareTo(a)); // Descending
        if (scores.Count > MaxHighscores)
            scores.RemoveAt(scores.Count - 1);

        for (int i = 0; i < scores.Count; i++)
            PlayerPrefs.SetInt($"{HighscoreKey}{i}", scores[i]);
        PlayerPrefs.Save();
    }

    public static List<int> GetHighscores()
    {
        List<int> scores = new List<int>();
        for (int i = 0; i < MaxHighscores; i++)
        {
            if (PlayerPrefs.HasKey($"{HighscoreKey}{i}"))
                scores.Add(PlayerPrefs.GetInt($"{HighscoreKey}{i}"));
        }
        return scores;
    }
}