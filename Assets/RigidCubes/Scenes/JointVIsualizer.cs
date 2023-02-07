using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointVIsualizer : MonoBehaviour
{
    RigidCubes.JointsSkeleton m_skeleton;

    public bool m_init = false;

    void OnEnable()
    {
        m_skeleton = new RigidCubes.JointsSkeleton(RigidCubes.CoordinateConversion.ZReverse, transform, 100);
    }

    void OnDisable()
    {
        m_skeleton.Dispose();
        m_skeleton = null;
    }

    public void OnSkeleton(SkeletonHeader header, JointDefinition[] joints)
    {
        if (m_skeleton != null)
        {
            m_skeleton.Dispose();
        }
        m_skeleton = new RigidCubes.JointsSkeleton(RigidCubes.CoordinateConversion.XReverse, transform, header.jointCount);
        for (int i = 0; i < joints.Length; ++i)
        {
            var joint = joints[i];
            m_skeleton.AddJointRelative(i, Quaternion.identity, new Vector3(joint.xFromParent, joint.yFromParent, joint.zFromParent),
            $"[{i:000}] => {joint.parentBoneIndex}");
        }
        for (int i = 0; i < joints.Length; ++i)
        {
            var joint = joints[i];
            m_skeleton.SetParentRelative(i, joint.parentBoneIndex);
        }
        m_skeleton.AutoHeadTailShapes();
        m_skeleton.SetupSkinning();
    }

    public void OnFrame(FrameHeader header, Quaternion[] rotations)
    {
        if (m_skeleton == null)
        {
            return;
        }
        if (m_init)
        {
            return;
        }
        m_skeleton.SetTransformRelative(0, new RigidCubes.RigidTransform(rotations[0], header.RootPosition));
        for (int i = 1; i < rotations.Length; ++i)
        {
            m_skeleton.SetRotationRelative(i, rotations[i]);
        }
    }

    public void OnMatrix(FrameHeader header, Matrix4x4[] matrices)
    {
        if (m_skeleton == null)
        {
            return;
        }
        if (m_init)
        {
            return;
        }
        for (int i = 0; i < matrices.Length; ++i)
        {
            var m = matrices[i];
            // m = m.transpose;
            m_skeleton.SetTransformAbsolute(i, new RigidCubes.RigidTransform(m.rotation, m.GetColumn(3)));
        }
    }

    void Update()
    {
        if (m_skeleton != null)
        {
            if (m_init)
            {
                m_skeleton.InitPose();
            }
        }
    }
}
