/* =======================================================================================================
 * AK Studio
 * 
 * Version 2.0 by Alexandr Kuznecov
 * 06.01.2023
 * =======================================================================================================
 */

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
public class ComponentRifle : MonoBehaviour
{
    #region Editor Properties
    [Header("Properties")]
    [SerializeField] private Animator m_HandsAnimator = null;
    [SerializeField] private Animation m_CameraAnimation = null;
    [SerializeField] private GameObject m_UIBulletsPanel = null;
    [SerializeField] private Text m_UIBullets = null;
    [SerializeField] private RectTransform m_UICross = null;
    [SerializeField] private Image m_CrossLeft = null;
    [SerializeField] private Image m_CrossRight = null;

    [Header("Weapons")]
    [SerializeField] private Text[] m_UIBulletCounts = null;
    [SerializeField] private Image[] m_UIBulletImages = null;

    [Header("Objects")]
    [SerializeField] private GameObject[] m_PlayerRifles = null;
    #endregion

    #region Fields
    public static ComponentRifle Instance = null;
    public static bool Reload { get { return Instance.onReload; } set { Instance.onReload = value;  } }
    public static bool IsActive { get { return Instance.isActive; } set { Instance.isActive = value; } }
    #endregion

    #region Private Values
    private const float MOVEMENT_THRESHOLD = 5.0f;  // Минимальный порог движения пресонажа, при котором увеличиваеться прицел. 
    private const float CROSS_MOVE_SPEED = 15.0f;   // Скорость анимации прицела
    private const float CROSS_BACK_SPEED = 10.0f;   // Скорость возврата в начальную позицию
    private const float RAYCAST_DISTANCE = 1000.0f; // Дальность райкаста
    private const string NO_WEAPON_IDLE = "Base Layer.No_Weapon_Idle";

    public bool isActive = false;
    private bool onFire = false;
    private bool onClick = false; // Обработка клика для автоматического и ручного оружия.
    private bool onReload = false;
    private bool onUpdatePlayerRifles = false;
    private WeaponData W = null;
    private SWeaponItem currentWeapon = null;
    private AudioClip[] shotSounds = null;

    private FastRandom Rand = new FastRandom();
    
    private int layerMask = -1;
    private float lastTimeFire = 1.0f;

    private float currentSpread = 0.0f; // Текущий разброс

    private Transform tWeapon = null;       // Позичия пушки
    private Transform tMuzzle = null;       // Позичия ствола, должно быть дочерним элеметном с названием Muzzle
    private Transform tBulletOut = null;    // Позиция вылетания пуль, должно быть дочерним элеметном с названием Bullet_Out
    private GameObject DropObject = null;
    #endregion

    #region Public Methods
    /// <summary>
    /// Установить пушку в компонент
    /// </summary>
    /// <param name="weapon"></param>
    public static void SetWeapon(SWeaponItem weapon)
    {
        if (Instance.m_CameraAnimation.isPlaying)
            Instance.m_CameraAnimation.Stop();

        if (Instance.W != null && Instance.m_HandsAnimator != null)
        {
            Instance.Off();
        }

        if (weapon != null)
        {
            Instance.currentWeapon = weapon;
            Instance.W = weapon.Weapon;
            Instance.shotSounds = PlayerController.BulletStore.GetStore<SBulletStore[]>(Instance.W.BulletType).ShotSounds;
            Instance.On();
        }

        Instance.onUpdatePlayerRifles = true;
    }

    /// <summary>
    /// Возвращает текущую пушку.
    /// </summary>
    /// <returns></returns>
    public static SWeaponItem GetWeapon() { return Instance.currentWeapon; }

    /// <summary>
    /// Возвращает имя текущей пушки.
    /// </summary>
    /// <returns></returns>
    public static string GetWeaponName() { return (Instance.currentWeapon != null) ? Instance.currentWeapon.Weapon.WeaponName : ""; }

    /// <summary>
    /// Выброс пушки
    /// </summary>
    /// <param name="drop"></param>
    public static void DropWeapon(GameObject drop)
    {
        ObjectItem oitem = drop.GetComponentInChildren<ObjectItem>(true);
        if (oitem != null)
        {
            oitem.SetCurrentBullets(Instance.currentWeapon.CurrentBullets);
        }

        Instance.DropObject = drop;
        Instance.Off();
    }

    /// <summary>
    /// Обновить статистику оружия
    /// </summary>
    public static void UpdateHUD()
    {
        if (Instance.W != null && Instance.currentWeapon != null)
        {
            Instance.m_UICross.gameObject.SetActive(true);
            Instance.m_UIBulletsPanel.SetActive(true);

            // Иконка пулей
            foreach(Image img in Instance.m_UIBulletImages)
            {
                if (img.name == Instance.W.name)
                {
                    img.gameObject.SetActive(true);
                }
                else
                {
                    img.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Instance.m_UICross.gameObject.SetActive(false);
            Instance.m_UIBulletsPanel.SetActive(false);
        }

        // Количество пуль
        foreach (SWeaponItem item in PlayerController.PlayerStore)
        {
            if (item.Weapon != null)
            {
                int total = PlayerController.BulletStore.GetStore<SBulletStore[]>(item.Weapon.BulletType).Count;
                string count = (total + item.CurrentBullets).ToString();

                // Количество патронов под пушкой
                switch (item.Weapon.BulletType)
                {
                    case EBulletType.s45mm: Instance.m_UIBulletCounts[0].text = count; break;
                    case EBulletType.b26mm: Instance.m_UIBulletCounts[1].text = count; break;
                    case EBulletType.cr9v: Instance.m_UIBulletCounts[2].text = count; break;
                }

                // Текущая пушка
                if (Instance.W != null && item.Weapon.BulletType == Instance.W.BulletType)
                {
                    Instance.m_UIBullets.text = $"{Instance.currentWeapon.CurrentBullets} / {total}";
                }
            }
        }
    }
    #endregion

    #region Private Methods
    private void Awake()
    {
        // Синглтон
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // Init
        layerMask = LayerMask.GetMask("Player", "Ignore Raycast", "Player Wall");
        layerMask = ~layerMask;
        m_UIBulletsPanel.SetActive(false);
    }

    private void OnDisable()
    {
        isActive = false;
    }

    private void On()
    {
        isActive = true;
        currentSpread = W.SprayWeightMove;
        UpdateHUD();
    }

    private void Off()
    {
        if (m_HandsAnimator != null)
            m_HandsAnimator.SetBool($"Weapon_{W.WeaponName}", false);

        // UI
        if (m_UIBulletsPanel != null) m_UIBulletsPanel.SetActive(false);
        if (m_UICross != null) m_UICross.gameObject.SetActive(false);

        tMuzzle = null;
        tBulletOut = null;
        isActive = false;
        onReload = false;
        
        currentWeapon = null;
        W = null;
        onUpdatePlayerRifles = true;
    }

    private void Update()
    {
        float t = Time.time;
        AnimatorStateInfo asi = m_HandsAnimator.GetCurrentAnimatorStateInfo(0);

        if (onUpdatePlayerRifles && asi.IsName(NO_WEAPON_IDLE))
        {
            
            foreach (GameObject o in m_PlayerRifles)
            {
                if (W != null && o.name == W.WeaponName)
                {
                    o.SetActive(true);

                    // Найдем дочерние элементы в текущей пушке
                    tWeapon = o.transform;
                    Transform[] childs = o.GetComponentsInChildren<Transform>(true);
                    foreach (Transform tr in childs)
                    {
                        switch (tr.name)
                        {
                            case "Muzzle": tMuzzle = tr; break;
                            case "Bullet_Out": tBulletOut = tr; break;
                        }
                    }
                }
                else
                {
                    // Отключим пушку
                    o.SetActive(false);
                }
            }

            if (W != null) m_HandsAnimator.SetBool($"Weapon_{W.WeaponName}", true);

            // Выбросить пушку
            if(Instance.DropObject != null)
            {
                Instance.DropObject.SetActive(true);
                Instance.DropObject.transform.position = transform.position + transform.up;
                Rigidbody rbody = Instance.DropObject.GetComponent<Rigidbody>();
                if (rbody != null)
                {
                    rbody.AddForce(transform.forward * 3.0f, ForceMode.Impulse);
                    rbody.AddTorque(transform.forward * 0.2f, ForceMode.Impulse);
                }
                Instance.DropObject = null;
            }

            onUpdatePlayerRifles = false;
        }

        if (!isActive) return;

        
        bool fireRunReload = false;
        if (asi.IsName($"Base Layer.{W.WeaponName}.Idle"))
        {
            // Конец перезарядки
            if (onReload)
            {
                int totalBullets = PlayerController.BulletStore.GetStore<SBulletStore[]>(W.BulletType).Count;
                int all = totalBullets + currentWeapon.CurrentBullets;
                if(all > W.Magazine)
                {
                    PlayerController.BulletStore.SetStore<SBulletStore[]>(W.BulletType, all - W.Magazine);
                    currentWeapon.CurrentBullets = W.Magazine;
                } 
                else
                {
                    PlayerController.BulletStore.SetStore<SBulletStore[]>(W.BulletType, 0);
                    currentWeapon.CurrentBullets = all;
                }

                // Кинем магазин                
                GameManager.PoolSpawn(PlayerController.BulletStore.GetStore<SBulletStore[]>(Instance.W.BulletType).BulletStorePrefabs,
                    tWeapon.position + Vector3.down * 0.2f,
                    PlayerController.MainCharacterController.transform.rotation);

                UpdateHUD();
                onReload = false;
            }

            
            if (!onReload && !onFire && t - lastTimeFire > W.Speed)
            {
                bool stopCamera = true;
                if (Input.GetButton("Fire_1"))
                {
                    if (!onClick)
                    {
                        onClick = !W.Auto;
                        onFire = true;
                        if (currentWeapon.CurrentBullets > 0)
                        {
                            // Выстрел
                            m_HandsAnimator.CrossFade($"Base Layer.{W.WeaponName}.Fire", 1.0f, 0, 0.0f, 0.4f); // Длинна анимации 0.25, скорострельность 0.1 transitionNormalTime = 0.4f
                            m_CameraAnimation.Play($"{W.WeaponName}_Spray_Animation");

                            if (currentSpread < W.SprayWeightMax) currentSpread += W.SprayStepAdd;
                            stopCamera = false;
                        }
                        else
                        {
                            // Кончились патроны - запустим перезарядку
                            fireRunReload = true;

                            // Пустой ствол
                            onClick = true;
                            onFire = false;
                            
                            m_HandsAnimator.Play($"Base Layer.{W.WeaponName}.Empty_Fire");
                        }
                        lastTimeFire = t;
                    }
                } 
                else
                {
                    onClick = false;
                    stopCamera = true;
                }

                if (stopCamera)
                {
                    m_CameraAnimation.Stop();
                }
            }
        } // idle

        // Перезарядка
        if (!onReload && currentWeapon.CurrentBullets < W.Magazine && (Input.GetButtonDown("Reload") || fireRunReload))
        {
            if (PlayerController.BulletStore.GetStore<SBulletStore[]>(W.BulletType).Count > 0)
            {
                onReload = true;
                m_HandsAnimator.Play($"Base Layer.{W.WeaponName}.Reload");
                if (m_CameraAnimation.isPlaying) m_CameraAnimation.Stop();
            }
        }

        // Плавный возврат курсора наместо.
        if (!m_CameraAnimation.isPlaying && Mathf.Abs(m_CameraAnimation.transform.localRotation.eulerAngles.x) > 0.001f)
        {
            m_CameraAnimation.transform.localRotation = Quaternion.Lerp(m_CameraAnimation.transform.localRotation,
                                                                        Quaternion.identity,
                                                                        CROSS_BACK_SPEED * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (!isActive) return;
        if (W == null) return;

        float t = Time.time;

        // Лапки курсора
        if(currentSpread < W.SprayWeightMove
                && (PlayerController.MainCharacterController.velocity.sqrMagnitude > MOVEMENT_THRESHOLD 
                || !PlayerController.MainCharacterController.isGrounded))
        {
            currentSpread = W.SprayWeightMove;
        }

        if(currentSpread > W.SprayWeightMin)
        {
            currentSpread -= W.SprayStepDec * Time.fixedDeltaTime;
        }

        // Востановление курсора
        float v = Mathf.Lerp(m_CrossRight.rectTransform.localPosition.x, currentSpread, CROSS_MOVE_SPEED * Time.fixedDeltaTime);
        m_CrossLeft.rectTransform.localPosition = new Vector3(-v, 0, 0);
        m_CrossRight.rectTransform.localPosition = new Vector3(v, 0, 0);

        // Реализация стрельбы
        if (onFire && currentWeapon.CurrentBullets > 0)
        {
            // Реализация разброса
            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
            float r = 0.99f;
            Vector3 randSpread = new Vector3(Rand.Range(-r, r), Rand.Range(-W.SprayHeight, W.SprayHeight), 0);
            Vector3 screenCorrect = (currentSpread - W.SprayStepAdd) * randSpread;

            // Райкаст пули
            if (Physics.Raycast(PlayerController.MainCamera.ScreenPointToRay(screenCenter + screenCorrect), out RaycastHit hit, RAYCAST_DISTANCE, layerMask))
            {
                // Звук
                Utils.PlayRandomSound(PlayerController.AudioSource, shotSounds);

                // Создаем дырку
                GameManager.SpawnBulletHole(hit.point, Quaternion.FromToRotation(Vector3.back, hit.normal), hit.collider.transform);                

                // Создаем гильзу
                if (tBulletOut != null)
                {
                    GameObject bullet = GameManager.PoolSpawn(PlayerController.BulletStore.GetStore<SBulletStore[]>(W.BulletType).BulletEmptyPrefabs,
                        tBulletOut.position,
                        tBulletOut.rotation);

                    if (bullet != null)
                    {
                        Rigidbody rbody = bullet.GetComponent<Rigidbody>();
                        if (rbody != null)
                        {
                            float f = 0.45f;
                            Vector3 random = new Vector3(Rand.Range(-f, f), Rand.Range(-f, f), Rand.Range(-f, f));
                            Transform tr   = PlayerController.MainCharacterController.transform;
                            Vector3 dir    = -tr.right * 1.5f + tr.up + random;

                            rbody.velocity = Vector3.zero;
                            rbody.AddForce(dir, ForceMode.Impulse);
                        }
                    }
                }

                if (tMuzzle != null)
                {
                    // Шлейф позиция
                    GameManager.SpawnBulletTrail(W.BulletType, tMuzzle.transform.position, hit.point);
                }

                // Толкаем предмет
                Rigidbody body = hit.collider.attachedRigidbody;

                if (body != null && !body.isKinematic)
                {
                    // Давление на предмет
                    body.AddForceAtPosition(-hit.normal.normalized * 2.5f, hit.point, ForceMode.Impulse);
                }
                
                bool miss = true; // Засчитаем как промах

                // Отправим дамаг объекту ToDo: Убрать всех монстров в системы и тогда удалить этот метод.
                Component[] hitComponents = hit.collider.GetComponents(typeof(IDestructible));
                if (hitComponents.Length > 0)
                {
                    IDestructible obj = hitComponents[0] as IDestructible;
                    obj.Hit(hit, W.Damage);
                    GameManager.GameStatistic.bullet_hits++;
                    GameManager.GameStatistic.damage += W.Damage;
                    miss = false;
                }

                if (miss)
                {
                    // Отправим дамаг системе
                    Component hitComponent = hit.collider.GetComponentInParent(typeof(IDestructible));
                    if (hitComponent != null)
                    {
                        IDestructible obj = hitComponent as IDestructible;
                        obj.Hit(hit, W.Damage);
                        GameManager.GameStatistic.bullet_hits++;
                        GameManager.GameStatistic.damage += W.Damage;
                        miss = false;
                    }
                }

                if (miss) GameManager.GameStatistic.bullet_misses++;

                GameManager.GameStatistic.bullet_total++;
                currentWeapon.CurrentBullets--;
                UpdateHUD();
            }

            onFire = false;
        }
    }
    #endregion
}
