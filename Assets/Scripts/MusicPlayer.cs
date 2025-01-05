using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [Header("Sounds")]
    [Range(60f, 210.0f)]
    [SerializeField] private float m_Tempo = 140.0f;
    [SerializeField] private AudioClip[] m_Fast = null;
    [SerializeField] private AudioClip[] m_Slow = null;

    public static MusicPlayer Instance = null;

    private AudioSource as_Audio = null;
    private ETypeMusic queue = ETypeMusic.Fast;
    private bool onChangeTrack = false;

    private float tempoTime = 1.0f; // Темп в секундах

    public static void Play(ETypeMusic t) { Instance.EventPlay(t); }
    public void EventPlay(ETypeMusic t)
    {
        if (as_Audio.isPlaying)
        {
            queue = t;
            onChangeTrack = true;
        }
        else
        {
            SetTrack(t);
        }
    }

    private void Awake()
    {
        // Синглтон
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        as_Audio = GetComponent<AudioSource>();
        as_Audio.loop = true;
        Play(ETypeMusic.Slow);
        tempoTime = 120 / m_Tempo;
    }

    
    private void FixedUpdate()
    {
        if (as_Audio.clip != null && onChangeTrack && as_Audio.time % tempoTime < 0.05f)
        {
            Debug.Log("MusicPlayer.ChangeTrack");
            SetTrack(queue);
            onChangeTrack = false;
        }

        if (!as_Audio.isPlaying)
        {
            SetTrack(queue);
        }
    }

    private void SetTrack(ETypeMusic t)
    {
        Debug.Log("MusicPlayer.SetTrack()");        
        switch (t)
        {
            case ETypeMusic.Fast:
                queue = ETypeMusic.Fast;

                PlayRandomSound(m_Fast);
                
                break;
            case ETypeMusic.Slow:
                queue = ETypeMusic.Slow;
                
                PlayRandomSound(m_Slow);
                break;
        }
    }
    
    public void PlayRandomSound(AudioClip[] arr)
    {
        // Берем произвольный звук, 0 элемент массива это прошлый проигранный звук, чтобы не выпало два одинаковых подряд
        int n = Random.Range(1, arr.Length);
        as_Audio.clip = arr[n];
        as_Audio.time = 0;
        as_Audio.Play();

        // Передвигаем проигранный звук в начало массива
        arr[n] = arr[0];
        arr[0] = as_Audio.clip;
    }
}

public enum ETypeMusic
{
    Fast,
    Slow
}
