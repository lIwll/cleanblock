using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UEngine.UI
{
    public class UiFrameAnimation:MonoBehaviour
    {
        [HideInInspector]
        public string SpritePath = "";

        public int Fps;
        public bool Loop;
        public string Prefix;
        public int BeginIndex;
        public int Count;

        private Image m_Image = null;
        protected float mDelta = 0f;
        protected int m_Index = 0;
        protected bool m_Active = false;
        private string m_SpritePathPre = "";
        private string m_LastName = "";

        protected void Update()
        {
            if (m_Active && Count > 1 && m_Image != null && Application.isPlaying && Fps > 0)
            {
                mDelta += Time.unscaledDeltaTime;
                float rate = 1f / Fps;

                if (mDelta < rate)
                    return;

                mDelta = (rate > 0f) ? mDelta - rate : 0f;

                if (++m_Index >= Count)
                {
                    m_Index = BeginIndex;
                    m_Active = Loop;
                }

                if (!m_Active)
                    return;

                string curPath = m_SpritePathPre + m_Index + m_LastName;

                UResourceManager.LoadResource< UnityEngine.Object >(curPath, (path, guid, tmpObj) =>
                {
                    //Debug.Log("Sprite path: " + curPath + "  :" + tmpObj.name);
                    Texture2D texture = (Texture2D)tmpObj.Res;
                    m_Image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                });       
               
            }
        }

        private void Init()
        {
            if (m_SpritePathPre != "")
                return;

            m_Index = BeginIndex;

            int lastIndex = SpritePath.LastIndexOf(".");
            m_LastName = SpritePath.Substring(lastIndex);
            int pos = SpritePath.LastIndexOf(Prefix);
            m_SpritePathPre = SpritePath.Substring(0, pos) + Prefix;

            if (m_Image == null)
            {
                m_Image = transform.GetComponent<Image>();
            }
        }

        public void Play()
        {
            m_Active = true;
            Init();
        }

        public void Pause()
        {
            m_Active = false;
        }

        public void Replay()
        {
            m_Active = true;
            Init();

            if (m_Image != null)
            {
                UResourceManager.LoadResource< UnityEngine.Object >(SpritePath, (path, guid, tmpObj) =>
                {
                    Texture2D texture = (Texture2D)tmpObj.Res;
                    m_Image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                });    
            }
        }

    }
}
