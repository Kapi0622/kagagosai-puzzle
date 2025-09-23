using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; 

public class CreateManager : MonoBehaviour
{
    public static CreateManager instance;

    [Header("お皿関連")]
    public List<Sprite> dishSprites;
    public Image puzzleBoardImage;
    private int currentDishIndex = 0;

    [Header("パレット関連")]
    public List<Sprite> allPieceSprites;
    public GameObject piecePaletteContent;
    public GameObject pieceButtonPrefab;

    [Header("ピース生成関連")]
    public Canvas mainCanvas; // Canvasの拡大率を取得するために追加
    public GameObject piecePrefab;
    public Transform pieceParent; // 生成したピースの親
    public Color selectedColor = Color.white;
    
    [Header("UI関連")]
    public GameObject screenshotFeedbackText;

    // 現在ドラッグしているピースを記憶する変数
    private PieceController pieceBeingDragged; 

    void Awake() 
    {
        instance = this; 
    }

    void Start()
    {
        InitializePalette();
        UpdateDish();
    }
    
    void Update()
    {
        if (pieceBeingDragged != null)
        {
            pieceBeingDragged.transform.position = Mouse.current.position.ReadValue();
        }
    }

    // パレットのボタンを自動生成する
    void InitializePalette()
    {
        foreach (Sprite pieceSprite in allPieceSprites)
        {
            GameObject buttonObj = Instantiate(pieceButtonPrefab, piecePaletteContent.transform);
            buttonObj.GetComponent<Image>().sprite = pieceSprite;
            // ボタンに、対応するスプライトを渡してドラッグ開始の合図を送るコンポーネントを追加
            DraggablePaletteItem item = buttonObj.AddComponent<DraggablePaletteItem>();
            item.pieceSprite = pieceSprite;
        }
    }

    // パレットからピースを生成する（DraggablePaletteItemから呼ばれる）
    public void StartDraggingNewPiece(Sprite pieceSprite)
    {
        if (pieceSprite == null) return;

        GameObject newPiece = Instantiate(piecePrefab, pieceParent);
        Image pieceImage = newPiece.GetComponent<Image>();
        pieceImage.sprite = pieceSprite;
        pieceImage.color = selectedColor;
        
        // Layout Elementのサイズを適用するためにCanvas Groupを一時的に無効化
        CanvasGroup pieceCanvasGroup = newPiece.GetComponent<CanvasGroup>();
        if(pieceCanvasGroup != null) pieceCanvasGroup.blocksRaycasts = false;

        // ドラッグ中のピースとして記憶
        pieceBeingDragged = newPiece.GetComponent<PieceController>();
        pieceBeingDragged.isCreateModePiece = true;
    }

    // ドラッグを終了する（DraggablePaletteItemから呼ばれる）
    public void EndDraggingNewPiece()
    {
        if (pieceBeingDragged != null)
        {
            CanvasGroup pieceCanvasGroup = pieceBeingDragged.GetComponent<CanvasGroup>();
            if(pieceCanvasGroup != null) pieceCanvasGroup.blocksRaycasts = true;
                
            // マウス座標と、お皿の中心座標との距離を計算
            float distance = Vector2.Distance(Mouse.current.position.ReadValue(), puzzleBoardImage.transform.position);

            // 皿の半径（拡大率を考慮）を取得
            float scaledRadius = (puzzleBoardImage.rectTransform.sizeDelta.x / 2f) * mainCanvas.scaleFactor;

            // もし距離が皿の半径より大きいなら（＝皿の外）、ピースを削除
            if (distance > scaledRadius)
            {
                Destroy(pieceBeingDragged.gameObject);
            }

            pieceBeingDragged = null;
        }
    }
    
    // カラーボタンから呼ばれる
    public void SelectColor(Image buttonImage)
    {
        selectedColor = buttonImage.color;
    }

    // お皿を切り替える（ボタンから呼ぶ）
    public void ChangeDish(int direction)
    {
        currentDishIndex += direction;
        if (currentDishIndex >= dishSprites.Count) currentDishIndex = 0;
        if (currentDishIndex < 0) currentDishIndex = dishSprites.Count - 1;
        UpdateDish();
    }

    void UpdateDish()
    {
        puzzleBoardImage.sprite = dishSprites[currentDishIndex];
    }

    // スクリーンショットを撮影する（ボタンから呼ぶ）
    public void TakeScreenshot()
    {
        StartCoroutine(CaptureScreenshotCoroutine());
    }
    
    private IEnumerator CaptureScreenshotCoroutine()
    {
        // UIを一時的に非表示にするなどの処理
        // 例: piecePaletteContent.SetActive(false);
        yield return new WaitForEndOfFrame();

        ScreenCapture.CaptureScreenshot("KagaGosai_MyCreation.png");
        Debug.Log("スクリーンショットを保存しました！");

        if (screenshotFeedbackText != null)
        {
            screenshotFeedbackText.SetActive(true); // メッセージを表示
            yield return new WaitForSeconds(2f); // 2秒待つ
            screenshotFeedbackText.SetActive(false); // メッセージを非表示
        }
    }
}