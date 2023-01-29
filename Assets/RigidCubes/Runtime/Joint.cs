using UnityEngine;

namespace RigidCubes
{
    public class Joint
    {
        public RigidTransform Transform;
        public RigidTransform InitialFromParent;
        public Matrix4x4 Shape = Matrix4x4.Scale(new Vector3(0.02f, 0.02f, 0.02f));
        public Matrix4x4 ShapeAndTransform => Transform.Matrix * Shape;

        /// <summary>
        /// cube の向きとサイズを決める
        /// [回転]
        /// Y軸 head->tail
        /// Z軸 world zaxis
        /// [width, height, depth]
        /// 長さを head-tail
        /// </summary>
        /// <param name="child"></param>
        public void SetTail(Joint child)
        {
            var tail = child.InitialFromParent.Translation;
            var y = tail.normalized;
            var z = Vector3.forward;
            var x = Vector3.Cross(y, z).normalized;
            z = Vector3.Cross(x, y).normalized;

            var r = new Matrix4x4(
                new Vector4(x.x, x.y, x.z, 0),
                new Vector4(y.x, y.y, y.z, 0),
                new Vector4(z.x, z.y, z.z, 0),
                new Vector4(0, 0, 0, 1)
                );
            var s = Matrix4x4.Scale(new Vector3(0.02f, tail.magnitude, 0.02f));
            Shape = r * s;
        }
    }
}