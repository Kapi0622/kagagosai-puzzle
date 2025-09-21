using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PieceController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public string shapeType;

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

    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.instance.PlaySE(AudioManager.instance.grabPiece);
        
        initialPosition = rectTransform.anchoredPosition;
        initialParent = transform.parent;
        transform.SetParent(canvas.transform, true);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.SetParent(initialParent);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        GameObject droppedOnObject = eventData.pointerCurrentRaycast.gameObject;

        if (droppedOnObject != null && droppedOnObject.CompareTag("PieceTarget"))
        {
            TargetController target = droppedOnObject.GetComponent<TargetController>();

            if (target != null && target.shapeType == this.shapeType)
            {
                // 正解！
                AudioManager.instance.PlaySE(AudioManager.instance.placePiece);
                
                rectTransform.position = target.transform.position;
                this.enabled = false;

                target.gameObject.SetActive(false); // ターゲットを非表示にする

                transform.SetParent(target.transform.parent); // 所属をPuzzleBoardに変更

                GameManager.instance.PiecePlacedCorrectly();
            }
            else
            {
                // 不正解（形が違う）
                rectTransform.anchoredPosition = initialPosition;
            }
        }
        else
        {
            // 不正解（ターゲット以外）
            rectTransform.anchoredPosition = initialPosition;
        }
    }
}