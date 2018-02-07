using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Server.Models
{
    public class ArrayWrapper
    {
        private const string separator = "FUCKTHISSHIT";

        public string ArrayString;

        public T[] GetArray<T>()
        {
            List<T> ret = new List<T>();

            foreach (var item in ArrayString.Split(new string[] { separator }, StringSplitOptions.None))
            {
                ret.Add(JsonUtility.FromJson<T>(item));
            }

            return ret.ToArray();
        }

        public void SetArray<T>(T[] array)
        {
            string val = "";
            foreach(var o in array)
            {
                val += JsonUtility.ToJson(o);
                val += separator;
            }
            val = val.Substring(0, val.Length - separator.Length);
            ArrayString = val;
        }
    }
}
