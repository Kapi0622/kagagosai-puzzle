using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    // ... (AudioClipの変数は変更なし) ...
    [Header("BGM")]
    public AudioClip titleBGM, gameBGM, resultBGM;
    [Header("効果音 (SE)")]
    public AudioClip grabPiece, placePiece, stageClear, buttonClick;
    public AudioClip countdownTick;  
    public AudioClip countdownStart; 
    public AudioClip gameOver;      

    private AudioSource bgmSource;
    private AudioSource seSource;

    void Awake()
    {
        Debug.Log("--- Awake()が呼ばれました --- 名前: " + gameObject.name + " ID: " + GetInstanceID());
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AudioManagerのインスタンスを生成しました。ID: " + GetInstanceID());
        }
        else
        {
            Debug.LogWarning("AudioManagerのインスタンスは既に存在します。新しいインスタンスを破棄します。 破棄ID: " + GetInstanceID());
            Destroy(gameObject);
        }
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name + " がロードされました。BGMを再生します。");
        if (scene.name == "TitleScene") PlayBGM(titleBGM);
        else if (scene.name == "GameScene") PlayBGM(gameBGM);
        else if (scene.name == "ResultScene") PlayBGM(resultBGM);
        else if (scene.name == "CreateScene") PlayBGM(titleBGM); 
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null)
        {
            Debug.Log("bgmSourceがnullのため、新しく作成します。");
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }
        
        if (clip != null && bgmSource.clip != clip)
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    public void PlaySE(AudioClip clip)
    {
        Debug.Log("--- PlaySEが呼ばれました ---");
        if (seSource == null)
        {
            Debug.Log("seSourceがnullのため、新しく作成します。");
            seSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Debug.Log("seSourceは既に存在します。");
        }

        if (clip != null)
        {
            Debug.Log("SEを再生します: " + clip.name);
            seSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("再生するSEクリップが指定されていません。");
        }
    }
    
    // BGMの音量を設定・保存する
    public void SetBgmVolume(float volume)
    {
        bgmSource.volume = volume;
    }

// SEの音量を設定・保存する
    public void SetSeVolume(float volume)
    {
        seSource.volume = volume;
    
        if(Time.timeSinceLevelLoad > 0.1f)
        {
            PlaySE(buttonClick);
        }
    }
}