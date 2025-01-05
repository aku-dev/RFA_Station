using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TinyJson;

public sealed class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;
    public static bool IsReady { get; private set; } = false;

    private Dictionary<string, string> localizedText = new Dictionary<string, string>();    
    private readonly string missingTextString = "Localized text not found";
    private string language = "en";
	private bool firstStart = true;

    /// <summary>
    /// Установить язык
    /// </summary>
    /// <param name="lang">ru, en</param>
    public static void SetLanguage(string lang) { Instance.EventSetLanguage(lang); }
    public void EventSetLanguage(string lang)
    {
        if (firstStart || lang != language)
        {
			firstStart = false;
            IsReady = false;
            language = lang;
            LoadLanguageData();
        }
    }

    /// <summary>
    /// Вернуть строку из файла языка
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>значение на текущем языке</returns>
    public static string GetLocalizedValue(string key) { return Instance.EventGetLocalizedValue(key); }
    public string EventGetLocalizedValue(string key)
    {
        string result = missingTextString;
        if (IsReady == false)
        {
            return result;
        }

        if (localizedText.ContainsKey(key))
        {
            result = localizedText[key];
        }
        else
        {
            Debug.Log($"Localized text not found for key: {key}");
        }

        return result;
    }


    /// <summary>
    /// Вернуть колиичество символов
    /// </summary>
    /// <param name="key">ключ</param>
    /// <returns>символов/returns>
    public static int GetLocalizedValueSize(string key) { return Instance.EventGetLocalizedValueSize(key); }
    public int EventGetLocalizedValueSize(string key)
    {
        if (IsReady == false)
        {
            return 0;
        }

        if (localizedText.ContainsKey(key))
        {
            return localizedText[key].Length;
        }

        return 0;
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
        //LoadLanguageData();
    }

    private void LoadLanguageData()
    {
        localizedText = new Dictionary<string, string>();
        string filePath = Path.Combine(Application.streamingAssetsPath, $"{language}.json");

        if (File.Exists(filePath))
        {
            string data = File.ReadAllText(filePath);

            LocalizationValues loadedData = data.FromJson<LocalizationValues>();

            for (int i = 0; i < loadedData.values.Length; i++)
            {
                localizedText.Add(loadedData.values[i].key, loadedData.values[i].value);
            }

            Debug.Log($"Data loaded, file: {language}.json, dictionary contains: {localizedText.Count} entries");
        }
        else
        {
            Debug.LogError($"Cannot find file! {filePath}");
            language = "en";
        }

        IsReady = true;
    }
}

[System.Serializable]
public class LocalizationValues
{
    public LocalizationValue[] values;
}

[System.Serializable]
public class LocalizationValue
{
    public string key;
    public string value;
}
