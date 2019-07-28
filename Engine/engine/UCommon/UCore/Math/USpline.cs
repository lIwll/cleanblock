using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UEngine
{
    public class USpline
    {
        List< USplineControlPoint > mControlPoints = new List< USplineControlPoint >();
        List< USplineControlPoint > mSegments = new List< USplineControlPoint >();

        public int mGranularity = 20;

        public USplineControlPoint this[int idx]
        {
            get
            {
                if (idx > -1 && idx < mSegments.Count)
                    return mSegments[idx];

                return null;
            }
        }

        public List< USplineControlPoint > Segments
        {
            get
            {
                return mSegments;
            }
        }

        public List< USplineControlPoint > ControlPoints
        {
            get
            {
                return mControlPoints;
            }
        }

        public USplineControlPoint NextControlPoint(USplineControlPoint controlpoint)
        {
            if (mControlPoints.Count == 0)
                return null; 

            int i = controlpoint.mControlPointIndex + 1;
            if (i < mControlPoints.Count)
                return mControlPoints[i];

            return null;
        }

        public USplineControlPoint PreviousControlPoint(USplineControlPoint controlpoint)
        {
            if (mControlPoints.Count == 0)
                return null;

            int i = controlpoint.mControlPointIndex - 1;
            if (i >= 0)
                return mControlPoints[i];

            return null;
        }

        public Vector3 NextPosition(USplineControlPoint controlpoint)
        {
            USplineControlPoint seg = NextControlPoint(controlpoint);
            if (seg != null)
                return seg.mPosition;

            return controlpoint.mPosition;
        }

        public Vector3 PreviousPosition(USplineControlPoint controlpoint)
        {
            USplineControlPoint seg = PreviousControlPoint(controlpoint);
            if (seg != null)
                return seg.mPosition;

            return controlpoint.mPosition;
        }

        public Vector3 PreviousNormal(USplineControlPoint controlpoint)
        {
            USplineControlPoint seg = PreviousControlPoint(controlpoint);
            if (seg != null)
                return seg.mNormal;

            return controlpoint.mNormal;
        }

        public Vector3 NextNormal(USplineControlPoint controlpoint)
        {
            USplineControlPoint seg = NextControlPoint(controlpoint);
            if (seg != null)
                return seg.mNormal;

            return controlpoint.mNormal;
        }

        public USplineControlPoint LenToSegment(float t, out float localF)
        {
            USplineControlPoint seg = null;

            t = Mathf.Clamp01(t);

            float len = t * mSegments[mSegments.Count - 1].mDist;

            int idx = 0;
            for (idx = 0; idx < mSegments.Count; idx ++)
            {
                if (mSegments[idx].mDist >= len)
                {
                    seg = mSegments[idx];

                    break;
                }
            }

            if (idx == 0)
            {
                localF = 0f;

                return seg;
            }

            USplineControlPoint prevSeg = mSegments[seg.mSegmentIndex - 1];
            float PrevLen = seg.mDist - prevSeg.mDist;
            localF = (len - prevSeg.mDist) / PrevLen;

            return prevSeg;
        }

        public static Vector3 CatmulRom(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
        {
            double DT1 = -0.5; 
            double DT2 = 1.5; 
            double DT3 = -1.5; 
            double DT4 = 0.5;

            double DE2 = -2.5; 
            double DE3 = 2; 
            double DE4 = -0.5;

            double DV1 = -0.5;
            double DV3 = 0.5;

            double FAX = DT1 * T0.x + DT2 * P0.x + DT3 * P1.x + DT4 * T1.x;
            double FBX = T0.x + DE2 * P0.x + DE3 * P1.x + DE4 * T1.x;
            double FCX = DV1 * T0.x + DV3 * P1.x;
            double FDX = P0.x;

            double FAY = DT1 * T0.y + DT2 * P0.y + DT3 * P1.y + DT4 * T1.y;
            double FBY = T0.y + DE2 * P0.y + DE3 * P1.y + DE4 * T1.y;
            double FCY = DV1 * T0.y + DV3 * P1.y;
            double FDY = P0.y;

            double FAZ = DT1 * T0.z + DT2 * P0.z + DT3 * P1.z + DT4 * T1.z;
            double FBZ = T0.z + DE2 * P0.z + DE3 * P1.z + DE4 * T1.z;
            double FCZ = DV1 * T0.z + DV3 * P1.z;
            double FDZ = P0.z;

            float FX = (float)(((FAX * f + FBX) * f + FCX) * f + FDX);
            float FY = (float)(((FAY * f + FBY) * f + FCY) * f + FDY);
            float FZ = (float)(((FAZ * f + FBZ) * f + FCZ) * f + FDZ);

            return new Vector3(FX, FY, FZ);
        }

        public Vector3 InterpolateByLen(float tl)
        {
            float localF;
            USplineControlPoint seg = LenToSegment(tl, out localF);

            return seg.Interpolate(localF);
        }

        public Vector3 InterpolateNormalByLen(float tl)
        {
            float localF;
            USplineControlPoint seg = LenToSegment(tl, out localF);

            return seg.InterpolateNormal(localF);
        }

        public USplineControlPoint AddControlPoint(Vector3 pos, Vector3 up)
        {
            USplineControlPoint cp = new USplineControlPoint();

            cp.Init(this);

            cp.mPosition = pos;

            cp.mNormal = up;

            mControlPoints.Add(cp);

            cp.mControlPointIndex = mControlPoints.Count - 1;

            return cp;
        }

        public void Clear()
        {
            mControlPoints.Clear();
        }

        void RefreshDistance()
        {
            if (mSegments.Count < 1)
                return;

            mSegments[0].mDist = 0f;

            for (int i = 1; i < mSegments.Count; i ++)
            {

                float prevLen = (mSegments[i].mPosition - mSegments[i - 1].mPosition).magnitude;

                mSegments[i].mDist = mSegments[i - 1].mDist + prevLen;
            }
        }

        public void RefreshSpline()
        {
            mSegments.Clear();

            for (int i = 0; i < mControlPoints.Count; i ++)
            {
                if (mControlPoints[i].IsValid)
                {
                    mSegments.Add(mControlPoints[i]);

                    mControlPoints[i].mSegmentIndex = mSegments.Count - 1;
                }
            }

            RefreshDistance();
        }
    }
}
