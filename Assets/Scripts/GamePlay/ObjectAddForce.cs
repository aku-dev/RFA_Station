using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ObjectAddForce : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Vector3 m_Target = Vector3.zero;
    [SerializeField] private float m_Force = 8.0f;

    private Rigidbody m_Rigidbody = null;

    private void OnEnable()
    {
        if (m_Rigidbody == null) m_Rigidbody = GetComponent<Rigidbody>();
        if (m_Rigidbody != null)
        {
            Vector3 direction = m_Target;
            if (direction == Vector3.zero)
            {
                direction = new Vector3(Random.Range(0.0f, 0.9f), Random.Range(0.0f, 0.9f), Random.Range(0.0f, 0.9f));
            }
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.AddForce(direction * m_Force, ForceMode.Impulse);
        }
    }
}
