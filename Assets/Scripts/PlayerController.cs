/* =======================================================================================================
 * AK Studio
 * 
 * Version 2.0 by Alexandr Kuznecov
 * 06.01.2023
 * =======================================================================================================
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Editor Properties
    [Header("UI Controls")]
    [SerializeField] private Image m_UIHealth = null;
    [SerializeField] private Text m_UIHealthText = null;
    [SerializeField] private Animation m_BloodOut = null;
    [SerializeField] private Color m_HealthNormal = Color.white;
    [SerializeField] private Color m_HealthCritical = Color.red;

    [SerializeField] private RectTransform m_UIRectDamage = null;
    [SerializeField] private Image m_UIDamage = null;    
    [SerializeField] private Animation m_AUIDamage = null;
    [SerializeField] private Animation m_UIWakeUp = null;
    [SerializeField] private Image[] m_UIStackWeapons = null;
    [SerializeField] private Color m_UIStackActive = Color.white;
    [SerializeField] private Color m_UIStackUnActive = Color.gray;

    [Header("Objects")]
    [SerializeField] private Camera m_Camera = null;
    [SerializeField] private Animation m_CameraAnimation = null;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] m_FootstepSounds = null;
    [SerializeField] private AudioClip[] m_PainSounds = null;
    [SerializeField] private AudioClip[] m_DeathSounds = null;

    [Header("Properties")]
    [SerializeField] private float m_WalkSpeed = 7.0f;
    [SerializeField] private float m_StealSpeed = 2.0f;
    [SerializeField] private float m_JumpHeight = 4.5f;
    [SerializeField] private bool m_GodMode = false;

    [Header("Slots")]
    [SerializeField] private SWeaponItem[] Store = null;    

    [Header("System")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private float health = 100.0f;
    [SerializeField] private SBulletStore[] m_BulletStore = null;

    #endregion

    #region Fields
    public static PlayerController Instance = null;
    public static Camera MainCamera { get { return Instance.m_Camera; } private set { } }
    public static CharacterController MainCharacterController { get { return Instance.m_CharacterController; } private set { } }
    public static SBulletStore[] BulletStore { get { return Instance.m_BulletStore; } set { Instance.m_BulletStore = value; } }
    public static SWeaponItem[] PlayerStore { get { return Instance.Store; } private set { } }

    // Здоровье
    public static float Health { get { return Instance.health; } set { Instance.health = Mathf.Clamp(value, 0, 100); } }
    public static AudioSource AudioSource { get { return Instance.m_AudioSource; } private set { } }

    public static bool IsActive
    {  // Активность, при включении активности обратно нужно сбросить все ускорения управления.
        get { return Instance.isActive; }
        set {
            Instance.isActive = value;
            Instance.axis = Vector2.zero;
            Instance.axis.y = Utils.GetNormalAngle(Instance.m_Camera.transform.localRotation.eulerAngles.x);

            // Костыль: Если игрок оказался живой, вернем ему немного здоровья чтобы работали сохранения.
            if (Instance.health <= 0) { Instance.health = 1.0f; }
        }
    } 

    public static Vector2 Axis
    {
        get { return Instance.axis; }
        set { Instance.axis = value; }
    }

    #endregion

    #region Private Values
    private const float LOOK_SMOOTHING = 2.0f;       // 2.0f
    private const float MOVE_SMOOTHING = 0.001f;     // 0.12 ускорение и замедление, в настройках проекта настройки кнопки не сработает! 0.006f
    private const float MAX_HIGHT_FOR_DAMAGE = 5.0f;
    private const float CRITICAL_HEALTH_VALUE = 30.0f;
    private const float FLASH_DAMAGE_FREQ = 0.1f; // Время прошлого

    private AudioSource m_AudioSource = null;
    private Rigidbody m_RigidBody = null;
    private CharacterController m_CharacterController = null;
    private PlayerHeadBob m_PlayerHeadBob = null;
    private ComponentRifle m_ComponentRifle = null;

    private CollisionFlags m_CollisionFlags = CollisionFlags.None;

    private Vector2 smoothV = Vector2.zero;
    private Vector2 axis = Vector2.zero;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 desiredMove = Vector3.zero;
    private Vector3 inputVelocity = Vector3.zero;
    private Vector3 damagePoint = Vector3.zero;

    private int activeSlot = 0;
    private float slowdownSpeed = 0.0f;
    private int layerMask = -1;
    private float lastDamageTime = 0.0f;
    private float lastTimeWheel = 0.0f;
    private bool lastOnGround = false;
    private float lastYPosition = 0;
    private float capsuleHeight = 1f;
    private float capsuleHeightVelocity = 0;

    private bool onTeleport = false;
    private bool onCrouch = false;

    #endregion

    #region Public Methods
    /// <summary>
    /// Установить время бессмертия
    /// </summary>
    /// <param name="t"></param>
    public static void SetImmortalTime(float t)
    {
        Instance.lastDamageTime = Time.time + t;
    }

    /// <summary>
    /// Вылечить полностью игрока.
    /// </summary>
    public void CurePlayer()
    {
        health = 100.0f;
    }

    /// <summary>
    /// Проснуться
    /// </summary>
    public void WakeUp()
    {
        m_UIWakeUp.gameObject.SetActive(true);
        m_UIWakeUp.Play();
    }

    /// <summary>
    /// Телепортировать игрока.
    /// </summary>
    /// <param name="t">позиция</param>
    public void Teleport(Transform t) { Instance.Teleport(t.position, t.rotation); }

    /// <summary>
    /// Телепортировать игрока.
    /// </summary>
    /// <param name="pos">позиция</param>
    /// <param name="rot">вращение</param>
    public void Teleport(Vector3 pos, Quaternion rot)
    {
        if (!onTeleport)
        {
            Debug.Log("PlayerController.Teleport()");

            onTeleport = true;
            lastDamageTime = Time.time + 5.0f; // Чтобы небыло удара после телепорта.
            transform.position = pos;

            inputVelocity = Vector3.zero;
            desiredMove = Vector3.zero;
            Vector2 axis = Vector2.zero;

            transform.rotation = Quaternion.Euler(0, Utils.GetNormalAngle(rot.eulerAngles.y), 0);
            axis.y = Utils.GetNormalAngle(rot.eulerAngles.x);
            Axis = axis;

            WakeUp();

            StartCoroutine(Instance.CEndTeleport());            
        }
    }

    /// <summary>
    /// Наносимый урон.
    /// </summary>
    /// <param name="d">размер урона</param>
    public void Damage(float d, Vector3 pos)
    {
        if (onTeleport || !isActive) return;

        float t = Time.time;

        if (t - lastDamageTime > FLASH_DAMAGE_FREQ)
        {
            lastDamageTime = t;

            m_AUIDamage.Play();
            m_UIDamage.canvasRenderer.SetAlpha(1.0f);
            m_UIDamage.CrossFadeAlpha(0.0f, 1.5f, false);

            if (!m_GodMode)
            {
                // При критическом здоровье дамаг немного уменьшаем
                if (health < CRITICAL_HEALTH_VALUE) 
                    d = d * 0.6f;
                health -= d;
                health = Mathf.Clamp(health, 0, 100);
                GameManager.GameStatistic.health += d;
            }

            if (health == 0) // Сдох
            {
                Death();
            }
            else
            {
                Utils.PlayRandomSound(m_AudioSource, m_PainSounds);
                damagePoint = pos;
            }
        }
    }

    /// <summary>
    /// Смерть игрока
    /// </summary>
    public void Death()
    {
        if (onTeleport || !isActive) return;

        isActive = false;

        m_RigidBody.useGravity = true;
        m_RigidBody.isKinematic = false;
        m_RigidBody.drag = 15f;
        m_RigidBody.angularDrag = 1f;

        m_RigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        m_CharacterController.enabled = false;

        m_RigidBody.AddForce(transform.up * 10f, ForceMode.Impulse);
        m_RigidBody.AddTorque(transform.forward * 1f, ForceMode.Impulse);

        ComponentRifle.SetWeapon(null);

        // Кровь оставим на экране
        m_BloodOut.gameObject.SetActive(true);
        m_BloodOut[m_BloodOut.clip.name].time = 0.01f;
        m_BloodOut[m_BloodOut.clip.name].speed = 0;

        // Проиграем звук.
        Utils.PlayRandomSound(m_AudioSource, m_DeathSounds);

        // Покажем подсказку что игрок умер.
        GameManager.ShowDialogText("gameover");

        // Через некоторое время покажем меню.
        GameManager.Instance.GameOver();

        UpdateControls();
    }    

    public void PlaySound(AudioClip clip)
    {
        m_AudioSource.PlayOneShot(clip, 1.0f);
    }

    /// <summary>
    /// Выбирает пушку из слота игрока
    /// </summary>
    /// <param name="s">номер слота</param>
    /// <param name="force">принудительно достать пушку</param>
    /// <returns>успешно ли выбрали слот</returns>
    public bool SelectSlot(int s, bool force = false)
    {
        ComponentRifle.Reload = false;

        SWeaponItem w = null;

        if (activeSlot != s || force)
        {
            w = WeaponStore.GetWeaponFromArray(Store, s);
            if (w != null)
            {
                activeSlot = s;
                ComponentRifle.SetWeapon(w);
                slowdownSpeed = w.Weapon.Mass * 0.5f;
            }
        }

        if (w == null)
        {
            activeSlot = 0;
            ComponentRifle.SetWeapon(null);
            slowdownSpeed = 0;
        }

        return (w != null);
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
    }

    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_RigidBody = GetComponent<Rigidbody>();
        m_RigidBody.useGravity = false;
        m_RigidBody.isKinematic = true;

        m_AudioSource = GetComponent<AudioSource>();

        layerMask = LayerMask.GetMask("Player", "Ignore Raycast", "Player Wall");
        layerMask = ~layerMask;
        
        m_UIRectDamage.gameObject.SetActive(true);
        m_UIDamage.canvasRenderer.SetAlpha(0.0f);        
        WakeUp();

        m_PlayerHeadBob = GetComponentInChildren<PlayerHeadBob>();
        m_PlayerHeadBob.OnStep += OnFootStep;
        capsuleHeight = m_CharacterController.height;

        // Компоненты оружия
        m_ComponentRifle = GetComponent<ComponentRifle>();
    }

    private void Update()
    {
        if (onTeleport || !isActive || Time.timeScale == 0) return;

        float t = Time.time;

        // Вращение камеры
        Vector2 md = new Vector2(Input.GetAxisRaw("Mouse_X"), (GameManager.GameSettings.invertmousey) ? -Input.GetAxisRaw("Mouse_Y") : Input.GetAxisRaw("Mouse_Y"));
        md = Vector2.Scale(md, new Vector2(GameManager.GameSettings.mouse * LOOK_SMOOTHING, GameManager.GameSettings.mouse * LOOK_SMOOTHING));

        //smoothV = Vector2.Lerp(smoothV, md, 1.0f / LOOK_SMOOTHING);
        //smoothV = Vector2.Lerp(smoothV, md, Time.deltaTime * 100.0f);
        smoothV = Vector2.Lerp(smoothV, md, Time.deltaTime * 50.0f);
        axis.y -= smoothV.y;
        axis.y = Mathf.Clamp(axis.y, -80f, 65f);

        // Плавный поворот камеры если вращение уехало.
        m_Camera.transform.localRotation = Quaternion.AngleAxis(axis.y, Vector3.right);

        axis.x = m_CharacterController.transform.localRotation.eulerAngles.y + smoothV.x;
        m_CharacterController.transform.localRotation = Quaternion.Euler(new Vector3(0, axis.x, 0));

        // Прыжок
        if (Input.GetButtonDown("Jump") && m_CharacterController.isGrounded)
        {
            moveDirection.y = m_JumpHeight;
        }
        else
        {
            if (moveDirection.y > 1.3f * Physics.gravity.y)
            {
                moveDirection += 1.3f * Time.deltaTime * Physics.gravity;
            }
        }

        // Присесть
        if (Input.GetButton("Crouch"))
        {
            onCrouch = true;
            CapsuleScale();
        } 
        else
        {
            onCrouch = false;

            // Луч наверх чтобы не вростать головой в текстуры
            float halfRadius = m_CharacterController.radius * 0.5f;
            float crouchRayLength = capsuleHeight - halfRadius;
            Ray crouchRay = new Ray(m_RigidBody.position + Vector3.up * halfRadius, Vector3.up);
            
            if (!Physics.SphereCast(crouchRay, halfRadius, crouchRayLength, layerMask, QueryTriggerInteraction.Ignore))
            {
                CapsuleScale();
            }
        }

        Vector2 mv = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        Vector3 inputVector = m_RigidBody.transform.forward * mv.x + m_RigidBody.transform.right * mv.y;

        // Нормализуем чтобы при движении наискосок небыло ускорения.
        if (Mathf.Abs(mv.x) > 0.8f || Mathf.Abs(mv.y) > 0.8f)
        {
            inputVector = inputVector.normalized;            
        }

        // Сглаживание движения
        desiredMove = Vector3.SmoothDamp(desiredMove, inputVector, ref inputVelocity, MOVE_SMOOTHING);

        // Если замедление или присядка до тех пор пока не выпрямился.
        float speed = m_WalkSpeed - slowdownSpeed;
        if(Input.GetButton("Steal") || m_CharacterController.height <=  capsuleHeight - 0.01f)
        {
            speed = m_StealSpeed;
        }

        moveDirection.x = desiredMove.x * speed;
        moveDirection.z = desiredMove.z * speed;

        if (m_ComponentRifle != null)
        {
            // Выбор слота
            if (Input.GetButtonDown("Slot_1")) SelectSlot(1);
            if (Input.GetButtonDown("Slot_2")) SelectSlot(2);
            if (Input.GetButtonDown("Slot_3")) SelectSlot(3);

            // Колесо мыши            
            float msw = Input.GetAxis("Mouse_Scroll");
            if (Input.GetButtonDown("Next")) msw = 1.0f;

            if (msw != 0 && t - lastTimeWheel > 0.45f)
            {
                SelectSlot(GetNextSlotIndex((msw < 0)), true);
                lastTimeWheel = t;
            }

            // Выкинуть оружие
            if (activeSlot != 0 && activeSlot != 3 && Input.GetButtonDown("Drop")) // Нельзя выкинуть пистолет
            {                
                GameObject drop = Store[activeSlot - 1].ObjectScene;
                ComponentRifle.DropWeapon(drop);

                Store[activeSlot - 1].Weapon = null;
                activeSlot = 0;
                slowdownSpeed = 0;
            }
        }

        m_CollisionFlags = m_CharacterController.Move(moveDirection * Time.deltaTime);
    }

    /// <summary>
    /// Вернуть индекс свободного слота
    /// </summary>
    /// <param name="listDown">сторона выбора false вверх true вниз</param>
    /// <returns>индекс свободного слота</returns>
    private int GetNextSlotIndex(bool listDown = true)
    {
        for (int i = 1; i < Store.Length + 1; i++)
        {
            int s = (listDown) ? activeSlot + i : activeSlot - i;
            if (s < 1) s = Store.Length;
            if (s > Store.Length) s = i;
            if (Store[s - 1].Weapon != null)
                return s;
        }

        return 1;
    }

    private void FixedUpdate()
    {
        if (onTeleport || !isActive || Time.timeScale == 0) return;

        float t = Time.time;

        // Высота и падение.
        float py = transform.position.y;
        if (!m_CharacterController.isGrounded)
        {
            if (py > lastYPosition) lastYPosition = py;
            lastOnGround = true;
        }
        else
        {
            //if (py - lastYPosition < -MAX_HIGHT_FOR_DAMAGE) { Damage(30.0f, -Vector3.forward); }
            if (lastOnGround) { OnFootStep(); lastOnGround = false; }
            lastYPosition = py;
        }

        UpdateControls();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (onTeleport || !isActive || Time.timeScale == 0) return;

        // Потолок
        if (m_CollisionFlags == CollisionFlags.Above)
        {
            moveDirection.y += Physics.gravity.y * 2.0f * Time.fixedDeltaTime;
        }

        // Земля или пол
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic)
        {
            return;
        }

        // Давление на предмет
        Vector3 move = moveDirection;
        move.y = 0;
        if (move.sqrMagnitude > 0.1f)
        {
            body.AddForceAtPosition(move.normalized * 6f, hit.point, ForceMode.Force);
        }
    }

    private void UpdateControls()
    {
        float t = Time.time;
        // Значение здоровья
        if (m_UIHealth != null) m_UIHealth.fillAmount = health * 0.01f;
        if (m_UIHealthText != null) m_UIHealthText.text = Mathf.Round(health).ToString();

        // Цвет здоровья.
        if (t - lastDamageTime < 0.3f || health < CRITICAL_HEALTH_VALUE)
        {
            m_UIHealth.color = m_HealthCritical;

            // Востанавливаем здоровье потихоничку
            if (health < CRITICAL_HEALTH_VALUE)
                health += 0.01f;
        }
        else
        {
            m_UIHealth.color = m_HealthNormal;
        }
        
        // Кровавый экран.
        m_BloodOut.gameObject.SetActive(health < CRITICAL_HEALTH_VALUE);


        // Сектор дамага.
        Vector3 pd = (m_CharacterController.transform.position - damagePoint).normalized;
        pd.y = 0;

        Vector3 pf = Vector3.ProjectOnPlane(-m_CharacterController.transform.forward, Vector3.up);
        float a = Vector3.SignedAngle(pf, pd, Vector3.up);
        m_UIRectDamage.localRotation = Quaternion.AngleAxis(-a, Vector3.forward);

        // Обновление пушек.
        for (int i = 0; i < m_UIStackWeapons.Length; i++)
        {
            if (PlayerStore[i].Weapon != null)
            {
                if (ComponentRifle.GetWeaponName() == PlayerStore[i].Weapon.name)
                {
                    m_UIStackWeapons[i].color = m_UIStackActive;
                }
                else
                {
                    m_UIStackWeapons[i].color = m_UIStackUnActive;
                }
                m_UIStackWeapons[i].gameObject.SetActive(true);
            }
            else
            {
                m_UIStackWeapons[i].gameObject.SetActive(false);
            }
        }
    }

    private void CapsuleScale()
    {        
        float t = (!onCrouch) ? capsuleHeight : capsuleHeight * 0.5f;
        if (Mathf.Abs(m_CharacterController.height - t) > 0.01f)
        {
            m_CharacterController.height = Mathf.SmoothDamp(m_CharacterController.height, t, ref capsuleHeightVelocity, 0.1f);
            m_CharacterController.center = new Vector3(0, m_CharacterController.height * 0.5f, 0);

            m_CameraAnimation.transform.localPosition = new Vector3(0, m_CharacterController.height - 0.1f, 0);
        }
    }

    private void OnFootStep()
    {
        if (m_CharacterController.isGrounded)
        {
            Utils.PlayRandomSound(m_AudioSource, m_FootstepSounds);
        }
    }

    private IEnumerator CEndTeleport()
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime * 2); // Нужно дождаться как минимум двух Fixed Обновлений.
        onTeleport = false;
    }

    #endregion
}
