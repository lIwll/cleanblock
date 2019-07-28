using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

namespace UEngine
{
    public static class UNavMeshManager
    {
		struct SNavMeshData
		{
			public IResource mRes;
			public NavMeshData mData;
		}
        private static List< SNavMeshData > mData;

        private static List< NavMeshDataInstance > mDataInstances;

        static UNavMeshManager()
        {
            mData = new List< SNavMeshData >(5);

            mDataInstances = new List< NavMeshDataInstance >();
        }

        public static void Add(IResource res, NavMeshData data)
        {
			UBundleManager.AddReferenceCount(res);

			mData.Add(new SNavMeshData() { mRes = res, mData = data });
        }

        public static void Reset()
        {
			for (int i = 0; i < mDataInstances.Count; ++ i)
				mDataInstances[i].Remove();
            mDataInstances.Clear();

			for (int i = 0; i < mData.Count; ++ i)
				UBundleManager.DelReferenceCount(mData[i].mRes);
			mData.Clear();
        }

        public static void Build()
        {
			for (int i = 0; i < mData.Count; ++ i)
				mDataInstances.Add(NavMesh.AddNavMeshData(mData[i].mData));
        }
    }
}
