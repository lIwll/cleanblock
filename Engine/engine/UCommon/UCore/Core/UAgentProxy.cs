using System;
using System.Collections.Generic;

using UnityEngine;

namespace UEngine
{
    [AddComponentMenu("")]
    public class UAgentProxy : MonoBehaviour
    {
        protected GameObject mObject;

        protected bool mIsInited = false;

        void Start()
        {
            OnStart();
        }

        void Update()
        {
            OnUpdate();
        }

        void OnDrawGizmos()
        {
            OnRenderNormal();
        }

        void OnDrawGizmosSelected()
        {
            OnRenderSelected();
        }

        public void OnValidate()
        {
            if (!mIsInited)
                OnInit();

            OnUpdateAtribute();
        }

        public virtual void OnInit()
        {
            mIsInited = true;
        }

        public virtual void ApplyAttributeChanged()
        {
            mIsInited = true;
        }

        public virtual void CancelAttributeChanged()
        {
            mIsInited = true;
        }

        public virtual string GetAgentType()
        {
            return "UAgent";
        }

        public virtual void OnSelected()
        {
        }

        public virtual void OnUnselected()
        {
        }

        protected virtual void OnUpdateAtribute()
        {
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnRenderNormal()
        {
        }

        protected virtual void OnRenderSelected()
        {
        }
    }

    [AddComponentMenu("")]
    public class UDesignRoot : MonoBehaviour
    {
    }
}
