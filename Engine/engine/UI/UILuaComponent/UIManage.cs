using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UEngine;
using UEngine.Data;
using UEngine.UIExpand;
using UEngine.FreeType;
using UEngine.UI.ricktext;
using UEngine.UIAnimation;

using Newtonsoft.Json;

namespace UEngine.UI.UILuaBehaviour
{
    [ExecuteInEditMode]
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;

        public static bool IsPanelExpand = true;

        public static int OffsetSize = 0;

        public static float ExpandChangeValue = 60f;

        public static string fontPath = "Font/jht.ttf";

        public static GameObject UIRoot;
        
        public static RectTransform UIRootRectTransform;

        public static Canvas UIRootCanvas;
        
        public static GameObject UICameraObj;

        public static GameObject UICachePool;
        public static GameObject UIMask;
        
        public static Color MaskColor = Color.black;
        public static Color ToggleChangeColor = new Color( 1, 1, 1, 0.99f );

        public static GameObject WorldUICameraObj;
        public static GameObject WorldUICanvasObj;

        public static GameObject UIPreviewLight;
		private static Light mUIPreviewLight;

        public static Camera UICamera;
        public static Camera WorldUICamera;

        public static Canvas WorldUICanvas;

        public static Vector3 CenterPoint;

        public static float UIRootWidth;

        public static float UIRootHeight;

        public static float UIRootRate;

        public static Dictionary<string, SpriteData> spritedic;

        public static Dictionary<string, BorderData> texturedic = new Dictionary<string, BorderData>();

        public static List<RenderTask> mRenderTasks = new List<RenderTask>();

        public static List<ResQuote> mResList = new List<ResQuote>();
        
        public static IResource UiGrayMaterial;

        public static Font emtyFont;

        static Dictionary<string, ResQuote> mResMap = new Dictionary<string, ResQuote>();

        static Dictionary<string, SpriteQuote> mSpriteMap = new Dictionary<string, SpriteQuote>();

        public static Camera mCameraPreview;

        float distance = 30;

        //public static Dictionary<string, GameObject> UICacheObj = new Dictionary<string, GameObject>();

        public static Dictionary<string, Cache> UIMaxObj = new Dictionary<string, Cache>();

        public static Dictionary<string, Cache> UIMinObj = new Dictionary<string, Cache>();

        public static List<GameObject> LayerVesselList = new List<GameObject>();

        public static List<GameObject> LayerObjList = new List<GameObject>();

        static List<GameObject> CameraList = new List<GameObject>();

        public static SortedDictionary<string, Color> mColorLibray = new SortedDictionary<string, Color>();

        public static SortedDictionary<string, int> mFontSizeLibray = new SortedDictionary<string, int>();

        public const bool IsAsyn = false;

        public List<List<Canvas>> canvasList = new List<List<Canvas>>();

        void Awake()
        {
            if (USystemConfig.Instance.IsDevelopMode)
            {
                Dictionary<string, CFont>.Enumerator it1 = FontFactory.fontDic.GetEnumerator();
                while(it1.MoveNext())
                {
                    var item = it1.Current.Value;
                    for (int i = 0 ; i < item.atlasList.Count ; i++)
                    {
                        item.atlasList[i].ClearAll();
                    }
                    item.atlasList.Clear();
                    item.fontDataList.Clear();
                }
                FontFactory.fontDic.Clear();
            }
        }

        void OnEnable ( )
        {
            Init();

            if (!Application.isPlaying && USystemConfig.Instance.IsDevelopMode)
                ReadUIConfig();
        }

        void Start() 
        {
            
        }

        //初始化
        public void Init()
        {
            instance = this;
            emtyFont = new Font();

            if (Application.isPlaying && !USystemConfig.Instance.IsDevelopMode)
            {
                UICachePool = new GameObject( "UICachePool" );
                UICachePool.layer = LayerMask.NameToLayer( "UI" );
                UICachePool.transform.SetParent( gameObject.transform );
                UICachePool.transform.SetSiblingIndex( 0 );
                UICachePool.transform.localPosition = Vector3.zero;
                UICachePool.transform.localScale = Vector3.one;
                RectTransform rect = UICachePool.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;

                UIMask = new GameObject( "UIMask" );
                UIMask.layer = LayerMask.NameToLayer( "UI" );
                UIMask.SetActive( false );
                UIMask.transform.SetParent( UICachePool.transform );
                UIMask.transform.localPosition = Vector3.zero;
                UIMask.transform.localScale = Vector3.one;
                RectTransform _rect = UIMask.AddComponent<RectTransform>();
                _rect.anchorMin = Vector2.zero;
                _rect.anchorMax = Vector2.one;
                _rect.sizeDelta = Vector2.zero;
                Image image = UIMask.AddComponent<Image>();
                image.color = MaskColor;
            }
            
            UICameraObj = GameObject.FindWithTag( "UICamera" );
            UIPreviewLight = GameObject.Find( "PreviewLight" );
			if (null != UIPreviewLight)
				mUIPreviewLight = UIPreviewLight.GetComponent< Light >();

            UIRoot = gameObject;
            

            if (Application.isPlaying && !USystemConfig.Instance.IsDevelopMode)
            {
                GameObject DefaultLayerObj = transform.GetChild( 1 ).gameObject;
                GameObject DefaultVessel = new GameObject( "UIVessel0" );//DefaultLayerObj.transform.GetChild( 0 ).gameObject;
                DefaultVessel.transform.SetParent( DefaultLayerObj.transform );
                DefaultVessel.transform.localPosition = Vector3.zero;
                DefaultVessel.transform.localScale = Vector3.one;

                RectTransform v_rect = DefaultVessel.AddComponent<RectTransform>();
                
                v_rect.anchorMin = Vector2.zero;
                v_rect.anchorMax = Vector2.one;
                v_rect.sizeDelta = Vector2.zero;

                LayerObjList.Add( DefaultLayerObj );
                LayerVesselList.Add( DefaultVessel );
                canvasList.Add( new List<Canvas>());
                CameraList.Add( UICameraObj.transform.GetChild( 0 ).gameObject );
                UIRootRectTransform = DefaultLayerObj.GetComponent<RectTransform>();
                UIRootCanvas = DefaultLayerObj.GetComponent<Canvas>();
                UICamera = CameraList[0].GetComponent<Camera>();
                //UICamera.clearFlags = CameraClearFlags.Nothing;
            }
            else
            {
                UIRootRectTransform = GetComponent<RectTransform>();
            }

            Vector3[] corners = new Vector3[4];
            UIRootRectTransform.GetWorldCorners(corners);
            CenterPoint = corners[0];
            UIRootWidth = corners[2].x - corners[1].x;
            UIRootHeight = corners[1].y - corners[0].y;
            UIRootRate = UIRootWidth / UIRootRectTransform.sizeDelta.x;

            if (!mCameraPreview)
            {
                GameObject cameraObj = GameObject.Find( "PreviewCamera" );

                if (cameraObj)
                {
                    mCameraPreview = cameraObj.GetComponent<Camera>();

                    mCameraPreview.orthographicSize = 3;

                    cameraObj.transform.position = new Vector3(0, 0, distance);

                    cameraObj.transform.localEulerAngles = new Vector3(0, -180, 0);

                    SetCameraRotation(45, 0);
                }
            }
        }

        public static void CreateWorldUI ( ) 
        {
            WorldUICameraObj = new GameObject( "WorldUICamera" );
            WorldUICameraObj.transform.SetParent( Camera.main.transform );
            WorldUICameraObj.transform.localEulerAngles = Vector3.zero;
            WorldUICameraObj.transform.localPosition = Vector3.zero;

            WorldUICamera = WorldUICameraObj.AddComponent<Camera>();
            WorldUICamera.depth = -1;
            WorldUICamera.clearFlags = CameraClearFlags.Depth;
            WorldUICamera.orthographic = false;
            WorldUICamera.cullingMask = -1;
            WorldUICamera.cullingMask &= ~( 1 << LayerMask.NameToLayer( "UI" ));
            WorldUICamera.useOcclusionCulling = false;
            WorldUICamera.allowHDR = false;
            WorldUICamera.allowMSAA = false;


            WorldUICanvasObj = new GameObject( "WorldUICanvas" );
            WorldUICanvasObj.transform.SetParent( UIRoot.transform.parent );
            WorldUICanvasObj.transform.localEulerAngles = Vector3.zero;
            WorldUICanvasObj.transform.localPosition = Vector3.zero;

            WorldUICanvas = WorldUICanvasObj.AddComponent< Canvas >();
            WorldUICanvasObj.AddComponent<CanvasScaler>();
            WorldUICanvasObj.AddComponent<GraphicRaycaster>();
            WorldUICanvas.renderMode = RenderMode.WorldSpace;
            WorldUICanvas.worldCamera = WorldUICamera;
            WorldUICanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            WorldUICanvasObj.transform.localScale = UIRootRectTransform.localScale;
        }

        public static void ForceUpdateCanvases ( ) 
        {
            Canvas.ForceUpdateCanvases();
        }

        public static Vector2 GetUIRootSize()
        {
            return UIRootRectTransform.sizeDelta;
        }

        public static void ReadUIConfig ( ) 
        {
            ReadTextureConfig();

            string SpriteConfig = UFileAccessor.ReadStringFile( "data/spriteconfig.txt" );
            if (!string.IsNullOrEmpty( SpriteConfig ))
            {
                object dic = JsonConvert.DeserializeObject<Dictionary<string, SpriteData>>( SpriteConfig );
                if (dic != null)
                {
                    spritedic = dic as Dictionary<string, SpriteData>;
                    if (spritedic.Count == 0)
                    {
                        ULogger.Warn( "SpriteConfig为0" );
                    }
                    else
                    {
                        //ULogger.Info( "SpriteConfig为" + spritedic.Count );
                    }
                }
                else
                {
                    spritedic = new Dictionary<string, SpriteData>();
                }
            }
            ColorConfigInit();
        }

        public static void ReadTextureConfig ( ) 
        {
            texturedic = new Dictionary<string, BorderData>();
            string Config = UFileAccessor.ReadStringFile( "data/textureconfig.txt" );
            
            if (string.IsNullOrEmpty(Config))
            {
                return;
            }

            string[] files = JsonConvert.DeserializeObject<string[]>(Config);
            
            JsonSerializerSettings set = new JsonSerializerSettings();
            set.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            set.Formatting = Formatting.Indented;
            for (int i = 0 ; i < files.Length ; i++)
            {
                string path = UCoreUtil.AssetPathNormalize( files[i] );
                string readPath = path.Substring( 0, path.Length - 3 ) + "txt";
                string json = UFileAccessor.ReadStringFile( readPath );
                if (!string.IsNullOrEmpty(json))
                {
                    SortedDictionary<string, UVData> data = JsonConvert.DeserializeObject<SortedDictionary<string, UVData>>( json, set );
                    SortedDictionary<string, UVData>.Enumerator it = data.GetEnumerator();
                    while(it.MoveNext())
                    {
                        var item = it.Current;
                        string texPath = UCoreUtil.AssetPathNormalize( item.Key );
                        BorderData bd = new BorderData();
                        //bd.x = item.Value.x;
                        //bd.y = item.Value.y;
                        //bd.w = item.Value.w;
                        //bd.h = item.Value.h;
                        bd._x = item.Value._x;
                        bd._y = item.Value._y;
                        bd._w = item.Value._w;
                        bd._h = item.Value._h;
                        bd.path = path;
                        if (texturedic.ContainsKey( texPath ))
                            texturedic[texPath] = bd;
                        else 
                        {
                            texturedic.Add( texPath, bd );
                        }
                    }
                }
            }
        }

        public static void ReadTextureConfigWithPath ( string ConfigPath )
        {
            texturedic = new Dictionary<string, BorderData>();
            string Config = UFileAccessor.ReadStringFile( ConfigPath );

            if (string.IsNullOrEmpty( Config ))
            {
                return;
            }

            string[] files = JsonConvert.DeserializeObject<string[]>( Config );

            JsonSerializerSettings set = new JsonSerializerSettings();
            set.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            set.Formatting = Formatting.Indented;
            for (int i = 0 ; i < files.Length ; i++)
            {
                string path = UCoreUtil.AssetPathNormalize( files[i] );
                string readPath = path.Substring( 0, path.Length - 3 ) + "txt";
                string json = UFileAccessor.ReadStringFile( readPath );
                if (!string.IsNullOrEmpty( json ))
                {
                    SortedDictionary<string, UVData> data = JsonConvert.DeserializeObject<SortedDictionary<string, UVData>>( json, set );
                    SortedDictionary<string, UVData>.Enumerator it = data.GetEnumerator();
                    while (it.MoveNext())
                    {
                        var item = it.Current;
                        string texPath = UCoreUtil.AssetPathNormalize( item.Key );
                        BorderData bd = new BorderData();
                        //bd.x = item.Value.x;
                        //bd.y = item.Value.y;
                        //bd.w = item.Value.w;
                        //bd.h = item.Value.h;
                        bd._x = item.Value._x;
                        bd._y = item.Value._y;
                        bd._w = item.Value._w;
                        bd._h = item.Value._h;
                        bd.path = path;
                        if (texturedic.ContainsKey( texPath ))
                            texturedic[texPath] = bd;
                        else
                        {
                            texturedic.Add( texPath, bd );
                        }
                    }
                }
            }
        }

        //打开
        public PanelManagerPort OpenUI(string path, bool active = true, bool IsCache = false)
        {
            path = "Data/bytes/" + path + ".byte";
            SetActiveLayer( 0, true );
            //ULogger.Info("面板" + path + "开始加载...");
            return UILoader.LoadUIData( path, active, LayerVesselList[0].transform, IsCache );
        }

        public PanelManagerPort OpenUI(string path, int index, bool active = true, bool IsCache = false)
        {
            path = "Data/bytes/" + path + ".byte";
            //ULogger.Info("面板" + path + "开始加载至第 " + index + " 层...");
            if (LayerVesselList.Count == 0)
            {
                SetActiveLayer( 0, true );

                ULogger.Error("Layer创建的层级为空");
                return UILoader.LoadUIData( path, active, LayerVesselList[0].transform, IsCache );
            }

            if (index >= 0 && index < LayerVesselList.Count)
            {
                SetActiveLayer( index, true );

                var panel = UILoader.LoadUIData( path, active, LayerVesselList[index].transform, IsCache );
                panel.panelmanager.IsMainPanel = true;
                panel.panelmanager.LayerID = index;
                
                var canvas = panel.panelmanager.gameObject.AddComponent<Canvas>();
                panel.panelmanager.mCanvas = canvas;

                canvas.overrideSorting = true;
                canvas.sortingLayerName = canvasList[index].Count.ToString();
                canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
                panel.panelmanager.SortLayerID = canvasList[index].Count;
                Canvas[] canvaes = panel.panelmanager.GetComponentsInChildren<Canvas>();
                for (int i = 0 ; i < panel.panelmanager.Particles.Count ; i++)
                {
                    panel.panelmanager.Particles[i].sortingLayerName = canvas.sortingLayerName;
                }

                for (int i = 0 ; i < panel.panelmanager.SubCanvaes.Count ; i++)
                {
                    panel.panelmanager.SubCanvaes[i].overrideSorting = true;
                    panel.panelmanager.SubCanvaes[i].sortingLayerName = canvas.sortingLayerName;
                }

                canvasList[index].Add( canvas );
                panel.panelmanager.gameObject.AddComponent<GraphicRaycaster>();
                canvas.overrideSorting = true;

                return panel;
            }
            else
            {
                SetActiveLayer( 0, true );

                ULogger.Error("输入层级越界");
                return UILoader.LoadUIData( path, active, LayerVesselList[0].transform, IsCache );
            }
        }

        public void SetActiveLayer ( int index, bool active ) 
        {
            if (active)
            {
                if (canvasList[index].Count == 0)
                {
                    LayerObjList[index].SetActive( true );
                    CameraList[index].SetActive( true );
                }
            }
            else
            {
                if (LayerVesselList[index].transform.childCount == 1 && index != 0)
                {
                    LayerObjList[index].SetActive( false );
                    CameraList[index].SetActive( false );
                }
            }
        }

        public void RemoveCanvasSortLayerID ( int index, int CanvasIndex ) 
        {
            List<Canvas> canvas = canvasList[index];
            canvas.RemoveAt( CanvasIndex );
            if (CanvasIndex != canvas.Count)
            {
                for (int i = CanvasIndex ; i < canvas.Count ; i++)
                {
                    canvas[i].overrideSorting = true;
                    canvas[i].sortingLayerName = i.ToString();
                    var panel = canvas[i].GetComponent<PanelManager>();
                    panel.SetSubCanvas_ParticleSortLayerID( i );
                }
            }
        }

        public void SortCanvasSortLayerID ( int index, int CanvasIndex, int TargetIndex )
        {
            List<Canvas> canvas = canvasList[index];
            var _Canvas = canvas[CanvasIndex];
            if (TargetIndex < 0 || TargetIndex >= canvas.Count - 1)
            {
                TargetIndex = canvas.Count - 1;
            }

            canvas.RemoveAt( CanvasIndex );
            canvas.Insert( TargetIndex, _Canvas );

            if (CanvasIndex != canvas.Count)
            {
                for (int i = 0 ; i < canvas.Count ; i++)
                {
                    canvas[i].overrideSorting = true;
                    canvas[i].sortingLayerName = i.ToString();
                    var panel = canvas[i].GetComponent<PanelManager>();
                    panel.SetSubCanvas_ParticleSortLayerID( i );
                }
            }
        }

        public PanelManagerPort OpenWorldUI ( string path, bool active = true, bool IsCache = false ) 
        {
            path = "Data/bytes/" + path + ".byte";
            //ULogger.Info( "面板" + path + "开始加载......." );
            return UILoader.LoadUIData( path, active, WorldUICanvas.transform, IsCache );
        }

        //关闭
        public void CloseUI(PanelManagerPort panelmanager)
        {
            panelmanager.ClosePanel();
        }

        public void CreateLayer(int sum) 
        {
            for (int i = 1; i < sum; i++)
            {
                AddLayer();
            }
        }

        public void AddLayer()
        {
            GameObject go = new GameObject("UILayer" + LayerVesselList.Count);
            go.layer = LayerMask.NameToLayer( "UI" );
            go.transform.SetParent(gameObject.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            GameObject Vessel = new GameObject( "UIVessel" + LayerVesselList.Count );
            Vessel.layer = LayerMask.NameToLayer( "UI" );
            Vessel.transform.SetParent( go.transform );
            Vessel.transform.localPosition = Vector3.zero;
            Vessel.transform.localScale = Vector3.one;
            RectTransform v_rect = Vessel.AddComponent<RectTransform>();
            v_rect.anchorMin = Vector2.zero;
            v_rect.anchorMax = Vector2.one;
            v_rect.sizeDelta = Vector2.zero;
            
            GameObject camera = new GameObject( "UICamera" + LayerVesselList.Count );
            camera.transform.SetParent( UICameraObj.transform );
            camera.transform.localPosition = Vector3.zero;
            camera.transform.localScale = Vector3.one;

            Camera _Camera = camera.AddComponent<Camera>();
            if (LayerVesselList.Count == 0)
            {
                //UICamera.enabled = false;
            }
            else
            {
                _Camera.CopyFrom( UICamera );
            }
            _Camera.depth = 2 + LayerVesselList.Count;
            //_Camera.clearFlags = CameraClearFlags.Depth;
            //_Camera.cullingMask = 0;
            //_Camera.cullingMask |= ( 1 << LayerMask.NameToLayer( "UI" ) );

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.pixelPerfect = false;
            canvas.worldCamera = _Camera;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;

            CanvasScaler canvasScaler = go.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2( 1280, 720 );
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasScaler.referencePixelsPerUnit = 100;

            go.AddComponent<GraphicRaycaster>();

            //v_rect.sizeDelta = rect.sizeDelta;

            camera.transform.localPosition = new Vector3( 0, 0, 250 * LayerVesselList.Count );

            LayerVesselList.Add( Vessel );
            LayerObjList.Add( go );

            CameraList.Add( camera );

            {
                go.SetActive( false );
                camera.SetActive( false );
            }

            canvasList.Add( new List<Canvas>() );
        }

        public void SetLayerVisable ( int index, bool active ) 
        {
            if (index >= 0 && index < LayerVesselList.Count)
            {
                LayerVesselList[index].SetActive( active );
            }
        }

        public GameObject GetLayerObj ( int index ) 
        {
            if (index >= 0 && index < LayerVesselList.Count)
            {
                return LayerVesselList[index];
            }

            return null;
        }

        public static SpriteQuote SynGetSprite ( string path, Texture2D tex ) 
        {
            SpriteQuote spriteQuote = null;

            if (mSpriteMap.ContainsKey( path ))
                spriteQuote = mSpriteMap[path];
            else
            {
                SpriteData spritedata = null;
                Sprite s = null;
                spriteQuote = new SpriteQuote();
                if (UIManager.spritedic.ContainsKey( path ))
                {
                    spritedata = UIManager.spritedic[path];
                }
                else
                    spritedata = new SpriteData();

                Vector4 border = new Vector4( spritedata.x, spritedata.y, spritedata.z, spritedata.w );

                if (UIManager.texturedic.ContainsKey( path ))
                {
                    BorderData bordata = UIManager.texturedic[path];
                    if (tex)
                    {
                        //s = Sprite.Create( tex, new Rect( bordata.x * tex.width, bordata.y * tex.height, bordata.w * tex.width, bordata.h * tex.height ), new Vector2( 0.5f, 0.5f ), 100, 1, SpriteMeshType.Tight, border );
                        s = Sprite.Create( tex, new Rect( bordata._x, bordata._y, bordata._w, bordata._h ), new Vector2( 0.5f, 0.5f ), 100, 1, SpriteMeshType.Tight, border );
                        s.name = "Sprite: " + path;
                    }
                }
                else
                {
                    if (tex)
                    {
                        s = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( 0.5f, 0.5f ), 100, 1, SpriteMeshType.Tight, border );
                        s.name = "Sprite: " + path;
                    }
                }
                spriteQuote.sprite = s;
                spriteQuote.path = path;
                mSpriteMap.Add(path, spriteQuote);
            }
            spriteQuote.index++;
            return spriteQuote;
        }

        public static void ReleaseSprite ( SpriteQuote spriteQuote ) 
        {
            spriteQuote.index--;
            if (spriteQuote.index == 0)
            {
                Destroy( spriteQuote.sprite );
                mSpriteMap.Remove( spriteQuote.path );
            }
        }

        public static void SynGetRes ( string path, Action<ResQuote> GetResTask, bool IsAsyn )
        {
            if (path.StartsWith("assets/resources/"))
            {
                path = path.Replace("assets/resources/", "");
            }

            if (mResMap.ContainsKey(path))
            {
                ResQuote resQuote = mResMap[path];
                GetResTask( resQuote );
                resQuote.index++;
            }
            else
            {
                if (IsAsyn)
                {
                    UResourceManager.LoadResource< UnityEngine.Object >(path, (_path, id, _obj)=>
                    {
                        IResource res;
                        res = _obj;
                        ResQuote resQuote = new ResQuote();
                        resQuote.res = res;
                        resQuote.path = path;
                        if (!mResMap.ContainsKey( path ))
                            mResMap.Add(path, resQuote);
                        GetResTask( resQuote );
                        resQuote.index++;
                    });
                }
                else
                {
                    IResource res;
                    res = UResourceManager.SynLoadResource< UnityEngine.Object >( path );
                    ResQuote resQuote = new ResQuote();
                    resQuote.res = res;
                    resQuote.path = path;
                    if (!mResMap.ContainsKey( path ))
                        mResMap.Add( path, resQuote );
                    GetResTask( resQuote );
                    resQuote.index++;
                }
            }
        }

        public static void ReleaseRes(ResQuote resQuote)
        {
            resQuote.index--;
            //if (resQuote.index == 0)
            //{
            //    UResourceManager.UnloadResource(resQuote.res);
            //    mResMap.Remove(resQuote.path);
            //}
        }

        public static void ReleaseAllRes ()
        {
            if (mResMap != null)
            {
                List<ResQuote> clearList = new List<ResQuote>();
                Dictionary<string,ResQuote>.Enumerator it = mResMap.GetEnumerator();
                while (it.MoveNext())
                {
                    var item = it.Current.Value;
                    if (item.index == 0)
                    {
                        clearList.Add( item );
                    }
                }

                for (int i = 0 ; i < clearList.Count ; i++)
                {
                    var resQuote = clearList[i];
                    UResourceManager.UnloadResource( resQuote.res );
                    mResMap.Remove( resQuote.path );
                }
            }
            ResClearTime = 0f;
        }

        public void SynGetNetImage ( string URL, Action<Texture2D> GetNetTextrueTask ) 
        {
            if (!string.IsNullOrEmpty(URL))
            {
                StartCoroutine( GetNetImage( URL, GetNetTextrueTask ) );
            }
        }

        private IEnumerator GetNetImage ( string URL, Action<Texture2D> GetNetTextrueTask )
        {
            string[] url = URL.Split('/');
            string path = url[url.Length - 1];
            if (string.IsNullOrEmpty(path))
            {
                yield break;
            }

            string resPath = (USystemConfig.ResourceFolder + "WWWTxetureCache/" + path).ToLower();
            if (System.IO.File.Exists( resPath )) 
            {
                string protocol = "";
                switch (Application.platform)
                {
                    case RuntimePlatform.IPhonePlayer:
                        break;
                    default:
                        protocol = "file://";
                        break;
                }
                WWW www = new WWW( protocol + resPath );
                while (www.isDone == false)
                    yield return www;

                if (www != null && string.IsNullOrEmpty( www.error ))
                { 
                    GetNetTextrueTask( www.texture );
                    ULogger.Debug( "从本地加载到文件:" + resPath );
                }
                else
                {
                    ULogger.Error( www.error );
                }
            }
            else
            {
                WWW www = new WWW( URL );
                while (www.isDone == false)
                    yield return www;

                if (www != null && string.IsNullOrEmpty( www.error ))
                {
                    Texture2D texture = www.texture;

                    GetNetTextrueTask( texture );

                    UCoreUtil.CreateFile( resPath, www.bytes );

                    ULogger.Debug( "从网络加载到文件:" + resPath );
                    //double time = ( double )Time.time - startTime;
                }
                else
                {
                    ULogger.Error(www.error);
                }
            }

            //double startTime = ( double )Time.time;
        }

        public static void CleanNetImage ( )
        {
            UCoreUtil.DeleteFolder( USystemConfig.ResourceFolder + "/WWWTxetureCache" );
        }

        public static void DeleteNetImage ( string TextureName ) 
        {
            UCoreUtil.DeleteFile( USystemConfig.ResourceFolder + "/WWWTxetureCache/" + TextureName );
        }

        //uint值转颜色
        public static Color RGBA(uint color)
        {
            uint a = 0xFF & color;
            uint b = 0xFF00 & color;
            b >>= 8;
            uint g = 0xFF0000 & color;
            g >>= 16;
            uint r = 0xFF000000 & color;
            r >>= 24;
            return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }

		public static Vector3 GetPreviewPosition(int i)
		{
			return new Vector3(100 * i + 10000, 100 * i + 10000, /*100 * i + 10000*/0);
		}

        //private float UICacheTime;
        private float TextClearTime;
        private static float ResClearTime;

        public static float ResCheckTime = 600f;
        void Update() 
        {
			UProfile.BeginSample("UIManager Update");
            //if (mRenderTasks.Count > 0)
            //{
            //    for (int i = 0; i < mRenderTasks.Count; ++ i)
            //        Render(mRenderTasks[i], i);
            //}
			UProfile.BeginSample("UIManager UIModel Task");

            //if (USystemConfig.Instance.UICacheIntervalTime < UICacheTime)
            //{
            //    foreach (var item in UICacheObj)
            //    {
            //        Destroy(item.Value);
            //    }
            //    UICacheObj.Clear();
            //    UICacheTime = 0f;
            //}
            //else
            //{
            //    UICacheTime += Time.deltaTime;
            //}

            if (ResCheckTime < ResClearTime)
            {
                ReleaseAllRes();
            }
            else
            {
                TextClearTime += Time.deltaTime;
            }
 
            if (USystemConfig.Instance.UICacheIntervalTime < TextClearTime)
            {
                FontFactory.ClearCodeTex( USystemConfig.Instance.TextCheckTime );
                TextClearTime = 0f;
            }
            else
            {
                TextClearTime += Time.deltaTime;
            }

			UProfile.EndSample();
        }

        void LateUpdate ( ) 
        {
            if (mRenderTasks.Count > 0)
            {
                for (int i = 0 ; i < mRenderTasks.Count ; ++i)
                    Render( mRenderTasks[i], i );
            }
        }

        private void Render(RenderTask task, int i)
        {
            if ( task == null || task.mRenderObject == null )
            {
                return;
            }
            GameObject target = task.mRenderObject.GetGameObj();
            if (!target)
            {
                return;
            }
            SetCameraRotation(task.Angle, task.offset_Y, task);
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

            var ambColor		= RenderSettings.ambientLight;
            var refIntensity	= RenderSettings.reflectionIntensity;
            var cubeMap			= RenderSettings.customReflection;
			var ambientMode		= RenderSettings.ambientMode;

			RenderSettings.ambientMode			= UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight			= task.ambColor;
            RenderSettings.reflectionIntensity	= task.Intensity;
            RenderSettings.customReflection		= task.mCubeMap;
			if (!task.SceneLight)
			{
				UIPreviewLight.SetActive(false);
			} else
			{
				mUIPreviewLight.color = task.difColor;
				mUIPreviewLight.intensity = task.difIntensity;
			}

            if (task.index != i)
            {
				target.transform.position -= GetPreviewPosition(task.index);// new Vector3(100 * task.index + 10000, 100 * task.index + 10000, /*100 * task.index + 10000*/0);
				target.transform.position += GetPreviewPosition(i);// new Vector3(100 * i + 10000, 100 * i + 10000, /*100 * i + 10000*/0);
                task.index = i;
            }
			mCameraPreview.transform.position += GetPreviewPosition(i);// new Vector3(100 * i + 10000, 100 * i + 10000, /*100 * i + 10000*/0);
            //target.transform.position += new Vector3( 1000, 1000, 1000 );
            //for (int i = 0 ; i < task.rendererList.Count ; i++)
            //{
            //    if (task.rendererList[i] != null)
            //    {
            //        task.rendererList[i].enabled = true;
            //    }
            //}

            //foreach (var renderer in renderers)
            //    renderer.enabled = true;

            //var renderTarget = mCameraPreview.targetTexture;
            mCameraPreview.enabled = true;


            mCameraPreview.targetTexture = task.mTexture;
            mCameraPreview.Render();

            //mCameraPreview.targetTexture = renderTarget;

            if (!task.SceneLight)
                UIPreviewLight.SetActive( true );

            //target.gameObject.transform.position -= new Vector3( 1000, 1000, 1000 );
			mCameraPreview.gameObject.transform.position -= GetPreviewPosition(i);// new Vector3(100 * i + 10000, 100 * i + 10000, /*100 * i + 10000*/0);
            //for (int i = 0 ; i < task.rendererList.Count ; i++)
            //{
            //    if (task.rendererList[i] != null)
            //    {
            //        task.rendererList[i].enabled = false;
            //    }
            //}

			RenderSettings.ambientMode			= ambientMode;
            RenderSettings.ambientLight			= ambColor;
            RenderSettings.reflectionIntensity	= refIntensity;
            RenderSettings.customReflection		= cubeMap;

            //foreach (var renderer in renderers)
            //    renderer.enabled = false;
            
            mCameraPreview.enabled = false;
        }

        private void SetCameraRotation(float angle, float offset, RenderTask task = null) 
        {
            if (mCameraPreview)
            {
                float z = Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * distance;

                mCameraPreview.gameObject.transform.position = new Vector3( 0, y + offset, z );

                if (task != null)
                {
                    mCameraPreview.transform.RotateAround(new Vector3(0, offset, 0), Vector3.up, task.Angle_Y);
                    UIPreviewLight.transform.eulerAngles = new Vector3( UIPreviewLight.transform.eulerAngles.x, task.Angle_Y + 180f, UIPreviewLight.transform.eulerAngles.z );
                }
                
                mCameraPreview.gameObject.transform.LookAt( new Vector3( 0, offset, 0 ), Vector3.up );

            }
        }

        public static void ColorConfigInit()
        {
            string json = UFileAccessor.ReadStringFile("data/colorconfig.txt");
            if (!string.IsNullOrEmpty(json))
            {
                List<ColorLibray> mColorList = JsonConvert.DeserializeObject<List<ColorLibray>>(json) as List<ColorLibray>;
                if (mColorList != null)
                {
                    mColorLibray.Clear();
                    for (int i = 0; i < mColorList.Count; i++)
                    {
                        mColorLibray.Add(mColorList[i].mColorName, new Color(mColorList[i].r, mColorList[i].g, mColorList[i].b, mColorList[i].a));
                    }
                }
            }

            string _json = UFileAccessor.ReadStringFile( "data/FontSizeConfig.txt" );
            if (!string.IsNullOrEmpty(_json))
                mFontSizeLibray = JsonConvert.DeserializeObject<SortedDictionary<string, int>>( _json ) as SortedDictionary<string, int>;
        }

        private static Material hideMat;
        public static Material GetHideMaterial ( ) 
        {
            if (hideMat == null)
            {
                hideMat = new Material( UShaderManager.FindShader( "UI/Particles/Hidden" ) );
            }
            return hideMat;
        }

        void OnDestroy() 
        {
            UResourceManager.UnloadResource(UiGrayMaterial);
        }

        public static Vector2 ScreenToViewPoint(Vector2 screenPos)
        {
            return new Vector2(screenPos.x / Screen.width * UIRootRectTransform.sizeDelta.x, screenPos.y / Screen.height * UIRootRectTransform.sizeDelta.y);
        }
    }

    public class RenderTask
    {
        public RenderTexture mTexture;
        public IActor mRenderObject;
        public Cubemap mCubeMap;
        public float Angle;
        public float offset_Y;
        public Color ambColor = new Color(0.6764706f, 0.6764706f, 0.6764706f);
        public Color difColor = new Color(0.6764706f, 0.6764706f, 0.6764706f, 1.0f);
        public float Intensity;
		public float difIntensity;
        public bool SceneLight = true;
        public float RotateSpeed = 0f;
        public float Angle_Y = 0f;
        public int index = 0;

        public RenderTask() { }

        public void Init ( RenderTexture texture, IActor _actor, float angle, float y, Cubemap _mCubeMap, Color _ambColor, float _Intensity, bool _SceneLight, Color _difColor, float _difIntensity ) 
        {
            mTexture = texture;
            mRenderObject = _actor;
            Angle = angle;
            offset_Y = y;
            ambColor = _ambColor;
            difColor = _difColor;
            mCubeMap = _mCubeMap;
            Intensity = _Intensity;
            difIntensity = _difIntensity;
            SceneLight = _SceneLight;
        }

		public RenderTask(RenderTexture texture, IActor _actor, float angle, float y, Cubemap _mCubeMap, Color _ambColor, float _Intensity, bool _SceneLight, Color _difColor, float _difIntensity)
        {
            mTexture = texture;
            mRenderObject = _actor;
            Angle = angle;
            offset_Y = y;
			ambColor = _ambColor;
			difColor = _difColor;
            mCubeMap = _mCubeMap;
            Intensity = _Intensity;
			difIntensity = _difIntensity;
            SceneLight = _SceneLight;
        }
    }

    public class ResQuote
    {
        public IResource res;
        public int index = 0;
        public string path;
    }

    public class SpriteQuote 
    {
        public Sprite sprite;
        public int index;
        public string path;
    }
}
