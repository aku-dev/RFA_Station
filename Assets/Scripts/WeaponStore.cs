using UnityEngine;

public static class WeaponStore
{
    /// <summary>
    /// Установить общее количество патронов типа
    /// </summary>
    /// <param name="objectArray">хранилище</param>
    /// <param name="t">тип пуль</param>
    /// <param name="v">количество</param>
    /// <param name="add">добавить а не заменить количество</param>
    /// <returns></returns>
    public static bool SetStore<T>(this SBulletStore[] objectArray, EBulletType t, int v, bool add = false)
    {
        for (int i = 0; i < objectArray.Length; i++)
        {
            if (objectArray[i].Type == t)
            { 
                 objectArray[i].Count = (add) ? objectArray[i].Count + v : v;
                return true; 
            }
        }
        return false;
    }

    /// <summary>
    /// Получить общее количество патронов из хранилища
    /// </summary>
    /// <param name="objectArray">хранилище</param>
    /// <param name="t">тип пуль</param>
    /// <returns></returns>
    public static SBulletStore GetStore<T>(this SBulletStore[] objectArray, EBulletType t)
    {
        for (int i = 0; i < objectArray.Length; i++)
        {
            if (objectArray[i].Type == t) return objectArray[i];
        }
        return null;
    }

    /// <summary>
    /// Вернет номер слота пушки
    /// </summary>
    /// <param name="w">Scriptable Object Weapon</param>
    /// <returns>номер</returns>
    public static int GetNumerSlot(WeaponData w)
    {
        switch (w.name)
        {
            case "FLH14": return 1;
            case "MU615": return 2;
            case "MAK5": return 3;
        }
        return 0;
    }

    public static SWeaponItem GetWeaponFromArray(SWeaponItem[] objectArray, int numpad)
    {
        for (int i = 0; i < objectArray.Length; i++)
        {
            if (objectArray[i].Weapon != null)
                switch (numpad)
                {
                    case 1:
                        if (objectArray[i].Weapon.name == "FLH14") return objectArray[i];
                        break;
                    case 2:
                        if (objectArray[i].Weapon.name == "MU615") return objectArray[i];
                        break;
                    case 3:
                        if (objectArray[i].Weapon.name == "MAK5") return objectArray[i];
                        break;
                }
        }
        return null;
    }
}

/// <summary>
/// Тип пушки
/// </summary>
public enum EWeaponType
{
    Rifle,
    Gun,
    Сrowbar
}

/// <summary>
/// Тип пули
/// </summary>
public enum EBulletType
{
    s45mm,
    b26mm,
    cr9v
}

[System.Serializable]
public class SWeaponItem
{
    public WeaponData Weapon = null; // Пушка
    public int CurrentBullets = 0;   // Патронов в стволе
    public GameObject ObjectScene = null;
}


[System.Serializable]
public class SBulletStore
{
    public EBulletType Type = EBulletType.s45mm; // Тип пули
    public int Count = 0;                        // Количество
    public AudioClip[] ShotSounds = null;        // Звуки выстрела
    public GameObject[] BulletEmptyPrefabs = null; // Массив пустых пуль.
    public GameObject[] BulletStorePrefabs = null; // Массив пустых магазинов.
    public STrail[] BulletTrails = null;           // Летящие шлейфы от пуль.
}

/// <summary>
/// Тип следа от пули
/// </summary>
[System.Serializable]
public class STrail
{
    public GameObject Prefab = null;
    public Transform Trail = null;
    public Vector3 stopPosition = Vector3.zero;
    public float timeSpawn = -1.0f;
}