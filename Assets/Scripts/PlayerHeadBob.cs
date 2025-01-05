using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadBob : MonoBehaviour
{
    public static PlayerHeadBob Instance = null;
    [SerializeField] private GameObject m_HandsObject = null;

    public delegate void StepAction();
    public event StepAction OnStep;

    private const float THRESHOLD_SPEED = 3.5f;
    private const float FIRST_STEP_DISTANCE = 0.2f;

    private CharacterController m_CharacterController = null;

    private float ySmoothVelo = 0f;
    private float bobTime = 0f;
    private float bobDistance = 1f;

    private int layerMask = -1;
    private float moveZ = 0;
    public static float MoveBack { get { return Instance.moveZ; } private set { } } // Уперся ли игрок в стену.


    [SerializeField] private float headSmoothing = 0.2f;    // 0.2    
    [SerializeField] private float bobAfterDistance = 1.8f; // 1.8

    [SerializeField] private float bobDuration = 0.23f; // 0.23
    [SerializeField] private float bobHeight = -0.3f; // -0.3

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
        m_CharacterController = GetComponentInParent<CharacterController>();
        layerMask = LayerMask.GetMask("Player", "Ignore Raycast", "Player Wall");
        layerMask = ~layerMask;

        StartCoroutine(CBackMoveHands());
    }

    private void Update()
    {
        if (PlayerController.IsActive)
        {
            Vector3 velo = m_CharacterController.velocity;
            float deltaV = new Vector2(velo.x, velo.z).magnitude;
            float deltaDistance = deltaV * Time.deltaTime;

            if (deltaV < THRESHOLD_SPEED) { bobDistance = FIRST_STEP_DISTANCE; }

            bobDistance -= deltaDistance;
            bobTime -= Time.deltaTime;

            if (bobDistance < 0 && m_CharacterController.isGrounded)
            {
                OnStep?.Invoke(); // Вызов делегата шага

                bobTime = bobDuration;
                bobDistance = bobAfterDistance;
            }

            float yTarget = m_CharacterController.velocity.y * 0.035f; // Качание при прыжке и падении. 0.035f

            if (bobTime > 0) yTarget += bobHeight;
            yTarget = Mathf.Clamp(yTarget, -1, 0.3f);

            float y = Mathf.SmoothDamp(m_HandsObject.transform.localPosition.y, yTarget * 0.06f, ref ySmoothVelo, headSmoothing);
            if (!double.IsNaN(y))
            {                
                m_HandsObject.transform.localPosition = new Vector3(m_HandsObject.transform.localPosition.x, 
                                                                    y,
                                                                    Mathf.Lerp(m_HandsObject.transform.localPosition.z, moveZ, Time.deltaTime * 20.0f));
            }
        }
    }

    private IEnumerator CBackMoveHands()
    {
        while (true)
        {
            yield return new WaitForSeconds(Time.fixedDeltaTime * 5);
            if (Physics.Raycast(PlayerController.MainCamera.transform.position, PlayerController.MainCamera.transform.forward, out RaycastHit hit, 0.5f, 1))
            {            
                moveZ = hit.distance - 0.6f;
            } else
            {
                moveZ = 0;
            }
        }
    }

}
