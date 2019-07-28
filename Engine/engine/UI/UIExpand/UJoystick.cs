using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace UEngine.UI.UILuaBehaviour
{
    public enum EJoystickType
    {
        MovePlayer,
        MoveCamera,
    }

    public class UJoystick : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        private Vector3 mOrigin;

        private Vector3 mDragDist = new Vector3();

        public float mMoveMaxDistance = 80;

        public float mActiveMoveDistance = 1;

        [HideInInspector]
        public bool mIsDragging = false;

        [HideInInspector]
        public Vector3 mMovePosiNorm;

        public Action onStart = null;
        public Action onStop = null;
        public Action<Vector3> onMove = null;

        void Start()
        {
            mOrigin = transform.localPosition;
        }

        void Update()
        {
        }

        void OnDrag(GameObject go, Vector2 delta)
        {
            if (!mIsDragging)
            {
                mIsDragging = true;

                mDragDist = new Vector3();

                if (null != onStart)
                    onStart();
            }

            mDragDist += new Vector3(delta.x, delta.y, 0f);

            var pos = transform.localPosition;

            float dis = Vector3.Magnitude(mDragDist);
            if (dis >= mMoveMaxDistance)
            {
                Vector3 vec = mDragDist * mMoveMaxDistance / dis;

                pos += vec;
            }
            else
            {
                pos += new Vector3(delta.x, delta.y, 0f);
            }

            dis = Vector3.Distance(pos, mOrigin);
            if (dis >= mMoveMaxDistance)
            {
                Vector3 vec = mOrigin + (pos - mOrigin) * mMoveMaxDistance / dis;

                transform.localPosition = vec;
            }
            else
            {
                transform.localPosition = pos;
            }

            if (Vector3.Distance(transform.localPosition, mOrigin) > mActiveMoveDistance)
            {
                mMovePosiNorm = (transform.localPosition - mOrigin).normalized;
                mMovePosiNorm = new Vector3(mMovePosiNorm.x, 0, mMovePosiNorm.y);

                if (null != onMove)
                    onMove(mMovePosiNorm);
            }
            else
            {
                mMovePosiNorm = Vector3.zero;
            }
        }

        public void OnDragOut()
        {
            mIsDragging = false;

            transform.localPosition = mOrigin;

            mMovePosiNorm = Vector3.zero;

            if (null != onStop)
                onStop();
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDrag(gameObject, eventData.delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnDragOut();
        }
    }
}