using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectTimer : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float m_TimeSeconds = 1f;
    [SerializeField] private GameObject m_ObjectOn = null;
    [SerializeField] private GameObject m_ObjectOff = null;
    [SerializeField] private UnityEvent m_Events = new UnityEvent();

    private Coroutine coroutine = null;

    public void Run()
    {
        if (m_ObjectOn != null) m_ObjectOn.SetActive(true);
        if (m_ObjectOff != null) m_ObjectOff.SetActive(false);
        m_Events.Invoke();
    }

    private void OnEnable()
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(CRun());
    }

    private void OnDisable()
    {
        if (coroutine != null) StopCoroutine(coroutine);
    }


    private IEnumerator CRun()
    {
        yield return new WaitForSeconds(m_TimeSeconds);
        Run();
    }
}
