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
        /// 多次进行http请求，直到返回200或者尝试次数为0位置
        /// </summary>
        /// <param name="url">请求的url</param>
        /// <param name="timeout">超时最大限制的时间</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retryDelay">重试的延迟时间</param>
        /// <param name="retryCallBack">重试开始的时候的回调</param>
        /// <param name="callback">成功返回的回调</param>
        public static void GetTextByTimes(string url, int timeout, int retryCount, int retryDelay, Action retryCallBack, System.Action<int, string> callback)
        {
            UCore.GameEnv.StartCoroutine(GetTextCoroutineByTimes(url, timeout, retryCount, retryDelay, retryCallBack, callback));
        }

        /// <summary>
        /// 进行多次http请求对应的协程函数
        /// </summary>
        /// <param name="url">请求的url</param>
        /// <param name="timeout">超时最大限制的时间</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retryDelay">重试的延迟时间</param>
        /// <param name="retryCallBack">重试开始的时候的回调</param>
        /// <param name="callback">成功返回的回调</param>
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

                //判断是否返回错误
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
                    // 若正确返回，则判断返回码是否为200
                    // 返回为200说明返回成功，则不进行重试操作
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
                    // 设置重新请求的延迟最小为1秒
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