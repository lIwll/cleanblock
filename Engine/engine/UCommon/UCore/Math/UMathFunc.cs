using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace UEngine
{
    public class UVertices
    {
        List< Vector3 > mVertices = new List< Vector3 >();

        public void Add(Vector3 v)
        {
            mVertices.Add(v);
        }

        public void Clear()
        {
            mVertices.Clear();
        }

        public Vector3[] ToArray()
        {
            return mVertices.ToArray();
        }
    }

    public static class UMathFunc
    {
        private static int fls(int x)
        {
            int pos = 0;
            if (0 != x)
            {
                int i = (x >> 1);
                for (; i != 0; ++ pos)
                    i >>= 1;

                return pos + 1;
            } else
            {
                pos = -1;
            }

            return pos + 1;
        }

        public static bool Differs(float a, float b)
        {
            return Mathf.Abs(a - b) > 0.0001f;
        }

        public static float WrapAngle(float angle)
        {
            while (angle > 180f)
                angle -= 360f;
            while (angle < -180f)
                angle += 360f;

            return angle;
        }

        public static int roundup_pow_of_two(int x)
        {
            return 1 << fls(x - 1);
        }

        public static bool CheckInTheArea(Vector3 pos, Vector3[] vertices)
        {
            int len = vertices.Length;
            if (len < 3)
                return false;

            bool symbol = false;

            for (int i = 0, j = len - 1; i < len; j = i ++) 
            {
                var p1 = vertices[i];
                var p2 = vertices[j];

                if (((p1.z > pos.z) != (p2.z > pos.z)) && (pos.x < (p2.x - p1.x) * (pos.z - p1.z) / (p2.z - p1.z) + p1.x))
                    symbol = !symbol;
            }

            return symbol;
        }

        public static bool CheckInTheArea(Vector3 pos, UVertices vertices)
        {
            return CheckInTheArea(pos, vertices.ToArray());
        }

        public static Vector3 AngleToVector3(float radian)
        {
            float r = Mathf.PI * 2f - radian + Mathf.PI * 0.5f;

            var dir = new Vector3(Mathf.Cos(r), 0, Mathf.Sin(r));
                dir.Normalize();

            return dir;
        }
    }
}
