using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerEnable : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameObject m_ObjectOn = null;
    [SerializeField] private GameObject m_ObjectOff = null;
    [SerializeField] private Animation m_Animation = null;
    [SerializeField] private UnityEvent m_Events = new UnityEvent();
    [SerializeField] private bool m_ExitActions = false;
    [SerializeField] private bool m_SelfDisable = false;

    private void OnEnable()
    {
        if (m_ExitActions)
        {
            if (m_ObjectOn != null) m_ObjectOn.gameObject.SetActive(false);
            if (m_ObjectOff != null) m_ObjectOff.gameObject.SetActive(true);
            if (m_Animation != null) m_Animation.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActiveAndEnabled && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (m_ObjectOn != null) m_ObjectOn.gameObject.SetActive(true);
            if (m_ObjectOff != null) m_ObjectOff.gameObject.SetActive(false);
            if (m_Animation != null) m_Animation.Play();
            m_Events.Invoke();

            if (m_SelfDisable) gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isActiveAndEnabled && m_ExitActions && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (m_ObjectOn != null) m_ObjectOn.gameObject.SetActive(false);
            if (m_ObjectOff != null) m_ObjectOff.gameObject.SetActive(true);
            if (m_Animation != null) m_Animation.Stop();

        }
    }
}
