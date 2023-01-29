using UnityEngine;

namespace RigidCubes
{
    public struct RigidTransform
    {
        public Quaternion Rotation;
        public Vector3 Translation;

        public RigidTransform(Quaternion r, Vector3 t)
        {
            Rotation = r;
            Translation = t;
        }

        public static RigidTransform Identity => new RigidTransform
        {
            Rotation = Quaternion.identity,
            Translation = Vector3.zero,
        };

        public static RigidTransform FromMatrix(Matrix4x4 m)
        {
            return new RigidTransform
            {
                Rotation = m.rotation,
                Translation = m.GetColumn(3),
            };
        }

        public RigidTransform Inversed()
        {
            var inv = Quaternion.Inverse(Rotation);
            return new RigidTransform
            {
                Rotation = inv,
                Translation = inv * (-Translation),
            };
        }

        public static RigidTransform operator *(RigidTransform lhs, RigidTransform rhs)
        {
            return new RigidTransform
            {
                Rotation = lhs.Rotation * rhs.Rotation,
                Translation = lhs.Rotation * rhs.Translation + lhs.Translation,
            };
        }

        public Matrix4x4 Matrix
        {
            get
            {
                var r = Matrix4x4.Rotate(Rotation);
                r.SetColumn(3, new Vector4(Translation.x, Translation.y, Translation.z, 1));
                return r;
            }
        }
    }
}