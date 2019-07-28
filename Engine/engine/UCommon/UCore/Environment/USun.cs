using System;

using UnityEngine;

namespace UEngine
{
	[DisallowMultipleComponent, AddComponentMenu("UEngine/Environment/Sun")]
	public class USun : MonoBehaviour
	{
		Light mLight = null;
		void OnEnable()
		{
			mLight = GetComponent< Light >();
			if (null != mLight)
				UEnvironment.Sun = this;
		}

		void OnDisable()
		{
			UEnvironment.Sun = null;
		}

		public Light GetLight()
		{
			return mLight;
		}
	}
}
