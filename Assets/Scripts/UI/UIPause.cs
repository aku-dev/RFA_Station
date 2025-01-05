using System.Collections;
using UnityEngine;

public class UIPause : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(CRunPause());
    }

    private void OnDisable()
    {
        GameManager.Instance.UnPause();
        GameManager.onPause = false;
    }

    private IEnumerator CRunPause()
    {
        while (GameManager.Instance == null)
        {
            yield return new WaitForEndOfFrame();
        }

        GameManager.Instance.Pause();
        GameManager.onPause = true;
    }
}
