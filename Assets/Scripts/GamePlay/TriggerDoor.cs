using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerDoor : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Animation m_Animation = null;
    [SerializeField] private AudioClip m_LockedSound = null;
    [SerializeField] private string m_LockedText = null;
    [SerializeField] private GameObject m_LockedObjectOn = null;
    [SerializeField] private GameObject m_LockedObjectOff = null;
    [SerializeField] private bool m_IsLock = false;
    [SerializeField] private bool m_IsOpen = false;

    [SerializeField] private UnityEvent m_Events = new UnityEvent();

    public void SetLock(bool b)
    {
        m_IsLock = b;
        if (m_IsLock && m_IsOpen)
        {
            m_Animation.PlayQueued("Door_Close", QueueMode.PlayNow);
        }
    }

    public bool GetLock()
    {
        return m_IsLock;
    }

    public bool IsOpen { get { return m_IsOpen; } private set { } }

    private void OnTriggerEnter(Collider other)
    {
        if (isActiveAndEnabled) // && other.gameObject.layer == LayerMask.NameToLayer("Player")
        {
            if (!m_IsLock)
            {
                if (!m_IsOpen) 
                    m_Animation.PlayQueued("Door_Open", QueueMode.PlayNow);

                m_IsOpen = true;
            }
            else
            {
                if (m_LockedText != null)
                    GameManager.ShowDialogText(m_LockedText);                
                if (m_LockedSound != null) 
                    GameManager.PlaySoundAtPosition(m_LockedSound, 1.0f, transform.position);
                if (m_LockedObjectOn != null)
                    m_LockedObjectOn.SetActive(true);
                if (m_LockedObjectOff != null)
                    m_LockedObjectOff.SetActive(false);
            }

            m_Events?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isActiveAndEnabled && other.gameObject.layer == LayerMask.NameToLayer("Player") && m_IsOpen)
        {
            if (m_IsOpen) 
                m_Animation.PlayQueued("Door_Close", QueueMode.CompleteOthers);

            m_IsOpen = false;
        }
    }
}
