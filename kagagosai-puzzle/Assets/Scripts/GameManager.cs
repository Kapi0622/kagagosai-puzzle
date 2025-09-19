using System.Collections.Generic;
using TMPro; // TextMeshProを扱うために必要
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // 他のスクリプトから簡単にアクセスできるようにする（シングルトン）

    [Header("ゲーム設定")]
    public float timeLimit = 180f; // 制限時間

    [Header("UI関連")]
    public TextMeshProUGUI timeText; // 時間表示用テキスト
    public TextMeshProUGUI scoreText; // スコア表示用テキスト

    [Header("パズル関連")]
    public List<TargetController> currentTargets; // 現在のステージのターゲットリスト
    public GameObject piecePrefab; // ピースのPrefab
    public Transform piecePanel; // ピースを生成する場所

    private float currentTime;
    private int score;
    private int placedPieces;
    
    public static int finalScore;

    void Awake()
    {
        // シングルトンの設定
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartNewGame();
    }

    void Update()
    {
        // 時間の更新
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            currentTime = 0;
            UpdateTimerUI(); // 時間を0に更新

            finalScore = score; // スコアを記録
            SceneManager.LoadScene("ResultScene"); 
        }
    }
    
    void StartNewGame()
    {
        score = 0;
        currentTime = timeLimit;
        placedPieces = 0;
        UpdateScoreUI();
        SpawnPiecesForCurrentTargets();
    }

    void SpawnPiecesForCurrentTargets()
    {
        // 現在設定されているターゲットに対応するピースを生成する
        foreach (TargetController target in currentTargets)
        {
            GameObject newPiece = Instantiate(piecePrefab, piecePanel);
            PieceController pieceController = newPiece.GetComponent<PieceController>();
            
            // ピースに自分の形を教える
            pieceController.shapeType = target.shapeType;

            // ピースの見た目（スプライト）を変更する ※ここ重要！
            // Imageコンポーネントを取得し、スプライトをResourcesフォルダから読み込んで設定します
            UnityEngine.UI.Image pieceImage = newPiece.GetComponent<UnityEngine.UI.Image>();
            pieceImage.sprite = Resources.Load<Sprite>(target.shapeType);
        }
    }

    public void PiecePlacedCorrectly()
    {
        score += 100;
        placedPieces++;
        UpdateScoreUI();

        // 全てのピースが置かれたかチェック
        if (placedPieces >= currentTargets.Count)
        {
            Debug.Log("ステージクリア！");
            // ここに次のステージに進む処理などを書く
        }
    }

    void UpdateTimerUI()
    {
        timeText.text = "時間 : " + Mathf.Ceil(currentTime).ToString();
    }
    void UpdateScoreUI()
    {
        scoreText.text = "スコア : " + score.ToString();
    }
    
}