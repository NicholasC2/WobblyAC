using System;
using System.Collections.Generic;
using System.Reflection;

public static class FieldUtility
{
    private static Dictionary<Type, MemberInfo[]> cache =
        new Dictionary<Type, MemberInfo[]>();

    public static EditableField[] GetFields(object obj)
    {
        if (obj == null)
            return Array.Empty<EditableField>();

        Type type = obj.GetType();

        if (!cache.TryGetValue(type, out var members))
        {
            List<MemberInfo> list = new List<MemberInfo>();

            BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            list.AddRange(type.GetFields(flags));

            foreach (var prop in type.GetProperties(flags))
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if (prop.GetIndexParameters().Length > 0)
                    continue;

                if (prop.DeclaringType == typeof(UnityEngine.Object))
                    continue;

                list.Add(prop);
            }

            members = list.ToArray();
            cache[type] = members;
        }

        List<EditableField> result = new List<EditableField>();

        foreach (var member in members)
        {
            result.Add(new EditableField(member.Name, obj, member));
        }

        return result.ToArray();
    }
}