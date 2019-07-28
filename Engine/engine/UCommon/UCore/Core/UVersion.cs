using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UEngine
{
    public class UVersionCode
    {
        private List< int > mTags = new List< int >();

        public UVersionCode(string version)
        {
            if (string.IsNullOrEmpty(version))
                return;

            var versions = version.Split('.');
            for (int i = 0; i < versions.Length; i ++)
            {
                int v;
                if (int.TryParse(versions[i], out v))
                    mTags.Add(v);
                else
                    mTags.Add(v);
            }
        }

        public string GetUpperVersion()
        {
            var lastTag = mTags[mTags.Count - 1] + 1;

            var sb = new StringBuilder();
            for (int i = 0; i < mTags.Count - 1; i ++)
                sb.AppendFormat("{0}.", mTags[i]);
            sb.Append(lastTag);

            return sb.ToString();
        }

        public string GetLowerVersion()
        {
            var lastTag = mTags[mTags.Count - 1] - 1;

            var sb = new StringBuilder();
            for (int i = 0; i < mTags.Count - 1; i ++)
                sb.AppendFormat("{0}.", mTags[i]);
            sb.Append(lastTag);

            return sb.ToString();
        }

        public string ToShortString()
        {
            var sb = new StringBuilder();
            foreach (var item in mTags)
                sb.Append(item);

            return sb.ToString();
        }

        public int Compare(UVersionCode code)
        {
            var count = mTags.Count < code.mTags.Count ? mTags.Count : code.mTags.Count;
            for (int i = 0; i < count; i ++)
            {
                if (mTags[i] == code.mTags[i])
                    continue;

                return mTags[i] > code.mTags[i] ? 1 : -1;
            }

            return 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var item in mTags)
                sb.AppendFormat("{0}.", item);
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
