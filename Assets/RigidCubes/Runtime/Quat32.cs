// from
// https://github.com/i-saint/Glimmer/blob/master/Source/MeshUtils/muQuat32.h
//
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RigidCubes
{
    public static class Quat32
    {
        const float SR2 = 1.41421356237f;
        const float RSR2 = 1.0f / 1.41421356237f;

        const float C = 0x3FF;

        const float R = 1.0f / 0x3FF;

        static uint pack(float a)
        {
            return (uint)((a * SR2 + 1.0f) * 0.5f * C);
        }
        static float unpack(uint a)
        {
            return ((a * R) * 2.0f - 1.0f) * RSR2;
        }
        static float square(float a) { return a * a; }
        static int dropmax(float a, float b, float c, float d)
        {
            if (a > b && a > c && a > d)
                return 0;
            if (b > c && b > d)
                return 1;
            if (c > d)
                return 2;
            return 3;
        }
        static float sign(float v) { return v < 0.0f ? -1.0f : 1.0f; }

        public struct Packed
        {
            const uint MASK_0 = (uint)0x3FF;
            const uint MASK_1 = (uint)0x3FF << 10;
            const uint MASK_2 = (uint)0x3FF << 20;

            public uint Self;
            public uint x0
            {
                get
                {
                    return Self & 0x3FF;
                }
                set
                {
                    Self = (Self & ~MASK_0) | (value);
                }
            }
            public uint x1
            {
                get
                {
                    return (Self >> 10) & 0x3FF;
                }
                set
                {
                    Self = (Self & ~MASK_1) | (value << 10);
                }
            }
            public uint x2
            {
                get
                {
                    return (Self >> 20) & 0x3FF;
                }
                set
                {
                    Self = (Self & ~MASK_2) | (value << 20);
                }
            }
            public uint drop
            {
                get
                {
                    return (Self >> 30) & 0x3;
                }
                set
                {
                    Self = (Self & ~(uint)0x3) | (value << 30);
                }
            }
        };

        public static uint Pack(float x, float y, float z, float w)
        {

            Packed value = default;

            float a0, a1, a2;
            value.drop = (uint)dropmax(square(x), square(y), square(z), square(w));
            if (value.drop == 0)
            {
                float s = sign(x);
                a0 = y * s;
                a1 = z * s;
                a2 = w * s;
            }
            else if (value.drop == 1)
            {
                float s = sign(y);
                a0 = x * s;
                a1 = z * s;
                a2 = w * s;
            }
            else if (value.drop == 2)
            {
                float s = sign(z);
                a0 = x * s;
                a1 = y * s;
                a2 = w * s;
            }
            else
            {
                float s = sign(w);
                a0 = x * s;
                a1 = y * s;
                a2 = z * s;
            }

            value.x0 = pack(a0);
            value.x1 = pack(a1);
            value.x2 = pack(a2);

            return value.Self;
        }

        public static Quaternion Unpack(uint src)
        {

            var value = new Packed
            {
                Self = src,
            };

            float a0 = unpack(value.x0);
            float a1 = unpack(value.x1);
            float a2 = unpack(value.x2);
            float tmp = 1.0f - (square(a0) + square(a1) + square(a2));
            float iss = UnityEngine.Mathf.Sqrt(tmp);

            switch (value.drop)
            {
                case 0:
                    return new Quaternion(iss, a0, a1, a2);
                case 1:
                    return new Quaternion(a0, iss, a1, a2);
                case 2:
                    return new Quaternion(a0, a1, iss, a2);
                default:
                    return new Quaternion(a0, a1, a2, iss);
            }
        }
    }
}
