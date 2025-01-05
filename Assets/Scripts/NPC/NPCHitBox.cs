using UnityEngine;

public class NPCHitBox : MonoBehaviour, IDestructible
{
    [SerializeField] private MonoBehaviour m_NPC = null;
    [Tooltip("Коэфицент дамага.")]
    [SerializeField] private float m_Coefficient = 2.0f;

    private IDestructible controller = null;
    private void Start()
    {
        controller = m_NPC.GetComponent<IDestructible>();
    }

    public void Hit(RaycastHit hit, float damage)
    {
        GameManager.GameStatistic.bullet_critical++;
        controller.Hit(hit, damage * m_Coefficient);
    }
}
