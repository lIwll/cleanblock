using System;
using System.Diagnostics;
using System.Collections.Generic;

using UnityEngine;

namespace UEngine
{
    public enum ELogType
    {
        eLT_Debug,
        eLT_Info,
        eLT_Warn,
        eLT_Profile,
        eLT_Error,
        eLT_ScriptError,
        eLT_Assert,
    }
    public delegate void LogSystemOutput(ELogType type, string msg);

    public class ULogger
    {
#if LOG_TO_DISK
        private StreamWriter
            mLogStream = null;
#endif

        public static LogSystemOutput mOutput = null;

        public static void Init()
        {
#if LOG_TO_DISK
            string logFile = string.Format("{0}/Game_{1}.log", "log/", DateTime.Now.ToString("yyyy-MM-dd"));

            mLogStream = new StreamWriter(logFile, true);
#endif
        }

        public static void Cleanup()
        {
#if LOG_TO_DISK
            mLogStream.Close();
#endif
        }

        public static void Debug(string format, params object[] args)
        {
            string str = string.Format("[Debug]:" + format, args);

            Output(ELogType.eLT_Debug, str);
        }

        public static void Info(string format, params object[] args)
        {
            string str = string.Format("[Info]:" + format, args);

            Output(ELogType.eLT_Info, str);
        }

        public static void Warn(string format, params object[] args)
        {
            string str = string.Format("[Warn]:" + format, args);

            Output(ELogType.eLT_Warn, str);
        }

		public static void Profile(string format, params object[] args)
		{
			ProfileImpl(format, args);
		}

        public static void Error(string format, params object[] args)
        {
            string str = string.Format("[Error]:" + format, args);

            Output(ELogType.eLT_Error, str);
        }

		public static void ScriptError(string format, params object[] args)
		{
			string str = string.Format("[Error]:" + format, args);

			Output(ELogType.eLT_ScriptError, str);
		}

        public static void Assert(bool check, string format, params object[] args)
        {
            if (!check)
            {
                string str = string.Format("[Assert]:" + format, args);

                Output(ELogType.eLT_Assert, str);
            }
        }

        public static void CallStack()
        {
            StackTrace trace = new StackTrace();

            Debug("Call stack:\n{0}\n", trace.ToString());
        }

        private static void Output(ELogType type, string msg)
        {
            if (null != mOutput)
            {
                mOutput(type, msg);
            } else
            {
#if ENABLE_UNITY_DEBUG_LOG
                switch (type)
                {
                    case ELogType.eLT_Info:     UnityEngine.Debug.Log(msg);         break;
                    case ELogType.eLT_Debug:    UnityEngine.Debug.Log(msg);         break;
                    case ELogType.eLT_Warn:     UnityEngine.Debug.LogWarning(msg);  break;
                    case ELogType.eLT_Error:    UnityEngine.Debug.LogError(msg);    break;
                    case ELogType.eLT_Assert:   UnityEngine.Debug.LogError(msg);    break;
                };
#endif
            }
        }

        public static void DebugWindowsLog(string mesg) 
        {
            System.Diagnostics.Trace.WriteLine( "[nslm]:" + DateTime.Now.ToString( "HH:mm:ss" ) + "." + DateTime.Now.Millisecond.ToString() + "  " + mesg );
        }

		[Conditional("ENABLE_PROFILER")]
		static void ProfileImpl(string format, params object[] args)
		{
			string str = string.Format("[Profile]:" + format, args);

			Output(ELogType.eLT_Profile, str);
		}
    }
}
