/* =======================================================================================================
 * AK Studio
 * 
 * Version 1.0 by Alexandr Kuznecov
 * 21.05.2023
 * =======================================================================================================
 */
using UnityEngine;
using UnityEngine.Events;

public class UIKey : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private string m_Key = "";
    [SerializeField] private GameObject m_ObjectOn = null;
    [SerializeField] private GameObject m_ObjectOff = null;
    [SerializeField] private UnityEvent m_Events = new UnityEvent();

    private void Update()
    {
        if (m_Key != "" && Input.GetButtonDown(m_Key))
        {
            if (m_ObjectOn != null) m_ObjectOn.SetActive(true);
            if (m_ObjectOff != null) m_ObjectOff.SetActive(false);
            m_Events.Invoke();
        }
    }
}
