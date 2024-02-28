using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyPosePlus : MonoBehaviour
{
    public GameObject src;
    public GameObject des;
    HumanPoseHandler m_srcPoseHandler;
    HumanPoseHandler m_destPoseHandler;
    


    void Start()
    {
        m_srcPoseHandler = new HumanPoseHandler(src.GetComponent<Animator>().avatar, src.transform);
        m_destPoseHandler = new HumanPoseHandler(des.GetComponent<Animator>().avatar, des.transform);

    }



    void LateUpdate()
    {
        HumanPose m_humanPose = new HumanPose();

        m_srcPoseHandler.GetHumanPose(ref m_humanPose);
        m_destPoseHandler.SetHumanPose(ref m_humanPose);
        des.transform.localPosition = src.transform.localPosition;
    }



}