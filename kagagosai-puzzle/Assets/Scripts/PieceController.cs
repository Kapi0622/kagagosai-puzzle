using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PieceController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public string shapeType; // GameManagerによって設定されるピースの形

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
        initialPosition = rectTransform.anchoredPosition;
        initialParent = transform.parent;
        transform.SetParent(canvas.transform, true); // ドラッグ中、最前面に表示するため
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.SetParent(initialParent); // 親を元に戻す
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        GameObject droppedOnObject = eventData.pointerCurrentRaycast.gameObject;

        if (droppedOnObject != null && droppedOnObject.CompareTag("PieceTarget"))
        {
            TargetController target = droppedOnObject.GetComponent<TargetController>();

            if (target != null && target.shapeType == this.shapeType)
            {
                // 形が一致した場合：正解！
                rectTransform.position = target.transform.position;
                this.enabled = false; // 自分を操作不能にする
                target.gameObject.SetActive(false); // ターゲットも非表示にする

                // GameManagerに正解を報告
                GameManager.instance.PiecePlacedCorrectly();
                Debug.Log("正解！ " + shapeType);
            }
            else
            {
                // 形が違う穴だったので不正解
                rectTransform.anchoredPosition = initialPosition;
            }
        }
        else
        {
            // ターゲット以外の場所だったので不正解
            rectTransform.anchoredPosition = initialPosition;
        }
    }
}