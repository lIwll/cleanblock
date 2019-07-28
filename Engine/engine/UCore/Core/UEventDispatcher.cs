using System;
using System.Collections.Generic;

namespace UEngine
{
    [Serializable]
    public class UEventException : Exception
    {
        public UEventException(string message)
            : base(message)
        { }

        public UEventException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public class UEventController
    {
        private Dictionary< string, Delegate > mTheRouter = new Dictionary< string, Delegate >();

        public Dictionary< string, Delegate > TheRouter
        {
            get { return mTheRouter; }
        }

        private List< string > mPermanentEvents = new List< string >();

        public void MarkAsPermanent(string eventType)
        {
            mPermanentEvents.Add(eventType);
        }

        public bool ContainsEvent(string eventType)
        {
            return mTheRouter.ContainsKey(eventType);
        }

        public void Cleanup()
        {
            List< string > eventToRemove = new List< string >();

            Dictionary< string, Delegate >.Enumerator it = mTheRouter.GetEnumerator();
            while (it.MoveNext())
            {
                var key = it.Current.Key;

                bool wasFound = false;
				for (int i = 0; i < mPermanentEvents.Count; ++ i)
				{
					if (key == mPermanentEvents[i])
					{
						wasFound = true;

						break;
					}
				}

                if (!wasFound)
                    eventToRemove.Add(key);
            }

			for (int i = 0; i < eventToRemove.Count; ++ i)
				mTheRouter.Remove(eventToRemove[i]);
        }

        private void OnListenerAdding(string eventType, Delegate listenerBeingAdded)
        {
            if (!mTheRouter.ContainsKey(eventType))
                mTheRouter.Add(eventType, null);

            Delegate d = mTheRouter[eventType];
            if (d != null && d.GetType() != listenerBeingAdded.GetType())
                throw new UEventException(string.Format("Try to add not correct event {0}. Current type is {1}, adding type is {2}.", eventType, d.GetType().Name, listenerBeingAdded.GetType().Name));
        }

        private bool OnListenerRemoving(string eventType, Delegate listenerBeingRemoved)
        {
            if (!mTheRouter.ContainsKey(eventType))
                return false;

            Delegate d = mTheRouter[eventType];
            if ((d != null) && (d.GetType() != listenerBeingRemoved.GetType()))
                throw new UEventException(string.Format("Remove listener {0}\" failed, Current type is {1}, adding type is {2}.", eventType, d.GetType(), listenerBeingRemoved.GetType()));

            return true;
        }

        private void OnListenerRemoved(string eventType)
        {
            if (mTheRouter.ContainsKey(eventType) && mTheRouter[eventType] == null)
                mTheRouter.Remove(eventType);
        }

        public void AddEventListener(string eventType, Action handler)
        {
            OnListenerAdding(eventType, handler);

            mTheRouter[eventType] = (Action)mTheRouter[eventType] + handler;
        }

        public void AddEventListener< T >(string eventType, Action< T > handler)
        {
            OnListenerAdding(eventType, handler);

            mTheRouter[eventType] = (Action< T >)mTheRouter[eventType] + handler;
        }

        public void AddEventListener< T, U >(string eventType, Action< T, U > handler)
        {
            OnListenerAdding(eventType, handler);

            mTheRouter[eventType] = (Action< T, U >)mTheRouter[eventType] + handler;
        }

        public void AddEventListener< T, U, V >(string eventType, Action< T, U, V > handler)
        {
            OnListenerAdding(eventType, handler);

            mTheRouter[eventType] = (Action< T, U, V >)mTheRouter[eventType] + handler;
        }

        public void AddEventListener< T, U, V, W >(string eventType, Action< T, U, V, W > handler)
        {
            OnListenerAdding(eventType, handler);

            mTheRouter[eventType] = (Action< T, U, V, W >)mTheRouter[eventType] + handler;
        }

        public void RmvEventListener(string eventType, Action handler)
        {
            if (OnListenerRemoving(eventType, handler))
            {
                mTheRouter[eventType] = (Action)mTheRouter[eventType] - handler;

                OnListenerRemoved(eventType);
            }
        }

        public void RmvEventListener< T >(string eventType, Action< T > handler)
        {
            if (OnListenerRemoving(eventType, handler))
            {
                mTheRouter[eventType] = (Action< T >)mTheRouter[eventType] - handler;

                OnListenerRemoved(eventType);
            }
        }

        public void RmvEventListener< T, U >(string eventType, Action< T, U > handler)
        {
            if (OnListenerRemoving(eventType, handler))
            {
                mTheRouter[eventType] = (Action< T, U >)mTheRouter[eventType] - handler;

                OnListenerRemoved(eventType);
            }
        }

        public void RmvEventListener< T, U, V >(string eventType, Action< T, U, V > handler)
        {
            if (OnListenerRemoving(eventType, handler))
            {
                mTheRouter[eventType] = (Action< T, U, V >)mTheRouter[eventType] - handler;

                OnListenerRemoved(eventType);
            }
        }

        public void RmvEventListener< T, U, V, W >(string eventType, Action< T, U, V, W > handler)
        {
            if (OnListenerRemoving(eventType, handler))
            {
                mTheRouter[eventType] = (Action< T, U, V, W >)mTheRouter[eventType] - handler;

                OnListenerRemoved(eventType);
            }
        }

        public void FireEvent(string eventType)
        {
            Delegate d;
            if (!mTheRouter.TryGetValue(eventType, out d))
                return;

            var callbacks = d.GetInvocationList();
            for (int i = 0; i < callbacks.Length; i ++)
            {
                Action callback = callbacks[i] as Action;

                if (callback == null)
                    throw new UEventException(string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));

                try
                {
                    callback();
                } catch (Exception ex)
                {
                    ULogger.Error(ex.Message);
                }
            }
        }

        public void FireEvent< T >(string eventType, T arg1)
        {
            Delegate d;
            if (!mTheRouter.TryGetValue(eventType, out d))
                return;

            var callbacks = d.GetInvocationList();
            for (int i = 0; i < callbacks.Length; i ++)
            {
                Action< T > callback = callbacks[i] as Action< T >;
                if (callback == null)
                    throw new UEventException(string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));

                try
                {
                    callback(arg1);
                } catch (Exception ex)
                {
                    ULogger.Error(ex.Message);
                }
            }
        }

        public void FireEvent< T, U >(string eventType, T arg1, U arg2)
        {
            Delegate d;
            if (!mTheRouter.TryGetValue(eventType, out d))
                return;

            var callbacks = d.GetInvocationList();
            for (int i = 0; i < callbacks.Length; i ++)
            {
                Action< T, U > callback = callbacks[i] as Action< T, U >;
                if (callback == null)
                    throw new UEventException(string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));

                try
                {
                    callback(arg1, arg2);
                } catch (Exception ex)
                {
                    ULogger.Error(ex.Message);
                }
            }
        }

        public void FireEvent< T, U, V >(string eventType, T arg1, U arg2, V arg3)
        {
            Delegate d;
            if (!mTheRouter.TryGetValue(eventType, out d))
                return;

            var callbacks = d.GetInvocationList();
            for (int i = 0; i < callbacks.Length; i ++)
            {
                Action< T, U, V > callback = callbacks[i] as Action< T, U, V >;
                if (callback == null)
                    throw new UEventException(string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));

                try
                {
                    callback(arg1, arg2, arg3);
                } catch (Exception ex)
                {
                    ULogger.Error(ex.Message);
                }
            }
        }

        public void FireEvent< T, U, V, W >(string eventType, T arg1, U arg2, V arg3, W arg4)
        {
            Delegate d;
            if (!mTheRouter.TryGetValue(eventType, out d))
                return;

            var callbacks = d.GetInvocationList();
            for (int i = 0; i < callbacks.Length; i ++)
            {
                Action< T, U, V, W > callback = callbacks[i] as Action< T, U, V, W >;
                if (callback == null)
                    throw new UEventException(string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));

                try
                {
                    callback(arg1, arg2, arg3, arg4);
                } catch (Exception ex)
                {
                    ULogger.Error(ex.Message);
                }
            }
        }
    }

    public class UEventDispatcher
    {
        internal static UEventController mEventController = new UEventController();

        public static Dictionary< string, Delegate > TheRouter
        {
            get { return mEventController.TheRouter; }
        }

        static public void MarkAsPermanent(string eventType)
        {
            mEventController.MarkAsPermanent(eventType);
        }

        static public void Cleanup()
        {
            mEventController.Cleanup();
        }

        static public void AddEventListener(string eventType, Action handler)
        {
            mEventController.AddEventListener(eventType, handler);
        }

        static public void AddEventListener< T >(string eventType, Action< T > handler)
        {
            mEventController.AddEventListener(eventType, handler);
        }

        static public void AddEventListener< T, U >(string eventType, Action< T, U > handler)
        {
            mEventController.AddEventListener(eventType, handler);
        }

        static public void AddEventListener< T, U, V >(string eventType, Action< T, U, V > handler)
        {
            mEventController.AddEventListener(eventType, handler);
        }

        static public void AddEventListener< T, U, V, W >(string eventType, Action< T, U, V, W > handler)
        {
            mEventController.AddEventListener(eventType, handler);
        }

        static public void RmvEventListener(string eventType, Action handler)
        {
            mEventController.RmvEventListener(eventType, handler);
        }

        static public void RmvEventListener< T >(string eventType, Action< T > handler)
        {
            mEventController.RmvEventListener(eventType, handler);
        }

        static public void RmvEventListener< T, U >(string eventType, Action< T, U > handler)
        {
            mEventController.RmvEventListener(eventType, handler);
        }

        static public void RmvEventListener< T, U, V >(string eventType, Action< T, U, V > handler)
        {
            mEventController.RmvEventListener(eventType, handler);
        }

        static public void RmvEventListener< T, U, V, W >(string eventType, Action< T, U, V, W > handler)
        {
            mEventController.RmvEventListener(eventType, handler);
        }

        static public void FireEvent(string eventType)
        {
            mEventController.FireEvent(eventType);
        }

        static public void FireEvent< T >(string eventType, T arg1)
        {
            mEventController.FireEvent(eventType, arg1);
        }

        static public void FireEvent< T, U >(string eventType, T arg1, U arg2)
        {
            mEventController.FireEvent(eventType, arg1, arg2);
        }

        static public void FireEvent< T, U, V >(string eventType, T arg1, U arg2, V arg3)
        {
            mEventController.FireEvent(eventType, arg1, arg2, arg3);
        }

        static public void FireEvent< T, U, V, W >(string eventType, T arg1, U arg2, V arg3, W arg4)
        {
            mEventController.FireEvent(eventType, arg1, arg2, arg3, arg4);
        }
    }
}
