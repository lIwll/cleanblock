using System.Collections;

using UnityEngine;

namespace UEngine
{
    public class USplineControlPoint
    {
        public Vector3 mPosition;
        public Vector3 mNormal;

        public int mControlPointIndex = -1;
        public int mSegmentIndex = -1;

        public float mDist;

        protected USpline mSpline;

        public USplineControlPoint NextControlPoint
        {
            get
            {
                return mSpline.NextControlPoint(this);
            }
        }

        public USplineControlPoint PreviousControlPoint
        {
            get
            {
                return mSpline.PreviousControlPoint(this);
            }
        }

        public Vector3 NextPosition
        {
            get
            {
                return mSpline.NextPosition(this);
            }
        }

        public Vector3 PreviousPosition
        {
            get
            {
                return mSpline.PreviousPosition(this);
            }
        }

        public Vector3 NextNormal
        {
            get
            {
                return mSpline.NextNormal(this);
            }
        }

        public Vector3 PreviousNormal
        {
            get { return mSpline.PreviousNormal(this); }
        }

        public bool IsValid
        {
            get
            {
                return (NextControlPoint != null);
            }
        }

        Vector3 GetNext2Position()
        {
            USplineControlPoint cp = NextControlPoint;
            if (cp != null)
                return cp.NextPosition;

            return NextPosition;
        }

        Vector3 GetNext2Normal()
        {
            USplineControlPoint cp = NextControlPoint;
            if (cp != null)
                return cp.NextNormal;

            return mNormal;
        }

        public Vector3 Interpolate(float localF)
        {
            localF = Mathf.Clamp01(localF);

            return USpline.CatmulRom(PreviousPosition, mPosition, NextPosition, GetNext2Position(), localF);
        }

        public Vector3 InterpolateNormal(float localF)
        {
            localF = Mathf.Clamp01(localF);

            return USpline.CatmulRom(PreviousNormal, mNormal, NextNormal, GetNext2Normal(), localF);
        }

        public void Init(USpline owner)
        {
            mSpline = owner;

            mSegmentIndex = -1;
        }
    }
}
