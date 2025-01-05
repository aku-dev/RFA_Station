using System.Collections;
using UnityEngine;

public class ObjectParticleSound : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_ParticleSystem = null;
    [SerializeField] private AudioClip m_Sound = null;
    [SerializeField] private float m_SelfDisableSec = 1.0f;
    [SerializeField] private bool m_OnEnable = true;

    public void OnPlay()
    {
        if (m_ParticleSystem != null) m_ParticleSystem.Play();        
        if (m_Sound != null) GameManager.PlaySoundAtPosition(m_Sound, 1.0f, transform.position);
        if(m_SelfDisableSec > 0) StartCoroutine(CDisable());
    }

    private void OnEnable()
    {
        if(m_OnEnable) OnPlay();
    }

    private void OnDisable()
    {
        if (m_OnEnable) StopAllCoroutines();
    }

    private IEnumerator CDisable()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(m_SelfDisableSec);
    }
}
