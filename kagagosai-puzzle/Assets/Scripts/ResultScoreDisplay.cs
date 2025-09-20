using TMPro;
using UnityEngine;

public class ResultScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void Start()
    {
        // "FinalScore"という名前で保存されたスコアを読み込む（もしなければ0を読み込む）
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        scoreText.text = "スコア : " + finalScore.ToString();
    }
}