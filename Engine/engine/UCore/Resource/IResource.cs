using System;

using UnityEngine;

using Object = UnityEngine.Object;

namespace UEngine
{
	public interface IResource
	{
		bool IsDone { get; set; }

		bool IsMain { get; }

		int RefCnt { get; }

		Object Res { get; }

		int InstanceID { get; }

		string RelativePath { get; set; }

		int AddRef();

		int Release();

		bool Cancel();

		T Load< T >(string name) where T : Object;

		void Unload(bool release);

		void AddDepend(IResource res);

		UObjectBase Instantiate< T >() where T : Object;
	}
}
