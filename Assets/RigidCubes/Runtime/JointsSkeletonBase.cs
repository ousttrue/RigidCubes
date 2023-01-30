using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RigidCubes
{
    public abstract class JointsSkeletonBase : IDisposable
    {
        Mesh m_mesh;
        Material m_material;
        SkinnedMeshRenderer m_smr;
        protected Transform m_root;
        protected List<Transform> m_bones = new List<Transform>();
        protected Dictionary<int, Joint> m_joints = new Dictionary<int, Joint>();
        protected Dictionary<Joint, Joint> m_parentMap = new Dictionary<Joint, Joint>();
        protected List<Vector4> m_colors = new List<Vector4>();
        protected MaterialPropertyBlock m_props = new MaterialPropertyBlock();
        bool m_initialized = false;

        protected JointsSkeletonBase(Transform root, int cubeCount = 100)
        {
            m_root = root;
            m_mesh = CreateCubes(100);
            m_material = new Material(Shader.Find("Standard"));
            m_smr = root.gameObject.AddComponent<SkinnedMeshRenderer>();
            m_smr.sharedMesh = m_mesh;
            m_smr.sharedMaterial = m_material;
        }

        public void Dispose()
        {
            foreach (var bone in m_bones)
            {
                if (bone != null)
                {
                    GameObject.Destroy(bone.gameObject);
                }
            }
            GameObject.Destroy(m_mesh);
        }

        public abstract void AddJoint(int id, Quaternion r, Vector3 t);
        public abstract void SetParent(int id, int parentId);

        public void SetTail(int head, int tail, Vector3 forward)
        {
            m_joints[head].SetTail(m_joints[tail].InitialFromParent.Translation, forward);
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

        Mesh CreateCubes(int cubeCount)
        {
            var builder = new MeshBuilder();
            for (int i = 0; i < cubeCount; ++i)
            {
                builder.PushCube();
            }
            var mesh = builder.ToMesh();
            mesh.name = "JointsSkeleton";
            return mesh;
        }

        public void Draw()
        {
            if (!m_initialized)
            {
                m_initialized = true;
                m_mesh.bindposes = m_bones.Select((x, i) => m_joints[i].Shape).ToArray();
                m_smr.bones = m_bones.ToArray();
            }
        }
    }
}