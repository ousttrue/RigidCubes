using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RigidCubes
{
    public abstract class JointsSkeletonBase : IDisposable
    {
        Mesh m_mesh;
        protected Transform m_root;
        protected Dictionary<int, Joint> m_joints = new Dictionary<int, Joint>();
        protected Dictionary<Joint, Joint> m_parentMap = new Dictionary<Joint, Joint>();

        protected Matrix4x4[] m_matrices = new Matrix4x4[128];
        protected List<Vector4> m_colors = new List<Vector4>();
        protected MaterialPropertyBlock m_props = new MaterialPropertyBlock();

        protected JointsSkeletonBase(Transform root)
        {
            m_root = root;
            m_mesh = CreateMesh();
        }

        public void Dispose()
        {
            GameObject.Destroy(m_mesh);
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

        public Mesh CreateMesh()
        {
            var builder = new MeshBuilder();

            // reauire Y-Up 1.0f size Shape.
            //
            //    7 6
            //    +-+
            //   / /|
            // 4+-+5+2
            //  | |/
            //  +-+
            //  0 1
            //  ------> x
            var s = 0.5f;
            var v0 = new Vector3(-s, 0, -s);
            var v1 = new Vector3(+s, 0, -s);
            var v2 = new Vector3(+s, 0, +s);
            var v3 = new Vector3(-s, 0, +s);
            var v4 = new Vector3(-s, 2 * s, -s);
            var v5 = new Vector3(+s, 2 * s, -s);
            var v6 = new Vector3(+s, 2 * s, +s);
            var v7 = new Vector3(-s, 2 * s, +s);

            builder.PushQuadrangle(v0, v1, v2, v3);
            builder.PushQuadrangle(v5, v4, v7, v6);
            builder.PushQuadrangle(v1, v0, v4, v5);
            builder.PushQuadrangle(v2, v1, v5, v6);
            builder.PushQuadrangle(v3, v2, v6, v7);
            builder.PushQuadrangle(v0, v3, v7, v4);

            var mesh = builder.ToMesh();
            mesh.name = "JointsSkeleton";
            return mesh;
        }

        public void Draw(Material material)
        {
            m_props.SetVectorArray("_Color", m_colors);
            Graphics.DrawMeshInstanced(m_mesh, 0, material, m_matrices, m_colors.Count, m_props);
        }
    }
}