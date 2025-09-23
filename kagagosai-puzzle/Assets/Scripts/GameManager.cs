using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // ゲームの状態を定義
    private enum GameState { HowToPlay, Countdown, Playing, GameOver }
    private GameState currentState;

    [Header("UI関連")]
    public Canvas mainCanvas; // メインキャンバス
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    public GameObject howToPlayPanel;
    public TextMeshProUGUI countdownText;
    public GameObject gameOverText;

    [Header("ゲーム設定")]
    public float timeLimit = 180f;

    [Header("パズル自動生成 設定")]
    public Transform puzzleBoardCenter;
    public float puzzleBoardRadius;
    public List<GameObject> allShapeTargetPrefabs;
    public GameObject piecePrefab;
    public Transform piecePanel;

    private float currentTime;
    private int score;
    private int stageCount = 0;
    private int pieceCount = 5;
    private int maxPieceCount = 10;
    private List<GameObject> unlockedShapeTargetPrefabs = new List<GameObject>();
    private List<GameObject> currentSpawnedTargets = new List<GameObject>();
    private List<GameObject> currentSpawnedPieces = new List<GameObject>();
    private int placedPieces;
    private List<Color> kagaGosaiColors = new List<Color>();

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
        
        ColorUtility.TryParseHtmlString("#4B0082", out Color ai);
        ColorUtility.TryParseHtmlString("#DC143C", out Color enji);
        ColorUtility.TryParseHtmlString("#228B22", out Color kusa);
        ColorUtility.TryParseHtmlString("#DAA520", out Color odo);
        ColorUtility.TryParseHtmlString("#483D8B", out Color kodaimurasaki);
        kagaGosaiColors.Add(ai); kagaGosaiColors.Add(enji); kagaGosaiColors.Add(kusa); kagaGosaiColors.Add(odo); kagaGosaiColors.Add(kodaimurasaki);
    }

    void Start()
    {
        currentState = GameState.HowToPlay;
        howToPlayPanel.SetActive(true);
        countdownText.gameObject.SetActive(false);
        gameOverText.SetActive(false);
        
        timeText.text = "時間 : " + Mathf.Ceil(timeLimit).ToString();
        scoreText.text = "スコア : 0";
    }

    void Update()
    {
        if (currentState == GameState.Playing)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerUI();
            }
            else
            {
                // 時間切れになった瞬間に一度だけ呼ばれるようにする
                if(currentState != GameState.GameOver)
                {
                    StartCoroutine(GameOverCoroutine());
                }
            }
        }
    }

    // カウントダウンを行うコルーチン
    private IEnumerator CountdownCoroutine()
    {
        currentState = GameState.Countdown;
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        AudioManager.instance.PlaySE(AudioManager.instance.countdownTick); // SE再生
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        AudioManager.instance.PlaySE(AudioManager.instance.countdownTick); // SE再生
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        AudioManager.instance.PlaySE(AudioManager.instance.countdownTick); // SE再生
        yield return new WaitForSeconds(1f);

        countdownText.text = "スタート！";
        AudioManager.instance.PlaySE(AudioManager.instance.countdownStart); // 別のSE再生
        yield return new WaitForSeconds(0.5f);

        countdownText.gameObject.SetActive(false);
        StartGameplay();
    }
    
    // ゲーム終了時の演出を行うコルーチン
    private IEnumerator GameOverCoroutine()
    {
        currentState = GameState.GameOver;
        currentTime = 0;
        UpdateTimerUI();

        // 「おわり」の表示とSE再生
        if (gameOverText != null) gameOverText.SetActive(true);
        AudioManager.instance.PlaySE(AudioManager.instance.gameOver);

        // 2秒間待ってからリザルト画面へ
        yield return new WaitForSeconds(2f);

        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.Save();
        SceneManager.LoadScene("ResultScene");
    }

    public void CloseHowToPlay()
    {
        howToPlayPanel.SetActive(false);
        StartCoroutine(CountdownCoroutine());
    }
    
    private void StartGameplay()
    {
        currentState = GameState.Playing;
        score = 0;
        stageCount = 0;
        currentTime = timeLimit;
        pieceCount = 5;
        for (int i = 0; i < 5; i++) { if (i < allShapeTargetPrefabs.Count) { unlockedShapeTargetPrefabs.Add(allShapeTargetPrefabs[i]); } }
        UpdateScoreUI();
        GenerateNewPuzzle();
    }

    void GenerateNewPuzzle()
    {
        foreach (var obj in currentSpawnedTargets) { Destroy(obj); }
        foreach (var obj in currentSpawnedPieces) { Destroy(obj); }
        currentSpawnedTargets.Clear();
        currentSpawnedPieces.Clear();
        placedPieces = 0;

        if (stageCount > 0)
        {
            
            // 2ステージごとにピースの数を増やす
            if (stageCount % 2 == 0 && pieceCount < maxPieceCount)
            {
                pieceCount++;
            }
        }

        List<GameObject> shapesForThisStage = new List<GameObject>();
        List<GameObject> tempAvailableList = new List<GameObject>(allShapeTargetPrefabs);
        for (int i = 0; i < pieceCount; i++)
        {
            if (tempAvailableList.Count == 0) break;
            int randomIndex = Random.Range(0, tempAvailableList.Count);
            shapesForThisStage.Add(tempAvailableList[randomIndex]);
            tempAvailableList.RemoveAt(randomIndex);
        }
        // --- ▼▼▼ ここが固定サイズ用の配置ロジック ▼▼▼ ---
        List<Vector2> placedPositions = new List<Vector2>();
        
        // ピースの基本サイズをpiecePrefabから一度だけ取得
        RectTransform pieceRect = piecePrefab.GetComponent<RectTransform>();
        float pieceRadius = pieceRect.sizeDelta.x / 2f;

        // Canvasの拡大率を考慮した実際の半径で計算
        float pieceScaledRadius = pieceRadius * mainCanvas.scaleFactor;
        float placeableScaledRadius = (puzzleBoardRadius * mainCanvas.scaleFactor) - pieceScaledRadius;

        foreach (GameObject targetPrefab in shapesForThisStage)
        {
            Vector2 spawnPosition = Vector2.zero;
            bool positionFound = false;
            int attempts = 0;

            while (!positionFound && attempts < 200)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                float randomDistance = Random.Range(0, placeableScaledRadius);
                spawnPosition = (Vector2)puzzleBoardCenter.position + (randomDirection * randomDistance);
                
                bool overlaps = false;
                foreach (Vector2 pos in placedPositions)
                {
                    // ピースの実際の大きさ（拡大率考慮済み）で距離を比較
                    if (Vector2.Distance(spawnPosition, pos) < (pieceScaledRadius * 2))
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
        // --- ▲▲▲ ここまでが固定サイズ用の配置ロジック ▲▲▲ ---

        foreach (GameObject target in currentSpawnedTargets)
        {
            GameObject newPiece = Instantiate(piecePrefab, piecePanel);
            PieceController pieceController = newPiece.GetComponent<PieceController>();
            TargetController targetController = target.GetComponent<TargetController>();

            pieceController.shapeType = targetController.shapeType;

            Image pieceImage = newPiece.GetComponent<Image>();
            pieceImage.sprite = Resources.Load<Sprite>(targetController.shapeType);
            pieceImage.color = kagaGosaiColors[Random.Range(0, kagaGosaiColors.Count)];
            // pieceImage.SetNativeSize(); // ← この行がないバージョンです
            currentSpawnedPieces.Add(newPiece);
        }
    }

    public void PiecePlacedCorrectly()
    {
        score += 100;
        placedPieces++;

        if (placedPieces >= currentSpawnedTargets.Count)
        {
            AudioManager.instance.PlaySE(AudioManager.instance.stageClear);
            score += 500;
            stageCount++;
            GenerateNewPuzzle();
        }
        
        UpdateScoreUI();
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