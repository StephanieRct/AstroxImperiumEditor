using System;
using System.Collections.Generic;

namespace AstroxEditor
{

    public struct double3
    {
        public double x;
        public double y;
        public double z;
        public double3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static double3 operator +(double3 a, double3 b) => new double3 { x = a.x + b.x, y = a.y + b.y, z = a.z + b.z };
        public static double3 operator -(double3 a, double3 b) => new double3 { x = a.x - b.x, y = a.y - b.y, z = a.z - b.z };
        public static double3 operator *(double3 a, double3 b) => new double3 { x = a.x * b.x, y = a.y * b.y, z = a.z * b.z };
        public static double3 operator /(double3 a, double3 b) => new double3 { x = a.x / b.x, y = a.y / b.y, z = a.z / b.z };

        public static double3 operator +(double3 a, double b) => new double3 { x = a.x + b, y = a.y + b, z = a.z + b };
        public static double3 operator -(double3 a, double b) => new double3 { x = a.x - b, y = a.y - b, z = a.z - b };
        public static double3 operator *(double3 a, double b) => new double3 { x = a.x * b, y = a.y * b, z = a.z * b };
        public static double3 operator /(double3 a, double b) => new double3 { x = a.x / b, y = a.y / b, z = a.z / b };
        public static double3 operator +(double b, double3 a) => new double3 { x = b + a.x, y = b + a.y, z = b + a.z };
        public static double3 operator -(double b, double3 a) => new double3 { x = b - a.x, y = b - a.y, z = b - a.z };
        public static double3 operator *(double b, double3 a) => new double3 { x = b * a.x, y = b * a.y, z = b * a.z };
        public static double3 operator /(double b, double3 a) => new double3 { x = b / a.x, y = b / a.y, z = b / a.z };
        public UnityEngine.Vector3 ToV3() => new UnityEngine.Vector3((float)x, (float)y, (float)z);
    }
    public struct double4
    {
        public double x;
        public double y;
        public double z;
        public double w;
        public double4(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public double3 xyz => new double3(x, y, z);
        public static double4 operator +(double4 a, double4 b) => new double4 { x = a.x + b.x, y = a.y + b.y, z = a.z + b.z, w = a.w + b.w };
        public static double4 operator -(double4 a, double4 b) => new double4 { x = a.x - b.x, y = a.y - b.y, z = a.z - b.z, w = a.w - b.w };
        public static double4 operator *(double4 a, double4 b) => new double4 { x = a.x * b.x, y = a.y * b.y, z = a.z * b.z, w = a.w * b.w };
        public static double4 operator /(double4 a, double4 b) => new double4 { x = a.x / b.x, y = a.y / b.y, z = a.z / b.z, w = a.w / b.w };
        public static double4 operator +(double4 a, double b) => new double4 { x = a.x + b, y = a.y + b, z = a.z + b, w = a.w + b };
        public static double4 operator -(double4 a, double b) => new double4 { x = a.x - b, y = a.y - b, z = a.z - b, w = a.w - b };
        public static double4 operator *(double4 a, double b) => new double4 { x = a.x * b, y = a.y * b, z = a.z * b, w = a.w * b };
        public static double4 operator /(double4 a, double b) => new double4 { x = a.x / b, y = a.y / b, z = a.z / b, w = a.w / b };
        public static double4 operator +(double b, double4 a) => new double4 { x = b + a.x, y = b + a.y, z = b + a.z, w = b + a.w };
        public static double4 operator -(double b, double4 a) => new double4 { x = b - a.x, y = b - a.y, z = b - a.z, w = b - a.w };
        public static double4 operator *(double b, double4 a) => new double4 { x = b * a.x, y = b * a.y, z = b * a.z, w = b * a.w };
        public static double4 operator /(double b, double4 a) => new double4 { x = b / a.x, y = b / a.y, z = b / a.z, w = b / a.w };

    }
    public struct quaternion
    {

        public double4 value;
        quaternion(double4 v)
        {
            value = v;
        }

        public static quaternion AxisAngle(double3 axis, double angle)
        {
            var s = Math.Sin(angle * 0.5);
            var c = Math.Cos(angle * 0.5);
            return new quaternion(new double4 { x = axis.x * s, y = axis.y * s, z = axis.z * s, w = c });
        }
    }


    public partial struct math
    {
        public static double dot(double3 a, double3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static double3 cross(double3 a, double3 b) => new double3
        {
            x = a.y * b.z - a.z * b.y,
            y = a.z * b.x - a.x * b.z,
            z = a.x * b.y - a.y * b.x
        };

        public static double length(double3 a) => Math.Sqrt(math.dot(a, a));
        public static double lengthSqr(double3 a) => math.dot(a, a);

        public static double dist(double3 a, double3 b) => length(a - b);
        public static double dist(double a, double b) => Math.Abs(a - b);
        public static double3 normalize(double3 a) => a * rcp(length(a));
        public static double3 normalize(double3 a, out double length)
        {
            length = math.length(a);
            return a * rcp(length);
        }

        public static double rcp(double a) => 1 / a;
        public static double3 rcp(double3 a) => new double3 { x = 1 / a.x, y = 1 / a.y, z = 1 / a.z };



        public static double3 rotate(quaternion q, double3 v)
        {
            double3 t = 2 * cross(q.value.xyz, v);
            return v + q.value.w * t + cross(q.value.xyz, t);
        }


    }
}