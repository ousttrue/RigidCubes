using UnityEngine;

namespace RigidCubes
{
    /// <summary>
    /// Joint 定義が親子関係を持っている。
    /// ex. mocopi, bvh.
    /// </summary>
    public class RelativeJointsSkeleton : JointsSkeletonBase
    {
        public RelativeJointsSkeleton(Transform root) : base(root) { }

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
            }
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
            m_matrices[id] = Matrix4x4.Scale(new Vector3(-1, 1, 1)) * m_root.localToWorldMatrix * joint.ShapeAndTransform;
        }
    }
}