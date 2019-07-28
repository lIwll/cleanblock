using System;

using UnityEngine;

using Object = UnityEngine.Object;

namespace UEngine
{
	// asset bundle
	public class UResource : URefObject, IResource
	{
		public virtual bool IsDone
		{ get; set; }

		public virtual bool IsMain
		{ get { return true;} }

		public virtual Object Res
		{ get { return null; } }

		public virtual int InstanceID
		{ get { return -1; } }

		public virtual string RelativePath
		{ get; set; }

		public bool Create(string path, byte[] data)
		{
			return true;
		}

		public virtual bool Cancel()
		{
			return true;
		}

		public virtual T Load< T >(string name) where T : Object
		{
			return default(T);
		}

		public virtual void Unload(bool release)
		{
		}

		public virtual void AddDepend(IResource res)
		{
		}

		public virtual UObjectBase Instantiate< T >() where T : Object
		{
			return null;
		}
	}
}
