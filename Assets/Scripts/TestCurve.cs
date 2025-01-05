using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCurve : MonoBehaviour
{
    [SerializeField] private Transform m_Transform = null;
    [SerializeField] private AnimationCurve m_AnimationCurve = null;


    private void Update()
    {
        //Keyframe kf = new Keyframe(Time.time, m_Transform.position.x, 0, 0, 0, 0);
        Keyframe kf = new Keyframe(Time.time, m_Transform.position.y, 0, 0, 0, 0);
        //Keyframe kf = new Keyframe(Time.time, m_Transform.eulerAngles.x, 0, 0, 0, 0);
        m_AnimationCurve.AddKey(kf);
    }
}
