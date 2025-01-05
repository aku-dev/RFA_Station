using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class TriggerDialog : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private string[] m_Text = null;
    [Range(0.01f, 10.0f)]
    [SerializeField] private float m_PauseSec = 0.01f;
    [SerializeField] private bool m_SelfDisable = true;

    private AudioSource as_Audio = null;
    private Coroutine coroutine = null;

    private void Start()
    {
        as_Audio = GetComponent<AudioSource>();
    }

    private void OnDisable()
    {
        if (coroutine != null) StopCoroutine(coroutine);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < 0.5f) return;

        if (isActiveAndEnabled && coroutine == null && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("TriggerDialog.OnTriggerEnter()");
            coroutine = StartCoroutine(CShowDialogs());            
        }
    }

    private IEnumerator CShowDialogs()
    {
        bool showText = true;
        while (!m_SelfDisable)
        {
            for (int i = 0; i < m_Text.Length; i++)
            {
                yield return new WaitForSeconds(m_PauseSec);

                string t = m_Text[i];

                if (t != null)
                {
                    GameManager.ShowDialogText(t, 0, as_Audio, showText);
                }

                yield return new WaitForSeconds(GameManager.DialogDuration);
            }
            showText = false;
        }

        gameObject.SetActive(false);
    }
}
