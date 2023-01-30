using System.Collections.Generic;
using UnityEngine;

namespace RigidCubes
{
    public class MeshBuilder
    {
        List<Vector3> m_vertices = new List<Vector3>();
        List<int> m_indices = new List<int>();

        // skinning
        List<BoneWeight> m_boneWeights = new List<BoneWeight>();

        public void PushQuadrangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int boneIndex)
        {
            var i = m_vertices.Count;
            m_vertices.Add(v0);
            m_vertices.Add(v1);
            m_vertices.Add(v2);
            m_vertices.Add(v3);

            m_indices.Add(i);
            m_indices.Add(i + 1);
            m_indices.Add(i + 2);

            m_indices.Add(i + 2);
            m_indices.Add(i + 3);
            m_indices.Add(i);

            {
                var boneWeight = new BoneWeight
                {
                    boneIndex0 = boneIndex,
                    boneIndex1 = default,
                    boneIndex2 = default,
                    boneIndex3 = default,
                    weight0 = 1.0f,
                    weight1 = default,
                    weight2 = default,
                    weight3 = default,
                };
                m_boneWeights.Add(boneWeight);
                m_boneWeights.Add(boneWeight);
                m_boneWeights.Add(boneWeight);
                m_boneWeights.Add(boneWeight);
            }
        }

        public void PushCube()
        {
            // There is a bone every 4 x 6 = 24 vertices.
            var i = m_vertices.Count;
            var boneIndex = i / 24;

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
            var v0 = new Vector3(-s, -s, -s);
            var v1 = new Vector3(+s, -s, -s);
            var v2 = new Vector3(+s, -s, +s);
            var v3 = new Vector3(-s, -s, +s);
            var v4 = new Vector3(-s, s, -s);
            var v5 = new Vector3(+s, s, -s);
            var v6 = new Vector3(+s, s, +s);
            var v7 = new Vector3(-s, s, +s);
            PushQuadrangle(v0, v1, v2, v3, boneIndex);
            PushQuadrangle(v5, v4, v7, v6, boneIndex);
            PushQuadrangle(v1, v0, v4, v5, boneIndex);
            PushQuadrangle(v2, v1, v5, v6, boneIndex);
            PushQuadrangle(v3, v2, v6, v7, boneIndex);
            PushQuadrangle(v0, v3, v7, v4, boneIndex);
        }

        public Mesh ToMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = m_vertices.ToArray();
            mesh.boneWeights = m_boneWeights.ToArray();
            mesh.subMeshCount = 1;
            mesh.SetTriangles(m_indices, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}