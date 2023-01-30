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

        static Quaternion ReverseX(Quaternion r)
        {
            return new Quaternion(-r.x, r.y, r.z, -r.w);
        }
        static Vector3 ReverseX(Vector3 t)
        {
            return new Vector3(-t.x, t.y, t.z);
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
            // var m = Matrix4x4.Scale(new Vector3(-1, 1, 1)) * joint.ShapeAndTransform;
            m_bones[id].localRotation = ReverseX(joint.Transform.Rotation);
            m_bones[id].localPosition = ReverseX(joint.Transform.Translation);
            m_bones[id].localScale = Vector3.one;
        }
    }
}
