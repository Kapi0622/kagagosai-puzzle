using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PieceController : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IPointerUpHandler
{
    public string shapeType;
    public bool isCreateModePiece = false;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 initialPosition;
    private Transform initialParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    // 押した瞬間に呼ばれる
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!this.enabled) return;

        // 操作の効果音を鳴らす
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySE(AudioManager.instance.grabPiece);
        }

        // どのモードでも、操作前の親と位置を記録する
        initialParent = transform.parent;
        initialPosition = rectTransform.anchoredPosition;
    }

    // ドラッグが「開始」した瞬間に呼ばれる
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!this.enabled) return;

        // 共通のドラッグ開始処理
        canvasGroup.blocksRaycasts = false; // 自分自身への当たり判定を無効化
        canvasGroup.alpha = 0.8f;
        transform.SetParent(canvas.transform, true); // 最前面に表示
    }

    // ドラッグ中に呼ばれる
    public void OnDrag(PointerEventData eventData)
    {
        if (!this.enabled) return;

        // 安定した差分加算方式でマウスに追従
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    // 指を離した時に呼ばれる
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!this.enabled) return;
        
        // 共通のドラッグ終了処理
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        // まず、元の親に戻す
        transform.SetParent(initialParent);

        if (isCreateModePiece)
        {
            // 「つくる」モードの処理
            // お皿の外にドロップされたら削除
            if (CreateManager.instance != null && !RectTransformUtility.RectangleContainsScreenPoint(CreateManager.instance.puzzleBoardImage.rectTransform, eventData.position, eventData.pressEventCamera))
            {
                Destroy(gameObject);
            }
        }
        else if (GameManager.instance != null) // GameSceneの処理
        {
            GameObject droppedOnObject = eventData.pointerCurrentRaycast.gameObject;
            bool success = false;

            if (droppedOnObject != null && droppedOnObject.CompareTag("PieceTarget"))
            {
                TargetController target = droppedOnObject.GetComponent<TargetController>();
                if (target != null && target.shapeType == this.shapeType)
                {
                    success = true;
                    AudioManager.instance.PlaySE(AudioManager.instance.placePiece);
                    
                    // 正解なので、親をPuzzleBoard（ターゲットの親）に正式に移動
                    transform.SetParent(target.transform.parent);
                    
                    rectTransform.position = target.transform.position;
                    target.gameObject.SetActive(false);
                    this.enabled = false;
                    
                    GameManager.instance.PiecePlacedCorrectly();
                }
            }

            // もし正解でなかったら（クリックのみで離した場合もここ）、元の位置に戻す
            if (!success)
            {
                rectTransform.anchoredPosition = initialPosition;
            }
        }
    }
}