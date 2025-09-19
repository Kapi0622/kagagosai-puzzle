using TMPro;
using UnityEngine;

public class ResultScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void Start()
    {
        scoreText.text = "スコア : " + GameManager.finalScore.ToString();
    }
}