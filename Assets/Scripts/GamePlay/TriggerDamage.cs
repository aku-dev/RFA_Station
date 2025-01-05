using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerDamage : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float m_Damage = 5.0f;
    [SerializeField] private float m_DamageFreq = 1.0f;

    private float lastDamage = -1;

    private void OnTriggerStay(Collider other)
    {
        if (isActiveAndEnabled && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            float t = Time.time;
            if (t - lastDamage > m_DamageFreq || lastDamage == -1)
            {
                PlayerController.Instance.Damage(m_Damage, other.ClosestPoint(transform.position));
                lastDamage = t;
            }
        }
    }
}
