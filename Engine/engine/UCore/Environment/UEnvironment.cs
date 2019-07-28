using System;

using UnityEngine;

namespace UEngine
{
	public class UEnvironment
	{
		static USun mSun = null;
		public static USun Sun
		{
			get { return mSun; }
			set { mSun = value; }
		}

		static UEnvironment()
		{
		}
	}
}
