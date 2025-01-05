using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDisable : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float m_TimeSeconds = 1f;

    private void OnEnable()
    {
        StartCoroutine(CRun());
    }

    private IEnumerator CRun()
    {
        yield return new WaitForSeconds(m_TimeSeconds);
        gameObject.SetActive(false);
    }
}
