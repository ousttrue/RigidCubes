using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RigidCubes
{
    public class Parser : MonoBehaviour
    {
        public UnityEngine.Events.UnityEvent<SkeletonHeader, JointDefinition[]> OnSkeleton;
        public UnityEngine.Events.UnityEvent<FrameHeader, Quaternion[]> OnFrame;
        public UnityEngine.Events.UnityEvent<FrameHeader, Matrix4x4[]> OnMatrix;

        const string MAGIC_FRAME = "SRHTFRM1";
        const string MAGIC_SKELETON = "SRHTSKL1";

        int m_skeleton;
        int m_frame;
        int m_jointCount;

        public void OnMessage(ArraySegment<byte> message)
        {
            var magic = System.Text.Encoding.ASCII.GetString(message.Array, 0, 8);
            switch (magic)
            {
                case MAGIC_SKELETON:
                    {
                        ++m_skeleton;
                        SkeletonHeader header = default;
                        using (var pin = new ArrayPin(message))
                        {
                            header = Marshal.PtrToStructure<SkeletonHeader>(pin.Ptr);
                        }
                        m_jointCount = header.jointCount;
                        var joints = new JointDefinition[m_jointCount];
                        using (var pin = new ArrayPin(joints))
                        {
                            Marshal.Copy(message.Array, message.Offset + Marshal.SizeOf<SkeletonHeader>(), pin.Ptr, Marshal.SizeOf<JointDefinition>() * m_jointCount);
                        }
                        OnSkeleton.Invoke(header, joints);
                    }
                    break;

                case MAGIC_FRAME:
                    {
                        ++m_frame;

                        FrameHeader header = default;
                        if (Marshal.SizeOf<FrameHeader>() != FrameHeader.VALIDATE_SIZE)
                        {
                            throw new Exception("invalid size");
                        }
                        using (var pin = new ArrayPin(message))
                        {
                            header = Marshal.PtrToStructure<FrameHeader>(pin.Ptr);
                        }

                        if (m_jointCount > 0)
                        {
                            var rotations = new Matrix4x4[m_jointCount];
                            using (var pin = new ArrayPin(rotations))
                            {
                                Marshal.Copy(message.Array, message.Offset + Marshal.SizeOf<FrameHeader>(), pin.Ptr, Marshal.SizeOf<Matrix4x4>() * m_jointCount);
                            }
                            OnMatrix.Invoke(header, rotations);
                        }
                    }
                    break;

                default:
                    Debug.LogWarning($"unknown: {magic:X}");
                    break;
            }
        }

        public void OnGUI()
        {
            GUILayout.Label($"skeleton: {m_skeleton}");
            GUILayout.Label($"frame: {m_frame}");
        }
    }
}