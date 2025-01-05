using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NPCWaveTrigger : MonoBehaviour
{
    [Tooltip("Список NPC")]
    [SerializeField] private MonoBehaviour[] m_Npc = null;

    [Tooltip("Блокируемые двери")]
    [SerializeField] private TriggerDoor[] m_Doors = null;

    [Tooltip("Одновременно атакующих")]
    [SerializeField] private int m_TogetherAttacks = 2;

    [Tooltip("Одновременно ждущих аттаки")]
    [SerializeField] private int m_TogetherWaits = 2;


    [Header("Events")]
    [SerializeField] private GameObject m_ObjectOn = null;
    [SerializeField] private GameObject m_ObjectOff = null;
    [SerializeField] private UnityEvent m_Events = new UnityEvent();


    private const float UPTATE_TIMER = 1.0f;   // Время обновления состояний.
    private const float MEMORY_DAMAGE = 5.0f;  // Время забывания аттаки
    private const float CRITICAL_HEALTH = 50.0f; // Критическое здоровье

    private bool isActive = false;
    private float lastUpdateTimer = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive && other.gameObject.layer == LayerMask.NameToLayer("Player") && isActiveAndEnabled)
        {
            MusicPlayer.Play(ETypeMusic.Fast);
            foreach (TriggerDoor d in m_Doors)
                d.SetLock(true);

            isActive = true;
        }
    }

    private void FixedUpdate()
    {
        if (!isActive) return;
        if (!PlayerController.IsActive)
        {
            for (int i = 0; i < m_Npc.Length; i++)
            {
                INPC npc = m_Npc[i].GetComponent<INPC>();
                npc.State = EState.Wander;
            }
            return;
        }

        float t = Time.time;

        if (t - lastUpdateTimer > UPTATE_TIMER)
        {
            List<SWeightTable> weightTable = new List<SWeightTable>(); // Таблица весов

            // Заполним массивы
            bool complete_wave = true;
            for (int i = 0; i < m_Npc.Length; i++)
            {
                INPC npc = m_Npc[i].GetComponent<INPC>();
                if (npc.IsActive == false) continue;

                // Определяем вес
                // Если игрок попал то высший вес
                // Если близкое растояние то выше вес
                // Если низкое здоровье то ниже вес.

                complete_wave = false;
                if (npc.LastDamageTime > 0 && t - npc.LastDamageTime < MEMORY_DAMAGE && npc.Health > CRITICAL_HEALTH)
                {
                    weightTable.Add(new SWeightTable(i, 0));
                }
                else
                {                       
                    float dis = (PlayerController.MainCamera.transform.position - npc.gameObject.transform.position).sqrMagnitude;
                    if (npc.Health < CRITICAL_HEALTH) // Если низкое здоровье то вес на уровне мобом которые далеко.
                    {
                        dis += 1000.0f;
                    }

                    weightTable.Add(new SWeightTable(i, dis));
                }
            }

            // Волна кончилась
            if (complete_wave)
            {
                MusicPlayer.Play(ETypeMusic.Slow);
                foreach (TriggerDoor d in m_Doors)
                    d.SetLock(false);

                gameObject.SetActive(false);
                if (m_ObjectOn != null) m_ObjectOn.SetActive(true);
                if (m_ObjectOff != null) m_ObjectOff.SetActive(false);
                m_Events.Invoke();
                return;
            }

            // Сортировка
            weightTable.Sort();

            int total = m_TogetherAttacks + m_TogetherWaits;
            if (m_TogetherAttacks > weightTable.Count) m_TogetherAttacks = weightTable.Count;
            if (total > weightTable.Count) m_TogetherWaits = 0;

            // Обновим состояния ботов.
            for (int i = 0; i < weightTable.Count; i++)
            {
                INPC npc = m_Npc[weightTable[i].index].GetComponent<INPC>();
                if (i < m_TogetherAttacks)
                {
                    npc.State = EState.Attack;
                } 
                else if (i < total)
                {
                    npc.State = EState.Wait;
                }
                else
                {
                    npc.State = EState.Wander;
                }
            }
            lastUpdateTimer = t;
        }
    }
}

public class SWeightTable: IComparable<SWeightTable>
{
    public int index;
    public float val;
    public SWeightTable(int i, float v)
    {
        index = i;
        val = v;
    }

    public int CompareTo(SWeightTable other)
    {
        return (val > other.val) ? 1 : -1;
    }
}