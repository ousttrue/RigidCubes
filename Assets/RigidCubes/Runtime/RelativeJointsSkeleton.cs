using UnityEngine;

namespace RigidCubes
{
    /// <summary>
    /// Joint 定義が親子関係を持っている。
    /// ex. mocopi, bvh.
    /// </summary>
    public class RelativeJointsSkeleton : JointsSkeletonBase
    {
        public RelativeJointsSkeleton(CoordinateConversion coords, Transform root, int cubeCount) : base(coords, root, cubeCount) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="r">parent relative</param>
        /// <param name="t">parent relative</param>
        public override void AddJoint(int id, Quaternion r, Vector3 t)
        {
            m_joints[id] = new Joint
            {
                Transform = RigidTransform.Identity,
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
            if (m_joints.TryGetValue(parentId, out Joint parent))
            {
                var child = m_joints[id];
                m_parentMap[child] = parent;
                child.Initial = parent.Initial * child.Initial;
            }
        }

        public void HeadTailShape(int head, int tail, Vector3 forward)
        {
            m_joints[head].HeadTailShape(m_joints[tail].InitialFromParent.Translation, forward);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="local">parent relative</param>
        public override void SetTransform(int id, RigidTransform local)
        {
            var (joint, parent) = GetJoint(id);
            joint.Transform = parent * local;
            // x-mirror for right handed coordinate
            m_bones[id].localRotation = Reverse(joint.Transform.Rotation);
            m_bones[id].localPosition = Reverse(joint.Transform.Translation);
            m_bones[id].localScale = Vector3.one;
        }
    }
}
