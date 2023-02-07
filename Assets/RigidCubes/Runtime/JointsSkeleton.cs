using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RigidCubes
{
    public class JointsSkeleton : IDisposable
    {
        Mesh m_mesh;
        Material m_material;
        SkinnedMeshRenderer m_smr;
        Transform m_root;
        List<Transform> m_bones = new List<Transform>();
        Dictionary<int, Joint> m_joints = new Dictionary<int, Joint>();
        Dictionary<Joint, Joint> m_parentMap = new Dictionary<Joint, Joint>();
        List<Vector4> m_colors = new List<Vector4>();
        MaterialPropertyBlock m_props = new MaterialPropertyBlock();
        CoordinateConversion m_coords;
        public JointsSkeleton(CoordinateConversion coords, Transform root, int cubeCount)
        {
            m_coords = coords;
            m_root = root;
            m_mesh = CreateCubes(cubeCount);
            m_material = new Material(Shader.Find("Standard"));
            m_smr = root.gameObject.GetComponent<SkinnedMeshRenderer>();
            if (m_smr == null)
            {
                m_smr = root.gameObject.AddComponent<SkinnedMeshRenderer>();
            }
            m_smr.updateWhenOffscreen = true;
            m_smr.sharedMesh = m_mesh;
            m_smr.sharedMaterial = m_material;
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

        Quaternion Reverse(Quaternion r)
        {
            switch (m_coords)
            {
                case CoordinateConversion.XReverse:
                    return new Quaternion(-r.x, r.y, r.z, -r.w);
                case CoordinateConversion.ZReverse:
                    return new Quaternion(r.x, r.y, -r.z, -r.w);
                default:
                    return r;
            }
        }

        Vector3 Reverse(Vector3 t)
        {
            switch (m_coords)
            {
                case CoordinateConversion.XReverse:
                    return new Vector3(-t.x, t.y, t.z);
                case CoordinateConversion.ZReverse:
                    return new Vector3(t.x, t.y, -t.z);
                default:
                    return t;
            }
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

        #region Absolute
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="r">world</param>
        /// <param name="t">world</param>
        public void AddJointAbsolute(int id, Quaternion r, Vector3 t, string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = $"[{id:000}]";
            }
            var bone = new GameObject(name).transform;
            bone.SetParent(m_root, false);
            bone.localRotation = Reverse(r);
            bone.localPosition = Reverse(t);
            m_bones.Add(bone);

            m_joints[id] = new Joint
            {
                Transform = new RigidTransform(r, t),
                InitialFromParent = new RigidTransform(r, t),
                Initial = new RigidTransform(r, t),
                UnityTransform = bone,
            };

            while (m_colors.Count <= id)
            {
                m_colors.Add(Color.white);
            }
            m_colors[id] = Color.white;
        }

        public void SetParentAbsolute(int id, int parentId)
        {
            if (m_joints.TryGetValue(parentId, out Joint parent))
            {
                var child = m_joints[id];
                m_parentMap[child] = parent;
                parent.Children.Add(child);
                child.InitialFromParent = parent.Initial.Inversed() * child.Initial;
                child.UnityTransform.localRotation = Reverse(child.Initial.Rotation);
                child.UnityTransform.localPosition = Reverse(child.Initial.Translation);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="world">world</param>
        public void SetTransformAbsolute(int id, RigidTransform world)
        {
            var (joint, _) = GetJoint(id);
            joint.Transform = world;
            // x-mirror for right handed coordinate
            m_bones[id].localRotation = Reverse(joint.Transform.Rotation);
            m_bones[id].localPosition = Reverse(joint.Transform.Translation);
            m_bones[id].localScale = Vector3.one;
        }
        #endregion

        #region Relative
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="r">parent relative</param>
        /// <param name="t">parent relative</param>
        public void AddJointRelative(int id, Quaternion r, Vector3 t, string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = $"[{id:000}]";
            }
            var bone = new GameObject(name).transform;
            bone.SetParent(m_root, false);
            m_bones.Add(bone);

            m_joints[id] = new Joint
            {
                Transform = RigidTransform.Identity,
                InitialFromParent = new RigidTransform(r, t),
                Initial = new RigidTransform(r, t),
                UnityTransform = bone,
            };

            while (m_colors.Count <= id)
            {
                m_colors.Add(Color.white);
            }
            m_colors[id] = Color.white;
        }

        public void SetParentRelative(int id, int parentId)
        {
            if (m_joints.TryGetValue(parentId, out Joint parent))
            {
                var child = m_joints[id];
                m_parentMap[child] = parent;
                parent.Children.Add(child);
                child.Initial = parent.Initial * child.Initial;
                child.UnityTransform.localRotation = Reverse(child.Initial.Rotation);
                child.UnityTransform.localPosition = Reverse(child.Initial.Translation);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="local">parent relative</param>
        public void SetTransformRelative(int id, RigidTransform local)
        {
            var (joint, parent) = GetJoint(id);
            joint.Transform = parent * local;
            // x-mirror for right handed coordinate
            m_bones[id].localRotation = Reverse(joint.Transform.Rotation);
            m_bones[id].localPosition = Reverse(joint.Transform.Translation);
            m_bones[id].localScale = Vector3.one;
        }

        public void SetRotationRelative(int id, Quaternion rotation)
        {
            var (joint, parent) = GetJoint(id);
            joint.Transform = parent * new RigidTransform(rotation, joint.InitialFromParent.Translation);
            // x-mirror for right handed coordinate
            m_bones[id].localRotation = Reverse(joint.Transform.Rotation);
            m_bones[id].localPosition = Reverse(joint.Transform.Translation);
            m_bones[id].localScale = Vector3.one;
        }
        #endregion

        #region Shape
        public void YAxisHeadTailShape(int head, int tail, Vector3 forward)
        {
            m_joints[head].YAxisHeadTailShape(m_joints[tail].InitialFromParent.Translation, forward);
        }

        public void AutoHeadTailShapes(Joint joint = null)
        {
            if (joint == null)
            {
                joint = m_joints[0];
            }
            if (joint.Children.Count == 1)
            {
                joint.YAxisHeadTailShape(joint.Children[0].InitialFromParent.Translation, Vector3.forward);
            }
            foreach (var child in joint.Children)
            {
                AutoHeadTailShapes(child);
            }
        }

        public void NegativeZAxisHeadTailShape(int head, int tail, Vector3 forward)
        {
            m_joints[head].NegativeZAxisHeadTailShape(m_joints[tail].InitialFromParent.Translation, forward);
        }

        public void SetShape(int id, Vector3 scalingCenter, Vector3 widthHeightDepth)
        {
            m_joints[id].SetShape(scalingCenter, widthHeightDepth);
        }
        #endregion

        (Joint, RigidTransform parent) GetJoint(int id)
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

        public void InitPose()
        {
            foreach (var id in m_joints.Keys.OrderBy(x => x))
            {
                var (joint, matrix) = GetJoint(id);
                SetTransformRelative(id, joint.InitialFromParent);
            }
        }

        public void SetupSkinning()
        {
            m_mesh.bindposes = m_bones.Select((x, i) => m_joints[i].Shape).ToArray();
            m_smr.bones = m_bones.ToArray();
        }
    }
}