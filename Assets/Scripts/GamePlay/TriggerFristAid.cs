using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerFristAid : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private GameObject m_OnObject = null;
    [SerializeField] private Animation m_Animation = null;

    [Header("Sounds")]
    [SerializeField] private AudioSource m_Audio = null;
    [SerializeField] private AudioClip m_AddHealth = null;
    [SerializeField] private AudioClip m_NoAddHealth = null;

    private float lastAddHealthTime = -1;

    private void OnTriggerEnter(Collider other)
    {
        if (isActiveAndEnabled && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (m_OnObject != null) m_OnObject.SetActive(true);
            if (m_Animation != null) m_Animation.Play();
            if (PlayerController.Health >= 100.0f)
            {
                m_Audio.clip = m_NoAddHealth;
                m_Audio.loop = false;
                m_Audio.Play();
            }
            else
            {
                m_Audio.clip = m_AddHealth;
                m_Audio.loop = true;
                m_Audio.Play();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActiveAndEnabled && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (PlayerController.Health < 100.0f)
            {
                float t = Time.time;
                if (t - lastAddHealthTime > 1.5f)
                {
                    PlayerController.Health += 10.0f;
                    lastAddHealthTime = t;
                }                
            }
            else
            {
                if (m_OnObject != null) m_OnObject.SetActive(false);
                if (m_Animation != null && m_Animation.isPlaying) m_Animation.Stop();
                if (m_Audio.isPlaying && m_Audio.loop == true) m_Audio.Stop();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isActiveAndEnabled && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (m_OnObject != null) m_OnObject.SetActive(false);
            if (m_Animation != null) m_Animation.Stop();
            m_Audio.Stop();
        }
    }
}
