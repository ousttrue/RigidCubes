using UnityEngine;

namespace RigidCubes
{
    /// <summary>
    /// Joint定義が親子関係を持っていない。
    /// ex. XR_EXT_hand_tracking, KinectAzure.
    /// </summary>
    public class AbsoluteJointsSkeleton : JointsSkeletonBase
    {
        public AbsoluteJointsSkeleton(CoordinateConversion coords, Transform root, int cubeCount) : base(coords, root, cubeCount) { }

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
                Initial = new RigidTransform(r, t),
            };

            while (m_colors.Count <= id)
            {
                m_colors.Add(Color.white);
            }
            m_colors[id] = Color.white;

            var bone = new GameObject($"[{id:000}]").transform;
            bone.SetParent(m_root, false);
            m_bones.Add(bone);
        }

        public override void SetParent(int id, int parentId)
        {
            // if (m_joints.TryGetValue(parentId, out Joint parent))
            // {
            //     var child = m_joints[id];
            //     m_parentMap[child] = parent;
            //     // calc relative initial value
            //     child.InitialFromParent = parent.Transform.Inversed() * child.InitialFromParent;
            // }
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
            m_bones[id].localRotation = Reverse(joint.Transform.Rotation);
            m_bones[id].localPosition = Reverse(joint.Transform.Translation);
            m_bones[id].localScale = Vector3.one;
        }
    }
}