using UnityEngine;
using UnityEngine.EventSystems;

// IPointerUpHandlerを追加
public class DraggablePaletteItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Sprite pieceSprite;

    // このボタン上でドラッグが「開始」された瞬間に呼ばれる
    public void OnBeginDrag(PointerEventData eventData)
    {
        // CreateManagerに、このスプライトでピースのドラッグを開始するよう命令
        AudioManager.instance.PlaySE(AudioManager.instance.grabPiece);
        CreateManager.instance.StartDraggingNewPiece(pieceSprite);
    }

    // ドラッグが「継続」している間、呼ばれ続ける（中身は空でOK）
    // これを実装することで、Scroll Rectにイベントが奪われるのを防ぐ
    public void OnDrag(PointerEventData eventData)
    {
        // 何もしない
    }

    // ドラッグが「終了」した瞬間に呼ばれる
    public void OnEndDrag(PointerEventData eventData)
    {
        // CreateManagerに、ドラッグの終了を伝える
        CreateManager.instance.EndDraggingNewPiece();
    }
}