using UnityEngine;
using UnityEngine.SceneManagement; // シーン管理に必要

public class SceneController : MonoBehaviour
{
    
    // ゲーム画面に移動する
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    // タイトル画面に移動する
    public void BackToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
    
    public void GoToCreateScene()
    {
        SceneManager.LoadScene("CreateScene");
    }
}