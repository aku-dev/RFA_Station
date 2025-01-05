using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_ObjectParticleSystem = null;
    [SerializeField] private GameObject m_ObjectGameObject = null;

    private new Animation animation = null;
    private new AudioSource audio = null;

    private void Start()
    {
        animation = GetComponent<Animation>();
        audio = GetComponent<AudioSource>();
    }

    public void Rewind(float t)
    {
        animation[animation.clip.name].time = t;
    }

    public void PlayParticleSystem()
    {
        m_ObjectParticleSystem.Play();
    }

    public void MoveGameObject()
    {
        m_ObjectGameObject.transform.position = transform.position;
        m_ObjectGameObject.transform.rotation = transform.rotation;
    }

    public void PlaySound(AudioClip a)
    {
        audio.PlayOneShot(a);
    }

    public void PlayPositionSound(AudioClip a)
    {
        GameManager.PlaySoundAtPosition(a, 1.0f, transform.position);
    }

    public void PlayPlayerSound(AudioClip a)
    {
        PlayerController.Instance.PlaySound(a);
    }
}
