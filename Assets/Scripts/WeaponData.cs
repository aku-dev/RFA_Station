using UnityEngine;

[CreateAssetMenu(fileName = "New WeaponData", menuName = "Weapon Data", order = 51)]
public class WeaponData : ScriptableObject
{
    [Header("Properties")]
    [Tooltip("Уникальное имя.")]
    [SerializeField] private string _WeaponName = "Weapon_1";
    public string WeaponName => _WeaponName;

    [Tooltip("Тип оружия, винтовка или пистолет. В зависимости от этого подключиться нужный модуль.")]
    [SerializeField] private EWeaponType _Type = EWeaponType.Rifle;
    public EWeaponType Type => _Type;

    [Tooltip("Тип пуль.")]
    [SerializeField] private EBulletType _BulletType = EBulletType.s45mm;
    public EBulletType BulletType => _BulletType;

    [Tooltip("Автоматический режим.")]
    [SerializeField] private bool _Auto = true;
    public bool Auto => _Auto;

    [Tooltip("Размер магазина патронов.")]
    [SerializeField] private int _Magazine = 8;
    public int Magazine => _Magazine;

    [Tooltip("Скорострельность. Длительность анимации разброса расчитываеться по формуле Speed * Magazine = Animation Seconds")]
    [SerializeField] private float _Speed = 0.5f;
    public float Speed => _Speed;

    [Tooltip("Наносимый урон от одной пули.")]
    [SerializeField] private float _Damage = 10.0f;
    public float Damage => _Damage;

    [Tooltip("Масса оружия в кг, влияет на скорость ходьбы. диапазон 1кг-10кг")]
    [SerializeField] private float _Mass = 10.0f;
    public float Mass => _Mass;

    [Tooltip("Величина разброса минимальная, при недодвижной стрельбе или прицеливании.")]
    [SerializeField] private float _SprayWeightMin = 5.0f;
    public float SprayWeightMin => _SprayWeightMin;

    [Tooltip("Величина разброса в ширину при максимальных значениях.")]
    [SerializeField] private float _SprayWeightMax = 50.0f;
    public float SprayWeightMax => _SprayWeightMax;

    [Tooltip("Величина разброса в ширину при движении.")]
    [SerializeField] private float _SprayWeightMove = 20.0f;
    public float SprayWeightMove => _SprayWeightMove;

    [Tooltip("Шаг увеличения разброса при выстреле.")]
    [SerializeField] private float _SprayStepAdd = 5.0f;
    public float SprayStepAdd => _SprayStepAdd;

    [Tooltip("Шаг уменьшения разброса после выстрела или бега.")]
    [SerializeField] private float _SprayStepDec = 1.0f;
    public float SprayStepDec => _SprayStepDec;

    [Tooltip("Высота разброса. Нормализованное значение где 1 = Ширина.")]
    [Range(0.01f, 2f)]
    [SerializeField] private float _SprayHeight = 0.39f;
    public float SprayHeight => _SprayHeight;

}
