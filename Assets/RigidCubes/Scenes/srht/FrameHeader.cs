
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public enum FrameFlags : UInt32
{
    NONE = 0,
    // enableed rotation is Quat32: disabled rotation is float4(x, y, z, w)
    USE_QUAT32 = 0x1,
};

[StructLayout(LayoutKind.Sequential)]
public struct FrameHeader
{
    public const int VALIDATE_SIZE = 40;

    UInt64 magic;
    // std::chrono::nanoseconds
    public Int64 time;
    public FrameFlags flags;
    public UInt16 skeletonId;
    // root position
    public float x;
    public float y;
    public float z;

    public Vector3 RootPosition => new Vector3(x, y, z);
};
