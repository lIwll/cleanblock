using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UEngine.UI.UILuaBehaviour;

namespace UEngine.UIAnimation
{
    public class UIAnimationManage : MonoBehaviour
    {
        static List<ULuaBehaviourBase> UIBaseList = new List<ULuaBehaviourBase>();

        static List<UIAnimatorBase> UIAniBaseList = new List<UIAnimatorBase>();

        static List<AnimatorTask> UIAnimtorList = new List<AnimatorTask>();

        void Awake ( ) 
        {
            
        }

        void Start ( ) 
        {

        }

        void OnEnable ( ) 
        {

        }

        void Update ( ) 
        {
            UIBaseUpdate();
        }

        void LateUpdate ( ) 
        {
            UIAnimatorUpdata();
        }

        void UIAnimatorUpdata ( ) 
        {
            for (int i = 0 ; i < UIAnimtorList.Count ; i++)
            {
                if (UIAnimtorList[i] != null)
                {
                    UIAnimtorList[i].Update(Time.deltaTime);
                }
            }
        }

        void UIBaseUpdate ( ) 
        {
            for (int i = 0 ; i < UIBaseList.Count ; i++)
            {
                if (UIBaseList[i])
                {
                    UIBaseList[i].ILateUpdate();
                }
            }
        }

        public static void AddUIBase ( ULuaBehaviourBase uiBase ) 
        {
            if (!UIBaseList.Contains( uiBase ))
            {
                UIBaseList.Add( uiBase );
            }
        }

        public static void RemoveUIBase ( ULuaBehaviourBase uiBase ) 
        {
            if (UIBaseList.Contains(uiBase))
            {
                UIBaseList.Remove( uiBase );
            }
        }

        public static void AddUIAniBase ( UIAnimatorBase aniBase ) 
        {
            UIAniBaseList.Add( aniBase );
        }

        public static void RemoveAniBase ( UIAnimatorBase aniBase ) 
        {
            if (UIAniBaseList.Contains( aniBase ))
            {
                UIAniBaseList.Remove( aniBase );
            }
        }

        public static void AddUIAnimtor ( AnimatorTask task ) 
        {
            UIAnimtorList.Add( task );
        }

        public static void RemoveAnimator ( UIAnimator Animator ) 
        {
            for (int i = 0 ; i < UIAnimtorList.Count ; i++)
            {
                if (Animator == UIAnimtorList[i].Animator)
                {
                    UIAnimtorList.RemoveAt(i);
                    break;
                }
            }
        }

        public static void RemoveAnimator ( AnimatorTask task ) 
        {
            if (UIAnimtorList.Contains( task ))
            {
                UIAnimtorList.Remove( task );
            }
        }

        public static AnimatorTask ContainsAnimator ( UIAnimator Animator ) 
        {
            for (int i = 0 ; i < UIAnimtorList.Count ; i++)
            {
                if (Animator == UIAnimtorList[i].Animator)
                {
                    return UIAnimtorList[i];
                }
            }
            return null;
        }
    }

    public class AnimatorTask 
    {
        public bool IsLoop;

        public UIAnimator Animator;

        public Action LoopFunction;

        public Action<int> LoopAniEnvents;

        public float mCurrentTime = 0f;

        public int index = 0;

        public Dictionary<int, AnimatorTaskHelp> mTaskDic = new Dictionary<int, AnimatorTaskHelp>();

        public PanelManager manager;

        public void Init ( ) 
        {
            Animator.Init( manager, this );
        }

        public void AddTask ( int Times, Action function, Action<int> AniEvents ) 
        {
            AnimatorTaskHelp help = new AnimatorTaskHelp( Times, function, AniEvents );
            mTaskDic.Add(index, help);
            index++;
        }

        public void Stop ( bool IsEnd ) 
        {
            if (IsEnd)
            {
                UpdataData(Animator.TotalFrame / 30);
            }

            UIAnimationManage.RemoveAnimator(this);
        }

        int TriggerIndex = 0;
        int mCurrentTimes = 0;
        public void Update ( float deltaTime ) 
        {
            UpdataData( mCurrentTime );
            if (IsLoop)
            {
                Animator.UpdateEvent( mCurrentTime, LoopAniEnvents );
                if (mCurrentTime >= Animator.TotalFrame / 30f)
                {
                    mCurrentTime = 0;
                    Animator.InitEvent();
                    if (LoopFunction != null)
                        LoopFunction();
                }
            }
            else
            {
                Animator.UpdateEvent( mCurrentTime, mTaskDic[TriggerIndex].AniEvent );
                if (mCurrentTime >= Animator.TotalFrame / 30f)
                {
                    mCurrentTime = 0;
                    Animator.InitEvent();
                    mCurrentTimes++;
                    if (mTaskDic[TriggerIndex].function != null)
                    {
                        mTaskDic[TriggerIndex].function();
                    }
                    if (mTaskDic[TriggerIndex].Times == mCurrentTimes)
                    {
                        TriggerIndex++;
                        mCurrentTimes = 0;
                        if (TriggerIndex == mTaskDic.Count)
                        {
                            Stop(false);
                        }
                    }
                }
            }

            mCurrentTime += deltaTime;
        }

        public List <UIAniProperty> properties = new List<UIAniProperty>();
        public void UpdataData ( float time ) 
        {
            for (int i = 0 ; i < properties.Count ; i++)
            {
                properties[i].Updata( time );
                Dictionary<string, UIAniProperty>.Enumerator it = properties[i].ChildPropertyDic.GetEnumerator();
                while(it.MoveNext())
                {
                    var item = it.Current.Value;
                    item.Updata(time);
                }
            }
        }
    }

    public class AnimatorTaskHelp 
    {
        public int Times = 0;

        public Action function;

        public Action<int> AniEvent;

        public AnimatorTaskHelp ( int times, Action _function, Action<int> _AniEvent ) 
        {
            function = _function;
            Times = times;
            AniEvent = _AniEvent;
        }
    }
}
