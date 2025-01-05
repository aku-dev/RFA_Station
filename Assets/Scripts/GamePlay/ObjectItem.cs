using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ObjectItem : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private EItemType m_Type = EItemType.FirstAidKit;
    [SerializeField] private EBulletType m_Bullet = EBulletType.s45mm;
    [SerializeField] private SWeaponItem m_WeaponItem = null;
    [SerializeField] private int m_Count = 1;
    [SerializeField] private AudioClip m_GetSound = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && isActiveAndEnabled)
        {
            switch(m_Type)
            {
                case EItemType.FirstAidKit:
                    //if (PlayerController.Health >= 100)
                        //PlayerController.FirstAidKits += 1;
                    //else
                        PlayerController.Health += 50;
                    break;
                case EItemType.Ammo:
                    PlayerController.BulletStore.SetStore<SBulletStore[]>(m_Bullet, m_Count, true);
                    ComponentRifle.UpdateHUD();
                    break;
                case EItemType.Weapon:
                    SWeaponItem wi = new SWeaponItem
                    {
                        CurrentBullets = m_WeaponItem.CurrentBullets,
                        ObjectScene = m_WeaponItem.ObjectScene,
                        Weapon = m_WeaponItem.Weapon
                    };
                    
                    int n = WeaponStore.GetNumerSlot(wi.Weapon);
                    if (n > 0)
                    {
                        PlayerController.PlayerStore[n - 1] = wi;
                        PlayerController.Instance.SelectSlot(n, true);
                        m_WeaponItem.ObjectScene.SetActive(false);
                        
                    }

                    break;
            }
            if (m_GetSound != null)
                GameManager.PlaySoundAtPosition(m_GetSound, 1.0f, transform.position);
            gameObject.SetActive(false);
        }
    }

    public void SetCurrentBullets(int i)
    {
        m_WeaponItem.CurrentBullets = i;
    }

}

public enum EItemType
{
    FirstAidKit,
    Ammo,
    Weapon,
    Key
}