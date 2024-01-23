using System;
using System.Reflection;

namespace BedroomPartyLeaderboard.Utils
{
    public static class ObjectExtensions
    {
        public static T Get<T>(this object obj, string fieldName)
        {
            return (T)obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
        }

        public static bool TryGet<T>(this object obj, string fieldName, out T val)
        {
            FieldInfo f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f == null)
            {
                val = default;
                return false;
            }
            val = (T)f.GetValue(obj);
            return true;
        }

        public static bool Set(this object obj, string fieldName, object value)
        {
            FieldInfo f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f == null)
            {
                return false;
            }

            f.SetValue(obj, value);
            return true;
        }

        public static bool SetProperty(this object obj, string propertyName, object value)
        {
            PropertyInfo p = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (p == null)
            {
                return false;
            }
            p.SetValue(obj, value, null);
            return true;
        }
    }
}
