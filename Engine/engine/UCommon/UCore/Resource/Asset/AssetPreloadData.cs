/*
Copyright (c) 2016 Radu

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

/*
DISCLAIMER
The reposiotory, code and tools provided herein are for educational purposes only.
The information not meant to change or impact the original code, product or service.
Use of this repository, code or tools does not exempt the user from any EULA, ToS or any other legal agreements that have been agreed with other parties.
The user of this repository, code and tools is responsible for his own actions.

Any forks, clones or copies of this repository are the responsability of their respective authors and users.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class AssetPreloadData
    {
        public long m_PathID;
        public uint Offset;
        public int Size;
        public ClassIDReference Type;
        public int Type1;
        public int Type2;

        public string TypeString;
        public int fullSize;
        public string InfoText;
        public string extension;

        public AssetsFile sourceFile;
        public GameObject gameObject;
        public string uniqueID;

        public EndianStream InitReader()
        {
            var reader = sourceFile.reader;
            reader.Position = Offset;
            return reader;
        }

        public string Deserialize()
        {
            var reader = InitReader();
            ClassStruct classStructure = null;
            if (sourceFile.ClassStructures.TryGetValue(Type1, out classStructure))
            {
                var sb = new StringBuilder();
                ClassStructHelper.ReadClassString(sb, classStructure.members, reader);
                return sb.ToString();
            }
            return null;
        }
    }

    public static class ClassStructHelper
    {
        public static void ReadClassString(StringBuilder sb, List<ClassMember> members, EndianStream reader)
        {
            for (int i = 0; i < members.Count; i++)
            {
                ReadStringValue(sb, members, reader, ref i);
            }
        }

        private static void ReadStringValue(StringBuilder sb, List<ClassMember> members, EndianStream reader, ref int i)
        {
            var member = members[i];
            var level = member.Level;
            var varTypeStr = member.Type;
            var varNameStr = member.Name;
            object value = null;
            var append = true;
            var align = (member.Flag & 0x4000) != 0;
            switch (varTypeStr)
            {
                case "SInt8":
                    value = reader.ReadSByte();
                    break;
                case "UInt8":
                    value = reader.ReadByte();
                    break;
                case "short":
                case "SInt16":
                    value = reader.ReadInt16();
                    break;
                case "UInt16":
                case "unsigned short":
                    value = reader.ReadUInt16();
                    break;
                case "int":
                case "SInt32":
                    value = reader.ReadInt32();
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    value = reader.ReadUInt32();
                    break;
                case "long long":
                case "SInt64":
                    value = reader.ReadInt64();
                    break;
                case "UInt64":
                case "unsigned long long":
                    value = reader.ReadUInt64();
                    break;
                case "float":
                    value = reader.ReadSingle();
                    break;
                case "double":
                    value = reader.ReadDouble();
                    break;
                case "bool":
                    value = reader.ReadBoolean();
                    break;
                case "string":
                    append = false;
                    var str = reader.ReadAlignedString();
                    sb.AppendFormat("{0}{1} {2} = \"{3}\"\r\n", (new string('\t', level)), varTypeStr, varNameStr, str);
                    i += 3;
                    break;
                case "vector":
                    {
                        if ((members[i + 1].Flag & 0x4000) != 0)
                            align = true;
                        append = false;
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level + 1)), "Array", "Array");
                        var size = reader.ReadInt32();
                        sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level + 1)), "int", "size", size);
                        var vector = GetMembers(members, level, i);
                        i += vector.Count - 1;
                        vector.RemoveRange(0, 3);
                        for (int j = 0; j < size; j++)
                        {
                            sb.AppendFormat("{0}[{1}]\r\n", (new string('\t', level + 2)), j);
                            int tmp = 0;
                            ReadStringValue(sb, vector, reader, ref tmp);
                        }
                        break;
                    }
                case "map":
                    {
                        if ((members[i + 1].Flag & 0x4000) != 0)
                            align = true;
                        append = false;
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level + 1)), "Array", "Array");
                        var size = reader.ReadInt32();
                        sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level + 1)), "int", "size", size);
                        var map = GetMembers(members, level, i);
                        i += map.Count - 1;
                        map.RemoveRange(0, 4);
                        var first = GetMembers(map, map[0].Level, 0);
                        map.RemoveRange(0, first.Count);
                        var second = map;
                        for (int j = 0; j < size; j++)
                        {
                            sb.AppendFormat("{0}[{1}]\r\n", (new string('\t', level + 2)), j);
                            sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level + 2)), "pair", "data");
                            int tmp1 = 0;
                            int tmp2 = 0;
                            ReadStringValue(sb, first, reader, ref tmp1);
                            ReadStringValue(sb, second, reader, ref tmp2);
                        }
                        break;
                    }
                case "TypelessData":
                    {
                        append = false;
                        var size = reader.ReadInt32();
                        reader.ReadBytes(size);
                        i += 2;
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                        sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), "int", "size", size);
                        break;
                    }
                default:
                    {
                        append = false;
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                        var aclass = GetMembers(members, level, i);
                        aclass.RemoveAt(0);
                        i += aclass.Count;
                        for (int j = 0; j < aclass.Count; j++)
                        {
                            ReadStringValue(sb, aclass, reader, ref j);
                        }
                        break;
                    }
            }
            if (append)
                sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
            if (align)
                reader.AlignStream(4);
        }

        private static object ReadValue(List<ClassMember> members, EndianBinaryReader reader, ref int i)
        {
            var member = members[i];
            var level = member.Level;
            var varTypeStr = member.Type;
            object value;
            var align = (member.Flag & 0x4000) != 0;
            switch (varTypeStr)
            {
                case "SInt8":
                    value = reader.ReadSByte();
                    break;
                case "UInt8":
                    value = reader.ReadByte();
                    break;
                case "short":
                case "SInt16":
                    value = reader.ReadInt16();
                    break;
                case "UInt16":
                case "unsigned short":
                    value = reader.ReadUInt16();
                    break;
                case "int":
                case "SInt32":
                    value = reader.ReadInt32();
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    value = reader.ReadUInt32();
                    break;
                case "long long":
                case "SInt64":
                    value = reader.ReadInt64();
                    break;
                case "UInt64":
                case "unsigned long long":
                    value = reader.ReadUInt64();
                    break;
                case "float":
                    value = reader.ReadSingle();
                    break;
                case "double":
                    value = reader.ReadDouble();
                    break;
                case "bool":
                    value = reader.ReadBoolean();
                    break;
                case "string":
                    value = reader.ReadAlignedString();
                    i += 3;
                    break;
                case "vector":
                    {
                        if ((members[i + 1].Flag & 0x4000) != 0)
                            align = true;
                        var size = reader.ReadInt32();
                        var list = new List<object>(size);
                        var vector = GetMembers(members, level, i);
                        i += vector.Count - 1;
                        vector.RemoveRange(0, 3);
                        for (int j = 0; j < size; j++)
                        {
                            int tmp = 0;
                            list.Add(ReadValue(vector, reader, ref tmp));
                        }
                        value = list;
                        break;
                    }
                case "map":
                    {
                        if ((members[i + 1].Flag & 0x4000) != 0)
                            align = true;
                        var size = reader.ReadInt32();
                        var dic = new List<KeyValuePair<object, object>>(size);
                        var map = GetMembers(members, level, i);
                        i += map.Count - 1;
                        map.RemoveRange(0, 4);
                        var first = GetMembers(map, map[0].Level, 0);
                        map.RemoveRange(0, first.Count);
                        var second = map;
                        for (int j = 0; j < size; j++)
                        {
                            int tmp1 = 0;
                            int tmp2 = 0;
                            dic.Add(new KeyValuePair<object, object>(ReadValue(first, reader, ref tmp1), ReadValue(second, reader, ref tmp2)));
                        }
                        value = dic;
                        break;
                    }
                case "TypelessData":
                    {
                        var size = reader.ReadInt32();
                        value = reader.ReadBytes(size);
                        i += 2;
                        break;
                    }
                default:
                    {
                        //var aClass = GetMembers(members, level, i);
                        //aClass.RemoveAt(0);
                        //i += aClass.Count;
                        //var obj = new ExpandoObject();
                        //var objdic = (IDictionary<string, object>)obj;
                        //for (int j = 0; j < aClass.Count; j++)
                        //{
                        //    var classmember = aClass[j];
                        //    var name = classmember.Name;
                        //    objdic[name] = ReadValue(aClass, reader, ref j);
                        //}
                        //value = obj;
                        value = null;
                        break;
                    }
            }
            if (align)
                reader.AlignStream(4);
            return value;
        }

        private static List<ClassMember> GetMembers(List<ClassMember> members, int level, int index)
        {
            var member2 = new List<ClassMember>();
            member2.Add(members[0]);
            for (int i = index + 1; i < members.Count; i++)
            {
                var member = members[i];
                var level2 = member.Level;
                if (level2 <= level)
                {
                    return member2;
                }
                member2.Add(member);
            }
            return member2;
        }

        private static void WriteValue(object value, List<ClassMember> members, System.IO.BinaryWriter write, ref int i)
        {
            var member = members[i];
            var level = member.Level;
            var varTypeStr = member.Type;
            var align = (member.Flag & 0x4000) != 0;
            switch (varTypeStr)
            {
                case "SInt8":
                    write.Write((sbyte)value);
                    break;
                case "UInt8":
                    write.Write((byte)value);
                    break;
                case "short":
                case "SInt16":
                    write.Write((short)value);
                    break;
                case "UInt16":
                case "unsigned short":
                    write.Write((ushort)value);
                    break;
                case "int":
                case "SInt32":
                    write.Write((int)value);
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    write.Write((uint)value);
                    break;
                case "long long":
                case "SInt64":
                    write.Write((long)value);
                    break;
                case "UInt64":
                case "unsigned long long":
                    write.Write((ulong)value);
                    break;
                case "float":
                    write.Write((float)value);
                    break;
                case "double":
                    write.Write((double)value);
                    break;
                case "bool":
                    write.Write((bool)value);
                    break;
                case "string":
                    //write.WriteAlignedString((string)value);
                    i += 3;
                    break;
                case "vector":
                    {
                        if ((members[i + 1].Flag & 0x4000) != 0)
                            align = true;
                        var list = (List<object>)value;
                        var size = list.Count;
                        write.Write(size);
                        var vector = GetMembers(members, level, i);
                        i += vector.Count - 1;
                        vector.RemoveRange(0, 3);
                        for (int j = 0; j < size; j++)
                        {
                            int tmp = 0;
                            WriteValue(list[j], vector, write, ref tmp);
                        }
                        break;
                    }
                case "map":
                    {
                        if ((members[i + 1].Flag & 0x4000) != 0)
                            align = true;
                        var dic = (List<KeyValuePair<object, object>>)value;
                        var size = dic.Count;
                        write.Write(size);
                        var map = GetMembers(members, level, i);
                        i += map.Count - 1;
                        map.RemoveRange(0, 4);
                        var first = GetMembers(map, map[0].Level, 0);
                        map.RemoveRange(0, first.Count);
                        var second = map;
                        for (int j = 0; j < size; j++)
                        {
                            int tmp1 = 0;
                            int tmp2 = 0;
                            WriteValue(dic[j].Key, first, write, ref tmp1);
                            WriteValue(dic[j].Value, second, write, ref tmp2);
                        }
                        break;
                    }
                case "TypelessData":
                    {
                        var bytes = ((object[])value).Cast<byte>().ToArray();
                        var size = bytes.Length;
                        write.Write(size);
                        write.Write(bytes);
                        i += 2;
                        break;
                    }
                default:
                    {
                        //var @class = GetMembers(members, level, i);
                        //@class.RemoveAt(0);
                        //i += @class.Count;
                        //var obj = (ExpandoObject)value;
                        //var objdic = (IDictionary<string, object>)obj;
                        //for (int j = 0; j < @class.Count; j++)
                        //{
                        //    var classmember = @class[j];
                        //    var name = classmember.Name;
                        //    WriteValue(objdic[name], @class, write, ref j);
                        //}
                        break;
                    }
            }
            //if (align)
            //    write.AlignStream(4);
        }
    }
}
