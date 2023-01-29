using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RigidCubes
{
    public abstract class JointsSkeletonBase
    {
        protected Transform m_root;
        protected Dictionary<int, Joint> m_joints = new Dictionary<int, Joint>();
        protected Dictionary<Joint, Joint> m_parentMap = new Dictionary<Joint, Joint>();

        protected Matrix4x4[] m_matrices = new Matrix4x4[128];
        protected List<Vector4> m_colors = new List<Vector4>();
        protected MaterialPropertyBlock m_props = new MaterialPropertyBlock();

        protected JointsSkeletonBase(Transform root)
        {
            m_root = root;
        }

        public abstract void AddJoint(int id, Quaternion r, Vector3 t);
        public abstract void SetParent(int id, int parentId);

        public void SetTail(int head, int tail)
        {
            m_joints[head].SetTail(m_joints[tail]);
        }

        protected (Joint, RigidTransform parent) GetJoint(int id)
        {
            var joint = m_joints[id];
            if (m_parentMap.TryGetValue(joint, out Joint parent))
            {
                return (joint, parent.Transform);
            }
            else
            {
                return (joint, RigidTransform.Identity);
            }
        }

        public abstract void SetTransform(int id, RigidTransform transform);

        public void InitPose()
        {
            foreach (var id in m_joints.Keys.OrderBy(x => x))
            {
                var (joint, matrix) = GetJoint(id);
                SetTransform(id, joint.InitialFromParent);
            }
        }

        public void Draw(Mesh mesh, Material material)
        {
            m_props.SetVectorArray("_Color", m_colors);
            Graphics.DrawMeshInstanced(mesh, 0, material, m_matrices, m_colors.Count, m_props);
        }
    }
}