using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // シーン管理に必要

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("ゲーム設定")]
    public float timeLimit = 180f;

    [Header("UI関連")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;

    [Header("パズル自動生成 設定")]
    public Transform puzzleBoardCenter;
    public float puzzleBoardRadius;
    public List<GameObject> allShapeTargetPrefabs; // 全種類のターゲットPrefab（アンロック順）
    public GameObject piecePrefab;
    public Transform piecePanel;

    // --- プライベート変数 ---
    private float currentTime;
    private int score;
    private int stageCount = 0;
    private int pieceCount = 5;
    private int maxPieceCount = 8;
    private List<GameObject> unlockedShapeTargetPrefabs = new List<GameObject>();
    private List<GameObject> currentSpawnedTargets = new List<GameObject>();
    private List<GameObject> currentSpawnedPieces = new List<GameObject>();
    private int placedPieces;
    
    // 加賀五彩の色リスト（ウェブカラーコードで指定）
    private List<Color> kagaGosaiColors = new List<Color>();

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }

        // 色リストを初期化
        // ColorUtility.TryParseHtmlString("#FF0000", out Color newColor) のようにして色を変換
        ColorUtility.TryParseHtmlString("#4B0082", out Color ai);      // 藍 (インディゴ)
        ColorUtility.TryParseHtmlString("#DC143C", out Color enji);     // 臙脂 (クリムゾン)
        ColorUtility.TryParseHtmlString("#228B22", out Color kusa);     // 草 (フォレストグリーン)
        ColorUtility.TryParseHtmlString("#DAA520", out Color odo);      // 黄土 (ゴールデンロッド)
        ColorUtility.TryParseHtmlString("#483D8B", out Color kodaimurasaki); // 古代紫 (ダークスレートブルー)

        kagaGosaiColors.Add(ai);
        kagaGosaiColors.Add(enji);
        kagaGosaiColors.Add(kusa);
        kagaGosaiColors.Add(odo);
        kagaGosaiColors.Add(kodaimurasaki);
    }

    void Start()
    {
        Debug.Log("GameManager Start: ゲームを開始します。");
        StartNewGame();
    }

    void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            // 時間切れの処理
            currentTime = 0;
            UpdateTimerUI();

            // "FinalScore"という名前でスコアを保存
            PlayerPrefs.SetInt("FinalScore", score);
            PlayerPrefs.Save(); // 念のため保存を確定

            // リザルト画面へ
            SceneManager.LoadScene("ResultScene");
        }
    }

    void StartNewGame()
    {
        score = 0;
        stageCount = 0;
        currentTime = timeLimit;
        pieceCount = 3;

        unlockedShapeTargetPrefabs.Clear();
        for (int i = 0; i < 3; i++)
        {
            if (i < allShapeTargetPrefabs.Count)
            {
                unlockedShapeTargetPrefabs.Add(allShapeTargetPrefabs[i]);
            }
        }

        UpdateScoreUI();
        GenerateNewPuzzle();
    }

    void GenerateNewPuzzle()
    {
        Debug.Log("GenerateNewPuzzle: 新しいパズルを生成開始します。ステージ: " + (stageCount + 1));

        // 1. 前のステージのオブジェクトを削除
        foreach (var obj in currentSpawnedTargets) { Destroy(obj); }
        foreach (var obj in currentSpawnedPieces) { Destroy(obj); } // ピースも削除
        currentSpawnedTargets.Clear();
        currentSpawnedPieces.Clear(); // ピースのリストもクリア
        placedPieces = 0;

        // 2. 難易度チェックと更新
        if (stageCount > 0)
        {
            if (stageCount % 2 == 0)
            {
                int nextUnlockIndex = unlockedShapeTargetPrefabs.Count;
                if (nextUnlockIndex < allShapeTargetPrefabs.Count)
                {
                    unlockedShapeTargetPrefabs.Add(allShapeTargetPrefabs[nextUnlockIndex]);
                }
            }
            if (stageCount % 5 == 0 && pieceCount < maxPieceCount)
            {
                pieceCount++;
            }
        }

        // 3. 今回のステージで使う形を選ぶ
        List<GameObject> shapesForThisStage = new List<GameObject>();
        List<GameObject> tempUnlockedList = new List<GameObject>(unlockedShapeTargetPrefabs);
        for (int i = 0; i < pieceCount; i++)
        {
            if (tempUnlockedList.Count == 0) break;
            int randomIndex = Random.Range(0, tempUnlockedList.Count);
            shapesForThisStage.Add(tempUnlockedList[randomIndex]);
            tempUnlockedList.RemoveAt(randomIndex);
        }

        // 4. ターゲットをランダムに配置（手動距離計算バージョン）
        List<Vector2> placedPositions = new List<Vector2>();
        foreach (GameObject targetPrefab in shapesForThisStage)
        {
            RectTransform pieceRect = targetPrefab.GetComponent<RectTransform>();
            float pieceRadius = pieceRect.sizeDelta.x / 2;
            float placeableRadius = puzzleBoardRadius - pieceRadius;

            Vector2 spawnPosition = Vector2.zero;
            bool positionFound = false;
            int attempts = 0;

            while (!positionFound && attempts < 200)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                float randomDistance = Random.Range(0, placeableRadius);
                spawnPosition = (Vector2)puzzleBoardCenter.position + (randomDirection * randomDistance);

                bool overlaps = false;
                foreach (Vector2 pos in placedPositions)
                {
                    if (Vector2.Distance(spawnPosition, pos) < (pieceRadius * 2))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    positionFound = true;
                }
                attempts++;
            }

            if (positionFound)
            {
                GameObject newTarget = Instantiate(targetPrefab, spawnPosition, Quaternion.identity, puzzleBoardCenter);
                currentSpawnedTargets.Add(newTarget);
                placedPositions.Add(spawnPosition);
            }
        }

        // 5. 対応するピースを生成
        foreach (GameObject target in currentSpawnedTargets)
        {
            GameObject newPiece = Instantiate(piecePrefab, piecePanel);
            PieceController pieceController = newPiece.GetComponent<PieceController>();
            TargetController targetController = target.GetComponent<TargetController>();

            pieceController.shapeType = targetController.shapeType;

            Image pieceImage = newPiece.GetComponent<Image>();
            pieceImage.sprite = Resources.Load<Sprite>(targetController.shapeType);
            
            pieceImage.color = kagaGosaiColors[Random.Range(0, kagaGosaiColors.Count)];
            
            currentSpawnedPieces.Add(newPiece);
        }
        Debug.Log("GenerateNewPuzzle: パズル生成完了。");
    }

    public void PiecePlacedCorrectly()
    {
        score += 100;
        placedPieces++;
        UpdateScoreUI();

        if (placedPieces >= currentSpawnedTargets.Count)
        {
            AudioManager.instance.PlaySE(AudioManager.instance.stageClear);
            score += 200;
            stageCount++;
            Debug.Log("ステージ " + stageCount + " クリア！");
            GenerateNewPuzzle();
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