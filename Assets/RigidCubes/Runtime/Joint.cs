using UnityEngine;

namespace RigidCubes
{
    public enum TailOrientation
    {
        Up,
        Down,
        Right,
        Left,
    }

    public class Joint
    {
        public RigidTransform Transform;
        public RigidTransform Initial;
        public RigidTransform InitialFromParent;

        const float CENTIMETER = 0.01f;
        public Matrix4x4 Shape = Matrix4x4.Scale(new Vector3(CENTIMETER, CENTIMETER, CENTIMETER));

        /// <summary>
        /// cube の向きとサイズを決める
        /// [回転]
        /// Y軸 head->tail
        /// Z軸 world zaxis
        /// [width, height, depth]
        /// 長さを head-tail
        /// </summary>
        /// <param name="child"></param>
        public void HeadTailShape(Vector3 localTail, Vector3 localForward)
        {
            var y = localTail.normalized;
            var z = localForward.normalized;
            var x = Vector3.Cross(y, z).normalized;
            z = Vector3.Cross(x, y).normalized;

            var r = new Matrix4x4(
                new Vector4(x.x, x.y, x.z, 0),
                new Vector4(y.x, y.y, y.z, 0),
                new Vector4(z.x, z.y, z.z, 0),
                new Vector4(0, 0, 0, 1)
                );
            var q = r.rotation;
            q = new Quaternion(-q.x, q.y, q.z, -q.w);

            var s = Matrix4x4.Scale(new Vector3(0.02f, localTail.magnitude, 0.02f));
            var center = Matrix4x4.Translate(new Vector3(0, 0.5f, 0));
            Shape = Matrix4x4.Rotate(q) * s * center;
        }

        public void SetShape(Vector3 scalingCenter, Vector3 widthHeightDepth)
        {
            var center = Matrix4x4.Translate(scalingCenter);
            var s = Matrix4x4.Scale(widthHeightDepth);
            Shape = s * center;
        }
    }
}
