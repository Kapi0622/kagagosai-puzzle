using UnityEngine;

public class TitleUIController : MonoBehaviour
{
    public GameObject themePanel; // Inspectorで設定するテーマ説明パネル

    // テーマ説明パネルを開く
    public void OpenThemePanel()
    {
        if (themePanel != null)
        {
            themePanel.SetActive(true);
        }
    }

    // テーマ説明パネルを閉じる
    public void CloseThemePanel()
    {
        if (themePanel != null)
        {
            themePanel.SetActive(false);
        }
    }
}