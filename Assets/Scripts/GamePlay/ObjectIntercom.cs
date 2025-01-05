using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ObjectIntercom : MonoBehaviour
{
    [Header("Properties")]
    [Range(0.5f, 10.0f)]
    [SerializeField] private float m_PauseSec = 0.5f;
    [SerializeField] private Text m_TextDialog = null;
    [SerializeField] private string[] m_Text = null;
    [SerializeField] private AudioSource[] m_AudioSources = null;

    [SerializeField] private int m_CountTextLoop = 4;

    [SerializeField] private bool isActive = true;

    private Coroutine coroutine = null;
    private int loopCounter = 0;

    #region Public Methods
    /// <summary>
    /// Установить новые текста для интеркома
    /// </summary>
    /// <param name="texts"></param>
    public void SetTexts(string[] texts)
    {
        m_Text = texts;
    }

    /// <summary>
    /// Включить интерком
    /// </summary>
    public void EventOn() 
    { 
        if (isActive && coroutine == null) 
            coroutine = StartCoroutine(CShowDialogs()); 
        isActive = true; 
    }

    /// <summary>
    /// Выключить интерком
    /// </summary>
    public void EventOff() 
    { 
        if (!isActive && coroutine != null) 
            StopCoroutine(coroutine); 
        isActive = false; 
    }
    #endregion

    #region Private Methods
    private void OnEnable()
    {
        if (isActive) EventOn();
    }

    private void OnDisable()
    {
        EventOff();
    }

    private IEnumerator CShowDialogs()
    {
        bool showText = true;
        while (true)
        {
            if (isActive)
            {
                for (int i = 0; i < m_Text.Length; i++)
                {
                    yield return new WaitForSeconds(m_PauseSec);

                    string t = m_Text[i];
                    float duration_sound = 0.0f;

                    if (t != null)
                    {
                        // Звук интеркома
                        AudioClip sound = Resources.Load<AudioClip>($"Voices/{GameManager.GameSettings.lang}/{t}");
                        if (sound == null && GameManager.GameSettings.lang != "en")
                        {
                            sound = Resources.Load<AudioClip>($"Voices/en/{t}");
                        }

                        if (sound != null)
                        {
                            for (int j = 0; j < m_AudioSources.Length; j++)
                            {
                                m_AudioSources[j].clip = sound;
                                m_AudioSources[j].Play();
                            }

                            duration_sound = sound.length + m_PauseSec;
                        }

                        // Текст интеркома
                        if (showText)
                        {
                            string translate_text = "Localized text not found";
                            if (LocalizationManager.IsReady)
                            {
                                translate_text = LocalizationManager.GetLocalizedValue(t);
                            }
                            m_TextDialog.text = translate_text;
                            m_TextDialog.canvasRenderer.SetAlpha(1.0f);
                            m_TextDialog.CrossFadeAlpha(0.0f, duration_sound, false);
                        }
                    }

                    yield return new WaitForSeconds(duration_sound);
                }
                showText = false;

                loopCounter++;
                if (m_CountTextLoop > 0 && m_CountTextLoop <= loopCounter)
                {
                    gameObject.SetActive(false);
                }
            }
            yield return new WaitForSeconds(0.5f);            
        }
    }
    #endregion
}
