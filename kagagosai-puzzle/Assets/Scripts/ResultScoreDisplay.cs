using TMPro;
using UnityEngine;
using unityroom.Api;

public class ResultScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    
    [Header("ランキング設定")]
    public int boardId = 1;

    void Start()
    {
        // "FinalScore"という名前で保存されたスコアを読み込む（もしなければ0を読み込む）
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        scoreText.text = "スコア : " + finalScore.ToString();
        
        UnityroomApiClient.Instance.SendScore(boardId, finalScore, ScoreboardWriteMode.HighScoreDesc);
        
        Debug.Log("スコア送信を試みました: " + finalScore);
    }
}