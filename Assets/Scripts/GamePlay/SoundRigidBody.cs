using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SoundRigidBody : MonoBehaviour
{
    [SerializeField] private AudioClip[] m_HitSounds = null;
    [SerializeField] private float m_MinHitVelocity = 0.5f;

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time < 0.1f) return;
        if (m_HitSounds.Length <= 0) return;

        float power = collision.relativeVelocity.sqrMagnitude;

        if (collision.gameObject != null && power >= m_MinHitVelocity * m_MinHitVelocity)
        {
            int n = Random.Range(1, m_HitSounds.Length);
            float volume = Mathf.Clamp(power, m_MinHitVelocity, 10) * 0.1f; // Громкость удара

            GameManager.PlaySoundAtPosition(m_HitSounds[n], volume, transform.position);
        }
    }
}
