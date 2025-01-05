/* =======================================================================================================
 * AK Studio
 * 
 * Version 2.0 by Alexandr Kuznecov
 * 06.01.2023
 * =======================================================================================================
 */

using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TinyJson;
using System.Collections;
using UnityEngine.SocialPlatforms.Impl;
using System;

public class GameManager : MonoBehaviour
{
    #region Editor Properties
    [Header("UI")]
    [SerializeField] private GameObject m_ObjectMenu = null;
    [SerializeField] private Text m_TextDialog = null;
    [SerializeField] private Text m_TextStatistic = null;
    [SerializeField] private GameObject m_MenuButtonContinue = null;

    [Header("Audio")]
    [SerializeField] private AudioSource m_SourceRigidbody = null;
    [SerializeField] private AudioSource m_SourceVoice = null;
    [SerializeField] private AudioSource m_SourceMusic = null;
    [SerializeField] private AudioMixer m_AudioMixer = null;

    [SerializeField] private AudioClip[] m_HoleSounds = null;

    [Header("Game Pools")]
    [SerializeField] private GameObject[] m_PoolBulletHoles = null;
    #endregion

    #region Fields
    public static GameManager Instance = null;
    public static SGameSettings GameSettings = new SGameSettings();
    public static SGameStatistic GameStatistic = new SGameStatistic();
    public static float DialogDuration { get; private set; } = 0.0f;
    public static bool IsGameOver { get; private set; } = false;
    public static bool IsEnableMenuKey { get; set; } = true;
    public static bool onPause { get; set; } = false;
    #endregion

    #region Private Values
    
    private const string NAME_VOLUME_MUSIC = "VOLUME_MUSIC";
    private const string NAME_VOLUME_EFFECT = "VOLUME_EFFECT";
    private const string NAME_VOLUME_VOICE = "VOLUME_VOICE";

    private const float SIMPLE_TEXT_DURATION = 0.08f;

    private string fileSavePath = "";
    private float timeStartGame = 0;

    #endregion

    #region Public Methods
    /// <summary>
    /// Создать шлейф от пули
    /// </summary>
    /// <param name="type">тип пули</param>
    /// <param name="posMuzzle">точка выхода</param>
    /// <param name="posHole">точка входа</param>
    public static void SpawnBulletTrail(EBulletType type, Vector3 posMuzzle, Vector3 posHole)
    {
        STrail[] arr = PlayerController.BulletStore.GetStore<SBulletStore[]>(type).BulletTrails;

        foreach(STrail t in arr)
        {
            if (!t.Prefab.activeSelf)
            {
                t.Prefab.SetActive(true);
                t.Prefab.transform.position = posMuzzle;
                t.Prefab.transform.LookAt(posHole, Vector3.up);
                t.Trail.localPosition = Vector3.zero;

                t.stopPosition = posHole;
                t.timeSpawn = Time.time;
                break;
            }
        }
    }
    
    /// <summary>
    /// Создать дырку от пули
    /// </summary>
    /// <param name="position">позиция</param>
    /// <param name="rotation">вращение</param>
    /// <param name="parent">предок</param>
    /// <returns></returns>
    public static GameObject SpawnBulletHole(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject obj = PoolSpawn(Instance.m_PoolBulletHoles, position, rotation, parent);
        if (obj != null)
        {
            ParticleSystem[] arr = obj.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in arr) ps.Play();

            Instance.m_SourceRigidbody.transform.position = position;
            Utils.PlayRandomSound(Instance.m_SourceRigidbody, Instance.m_HoleSounds);

            return obj;
        }
        return null;
    }

    /// <summary>
    /// Спавн объекта из пулла
    /// </summary>
    /// <param name="pool">пул</param>
    /// <param name="position">позиция</param>
    /// <param name="rotation">вращение</param>
    /// <param name="parent">предок</param>
    /// <returns></returns>
    public static GameObject PoolSpawn(GameObject[] pool, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (pool.Length > 0 && pool[0] != null)
        {
            Utils.Shift(pool);

            if (parent != null) pool[0].transform.parent = parent;
            pool[0].transform.position = position;
            pool[0].transform.rotation = rotation;            
            pool[0].gameObject.SetActive(true);

            return pool[0];
        }
        return null;
    }

    /// <summary>
    /// Показать диалог
    /// </summary>
    /// <param name="name">локализованное название</param>
    /// <param name="duration">время затухания, -1 никогда, 0 автомат, 4 секунды</param>
    /// <param name="ass">источник голоса</param>
    /// <param name="showText">показать ли текст</param>
    /// <returns></returns>
    public static void ShowDialogText(string name, float duration = 0.0f, AudioSource ass = null, bool showText = true)
    {
        float duration_sound = 0.0f;

        // Пробуем загрузить озвучку, если не получилось проверим английскую версию
        AudioClip sound = Resources.Load<AudioClip>($"Voices/{GameSettings.lang}/{name}");
        if (sound == null && GameSettings.lang != "en")
        {
            sound = Resources.Load<AudioClip>($"Voices/en/{name}");
        }

        if (sound != null)
        {
            PlayVoiceSound(sound, (ass != null) ? ass : Instance.m_SourceVoice);
            duration_sound = sound.length + 0.5f;
        }

        if (!showText)
        {
            DialogDuration = duration_sound;
            return;
        }

        // Загружаем текст
        string translate_text = "Localized text not found";        
        if (LocalizationManager.IsReady)
        {
            translate_text = LocalizationManager.GetLocalizedValue(name);            
        }

        Instance.m_TextDialog.text = translate_text;
        Instance.m_TextDialog.canvasRenderer.SetAlpha(1.0f);

        DialogDuration = 3.0f;
        if(duration_sound > 0)
        {
            DialogDuration = duration_sound;
        }
        else
        {
            DialogDuration = LocalizationManager.GetLocalizedValueSize(name) * SIMPLE_TEXT_DURATION;
        }
        if (DialogDuration < 3.0f) DialogDuration = 3.0f;

        if (duration >= 0)
        {
            Instance.m_TextDialog.CrossFadeAlpha(0.0f, DialogDuration, false);
        }
    }    

    public static void HideDialogText()
    {
        Instance.m_TextDialog.CrossFadeAlpha(0.0f, 2.0f, false);
    }

    /// <summary>
    /// Проиграть звук из нужной позиции
    /// </summary>
    /// <param name="clip">звук</param>
    /// <param name="volume">громкость</param>
    /// <param name="position">позиция</param>
    public static void PlaySoundAtPosition(AudioClip clip, float volume, Vector3 position)
    {
        if (Instance.m_SourceRigidbody == null && !Instance.m_SourceRigidbody.enabled) return;
        if (Instance.m_SourceRigidbody.isPlaying) Instance.m_SourceRigidbody.Stop();

        Instance.m_SourceRigidbody.clip = clip;
        Instance.m_SourceRigidbody.volume = volume;
        Instance.m_SourceRigidbody.transform.position = position;
        Instance.m_SourceRigidbody.Play();
    }

    /// <summary>
    /// Остановить звук играющий из нужной позиции
    /// </summary>
    public static void StopSoundAtPosition()
    {
        if (Instance.m_SourceRigidbody == null) return;
        if (Instance.m_SourceRigidbody.isPlaying) Instance.m_SourceRigidbody.Stop();
    }

    /// <summary>
    /// Проиграть озвучку
    /// </summary>
    /// <param name="clip">звук</param>
    /// <param name="ass">источник</param>
    public static void PlayVoiceSound(AudioClip clip, AudioSource ass = null)
    {
        if (ass == null) ass = Instance.m_SourceVoice;
        if (ass == null && !ass.enabled) return;

        if (ass.isPlaying) ass.Stop();
        ass.clip = clip;
        ass.Play();
    }

    /// <summary>
    /// Остановить озвучку
    /// </summary>
    /// <param name="ass">источник</param>
    public static void StopVoiceSound(AudioSource ass = null)
    {
        if (ass == null) ass = Instance.m_SourceVoice;
        if (ass == null) return;
        if (ass.isPlaying) ass.Stop();
    }

    public void EventShowDialogText(string name) { ShowDialogText(name, 1.0f, null, true); }

    /// <summary>
    /// Применить глобальные настройки
    /// </summary>
    public void ApplySettings(bool forcibly = false)
    {
        Application.targetFrameRate = (GameSettings.vsync) ? 60 : 720;
        QualitySettings.vSyncCount = (GameSettings.vsync) ? 1 : -1;
        QualitySettings.SetQualityLevel(GameSettings.quality, true);
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

        m_AudioMixer.SetFloat(NAME_VOLUME_MUSIC, GameSettings.music);
        m_AudioMixer.SetFloat(NAME_VOLUME_EFFECT, GameSettings.effects);
        m_AudioMixer.SetFloat(NAME_VOLUME_VOICE, GameSettings.voice);

        LocalizationManager.SetLanguage(GameSettings.lang);

        if (GameSettings.nosave || forcibly) GameSettings.Save();
    }

    public void ApplyMusicVolume(float v)
    {
        GameSettings.music = v;
        m_AudioMixer.SetFloat(NAME_VOLUME_MUSIC, GameSettings.music);
    }

    /// <summary>
    /// Установить глобальный язык
    /// </summary>
    /// <param name="lang"></param>
    public void SetLanguage(string lang)
    {
        LocalizationManager.SetLanguage(lang);
        GameSettings.lang = lang;
        GameSettings.Save();
    }

    /// <summary>
    /// Пауза метод для кнопок меню.
    /// </summary>
    public void EventPause()
    {
        onPause = true;
        Pause(true);
    }

    /// <summary>
    /// Пауза
    /// </summary>
    /// <param name="showCursor">показать курсор мыши</param>
    public void Pause(bool showCursor = true)
    {
        ShowCursor(showCursor);
        Time.timeScale = 0;

        AudioSource[] AllSounds = FindObjectsOfType<AudioSource>();
        foreach (AudioSource a in AllSounds)
        {
            if (m_SourceMusic != null && a.name != m_SourceMusic.name) a.Pause();
        }
    }

    /// <summary>
    /// Отключение паузы
    /// </summary>
    public void UnPause()
    {
        onPause = false;
        ShowCursor(false);
        Time.timeScale = 1;

        AudioSource[] AllSounds = FindObjectsOfType<AudioSource>();
        foreach (AudioSource a in AllSounds)
        {
            if (m_SourceMusic != null && a.name != m_SourceMusic.name) a.UnPause();
        }
    }

    /// <summary>
    /// Конец игры
    /// </summary>
    public void GameOver()
    {
        m_MenuButtonContinue.SetActive(false);
        StartCoroutine(CShowMenu());
        IsGameOver = true;
    }

    public void DebugSetFPS(int fps)
    {
        Application.targetFrameRate = fps;
        QualitySettings.vSyncCount = (fps < 80) ? 1 : -1;
    }

    public void CalculateStatistics()
    {
        Debug.Log($"bullet_total{GameStatistic.bullet_total}");
        Debug.Log($"bullet_critical{GameStatistic.bullet_critical}");
        Debug.Log($"bullet_misses{GameStatistic.bullet_misses}");
        Debug.Log($"bullet_hits{GameStatistic.bullet_hits}");


        GameStatistic.time = Time.time - timeStartGame;
        string stradd;
        int strlen = 30;
        string str = LocalizationManager.GetLocalizedValue("stat_title");

        stradd = LocalizationManager.GetLocalizedValue("stat_enemys");
        str += stradd + Utils.AddSplits($"{GameStatistic.enemys}", "_", strlen - stradd.Length) + Environment.NewLine;

        stradd = LocalizationManager.GetLocalizedValue("stat_enemy_shots");
        str += stradd + Utils.AddSplits($"{GameStatistic.bullet_enemy}", "_", strlen - stradd.Length) + Environment.NewLine;

        stradd = LocalizationManager.GetLocalizedValue("stat_player_shots");
        str += stradd + Utils.AddSplits($"{GameStatistic.bullet_total}", "_", strlen - stradd.Length) + Environment.NewLine;

        stradd = LocalizationManager.GetLocalizedValue("stat_player_damage");
        str += stradd + Utils.AddSplits(Mathf.Round(GameStatistic.damage).ToString(), "_", strlen - stradd.Length) + Environment.NewLine;

        stradd = LocalizationManager.GetLocalizedValue("stat_enemy_damage");
        str += stradd + Utils.AddSplits(Mathf.Round(GameStatistic.health).ToString(), "_", strlen - stradd.Length) + Environment.NewLine;

        float hit_rate = (GameStatistic.bullet_total > 0 && GameStatistic.bullet_hits > 0)
            ? Mathf.Round(100.0f / ((float)GameStatistic.bullet_total / GameStatistic.bullet_hits)) : 0;
        float crit_rate = (GameStatistic.bullet_hits > 0 && GameStatistic.bullet_critical > 0)
            ? Mathf.Round(100.0f / ((float)GameStatistic.bullet_hits / GameStatistic.bullet_critical)) : 0;

        stradd = LocalizationManager.GetLocalizedValue("stat_hit_rate");
        str += stradd + Utils.AddSplits((hit_rate).ToString(), "_", strlen - stradd.Length) + "%" + Environment.NewLine;

        stradd = LocalizationManager.GetLocalizedValue("stat_crit_rate");
        str += stradd + Utils.AddSplits((crit_rate).ToString(), "_", strlen - stradd.Length) + "%" + Environment.NewLine;

        stradd = LocalizationManager.GetLocalizedValue("stat_game_time");
        str += stradd + " " + Utils.FormatTime((int)GameStatistic.time).ToString() + Environment.NewLine;

        m_TextStatistic.text = str;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Показать или скрыть курсор
    /// </summary>
    /// <param name="lk">показать курсор</param>
    private void ShowCursor(bool show)
    {
        if(show)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        } 
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
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

        //fileSavePath = Path.Combine(Application.dataPath, GAME_SAVE_DIRECTORY);
        //if (!Directory.Exists(fileSavePath)) { Directory.CreateDirectory(fileSavePath); }
        //fileSavePath = Path.Combine(fileSavePath, "settings.json");

        GameSettings = SGameSettings.Load(fileSavePath);
        GameStatistic = new SGameStatistic();
        timeStartGame = Time.time;
    }

    private void Start()
    {
        Time.timeScale = 1;

        if(m_TextDialog != null) m_TextDialog.canvasRenderer.SetAlpha(0.0f);
        IsGameOver = false;

        ShowCursor(false);        
        ApplySettings();
    }

    private void Update()
    {
        float t = Time.time;
        if (Input.GetButtonDown("Cancel"))
        {
            m_ObjectMenu.SetActive(!m_ObjectMenu.activeSelf);
        }

        // Проходим по хранилищу шлейфов от пуль и отключаем активные.
        foreach (SBulletStore sb in PlayerController.BulletStore)
        {
            foreach(STrail tr in sb.BulletTrails)
            {
                if (tr.Prefab.activeSelf)
                {
                    if (t - tr.timeSpawn > 3.0f)
                    {
                        tr.Prefab.SetActive(false);
                        continue;
                    }

                    if ((tr.stopPosition - tr.Trail.position).sqrMagnitude < 0.5f)
                    {
                        tr.Prefab.SetActive(false);
                        continue;
                    }

                    // Движение.
                    tr.Trail.position = Vector3.MoveTowards(tr.Trail.position, tr.stopPosition, 100.0f * Time.deltaTime);                    
                }
            }            
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Pause();
        }
        else
        {
            if (!onPause) { UnPause(); }
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            if (!onPause) { UnPause(); }
        }
        else
        {
            Pause();
        }
    }

    private IEnumerator CShowMenu()
    {
        yield return new WaitForSeconds(5.0f);
        m_ObjectMenu.SetActive(true);
    }
    #endregion
}

#region Public Serializable Structures
[System.Serializable]
public class SGameStatistic
{    
    public int enemys = 0;
    public float damage = 0;
    public float health = 0;
    public float time = 0;
    public int bullet_enemy = 0;
    public int bullet_total = 0;
    public int bullet_hits = 0;
    public int bullet_critical = 0;
    public int bullet_misses = 0;
}


/// <summary>
/// Настройки игры
/// </summary>
[System.Serializable]
public class SGameSettings
{
    public bool nosave = true;
    public string lang = "ru";
    public float music = -23.0f;
    public float effects = 0.0f;
    public float voice = 0.0f;
    public float mouse = 0.8f;
    public float gamma = 0.0f;
    public int quality = 3;
    public int difficulty = 0;
    public bool joyxbox = true;
    public bool invertmousey = false;
    public bool smoothing = true;
    public bool vsync = true;
    public bool postprocess = true;
    public bool antialiasing = true;
    public bool noise = true;
    public bool fog = true;
    public bool bloom = true;
    public bool lights = true;
    public bool lockfps = true;

    //private static string _fileSavePath = "";
    private const string GAME_SAVE_DIRECTORY = "SaveData";


    public void Save()
    {
        nosave = false;
        PlayerPrefs.SetString(GAME_SAVE_DIRECTORY, this.ToJson());
        PlayerPrefs.Save();

        //Debug.Log(this.ToJson());
        //File.WriteAllText(_fileSavePath, this.ToJson());
    }

    public static SGameSettings Load(string f)
    {
        /*
         * 
         _fileSavePath = f;
        if (File.Exists(_fileSavePath))
        {
            string s = File.ReadAllText(_fileSavePath);
            if (s != "" && s != null)
            {
                Debug.Log(s.ToJson());
                return s.FromJson<SGameSettings>();
            }
        }*/

        string s = PlayerPrefs.GetString(GAME_SAVE_DIRECTORY, "");
        if (s != "")
            return s.FromJson<SGameSettings>();
        else
            return new SGameSettings();
    }
}
#endregion