using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEditorInternal;

using Newtonsoft.Json;
using DG.Tweening;

using UEngine.Data;
using UEngine.UIExpand;
using UEngine.UIAnimation;

namespace UEngine.UI.UILuaBehaviour
{
    public enum PanelExpandMode 
    {
        None,
        BlackBack,
        Transparent
    }

    [Serializable]
    [ExecuteInEditMode]
    public class PanelManager : MonoBehaviour
    {
        public bool IsMainPanel = false;
        public int LayerID = -1;
        public int SortLayerID = -1;
        public Canvas mCanvas;

        public List<Canvas> SubCanvaes =  new List<Canvas>();
        public List<ParticleSystemRenderer> Particles = new List<ParticleSystemRenderer>();

        RectTransform _rectTransform;

        //动画信息
        [SerializeField]
        public string AnimationPath;
        public UIAnimationData mAnimationData;

        private Animator _animator;
        public Animator mAnimator 
        {
            get 
            {
                if (!_animator)
                {
                    _animator = GetComponent<Animator>();
                }
                return _animator;
            }
        }

        public RectTransform rectTransform 
        {
            get 
            {
                if (!_rectTransform)
                {
                     _rectTransform = transform as RectTransform;
                }
                return _rectTransform;
            }
        }

        public CanvasGroup _canvasGroup;
        public CanvasGroup canvasGroup 
        {
            get 
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                    if (_canvasGroup == null)
                    {
                        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    }
                }

                return _canvasGroup;
            }
        }

        public PanelManager ParentPanel;

        public bool IsClone = false;
        public bool IsClose = false;

        public PanelExpandMode panelMode = PanelExpandMode.None;
        public float bottomValue = 0f;

        public List<string> BehGuid_List = new List<string>();
        public List<ULuaBehaviourBase> Beh_List = new List<ULuaBehaviourBase>();
        public List<ULuaBehaviourBasePort> BehPort_List = new List<ULuaBehaviourBasePort>();
        public Dictionary<string, ULuaBehaviourBase> Beh_Dic = new Dictionary<string, ULuaBehaviourBase>();
        public Dictionary<string, ULuaBehaviourBasePort> BehPort_Dic = new Dictionary<string, ULuaBehaviourBasePort>();

        //所有物体的引用
        public List<GameObject> GameObjectList = new List<GameObject>();
        public List<UIGuidData> GuidData = new List<UIGuidData>();
        public List<ResourcesTask> CloneResources = new List<ResourcesTask>();

        [SerializeField]
        public SortedDictionary<string, GameObject> LuaGuidObj_Dic = new SortedDictionary<string, GameObject>();
        public Dictionary<string, GameObject> EditorGuidObj_Dic = new Dictionary<string, GameObject>();//每个对象对应的Guid
        public SortedDictionary<string, string> GuidLua_Dic = new SortedDictionary<string, string>();
        public List<string> guidlist = new List<string>();
        public List<string> luaguidlist = new List<string>();
        public List<GameObject> ObjList = new List<GameObject>();

        public List<Tween> tweenList = new List<Tween>();

        public List<Color> tipscolor = new List<Color>();

        public List<ResQuote> mResList = new List<ResQuote>();

        public List<SpriteQuote> mSpriteList = new List<SpriteQuote>();

        public Dictionary<string, Sprite> SpriteDic = new Dictionary<string, Sprite>();

        public Dictionary<string, GameObject> ObjectPool = new Dictionary<string, GameObject>();

        public Dictionary<int, Action> CloseFuncDic = new Dictionary<int, Action>();

        public List<PanelManager> ChildPanel_List = new List<PanelManager>();

        //动画事件
        public Dictionary<int, Action< int > > AnimatorFunction = new Dictionary<int, Action< int > >();
        int AnimatorFunctionIndex = 0;

        void Awake ( ) 
        {
            //gameObject.AddComponent<Canvas>();
            //gameObject.AddComponent<GraphicRaycaster>();
        }

        void OnEnable ( ) 
        {
            if ( Application.isPlaying && USystemConfig.Instance.IsDevelopMode )
            {
                if (mAnimationData == null)
                {
                    string json = UFileAccessor.ReadStringFile( AnimationPath );
                    if (!string.IsNullOrEmpty( json ))
                    {
                        JsonSerializerSettings settings = new JsonSerializerSettings();

                        settings.NullValueHandling = NullValueHandling.Ignore;
                        settings.Formatting = Formatting.Indented;

                        mAnimationData = JsonConvert.DeserializeObject<UIAnimationData>( json, settings );
                    }
                }
                if (mAnimationData != null)
                {
                    string AnimationName = "";
                    Dictionary<string, UIAnimator>.Enumerator it = mAnimationData.PanelAnimatorDic.GetEnumerator();
                    while(it.MoveNext())
                    {
                        var item = it.Current.Key;
                        AnimationName = item;
                        break;
                    }
                    PlayAnimation( AnimationName, 1, null, null, true );
                }
            }

            if ( IsMainPanel && mCanvas != null )
            {
                mCanvas.overrideSorting = true;
            }
            //gameObject.AddComponent<Canvas>();
        }

        public void Init ( ) 
        {

        }

        public Color TipColor(int index) 
        {
            if (tipscolor.Count - 1 > index)
            {
                return tipscolor[index];
            }
            else
            {
                for (int i = 0; i < index - tipscolor.Count + 1; i++)
                {
                    tipscolor.Add(Color.white);
                }
                return tipscolor[index];
            }
        }

        public string AddGuidObj(string mName, string tempName,GameObject mGameObject, bool isSelf = false) 
        {
            if (LuaGuidObj_Dic.Count == 0)
            {
                for (int i = 0; i < luaguidlist.Count; i++)
                {
                    LuaGuidObj_Dic.Add(luaguidlist[i], ObjList[i]);
                }
            }
            if (mGameObject.GetComponentInParent<PanelManager>() != this)
            {
                return "添加的对象不在Panel: " + transform.name + " 里";
            }
            if (!string.IsNullOrEmpty(mName))
            {
                if (!LuaGuidObj_Dic.ContainsKey(mName))
                {
                    UiWidgetGuid guid = mGameObject.GetComponent<UiWidgetGuid>();
                    if (!string.IsNullOrEmpty(tempName) && LuaGuidObj_Dic.ContainsKey(tempName))
                    {
                        if (guid)
                        {
                            guid.UIGuid = mName;
                        }
                        LuaGuidObj_Dic.Remove(tempName);
                    }
                    else
                    {
                        if (isSelf)
                        {
                            guid.UIGuid = mName;
                        }
                        else
                        {
                            if (guid)
                            {
                                return "添加的对象 " + mGameObject.name + " 上已有Guid组件";
                            }
                            else
                            {
                                guid = mGameObject.AddComponent<UiWidgetGuid>();
                                guid.UIGuid = mName;
                            }
                        }
                    }
                    LuaGuidObj_Dic.Add(mName, mGameObject);
                    luaguidlist.Clear();
                    ObjList.Clear();
                    SortedDictionary<string, GameObject>.Enumerator it = LuaGuidObj_Dic.GetEnumerator();
                    while(it.MoveNext())
                    {
                        var item = it.Current;
                        luaguidlist.Add(item.Key);
                        ObjList.Add(item.Value);
                    }
                    return "";
                }
                else
                {
                    return "设置的guid不能重复命名";
                }
            }
            else
            {
                return "设置的guid不能为空";
            }
        }

        public void RemoveGuidObj(string mGuid) 
        {
            if (LuaGuidObj_Dic.Count == 0)
            {
                for (int i = 0; i < luaguidlist.Count; i++)
                {
                    LuaGuidObj_Dic.Add(luaguidlist[i], ObjList[i]);
                }
            }
            if (LuaGuidObj_Dic.ContainsKey(mGuid))
            {
                LuaGuidObj_Dic.Remove(mGuid);
                luaguidlist.Clear();
                ObjList.Clear();
                SortedDictionary<string, GameObject>.Enumerator it = LuaGuidObj_Dic.GetEnumerator();
                while (it.MoveNext())
                {
                    var item = it.Current;
                    luaguidlist.Add(item.Key);
                    ObjList.Add(item.Value);
                }
            }
        }

        public static PanelManager ManagerInit(GameObject item, PanelManager parentPanel)
        {
            PanelManager manager = item.GetComponent<PanelManager>();
            for (int i = 0; i < manager.BehGuid_List.Count; i++)
            {
                ULuaBehaviourBasePort port = null;
                if (manager.Beh_List[i].GetType() == typeof(ULuaBehaviourBase))
                {
                    port = new ULuaBehaviourBasePort();
                }
                else if (manager.Beh_List[i] is ULuaButton)
                {
                    port = new ULuaButtonPort();
                }
                else if (manager.Beh_List[i] is ULuaDropDown)
                {
                    port = new ULuaDropDownPort();
                }
                else if (manager.Beh_List[i] is ULuaInputField)
                {
                    port = new ULuaInputFieldPort();
                }
                else if (manager.Beh_List[i] is ULuaJoystick)
                {
                    port = new ULuaJoystickPort();
                }
                else if (manager.Beh_List[i] is ULuaRichText)
                {
                    port = new ULuaRichTextPort();
                }
                else if (manager.Beh_List[i] is ULuaScrollbar)
                {
                    port = new ULuaScrollbarPort();
                }
                else if (manager.Beh_List[i] is ULuaScrollRect)
                {
                    port = new ULuaScrollRectPort();
                }
                else if (manager.Beh_List[i] is ULuaSlider)
                {
                    port = new ULuaSliderPort();
                }
                else if (manager.Beh_List[i] is ULuaToggle)
                {
                    port = new ULuaTogglePort();
                }
                else if (manager.Beh_List[i] is ULuaToggleGroup)
                {
                    port = new ULuaToggleGroupPort();
                }
                else if (manager.Beh_List[i].GetType() == typeof(ULuaUIBase))
                {
                    port = new ULuaUIBasePort();
                }
                else if (manager.Beh_List[i] is ULuaRawImage)
                {
                    port = new ULuaRawImagePort();
                }
                else if (manager.Beh_List[i] is ULuaMiniMap)
                {
                    port = new ULuaMiniMapPort();
                }
                else if (manager.Beh_List[i] is ULuaScrollList)
                {
                    port = new ULuaScrollListPort();
                }
                else if (manager.Beh_List[i] is ULuaRadar)
                {
                    port = new ULuaRadarPort();
                }

                port.behbase = manager.Beh_List[i];
                manager.BehPort_List.Add(port);
                if (!manager.BehPort_Dic.ContainsKey(manager.BehGuid_List[i]))
                {
                    manager.BehPort_Dic.Add(manager.BehGuid_List[i], port);
                }
            }
            manager.ParentPanel = parentPanel;
            manager.IsClone = true;
            manager.mResList.Clear();
            return manager;
        }
        
        private static string[] PaiXu(List<string> string_list)
        {
            string[] array = string_list.ToArray();
            Array.Sort(array, (string a, string b) =>
            {
                return string.Compare(a, b);
            });
            return array;
        }

        public void SetVisible ( bool value ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_SetVisible_对象被销毁仍调用接口" );
                return;
            }

            gameObject.SetActive( value );
        }

        public bool GetVisible ( ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_GetVisible_对象被销毁仍调用接口" );
                return false;
            }

            return gameObject.activeSelf;
        }

        public ULuaBehaviourBasePort GetBehbyGuid(string id)
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_GetBehbyGuid_对象被销毁仍调用接口" );
                return new ULuaBehaviourBasePort();
            }

            if (BehPort_Dic != null && !string.IsNullOrEmpty(id) && BehPort_Dic.ContainsKey(id))
            {
                return BehPort_Dic[id];
            }
            else
            {
                ULogger.Error("面板: " + gameObject.name + " 上不存在" + "Guid: " + id);
                return null;
            }
        }

        public void SynGetSprite ( string path, Action<Sprite> SpriteTask, bool IsAsyn ) 
        {
            if (string.IsNullOrEmpty( path ))
            {
                return;
            }
            path = UCoreUtil.AssetPathNormalize( path );
            
            Texture2D tex = null;
            string texPath = "";
            if (UIManager.texturedic.ContainsKey( path ))
            {
                texPath = UIManager.texturedic[path].path;
            }
            else
            {
                texPath = path;
            }

            SynGetRes( texPath, ( IRes ) =>
            {
                if (IRes != null)
                {
                    if (IRes.Res)
                    {
                        tex = IRes.Res as Texture2D;
                        var spriteQuato = UIManager.SynGetSprite( path, tex );
                        mSpriteList.Add( spriteQuato );
                        SpriteTask( spriteQuato.sprite );
                    }
                }
            }, IsAsyn );
        }

        public void SynGetRes(string path, Action<IResource> ResTask, bool IsAsyn)
        {
            path = path.ToLower();
            //StatisticsResource.AddUIResource( path );
            
            if (path.StartsWith( "assets/resources/" ))
            {
                path = path.Replace( "assets/resources/", "" );
            }
            UIManager.SynGetRes( path, ( quato ) => 
            {
                ResQuote resQuato;
                resQuato = quato;
                mResList.Add( resQuato );
                ResTask( resQuato.res );
            } , IsAsyn);
        }

        public void ReleasRes() 
        {
            for (int i = 0; i < mResList.Count; i++)
            {
                UIManager.ReleaseRes(mResList[i]);
            }
            mResList.Clear();

            for (int i = 0 ; i < mSpriteList.Count; i++)
            {
                UIManager.ReleaseSprite( mSpriteList[i] );
            }
            mSpriteList.Clear();
        }

        public void ClosePanel() 
        {
            AnimatorCallBack = null;

            for (int i = 0 ; i < BehPort_List.Count ; i++)
            {
                BehPort_List[i].OnHide();
            }

            for (int i = 0; i < BehPort_List.Count; i++)
            {
                BehPort_List[i].OnClose();
                BehPort_List[i].IsClose = true;
            }

            for (int i = ChildPanel_List.Count  - 1; i >= 0 ; i--)
            {
                if (ChildPanel_List[i] != null)
                {
                    ChildPanel_List[i].ClosePanel();
                }
            }

            Dictionary<string, GameObject>.Enumerator it = ObjectPool.GetEnumerator();
            while(it.MoveNext())
            {
                var item = it.Current.Value;
                if(item)
                    item.GetComponent<PanelManager>().ClosePanel();
            }

            ReleasRes();

            for (int i = 0; i < tweenList.Count; ++ i)
            {
				var item = tweenList[i];

                if (!item.IsComplete())
                    item.OnComplete(null);
            }

            OnCloseEvent();

            if (ParentPanel && ParentPanel.ChildPanel_List.Contains( this ))
                ParentPanel.ChildPanel_List.Remove(this);

            if (mAnimationData != null && mAnimationData.PanelAnimatorDic != null)
            {
                Dictionary<string, UIAnimator>.Enumerator it2 = mAnimationData.PanelAnimatorDic.GetEnumerator();
                while (it2.MoveNext())
                {
                    var item = it2.Current.Value;
                    UIAnimationManage.RemoveAnimator(item);
                }
            }
            IsClose = true;

            if (IsMainPanel)
            {
                UIManager.instance.RemoveCanvasSortLayerID( LayerID, SortLayerID );
                UIManager.instance.SetActiveLayer( LayerID, false );
            }

            DestroyImmediate(gameObject);
        }

        public bool IsSort = false;

        private int SortInterval = 0;
        public void SetSortfrequency(int interval) 
        {
            SortInterval = interval;
        }

        List<Transform> SortTransList = new List<Transform>();

        List<Transform> TempTransList = new List<Transform>();

        public void ChildPanelSort()
        {
            SortTransList.Sort((a, b) => { return (int)(b.position.z - a.position.z) * 1000; });
            for (int i = 0; i < SortTransList.Count; i++)
            {
                SortTransList[i].SetSiblingIndex(i);
            }
        }

        int currtTime = 0;
        void SortCheck() 
        {
            if (IsSort)
            {
                if (currtTime == SortInterval)
                {
                    ChildPanelSort();
                    currtTime = 0;
                }
                else
                {
                    currtTime++;
                }
            }
        }

        public void PlayAni(UIAnimationBase anim, Action luafunc = null)
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_PlayAni_对象被销毁仍调用接口" );
                return;
            }

            Tween t = TweenFactoryTemp.GetTween(anim, this);
            t.OnComplete(() =>
            {
                if (luafunc != null)
                    luafunc();
            });
            t.Play();
        }

        public void PlayAni(UIAnimationBase anim, Action luafunc, bool isRepeat)
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_PlayAni_对象被销毁仍调用接口" );
                return;
            }

            bool isover = false;
            Tween t = TweenFactoryTemp.GetTween(anim, this);
            t.OnComplete(() =>
            {
                if (luafunc != null)
                    luafunc();
                isover = true;
            });
            t.PlayForward();
            if (isRepeat && isover)
                t.PlayBackwards();
        }

        public int AddAnimationEvent(Action< int > function) 
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_AddAnimationEvent_对象被销毁仍调用接口" );
                return 0;
            }

            AnimatorFunction.Add(AnimatorFunctionIndex, function);
            return AnimatorFunctionIndex++;
        }

        public void RemoveAnimationEvent ( int index )
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_RemoveAnimationEvent_对象被销毁仍调用接口" );
                return;
            }

            if (AnimatorFunction.ContainsKey(index))
            {
                AnimatorFunction.Remove(index);
            }
        }

        public void RemoveAllAnimationEvent ( ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_RemoveAllAnimationEvent_对象被销毁仍调用接口" );
                return;
            }

            AnimatorFunction.Clear();
        }

        public void TriggerEvent ( int id ) 
        {
            Dictionary <int, Action< int > >.Enumerator it = AnimatorFunction.GetEnumerator();
            while(it.MoveNext())
            {
                var item = it.Current.Value;
                item( id );
            }
        }

        public void PlayAnimation ( string AniName, int Times = 1 , Action function = null, Action<int> AniEvents = null, bool isResume = false ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_PlayAnimation_对象被销毁仍调用接口" );
                return;
            }

            if (mAnimationData == null)
            {
                string json = UFileAccessor.ReadStringFile( AnimationPath );
                if (!string.IsNullOrEmpty( json ))
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();

                    settings.NullValueHandling = NullValueHandling.Ignore;
                    settings.Formatting = Formatting.Indented;

                    mAnimationData = JsonConvert.DeserializeObject<UIAnimationData>( json, settings );
                }
            }

            if (mAnimationData != null && mAnimationData.PanelAnimatorDic != null && mAnimationData.PanelAnimatorDic.ContainsKey( AniName ))
            {

                UIAnimator Animtor = mAnimationData.PanelAnimatorDic[AniName];
                if (isResume)
                {
                    UIAnimationManage.RemoveAnimator( Animtor );
                }
                AnimatorTask task = UIAnimationManage.ContainsAnimator( Animtor );
                if (task != null)
                {
                    if (!task.IsLoop)
                    {
                        if (Times == -1)
                        {
                            task.IsLoop = true;
                            task.LoopFunction = function;
                            task.LoopAniEnvents = AniEvents;
                        }
                        else
                        {
                            task.AddTask( Animtor.Times * Times, function, AniEvents );
                        }
                    }
                }
                else
                {
                    task = new AnimatorTask();
                    task.Animator = Animtor;
                    task.manager = this;
                    task.Init();
                    if (Animtor.IsLoop)
                    {
                        task.IsLoop = true;
                        task.LoopFunction = function;
                        task.LoopAniEnvents = AniEvents;
                    }
                    else
                    {
                        if (Times == -1)
                        {
                            task.IsLoop = true;
                        }
                        else
                        {
                            task.AddTask( Animtor.Times * Times, function, AniEvents );
                        }
                    }
                    UIAnimationManage.AddUIAnimtor(task);
                }
            }
        }

        public void StopAnimation ( string AniName, bool IsEnd ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_StopAnimation_对象被销毁仍调用接口" );
                return;
            }

            if (mAnimationData != null && mAnimationData.PanelAnimatorDic != null && mAnimationData.PanelAnimatorDic.ContainsKey( AniName ))
            {
                UIAnimator Animtor = mAnimationData.PanelAnimatorDic[AniName];
                AnimatorTask task = UIAnimationManage.ContainsAnimator( Animtor );
                if (task != null)
                {
                    task.Stop(IsEnd);
                }
            }
        }

        public void PlayAnimator ( string AniName, Action callBack = null, float time = -1f ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_PlayAnimator_对象被销毁仍调用接口" );
                return;
            }

            if (mAnimator)
            {
                TimeAdd = 0f;
                if (mAnimatorDic.Count == 0)
                {
                    AnimatorInit();
                }

                int NameHash = Animator.StringToHash( AniName );
                
                if (mAnimatorDic.ContainsKey(AniName))
                {
                    AnimatorTime = mAnimatorDic[AniName].length;
                }
                if (time >= 0)
                {
                    mAnimator.Play( NameHash, 0, time );
                    AnimatorTime = 0;
                }
                else
                {
                    mAnimator.Play( NameHash, 0, 0 );
                }
                AnimatorCallBack = callBack;
            }
        }

        Action AnimatorCallBack;
        float AnimatorTime = 0f;
        float TimeAdd = 0f;
        void Update ( ) 
        {
            if (AnimatorCallBack != null)
            {
                if (TimeAdd >= AnimatorTime)
                {
                    if (AnimatorCallBack != null)
                    {
                        AnimatorCallBack();
                        AnimatorCallBack = null;
                    }
                    TimeAdd = 0f;
                }
                else
                {
                    TimeAdd += Time.deltaTime;
                }
            }
        }

        private Dictionary<string, AnimationClip> mAnimatorDic = new Dictionary<string, AnimationClip>();
        private void AnimatorInit ( ) 
        {
            var controller = mAnimator.runtimeAnimatorController;
            if (controller != null)
            {
                AnimationClip[] clips = controller.animationClips;

                for (int i = 0 ; i < clips.Length ; i++)
                {
                    if (clips[i] && !mAnimatorDic.ContainsKey(clips[i].name))
                    {
                        mAnimatorDic.Add( clips[i].name, clips[i] );
                    }
                }
            }
        }

        private Action<int> animatorEvent;
        public void AnimatorEvent ( int EventID ) 
        {
            if (animatorEvent != null)
            {
                animatorEvent( EventID );
            }
        }

        public void AddAnimatorEvent ( Action<int> aniEvent ) 
        {
            animatorEvent = aniEvent;
        }

        private ULuaBehaviourBase myself;
        private ULuaBehaviourBasePort myselfport;

        public ULuaBehaviourBasePort GetSelfBeh()
        {
            if (myselfport == null)
            {
                myself = transform.GetComponent<ULuaBehaviourBase>();
                if (!myself)
                {
                    UIType uitype = GetUIType();
                    switch (uitype)
                    {
                        case UIType.BaseTransform:
                            myself = gameObject.AddComponent<ULuaBehaviourBase>();
                            myselfport = new ULuaBehaviourBasePort();
                            break;
                        case UIType.Image:
                            myself = gameObject.AddComponent<ULuaUIBase>();
                            myselfport = new ULuaUIBasePort();
                            break;
                        case UIType.Text:
                            myself = gameObject.AddComponent<ULuaUIBase>();
                            myselfport = new ULuaUIBasePort();
                            break;
                        case UIType.Button:
                            myself = gameObject.AddComponent<ULuaButton>();
                            myselfport = new ULuaButtonPort();
                            break;
                        case UIType.Toggle:
                            myself = gameObject.AddComponent<ULuaToggle>();
                            myselfport = new ULuaTogglePort();
                            break;
                        case UIType.Dropdown:
                            myself = gameObject.AddComponent<ULuaDropDown>();
                            myselfport = new ULuaDropDownPort();
                            break;
                        case UIType.ScrollRect:
                            myself = gameObject.AddComponent<ULuaScrollRect>();
                            myselfport = new ULuaScrollRectPort();
                            break;
                        case UIType.InputField:
                            myself = gameObject.AddComponent<ULuaInputField>();
                            myselfport = new ULuaInputFieldPort();
                            break;
                        case UIType.Slider:
                            myself = gameObject.AddComponent<ULuaSlider>();
                            myselfport = new ULuaSliderPort();
                            break;
                        case UIType.Scrollbar:
                            myself = gameObject.AddComponent<ULuaScrollbar>();
                            myselfport = new ULuaScrollbarPort();
                            break;
                        case UIType.InlineText:
                            myself = gameObject.AddComponent<ULuaRichText>();
                            myselfport = new ULuaRichTextPort();
                            break;
                        case UIType.ToggleGroup:
                            myself = gameObject.AddComponent<ULuaToggleGroup>();
                            myselfport = new ULuaToggleGroupPort();
                            break;
                        case UIType.JoyStick:
                            myself = gameObject.AddComponent<ULuaJoystick>();
                            myselfport = new ULuaJoystickPort();
                            break;
                        case UIType.RawImage:
                            myself = gameObject.AddComponent<ULuaRawImage>();
                            myselfport = new ULuaRawImagePort();
                            break;
                        case UIType.MiniMap:
                            myself = gameObject.AddComponent<ULuaMiniMap>();
                            myselfport = new ULuaMiniMapPort();
                            break;
                        case UIType.ScrollList:
                            myself = gameObject.AddComponent<ULuaScrollList>();
                            myselfport = new ULuaScrollListPort();
                            break;
                        case UIType.Radar:
                            myself = gameObject.AddComponent<ULuaRadar>();
                            myselfport = new ULuaRadarPort();
                            break;
                        default:
                            myself = gameObject.AddComponent<ULuaBehaviourBase>();
                            myselfport = new ULuaBehaviourBasePort();
                            break;
                    }
                    myself.panelmanager = this;
                    myselfport.behbase = myself;
                }
                else
                {
                    if (myself.GetType() == typeof(ULuaBehaviourBase))
                    {
                        myselfport = new ULuaBehaviourBasePort();
                    }
                    else if (myself is ULuaButton)
                    {
                        myselfport = new ULuaButtonPort();
                    }
                    else if (myself is ULuaDropDown)
                    {
                        myselfport = new ULuaDropDownPort();
                    }
                    else if (myself is ULuaInputField)
                    {
                        myselfport = new ULuaInputFieldPort();
                    }
                    else if (myself is ULuaJoystick)
                    {
                        myselfport = new ULuaJoystickPort();
                    }
                    else if (myself is ULuaRichText)
                    {
                        myselfport = new ULuaRichTextPort();
                    }
                    else if (myself is ULuaScrollbar)
                    {
                        myselfport = new ULuaScrollbarPort();
                    }
                    else if (myself is ULuaScrollRect)
                    {
                        myselfport = new ULuaScrollRectPort();
                    }
                    else if (myself is ULuaSlider)
                    {
                        myselfport = new ULuaSliderPort();
                    }
                    else if (myself is ULuaToggle)
                    {
                        myselfport = new ULuaTogglePort();
                    }
                    else if (myself is ULuaToggleGroup)
                    {
                        myselfport = new ULuaToggleGroupPort();
                    }
                    else if (myself.GetType() == typeof(ULuaUIBase))
                    {
                        myselfport = new ULuaUIBasePort();
                    }
                    else if (myself is ULuaMiniMap)
                    {
                        myselfport = new ULuaMiniMapPort();
                    }
                    else if (myself is ULuaRadar)
                    {
                        myselfport = new ULuaRadarPort();
                    }
                    myselfport.behbase = myself;
                }
            }
            return myselfport;
        }

        public UIType GetUIType()
        {
            UIType _uitype = UIType.BaseTransform;
            Component[] components = transform.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++ i)
            {
				var com = components[i];

                if (com is Image || com is RayCastMask)
                    _uitype = UIType.Image;
                else if (com.GetType() == typeof( Text ) || ( com.GetType() == typeof( CText ) && ( ( CText )com ).RichText ))
                    _uitype = UIType.Text;
                else if (( com.GetType() == typeof( CText ) && !( ( CText )com ).RichText ))
                {
                    _uitype = UIType.InlineText;
                    break;
                }
                else if (com is Button)
                {
                    _uitype = UIType.Button;
                }
                else if (com is Toggle)
                {
                    _uitype = UIType.Toggle;
                    break;
                }
                else if (com is Dropdown || com is TDropdown)
                {
                    _uitype = UIType.Dropdown;
                    break;
                }
                else if (com is ScrollRect)
                {
                    _uitype = UIType.ScrollRect;
                    break;
                }
                else if (com is CInputField)
                {
                    _uitype = UIType.InputField;
                    break;
                }
                else if (com is Slider)
                {
                    _uitype = UIType.Slider;
                    break;
                }
                else if (com is Scrollbar)
                {
                    _uitype = UIType.Scrollbar;
                    break;
                }
                else if (com is UJoystick)
                {
                    _uitype = UIType.JoyStick;
                    break;
                }
                else if (com is ULuaRawImage)
                {
                    _uitype = UIType.RawImage;
                    break;
                }
                else if (com is MiniMap)
                {
                    _uitype = UIType.MiniMap;
                    break;
                }
                else if (com is UScrollList)
                {
                    _uitype = UIType.ScrollList;
                    break;
                }
                else if (com is ULuaRadar)
                {
                    _uitype = UIType.Radar;
                    break;
                }
            }
            return _uitype;
        }

        public void OnCloseEvent() 
        {
            Dictionary<int, Action>.Enumerator it = CloseFuncDic.GetEnumerator();
            while(it.MoveNext())
            {
                var func = it.Current.Value;
                func();
            }
        }

        int EventIndex = 0;
        public int AddCloseEvent(Action func)
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_AddCloseEvent_对象被销毁仍调用接口" );
                return 0;
            }

            CloseFuncDic.Add(EventIndex, func);
            return EventIndex++;
        }

        public void RemoveCloseEvent(int index) 
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_RemoveCloseEvent_对象被销毁仍调用接口" );
                return;
            }

            CloseFuncDic.Remove(index);
        }

        public void RemoveAllCloseEvent() 
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_RemoveAllCloseEvent_对象被销毁仍调用接口" );
                return;
            }

            CloseFuncDic.Clear();
        }

        Graphic[] graphics;
        public void PlayAlphaAnimation ( float Duration, uint delayTime ,float TargetValue = 0f ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_PlayAlphaAnimation_对象被销毁仍调用接口" );
                return;
            }

            graphics = null;
            UTimer.AddTimer( delayTime, 0, ( ) => 
            {
                if (this == null)
                {
                    return;
                }
                graphics = GetComponentsInChildren<Graphic>();
                if (graphics != null && graphics.Length > 0)
                {
                    for (int i = 0 ; i < graphics.Length ; i++)
                    {
                        graphics[i].CrossFadeAlpha( TargetValue, Duration, true );
                    }
                }
            } );
        }

        public void ChangeGraphicsAlpha ( float TargetValue ) 
        {
            if (IsClose)
            {
                ULogger.Warn( "PanelManager_ChangeGraphicsAlpha_对象被销毁仍调用接口" );
                return;
            }

            graphics = null;
            graphics = GetComponentsInChildren<Graphic>();
            if (graphics != null && graphics.Length > 0)
            {
                for (int i = 0 ; i < graphics.Length ; i++)
                {
                    graphics[i].color = new Color( graphics[i].color.r, graphics[i].color.g, graphics[i].color.b, TargetValue );
                }
            }
        }

        public void PanelManagerInit ( ) 
        {


            for (int i = 0 ; i < GuidData.Count ; i++)
            {
                UIGuidData guidData = GuidData[i];
                if (guidData.GameObjectIndex >= GameObjectList.Count)
                {
                    Debug.Log( gameObject.name );
                    continue;
                }
                GameObject go = GameObjectList[guidData.GameObjectIndex];

                if (Application.isEditor)
                {
                    luaguidlist.Add( guidData.UIGuid );
                    ObjList.Add( go );
                }

                LuaBeahavirInit( guidData, go );
            }
            LoadResources();
        }

        public void LoadResources ( ) 
        {
            for (int i = 0 ; i < CloneResources.Count ; i++)
            {
                ResourcesTask task = CloneResources[i];
                Type script = task.obj.GetType();
                MemberInfo memberInfo;
                PropertyInfo propertyInfo = script.GetProperty( task.memberName );
                Type type;

                if (propertyInfo != null)
                {
                    type = propertyInfo.PropertyType;
                    memberInfo = propertyInfo;
                }
                else
                {
                    FieldInfo fieldInfo = script.GetField( task.memberName );
                    memberInfo = fieldInfo;
                    type = fieldInfo.FieldType;
                }
                bool IsUIBase = false;
                if (task.obj is Image)
                {
                    Image image = ( Image )task.obj;
                    ULuaUIBase uiBase = image.GetComponent<ULuaUIBase>();
                    if (uiBase && ( task.ResPath.EndsWith( "png" ) || task.ResPath.EndsWith( "jpg" ) ) && task.memberName != "spriteState")
                    {
                        uiBase.SetImage( task.ResPath, USystemConfig.Instance.UILoadResIsAsyn );
                        IsUIBase = true;
                    }
                }

                if (IsUIBase)
                {
                    continue;
                }

                GetAssetFromPath( task.ResPath, type, ( value ) => 
                {
                    if (this == null)
                    {
                        return;
                    }
                    if (value is Sprite && task.obj is Image)
                    {
                        Sprite s = ( Sprite )value;
                        Image image = ( Image )task.obj;

                        if (s.border == Vector4.zero && image.type != Image.Type.Filled)
                        {
                            image.type = Image.Type.Simple;
                        }
                        else if (s.border != Vector4.zero && image.type != Image.Type.Filled)
                        {
                            image.type = Image.Type.Sliced;
                            image.fillCenter = true;
                        }
                    }

                    if (memberInfo != null && task.obj != null && value != null)
                    {
                        if (memberInfo is FieldInfo)
                            ( ( FieldInfo )memberInfo ).SetValue( task.obj, value );
                        else 
                        {
                            if (type == typeof( Mesh ) && task.obj is ParticleSystemRenderer)
                            {
                                var meshGame = ( GameObject )value;
                                value = meshGame.GetComponent<MeshFilter>().sharedMesh;
                            }
                            ( ( PropertyInfo )memberInfo ).SetValue( task.obj, value, null );
                        }
                    }
                } );
            }
        }

        private void GetAssetFromPath(string path, Type type, Action<UnityEngine.Object> GetAssetTask)
        {
            if (string.IsNullOrEmpty(path) || path == "none")
                return;

            if (type == typeof(Sprite))
            {
                SynGetSprite( path, ( sprite ) => 
                {
                    GetAssetTask(sprite);
                }, USystemConfig.Instance.UILoadResIsAsyn );
            } else
            {
                bool NotAnim = true;
                if (path.EndsWith( "controller" ))
                {
                    NotAnim = false;
                }
                SynGetRes( path, ( IRes ) => 
                {
                    if (IRes != null)
                    {
                        GetAssetTask( IRes.Res );
                    }
                }, USystemConfig.Instance.UILoadResIsAsyn & NotAnim );
            }
        }

        private void LuaBeahavirInit ( UIGuidData guid, GameObject obj )
        {
            if (Beh_List.Count != BehGuid_List.Count)
            {
                return;
            }

            int index = BehGuid_List.IndexOf( guid.UIGuid );

            ULuaBehaviourBase behbase = Beh_List[index];
            ULuaBehaviourBasePort port = null;
            switch (( UIType )guid.UIType)
            {
                case UIType.BaseTransform:
                    //behbase = obj.AddComponent<ULuaBehaviourBase>();
                    port = new ULuaBehaviourBasePort();
                    break;
                case UIType.Image:
                    //behbase = obj.AddComponent<ULuaUIBase>();
                    port = new ULuaUIBasePort();
                    break;
                case UIType.Text:
                    //behbase = obj.AddComponent<ULuaUIBase>();
                    port = new ULuaUIBasePort();
                    break;
                case UIType.Button:
                    //behbase = obj.AddComponent<ULuaButton>();
                    port = new ULuaButtonPort();
                    break;
                case UIType.Toggle:
                    //behbase = obj.AddComponent<ULuaToggle>();
                    port = new ULuaTogglePort();
                    break;
                case UIType.Dropdown:
                    //behbase = obj.AddComponent<ULuaDropDown>();
                    port = new ULuaDropDownPort();
                    break;
                case UIType.ScrollRect:
                    //behbase = obj.AddComponent<ULuaScrollRect>();
                    port = new ULuaScrollRectPort();
                    break;
                case UIType.InputField:
                    //behbase = obj.AddComponent<ULuaInputField>();
                    port = new ULuaInputFieldPort();
                    break;
                case UIType.Slider:
                    //behbase = obj.AddComponent<ULuaSlider>();
                    port = new ULuaSliderPort();
                    break;
                case UIType.Scrollbar:
                    //behbase = obj.AddComponent<ULuaScrollbar>();
                    port = new ULuaScrollbarPort();
                    break;
                case UIType.InlineText:
                    //behbase = obj.AddComponent<ULuaRichText>();
                    port = new ULuaRichTextPort();
                    break;
                case UIType.ToggleGroup:
                    //behbase = obj.AddComponent<ULuaToggleGroup>();
                    port = new ULuaToggleGroupPort();
                    break;
                case UIType.JoyStick:
                    //behbase = obj.AddComponent<ULuaJoystick>();
                    port = new ULuaJoystickPort();
                    break;
                case UIType.RawImage:
                    //behbase = obj.AddComponent<ULuaRawImage>();
                    port = new ULuaRawImagePort();
                    break;
                case UIType.MiniMap:
                    //behbase = obj.AddComponent<ULuaMiniMap>();
                    port = new ULuaMiniMapPort();
                    break;
                case UIType.ScrollList:
                    //behbase = obj.AddComponent<ULuaScrollList>();
                    port = new ULuaScrollListPort();
                    break;
                case UIType.Radar:
                    //behbase = obj.AddComponent<ULuaRadar>();
                    port = new ULuaRadarPort();
                    break;
            }
            //behbase.panelmanager = this;
            port.behbase = behbase;
            BehPort_List.Add( port );
            BehPort_Dic.Add( guid.UIGuid, port );
        }

        public void ForceUpdatePanel ( )
        {
            if (mCanvas != null)
            {

            }
        }

        public void SetSubCanvas_ParticleSortLayerID ( int layerID ) 
        {
            SortLayerID = layerID;
            for (int i = 0 ; i < SubCanvaes.Count ; i++)
            {
                SubCanvaes[i].overrideSorting = true;
                SubCanvaes[i].sortingLayerName = layerID.ToString();
            }

            for (int i = 0 ; i < Particles.Count ; i++)
            {
                Particles[i].sortingLayerName = layerID.ToString();
            }

            for (int i = 0 ; i < ChildPanel_List.Count ; i++)
            {
                ChildPanel_List[i].SetSubCanvas_ParticleSortLayerID( layerID );
            }
        }
    }
}
