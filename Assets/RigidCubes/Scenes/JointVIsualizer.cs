using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointVIsualizer : MonoBehaviour
{
    RigidCubes.JointsSkeleton m_skeleton;

    void OnEnable()
    {
        m_skeleton = new RigidCubes.JointsSkeleton(RigidCubes.CoordinateConversion.ZReverse, transform, 100);
    }

    void OnDisable()
    {
        m_skeleton.Dispose();
        m_skeleton = null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
