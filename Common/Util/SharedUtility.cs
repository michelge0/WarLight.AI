﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WarLight.AI
{
    public static class SharedUtility
    {
        public static TO As<TO>(this object o)
        {
            return (TO)o;
        }

        public static void AddTo<T>(this Dictionary<T, int> a, T key, int sumToAdd)
        {
            if (a.ContainsKey(key))
                a[key] += sumToAdd;
            else
                a.Add(key, sumToAdd);
        }

        public static string RemoveFromStartOfString(this string s, string toRemove, bool canLogContents = false)
        {
            if (!s.StartsWith(toRemove))
                throw new Exception(canLogContents ? "\"" + s + "\" does not start with \"" + toRemove + "\"" : "Does not start with");
            return s.Substring(toRemove.Length);
        }

        public static string RemoveFromEndOfString(this string s, string toRemove, bool canLogContents = false)
        {
            if (!s.EndsWith(toRemove))
                throw new Exception(canLogContents ? "\"" + s + "\" does not end with \"" + toRemove + "\"" : "Does not end with");
            return s.Substring(0, s.Length - toRemove.Length);
        }

        public static void RemoveAll<T>(this ICollection<T> col, IEnumerable<T> rem)
        {
            foreach (var r in rem)
                col.Remove(r);
        }
        public static bool IsEmpty<T>(this ICollection<T> col)
        {
            return (col.Count == 0);
        }

        public static bool IsEmpty<T>(this Stack<T> col)
        {
            return (col.Count == 0);
        }
        public static bool None<T>(this IEnumerable<T> a)
        {
            return !a.Any();
        }
        public static void AddRange<T>(this HashSet<T> hash, IEnumerable<T> add)
        {
            foreach (var a in add)
                hash.Add(a);
        }
        public static bool None<T>(this IEnumerable<T> a, Func<T, bool> match)
        {
            return !a.Any(match);
        }
        public static int RoundF(this float p, bool roundUpOnHalf = true)
        {
            var i = (int)p;
            if (p - i > 0.5)
                return i + 1;
            else
                if (roundUpOnHalf && p - i == 0.5)
                    return i + 1;
                else
                    return i;
        }

        public static int Round(this double p, bool roundUpOnHalf = true)
        {
            var i = (int)p;
            if (p - i > 0.5)
                return i + 1;
            else
                if (roundUpOnHalf && p - i == 0.5)
                    return i + 1;
                else
                    return i;
        }
        public static int Ceiling(double d)
        {
            var c = (int)d;
            if (c == d)
                return c;
            else
                return c + 1;
        }
        public static string JoinToStrings<T>(this IEnumerable<T> a, string seperator = "")
        {
            return string.Join(seperator, a.Select(o => o.ToString()).ToArray());
        }

        public static string JoinStrings(this IEnumerable<string> a, string seperator = "")
        {
            return string.Join(seperator, a.ToArray());
        }
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> array, bool throwOnDuplicate)
        {
            var hs = new HashSet<T>();
            foreach (var t in array)
            {
                Assert.Fatal(!throwOnDuplicate || !hs.Contains(t), "Duplicate key");

                hs.Add(t);
            }
            return hs;
        }


        /// <summary>
        /// Remove exactly one instance of the passed item from the passed list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="remove"></param>
        /// <remarks>
        /// This is different from Remove() since Remove() will remove as many as possible that match.  RemoveOne also throws a KeyNotFoundException if the item is not found
        /// </remarks>
        public static void RemoveOne<T>(this List<T> array, T remove)
        {
            var index = array.IndexOf(remove);
            if (index < 0)
                throw new KeyNotFoundException();
            array.RemoveAt(index);
        }

        public static IEnumerable<T> ExceptOne<T>(this IEnumerable<T> array, T item)
        {
            return array.Except(new T[] { item });
        }
        public static IEnumerable<T> ConcatOne<T>(this IEnumerable<T> array, T item)
        {
            return array.Concat(new T[] { item });
        }
        public static void RemoveWhere<T>(this List<T> list, Func<T, bool> pred)
        {
            for (int i = 0; i < list.Count; i++)
                if (pred(list[i]))
                {
                    list.RemoveAt(i);
                    i--;
                }
        }
        public static int ValueOrZero<T>(this Dictionary<T, int> a, T key)
        {
            int i;
            if (a.TryGetValue(key, out i))
                return i;
            else
                return 0;
        }
        /// <summary>
        /// This is needed for haXe, since it does not have an integer max function.  It also can't be called MaxInt since there's already one of those for Linq overload resolution.
        /// </summary>
        public static int MathMax(int i1, int i2)
        {
            if (i1 > i2)
                return i1;
            else
                return i2;
        }

        public static int MathMin(int i1, int i2)
        {
            if (i1 < i2)
                return i1;
            else
                return i2;
        }

        /// <summary>
        /// Most comparisons work just with f - s, however ActionScript only works when -1, 0 and 1 are returned, so define this helper function
        /// </summary>
        public static int CompareInts(int f, int s)
        {
            return f.CompareTo(s);
        }
        public static void ForEach<T>(this IEnumerable<T> a, Action<T> action)
        {
            foreach (T t in a)
                action(t);
        }
    }

}
