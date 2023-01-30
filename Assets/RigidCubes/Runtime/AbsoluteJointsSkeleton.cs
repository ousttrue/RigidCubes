using UnityEngine;

namespace RigidCubes
{
    /// <summary>
    /// Joint定義が親子関係を持っていない。
    /// ex. XR_EXT_hand_tracking, KinectAzure.
    /// </summary>
    public class AbsoluteJointsSkeleton : JointsSkeletonBase
    {
        public AbsoluteJointsSkeleton(Transform root) : base(root) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="r">world</param>
        /// <param name="t">world</param>
        public override void AddJoint(int id, Quaternion r, Vector3 t)
        {
            m_joints[id] = new Joint
            {
                Transform = new RigidTransform(r, t),
                InitialFromParent = new RigidTransform(r, t),
            };

            while (m_colors.Count <= id)
            {
                m_colors.Add(Color.white);
            }
            m_colors[id] = Color.white;
        }

        public override void SetParent(int id, int parentId)
        {
            if (m_joints.TryGetValue(parentId, out Joint parent))
            {
                var child = m_joints[id];
                m_parentMap[child] = parent;
                // calc relative initial value
                child.InitialFromParent = parent.Transform.Inversed() * child.InitialFromParent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="world">world</param>
        public override void SetTransform(int id, RigidTransform world)
        {
            var (joint, _) = GetJoint(id);
            joint.Transform = world;
            // x-mirror for right handed coordinate
            // m_matrices[id] = Matrix4x4.Scale(new Vector3(-1, 1, 1)) * m_root.localToWorldMatrix * joint.ShapeAndTransform;
            m_bones[id].localRotation = joint.Transform.Rotation;
            m_bones[id].localPosition = joint.Transform.Translation;
            m_bones[id].localScale = new Vector3(0.02f, 0.02f, 0.02f);
        }
    }
}