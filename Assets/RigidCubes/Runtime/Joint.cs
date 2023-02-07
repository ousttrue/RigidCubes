using System.Collections.Generic;
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
        public Transform UnityTransform;

        const float CENTIMETER = 0.01f;
        public Matrix4x4 Shape = Matrix4x4.Scale(new Vector3(CENTIMETER, CENTIMETER, CENTIMETER));

        public List<Joint> Children = new List<Joint>();

        public void SetShape(Quaternion r, Vector3 scalingCenter, Vector3 widthHeightDepth)
        {
            // x-mirror
            r = new Quaternion(-r.x, r.y, r.z, -r.w);
            var center = Matrix4x4.Translate(-scalingCenter);
            var s = Matrix4x4.Scale(widthHeightDepth);
            Shape = Matrix4x4.Rotate(r) * s * center;
        }

        public void SetShape(Vector3 scalingCenter, Vector3 widthHeightDepth)
        {
            SetShape(Quaternion.identity, scalingCenter, widthHeightDepth);
        }

        public void NegativeZAxisHeadTailShape(Vector3 localTail, Vector3 localUp)
        {
            var z = -localTail.normalized;
            var y = localUp.normalized;
            var x = Vector3.Cross(y, z).normalized;
            y = Vector3.Cross(z, x).normalized;
            var r = new Matrix4x4(
                new Vector4(x.x, x.y, x.z, 0),
                new Vector4(y.x, y.y, y.z, 0),
                new Vector4(z.x, z.y, z.z, 0),
                new Vector4(0, 0, 0, 1)
                );
            SetShape(r.rotation, new Vector3(0, 0, 0.5f), new Vector3(0.02f, localTail.magnitude, 0.02f));
        }

        /// <summary>
        /// cube の向きとサイズを決める
        /// [回転]
        /// Y軸 head->tail
        /// Z軸 world zaxis
        /// [width, height, depth]
        /// 長さを head-tail
        /// </summary>
        /// <param name="child"></param>
        public void YAxisHeadTailShape(Vector3 localTail, Vector3 localForward)
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
            SetShape(r.rotation, new Vector3(0, -0.5f, 0), new Vector3(0.02f, localTail.magnitude, 0.02f));
        }
    }
}
