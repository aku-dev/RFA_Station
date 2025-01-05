using UnityEngine;
using UnityEngine.Events;

public class ObjectEnable : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameObject m_ObjectOn = null;
    [SerializeField] private GameObject m_ObjectOff = null;
    [SerializeField] private ParticleSystem m_ParticleSystem = null;
    [SerializeField] private Animation m_Animation = null;
    [SerializeField] private AudioClip m_Sound = null;
    [SerializeField] private UnityEvent m_Events = new UnityEvent();

    private void OnEnable()
    {
        if (Time.time <= 0) return;
        if (m_ObjectOn != null) m_ObjectOn.SetActive(true);
        if (m_ObjectOff != null) m_ObjectOff.SetActive(false);
        if (m_ParticleSystem != null) m_ParticleSystem.Play();
        if (m_Animation != null) m_Animation.Play();
        if (m_Sound != null) GameManager.PlaySoundAtPosition(m_Sound, 1.0f, transform.position);
        m_Events.Invoke();
    }
}
