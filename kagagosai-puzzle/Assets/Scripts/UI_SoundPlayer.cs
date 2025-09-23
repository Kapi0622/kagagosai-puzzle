using UnityEngine;

public class UI_SoundPlayer : MonoBehaviour
{
    // ボタンクリック音を鳴らす
    public void PlayButtonClickSE()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySE(AudioManager.instance.buttonClick);
        }
    }
    
    public void PlayStageClearSE()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySE(AudioManager.instance.stageClear);
        }
    }

    // 他の効果音を鳴らす（例えば、皿切り替え音など）
    public void PlayCustomSE(AudioClip clipToPlay)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySE(clipToPlay);
        }
    }
}