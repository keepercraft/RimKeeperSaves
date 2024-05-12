using RimWorld.BaseGen;
using System;
using System.IO;
using System.Reflection;
using Verse;

namespace Keepercraft.RimKeeperSaves.Extensions
{
    public static class GenericExtension
    {
        public static void SetPrivateField(this object obj, string fieldName, object value)
        {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
            else
            {
                Log.Error("[RimKeeperSaves] SetPrivateField:" + fieldName);
            }
        }

        public static T GetPrivateField<T>(this object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                return (T)fieldInfo.GetValue(obj);
            }
            else
            {
                Log.Error("[RimKeeperSaves] SetPrivateField:" + fieldName);
                return default;
            }
        }

        public static bool IsSaveFile(this string path)
        {
            return path.EndsWith(".rws");
        }
    }
}
