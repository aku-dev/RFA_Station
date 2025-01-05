/* =======================================================================================================
 * AK Studio
 * 
 * Version 4.0 by Alexandr Kuznecov
 * 28.05.2023
 * =======================================================================================================
 */
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class NPCEnemyFlyer : MonoBehaviour, IDestructible, INPC
{
    #region Editor Properties
    [Header("Properties")]
    [Tooltip("Здоровье.")]
    [SerializeField] private float m_Health = 100;
    [Tooltip("Дистанция патрулирования.")]
    [SerializeField] private float m_ActiveDistance = 5.0f;
    [Tooltip("Наносимый урон от одной пули.")]
    [SerializeField] private float m_WeaponDamage = 0.5f;

    [Header("Elements")]
    [SerializeField] private Animation m_Animation = null;
    [Tooltip("Искры от урона.")]
    [SerializeField] private GameObject m_DamageFlash = null;
    [Tooltip("Отваливающиеся части.")]
    [SerializeField] private SDestructibleObjects[] m_DestructibleObjects = null;
    [Tooltip("Основная часть.")]
    [SerializeField] private GameObject m_Body = null;

    [SerializeField] private EState m_State = EState.Wander;
    private EState m_LastState = EState.Death;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] m_SoundShots = null;

    #endregion

    #region Fields
    // Здоровье
    public float Health { get { return m_Health; } set { m_Health = Mathf.Clamp(value, 0, 100); } }
    // Активность
    public bool IsActive { get; set; } = true;
    // Состояние
    public EState State { get { return m_State; } set { m_State = value; } } // m_LastState = m_State;
    // Время последнего урона
    public float LastDamageTime { get; private set; } = -1.0f;
    #endregion

    #region Private Values
    private const float RADIUS_TARGET = 4.0f; // Удвоенная высота агента чтобы не терять частоту кадров.
    private const float TURN_SPEED = 15.0f;
    private const float FIRE_SPEED = 0.02f;
    private const float MOVE_WAVE_FREQUENCY = 2.0f;
    private const float MOVE_WAVE_MAGNITUDE = 0.25f;
    private const int MAX_RAND_SEED = 65536;
    private const float CRITICAL_HEALTH = 50.0f; // Критическое здоровье

    private AudioSource as_Audio = null;
    private NavMeshAgent agent = null;
    private Collider m_Collider = null;
    private Vector3 startPoint = Vector3.zero;
    private float sineSeed = 0;
    private float fullHealth = 0;
    private float lastChangeTarget = 0;
    private float timeChangeTarget = 0;
    private float agentSpeed = 0;
    private float lastStafeTime = 0;    
    private float lastFire = 0;

    private int weaponIndex = 0;

    private bool switchFire = false; // Переключатель пушек.
    private bool onLastWeapon = false;    
    #endregion

    #region Public Methods
    public void Death()
    {
        if (!IsActive) return;

        m_Health = 0;
        m_Animation.gameObject.SetActive(false);
        m_Body.SetActive(true);
        m_State = EState.Death;

        agent.enabled = false;
        agent.speed = 0;
        agent.velocity = Vector3.zero;

        m_Collider.enabled = false;

        // Отключаем декали пулей.
        foreach(Projector h in GetComponentsInChildren<Projector>())
        {
            h.gameObject.SetActive(false);
        }

        IsActive = false;
        GameManager.GameStatistic.enemys++;
    }

    public void Hit(RaycastHit hit, float damage)
    {
        if (!IsActive) return;

        LastDamageTime = Time.time;
        m_Health -= damage;
        m_DamageFlash.SetActive(true);

        // Если мало здоровья стейфимся при попадании.
        if (m_Health < CRITICAL_HEALTH) lastStafeTime = 0;

        // Разрушаемые части
        if (m_DestructibleObjects.Length > 0)
        {
            float desctructStep = fullHealth / (m_DestructibleObjects.Length + 1);

            for (int i = 0; i < m_DestructibleObjects.Length; i++)
            {
                if (desctructStep * (i + 1) > m_Health && m_DestructibleObjects[i].Functioning.activeInHierarchy)
                {
                    m_DestructibleObjects[i].Functioning.SetActive(false);
                    m_DestructibleObjects[i].NoFunctioning.SetActive(true);
                    m_DestructibleObjects[i].NoFunctioning.transform.parent = null;
                }
            }

            // Последняя пушка
            onLastWeapon = (desctructStep * (m_DestructibleObjects.Length - 1) > m_Health);
        }        

        // Кончилось здоровье
        if (m_Health <= 0) Death();
    }

    public void WakeUp()
    {
        IsActive = true;

        agent.enabled = true;
        agent.updateRotation = false;
        agent.updatePosition = true;
        agentSpeed = agent.speed;
        m_Health = fullHealth;
        onLastWeapon = false;
        m_Collider.enabled = true;

        if (m_State == EState.Death) 
            m_State = EState.Wander;

        foreach (SDestructibleObjects o in m_DestructibleObjects)
        {
            o.Functioning.SetActive(true);
            o.NoFunctioning.SetActive(false);
            o.NoFunctioning.transform.parent = m_Animation.transform;
            o.NoFunctioning.transform.localPosition = Vector3.zero;
            o.NoFunctioning.transform.localRotation = Quaternion.identity;
        }
    }

    public void Attack()
    {
        m_State = EState.Attack;
    }
    #endregion

    #region Private Methods
    private void RandomTimeChangeTarget() { timeChangeTarget = Random.Range(5.0f, 3.0f); }

    private void Awake()
    {
        fullHealth = m_Health;

        as_Audio = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        m_Collider = m_Animation.GetComponent<Collider>();

        WakeUp();

        RandomTimeChangeTarget();
        agent.SetDestination(RandomNavMeshSphere());
        startPoint = transform.position;        
        sineSeed = Random.Range(0, MAX_RAND_SEED);
    }

    private void FixedUpdate()
    {
        if (!IsActive) return;

        float t = Time.time;
        Quaternion lookPoint = Quaternion.identity;
        Vector3 playerTarget = PlayerController.MainCamera.transform.position + new Vector3(0, -0.2f, 0); // Прицел делаем на грудь игрока

        // Качание в воздухе
        float floorSine = Mathf.Sin((t + sineSeed) * MOVE_WAVE_FREQUENCY) * MOVE_WAVE_MAGNITUDE;
        agent.baseOffset = 1.0f + floorSine;

        switch (m_State)
        {
            case EState.Idle:
                agent.SetDestination(transform.position);
                if (m_Animation.isPlaying)
                    m_Animation.Stop();
                break;
            case EState.Wander:
                // Смена статуса
                if (m_State != m_LastState)
                {
                    string scanClip = "Flyer_Scan_Animation";
                    m_Animation.Play(scanClip);
                    m_Animation[scanClip].time = Random.Range(0, m_Animation[scanClip].length);

                    // Если мало здоровья стейфимся при попадании.
                    if (m_Health < CRITICAL_HEALTH)
                    {
                        
                        // И Убегаем от игрока подальше
                        NavMeshHit hit;
                        NavMesh.SamplePosition(agent.transform.position + PlayerController.MainCharacterController.transform.forward, 
                            out hit, RADIUS_TARGET * 2.0f, Physics.DefaultRaycastLayers);
                        agent.SetDestination(hit.position);

                        agent.speed = agentSpeed * 6.0f; // Увеличим скорость убегания
                        lastChangeTarget = t;
                        lastStafeTime = 0; // Делаем стейф при смене статуса
                    }
                }

                // Если остановились нужно найти другую цель.
                if (agent.velocity == Vector3.zero && lastChangeTarget > 0)
                {
                    lastChangeTarget = 0;
                }

                // Смена цели следования
                if (t - lastChangeTarget > timeChangeTarget)
                {
                    m_Animation.transform.localRotation = Quaternion.identity;
                    agent.speed = agentSpeed;
                    agent.SetDestination(RandomNavMeshSphere());
                    lastChangeTarget = t;
                }

                // Возврат на точку спавна
                if (Utils.FastDistance(startPoint, transform.position) > m_ActiveDistance * m_ActiveDistance)
                {
                    agent.speed = agentSpeed * 2.0f;
                    agent.SetDestination(startPoint);
                    lastChangeTarget = t;
                }
                break;
            case EState.Wait:
                // Смена статуса
                if (m_State != m_LastState)
                {
                    m_Animation.Play("Flyer_Wake_Up_Animation");
                    agent.speed = agentSpeed * 3.0f; // Увеличим скорость догонки
                    agent.stoppingDistance = 4.0f;   // Останавливаемся недалеко от игрока

                    // Отключим пушки пока
                    foreach (SDestructibleObjects o in m_DestructibleObjects)
                    {
                        o.FirePoint.SetActive(false);
                    }
                }

                lookPoint = Quaternion.LookRotation(playerTarget - transform.position, Vector3.up);
                m_Animation.transform.localRotation = Quaternion.Euler(lookPoint.eulerAngles.x, 0, 0);
                agent.SetDestination(playerTarget);

                break;
            case EState.Attack:
                lookPoint = Quaternion.LookRotation(playerTarget - transform.position, Vector3.up);
                m_Animation.transform.localRotation = Quaternion.Euler(lookPoint.eulerAngles.x, 0, 0);                

                if (m_State != m_LastState)
                {
                    m_Animation.Play("Flyer_Wake_Up_Animation");                    
                    agent.speed = agentSpeed * 4.0f; // Увеличим скорость догонки
                    agent.stoppingDistance = 1.5f;   // Останавливаемся недалеко от игрока
                }

                // Огонь из всех орудий
                if (t - lastFire > FIRE_SPEED)
                {
                    // Огонь
                    bool isFire = !m_Animation.IsPlaying("Flyer_Wake_Up_Animation"); // Ждем полного открытия пушек
                    if (isFire) isFire = agent.remainingDistance < 3.0f; // Открываем огонь когда на растоянии ближе 3х метров

                    if (!switchFire 
                         && isFire
                         && m_DestructibleObjects[weaponIndex].Functioning.activeInHierarchy
                         && !m_DestructibleObjects[weaponIndex].FirePoint.activeInHierarchy)
                    {
                        GameObject fp = m_DestructibleObjects[weaponIndex].FirePoint;
                        fp.SetActive(true);
                        Utils.PlayRandomSound(as_Audio, m_SoundShots);
                        NPCUtils.Fire(fp.transform, m_WeaponDamage, EBulletType.s45mm);
                        GameManager.GameStatistic.bullet_enemy++;
                    }

                    // Переключение пушек
                    if (switchFire)
                    {
                        m_DestructibleObjects[weaponIndex].FirePoint.SetActive(false);
                        weaponIndex++;
                        if (weaponIndex > m_DestructibleObjects.Length - 1) weaponIndex = 0;
                    }

                    // Пушка отчаянья
                    if (onLastWeapon)
                    {
                        if (isFire && !m_Animation.IsPlaying("Flyer_Last_Weapon_Animation"))
                        {
                            m_Animation.Play("Flyer_Last_Weapon_Animation");                            
                        }
                        if (!isFire && m_Animation.IsPlaying("Flyer_Last_Weapon_Animation"))
                        {
                            m_Animation.Play("Flyer_Idle_Animation");
                        }
                    }                        

                    // Стейф при аттаке
                    if (t - lastStafeTime > 1.0f)
                    {
                        if (isFire)
                        {
                            float ampl = (m_Health < CRITICAL_HEALTH) ? 2.5f : 1.5f; // Силай стейфа зависит от здоровья
                            int halfSeed = MAX_RAND_SEED / 2;
                            Vector3 rndSign = new Vector3(Random.Range(0, MAX_RAND_SEED) > halfSeed ? 1: -1,
                                Random.Range(0, MAX_RAND_SEED) > halfSeed ? 1 : -1,
                                Random.Range(0, MAX_RAND_SEED) > halfSeed ? 1 : -1);

                            agent.velocity = new Vector3(rndSign.x * ampl, 0, rndSign.y * ampl);
                            lastStafeTime = t;
                        } 
                    }

                    agent.SetDestination(playerTarget);

                    lastFire = t;
                    switchFire = !switchFire;
                }

                break;
            default:
                if (m_State != m_LastState && m_LastState == EState.Attack)
                    m_Animation.Play("Flyer_Clam_Down_Animation");
                break;
        }

        // Вращение
        Quaternion targetRotation = Quaternion.identity;

        // Поворачиваемся к игроку при аттаке и ожидании, если застряли или если дистанция ближе 4 метров
        if    ((m_State == EState.Attack || m_State == EState.Wait) 
            && (agent.velocity == Vector3.zero || agent.remainingDistance < 4.0f))
        {
            targetRotation = lookPoint;
        }
        else if (agent.velocity != Vector3.zero)
        { 
            targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
        }

        agent.transform.localRotation = Quaternion.Lerp(agent.transform.localRotation, targetRotation, Time.fixedDeltaTime * TURN_SPEED);
        m_LastState = m_State;
    }

    private Vector3 RandomNavMeshSphere()
    {
        RandomTimeChangeTarget();

        Vector3 randDirection = Random.insideUnitSphere * RADIUS_TARGET;
        NavMeshHit hit;

        randDirection += agent.transform.position;
        NavMesh.SamplePosition(randDirection, out hit, RADIUS_TARGET, Physics.DefaultRaycastLayers);

        return hit.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        Gizmos.DrawSphere(transform.position, m_ActiveDistance);
    }
    #endregion
}