using System.Collections.Generic;
using UnityEngine;

namespace RigidCubes
{
    public class MeshBuilder
    {
        List<Vector3> m_vertices = new List<Vector3>();
        List<int> m_indices = new List<int>();

        public void PushQuadrangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
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
        }

        public Mesh ToMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = m_vertices.ToArray();
            mesh.subMeshCount = 1;
            mesh.SetTriangles(m_indices, 0);
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}