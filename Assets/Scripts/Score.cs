using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public Player player;
    public TMP_Text scoreText;

    void Update()
    {
        scoreText.text = player.currScore.ToString();
    }
}
