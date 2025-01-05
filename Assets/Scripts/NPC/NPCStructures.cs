/* =======================================================================================================
 * AK Studio
 * 
 * Version 2.0 by Alexandr Kuznecov
 * 06.01.2023
 * =======================================================================================================
 */

using UnityEngine;

public static class NPCUtils
{
    /// <summary>
    /// Огонь врага
    /// </summary>
    /// <param name="tr">трансформ дула оружия</param>
    /// <param name="damage">урон игроку</param>
    /// <param name="btype">тип шлейфа от пули</param>
    public static void Fire(Transform tr, float damage, EBulletType btype)
    {
        int playerMask = LayerMask.GetMask("Default", "Player");
        // Выстрел
        if (Physics.Raycast(tr.position, tr.forward, out RaycastHit h, 50.0f, playerMask))
        {
            // Шлейф от пули.
            GameManager.SpawnBulletTrail(btype, tr.position, h.point);

            // Игрока дамажим иначе спауним дырку
            PlayerController player = h.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Damage(damage, h.point);                
            }
            else
            {
                GameManager.SpawnBulletHole(h.point, Quaternion.FromToRotation(Vector3.back, h.normal), h.collider.transform);
            }
        }
    }
}

public enum EState
{
    Idle,
    Wander,
    Attack,
    Wait,
    Move,
    Death
}

interface IDestructible
{
    void Hit(RaycastHit hit, float damage);
}

interface INPC
{
    float Health { get; set; }
    float LastDamageTime { get;  }
    bool IsActive { get; set; }
    EState State { get; set; }

    void Death();
    void WakeUp();
    GameObject gameObject { get; }
}

[System.Serializable]
public class SDestructibleObjects
{
    public GameObject FirePoint;
    public GameObject Functioning;
    public GameObject NoFunctioning;
}