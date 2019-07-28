using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UEngine
{
    public static class UWeb
    {
        public static void GetText(string url, int timeout, System.Action<int, string> callback)
        {
            UCore.GameEnv.StartCoroutine(GetTextCoroutine(url, timeout, callback));
        }
        static IEnumerator GetTextCoroutine(string url, int timeout, System.Action<int, string> callback)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.timeout = timeout;
#if UNITY_2017
			yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif
    
            if (callback != null)
            {
#if UNITY_2017
				if (www.isNetworkError)
#else
                if(www.isError)
#endif
				{
                    callback(-2, www.error);
                } else
				{
                    callback((int)www.responseCode, www.downloadHandler.text);
                }
            }
        }

        /// <summary>
        /// ��ν���http����ֱ������200���߳��Դ���Ϊ0λ��
        /// </summary>
        /// <param name="url">�����url</param>
        /// <param name="timeout">��ʱ������Ƶ�ʱ��</param>
        /// <param name="retryCount">���Դ���</param>
        /// <param name="retryDelay">���Ե��ӳ�ʱ��</param>
        /// <param name="retryCallBack">���Կ�ʼ��ʱ��Ļص�</param>
        /// <param name="callback">�ɹ����صĻص�</param>
        public static void GetTextByTimes(string url, int timeout, int retryCount, int retryDelay, Action retryCallBack, System.Action<int, string> callback)
        {
            UCore.GameEnv.StartCoroutine(GetTextCoroutineByTimes(url, timeout, retryCount, retryDelay, retryCallBack, callback));
        }

        /// <summary>
        /// ���ж��http�����Ӧ��Э�̺���
        /// </summary>
        /// <param name="url">�����url</param>
        /// <param name="timeout">��ʱ������Ƶ�ʱ��</param>
        /// <param name="retryCount">���Դ���</param>
        /// <param name="retryDelay">���Ե��ӳ�ʱ��</param>
        /// <param name="retryCallBack">���Կ�ʼ��ʱ��Ļص�</param>
        /// <param name="callback">�ɹ����صĻص�</param>
        /// <returns></returns>
        static IEnumerator GetTextCoroutineByTimes(string url, int timeout, int retryCount, float retryDelay, Action retryCallBack, System.Action<int, string> callback)
        {
            while (retryCount >= 0)
            {
                UnityWebRequest www = UnityWebRequest.Get(url);
                www.timeout = timeout;
#if UNITY_2017
			    yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

                //�ж��Ƿ񷵻ش���
#if UNITY_2017
                if ( www.isNetworkError )
#else
                if ( www.isError )
#endif
                {
                    if ( retryCount == 0 )
                    {
                        if ( callback != null )
                        {
                            callback(-2, www.error);
                        }
                        break;
                    }
                }
                else
                {
                    // ����ȷ���أ����жϷ������Ƿ�Ϊ200
                    // ����Ϊ200˵�����سɹ����򲻽������Բ���
                    int responseCode = (int)www.responseCode;
                    if (responseCode == 200 || retryCount == 0)
                    {
                        if (callback != null)
                        {
                            callback(responseCode, www.downloadHandler.text);
                        }
                        break;
                    }
                }
                retryCount--;
                if ( retryCount >= 0 )
                {
                    float delayTime = retryDelay;
                    // ��������������ӳ���СΪ1��
                    delayTime = Mathf.Max(delayTime, 1f);
                    yield return new WaitForSeconds(delayTime);
                    if ( retryCallBack != null )
                    {
                        retryCallBack();
                    }
                }
            }
        }
    }
}