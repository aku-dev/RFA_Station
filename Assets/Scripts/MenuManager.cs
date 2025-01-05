/* =======================================================================================================
 * AK Studio
 * 
 * Version 3.0 by Alexandr Kuznecov
 * 28.05.2023
 * =======================================================================================================
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Dropdown m_DropdownQuality = null;
    [SerializeField] private Slider m_SliderMusic = null;
    [SerializeField] private Slider m_SliderEffects = null;
    [SerializeField] private Slider m_SliderMouse = null;
    [SerializeField] private Toggle m_ToggleInvert = null;
    [SerializeField] private Toggle m_ToggleVsync = null;

    private bool isBusy = true;
    private Dictionary<int, string> OptionValues = new Dictionary<int, string>();

    private void OnEnable()
    {
        isBusy = true;
        m_SliderMusic.value = GameManager.GameSettings.music;
        m_SliderEffects.value = GameManager.GameSettings.effects;        
        m_SliderMouse.value = GameManager.GameSettings.mouse;        

        m_ToggleInvert.isOn = GameManager.GameSettings.invertmousey;
        m_ToggleVsync.isOn = GameManager.GameSettings.vsync;

        m_DropdownQuality.options.Clear();
        OptionValues.Clear();
        OptionValues = new Dictionary<int, string>
        {
            {0, LocalizationManager.GetLocalizedValue("quality_value_0")},
            {1, LocalizationManager.GetLocalizedValue("quality_value_1")},
            {2, LocalizationManager.GetLocalizedValue("quality_value_2")},
            {3, LocalizationManager.GetLocalizedValue("quality_value_3")},
            {4, LocalizationManager.GetLocalizedValue("quality_value_4")},
            {5, LocalizationManager.GetLocalizedValue("quality_value_5")},
        };

        foreach (KeyValuePair<int, string> s in OptionValues)
        {
            m_DropdownQuality.options.Add(new Dropdown.OptionData(s.Value));
            if (s.Key == GameManager.GameSettings.quality)
            {
                m_DropdownQuality.value = m_DropdownQuality.options.Count;
            }
        }
        StartCoroutine(CRunPause());
    }

    private void OnDisable()
    {
        OnSave();
        GameManager.Instance.UnPause();
        GameManager.onPause = false;
    }

    /// <summary>
    /// Выход из игры
    /// </summary>
    public void OnGameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnSave()
    {
        if (Time.time > 0 && !isBusy)
            GameManager.Instance.ApplySettings(true);
    }

    /// <summary>
    /// Установка языка
    /// </summary>
    /// <param name="lang"></param>
    public void OnSetLang(string lang)
    {
        if (isBusy) return;
        LocalizationManager.SetLanguage(lang);
        GameManager.GameSettings.lang = lang;
        GameManager.GameSettings.Save();
    }

    public void OnSetQuality()
    {
        if (isBusy) return;
        foreach (KeyValuePair<int, string> s in OptionValues)
        {
            if (s.Value == m_DropdownQuality.options[m_DropdownQuality.value].text)
            {
                GameManager.GameSettings.quality = s.Key;
                break;
            }
        }
        OnSave();
    }

    public void OnSetMouse()
    {
        if (isBusy) return;
        GameManager.GameSettings.mouse = m_SliderMouse.value;
    }

    public void OnSetMusic()
    {
        if (isBusy) return;
        GameManager.Instance.ApplyMusicVolume(m_SliderMusic.value);
    }

    public void OnSetEffects()
    {
        if (isBusy) return;
        GameManager.GameSettings.effects = m_SliderEffects.value;
    }

    public void OnSetInvert()
    {
        if (isBusy) return;
        GameManager.GameSettings.invertmousey = m_ToggleInvert.isOn;
        OnSave();
    }

    public void OnSetVSync()
    {
        if (isBusy) return;
        GameManager.GameSettings.vsync = m_ToggleVsync.isOn;
        OnSave();
    }

    private IEnumerator CRunPause()
    {
        while (GameManager.Instance == null)
        {
            yield return new WaitForEndOfFrame();
        }

        GameManager.Instance.Pause();
        GameManager.onPause = true;
        isBusy = false;
    }
}
