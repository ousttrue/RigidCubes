using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct JointDefinition
{
    public const int VALIDATE_SIZE = 16;

    // parentBone(-1 for root)
    public UInt16 parentBoneIndex;
    // HumanBones or any number
    public UInt16 boneType;
    public float xFromParent;
    public float yFromParent;
    public float zFromParent;
};

public enum SkeletonFlags : UInt32
{
    NONE = 0,
    // if hasInitialRotation PackQuat X jointCount for InitialRotation
    HAS_INITIAL_ROTATION = 0x1,
};

[StructLayout(LayoutKind.Sequential)]
public struct SkeletonHeader
{
    public const int VALIDATE_SIZE = 16;

    UInt64 magic;
    public UInt16 skeletonId;
    public UInt16 jointCount;
    public SkeletonFlags flags;
};
