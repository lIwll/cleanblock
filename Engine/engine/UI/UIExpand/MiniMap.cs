using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UEngine;
using UEngine.UI;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace UEngine.UIExpand
{
    public class MiniMap : MonoBehaviour
    {
        public MiniMapConfig mMapConfig;

        public RectTransform MapRect;

        private Vector2 minPos;

        private Vector2 maxPos;

        private float width;

        private float height;

        private Image _mImage;

        public Image mImage 
        {
            get 
            {
                if (!_mImage)
                {
                    _mImage = MapRect.GetComponent<Image>();
                }
                return _mImage;
            }
        }

        public void LoadMap ( string SceneName, int X1, int Z1, int X2, int Z2, float Size, float offsetX, float offsetY )
        {
            string json = UFileAccessor.ReadStringFile("data/xml/" + SceneName + "_minimapconfig.txt");

            if (string.IsNullOrEmpty(json))
            {
                ULogger.Error(SceneName + "场景地图配置信息加载为空");
                return;
            }
            else
            {
                mMapConfig = JsonConvert.DeserializeObject<MiniMapConfig>(json);
            }

            if (X1 > mMapConfig.X || X2 > mMapConfig.X || Z1 > mMapConfig.Y || Z2 > mMapConfig.Y || X1 > X2 || Z1 > Z2)
            {
                ULogger.Error(SceneName + "小地图配置信息异常");
                return;
            }

            mImage.rectTransform.sizeDelta = new Vector2( mImage.rectTransform.sizeDelta.y / ( float )( Z2 - Z1 + 1 ) * ( float )( X2 - X1 + 1 ) * Size, mImage.rectTransform.sizeDelta.y * Size );
            mImage.rectTransform.anchoredPosition = new Vector2( offsetX, offsetY );

            float size = mMapConfig.CameraSize * 2;
            float min_X = mMapConfig.min_X + size * (X1 - 1);
            float min_Z = mMapConfig.min_Z + size * (Z1 - 1);
            float max_X = mMapConfig.max_X + size * (X2 - 1);
            float max_Z = mMapConfig.max_Z + size * (Z2 - 1);

            minPos = new Vector2(min_X, min_Z);
            maxPos = new Vector2(max_X, max_Z);

            width = (X2 - X1 + 1) * size;
            height = (Z2 - Z1 + 1) * size;
        }

        public Vector2 TransformPoint(Vector3 point) 
        {
            if (point.x > maxPos.x || point.x < minPos.x || point.z < minPos.y || point.z > maxPos.y)
            {
                ULogger.Warn("点: " + point.ToString() + "不在地图中");
                return Vector2.zero;
            }

            float X = Mathf.Abs(point.x - minPos.x);
            float Y = Mathf.Abs(point.z - minPos.y);

            Vector2 rectSize = mImage.rectTransform.sizeDelta;

            if (width <= 0 || height <= 0)
            {
                return Vector2.zero;
            }
            
            return new Vector2((X / width) * rectSize.x, (Y / height) * rectSize.y);
        }

        public Vector3 TransformScreenToWorld(Vector2 ScreenPos) 
        {
            Vector2 rectSize = mImage.rectTransform.sizeDelta;
            
            float X = ScreenPos.x / rectSize.x;
            float Y = ScreenPos.y / rectSize.y;

            X = X * width;
            Y = Y * height;
            
            return new Vector3(X + minPos.x, 0, Y + minPos.y);
        }
    }
}
