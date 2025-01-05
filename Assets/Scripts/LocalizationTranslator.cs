/* =======================================================================================================
 * AK Studio
 * 
 * Version 3.0 by Alexandr Kuznecov
 * 18.04.2023
 * =======================================================================================================
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationTranslator : MonoBehaviour
{
    [SerializeField] private Text[] m_Texts = null;

    private readonly Dictionary<int, string> textsDictionary = new Dictionary<int, string>(); // Массив данных для перевода id, name
    private bool isFullDictionary = false; // Заполнен ли словарь

    private void OnEnable()
    {
        if (!isFullDictionary)
        {
            foreach (Text t in m_Texts) 
                textsDictionary.Add(t.GetInstanceID(), t.text);
            isFullDictionary = true;
        }

        StartCoroutine(CRunUpdate());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void UpdateTranslation()
    {
        if (textsDictionary.Count < 1) return;

        foreach (Text t in m_Texts)
        {
            if (textsDictionary[t.GetInstanceID()] != null)
            {
                t.text = LocalizationManager.GetLocalizedValue(textsDictionary[t.GetInstanceID()]);
            }
        }
    }

    private IEnumerator CRunUpdate()
    {
        while (GameManager.Instance == null || !LocalizationManager.IsReady)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }

        UpdateTranslation();
    }

}
