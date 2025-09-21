using UnityEngine;
using UnityEngine.SceneManagement; // シーン管理に必要

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("BGM")]
    public AudioClip titleBGM;       // タイトル画面のBGM
    public AudioClip gameBGM;        // ゲーム画面のBGM
    public AudioClip resultBGM;      // リザルト画面のBGM

    [Header("効果音 (SE)")]
    public AudioClip grabPiece;      // ピースを掴んだ音
    public AudioClip placePiece;     // ピースがはまった音
    public AudioClip stageClear;     // ステージクリア音
    public AudioClip buttonClick;    // ボタンクリック音

    private AudioSource bgmSource;
    private AudioSource seSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return; // ここで処理を中断
        }

        // BGM用とSE用にAudioSourceを2つ用意する
        bgmSource = gameObject.AddComponent<AudioSource>();
        seSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true; // BGMはループ再生する
    }

    private void OnEnable()
    {
        // シーンがロードされた時に呼ばれるイベントに関数を登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // オブジェクトが破棄される時にイベントから関数を解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーンがロードされた時に自動で呼ばれる関数
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーン名に応じて再生するBGMを切り替える
        if (scene.name == "TitleScene")
        {
            PlayBGM(titleBGM);
        }
        else if (scene.name == "GameScene")
        {
            PlayBGM(gameBGM);
        }
        else if (scene.name == "ResultScene")
        {
            PlayBGM(resultBGM);
        }
    }

    // BGMを再生する関数
    public void PlayBGM(AudioClip clip)
    {
        if (clip != null && bgmSource.clip != clip) // 同じBGMが既に流れていなければ
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    // 効果音を再生する関数
    public void PlaySE(AudioClip clip)
    {
        if (clip != null)
        {
            seSource.PlayOneShot(clip);
        }
    }
}