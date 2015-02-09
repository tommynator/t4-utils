using System;
using System.Collections.Generic;
using System.Reflection;
using Cmune.Core.Types;

internal static class ReflectionHelper
{
    public const BindingFlags FieldBinder = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
    public const BindingFlags InvokeBinder = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod;

    /// <summary>
    /// Get the methods indicated by the attribute, defined by the generic basetype of this class.
    /// This procedure makes sure to retrieve even private members of base classes that are not naturally
    /// inherited by the derived class.
    /// </summary>
    /// <returns></returns>
    public static List<FieldInfo> GetAllFields(Type type, bool inherited)
    {
        List<FieldInfo> list = new List<FieldInfo>();

        while (type != typeof(object))
        {
            //private fields and methods of a base class are never accessible by reflection in a derived class.
            //to get them anyway we iterate recursivle through the derivation hierarchy and
            //perform the GetFields for each class separately
            FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            list.AddRange(fields);

            //if we don't want to get all inherited fields too we break here
            if (!inherited) break;

            type = type.BaseType;
        }

        list.Sort((p, q) => p.Name.CompareTo(q.Name));

        return list;
    }

    /// <summary>
    /// Get the methods indicated by the attribute, defined by the generic basetype of this class.
    /// This procedure makes sure to retrieve even private members of base classes that are not naturally
    /// inherited by the derived class.
    /// </summary>
    /// <returns></returns>
    public static List<PropertyInfo> GetAllProperties(Type type, bool inherited)
    {
        List<PropertyInfo> list = new List<PropertyInfo>();

        while (type != typeof(object))
        {
            //private fields and methods of a base class are never accessible by reflection in a derived class.
            //to get them anyway we iterate recursivle through the derivation hierarchy and
            //perform the GetFields for each class separately
            PropertyInfo[] props = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            list.AddRange(props);

            //if we don't want to get all inherited fields too we break here
            if (!inherited) break;

            type = type.BaseType;
        }

        list.Sort((p, q) => p.Name.CompareTo(q.Name));

        return list;
    }


    /// <summary>
    /// Use reflection top create an index table of all network methods and their local IDs.
    /// Network methods are indicated by the attribute, defined by the generic basetype of this class.
    /// </summary>
    public static List<MethodInfo> GetAllMethods(Type type, bool inherited)
    {
        List<MethodInfo> list = new List<MethodInfo>();

        while (type != typeof(object))
        {
            //private fields and methods of a base class are never accessible by reflection in a derived class.
            //to get them anyway we iterate recursivle through the derivation hierarchy and
            //perform the GetFields for each class separately
            MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            list.AddRange(methods);

            //if we don't want to get all inherited fields too we break here
            if (!inherited) break;

            type = type.BaseType;
        }

        list.Sort((p, q) => p.Name.CompareTo(q.Name));

        return list;
    }

    public static void FilterByAttribute<T>(Type attribute, List<T> members) where T : MemberInfo
    {
        members.RemoveAll(m => m.GetCustomAttributes(attribute, false).Length == 0);
    }

    public static List<MemberInfoMethod<T>> GetMethodsWithAttribute<T>(Type type, bool inherited)
    {
        return GetMethodsWithAttribute<T>(GetAllMethods(type, inherited));
    }

    public static List<MemberInfoMethod<T>> GetMethodsWithAttribute<T>(List<MethodInfo> members)
    {
        List<MemberInfoMethod<T>> list = new List<MemberInfoMethod<T>>(members.Count);
        foreach (var m in members)
        {
            T[] atts = m.GetCustomAttributes(typeof(T), false) as T[];
            if (atts != null && atts.Length > 0)
            {
                list.Add(new MemberInfoMethod<T>(m, atts[0]));
            }
        }
        return list;
    }

    public static List<MemberInfoField<T>> GetFieldsWithAttribute<T>(Type type, bool inherited)
    {
        return GetFieldsWithAttribute<T>(GetAllFields(type, inherited));
    }

    public static List<MemberInfoField<T>> GetFieldsWithAttribute<T>(List<FieldInfo> members)
    {
        List<MemberInfoField<T>> list = new List<MemberInfoField<T>>(members.Count);
        foreach (var m in members)
        {
            T[] atts = m.GetCustomAttributes(typeof(T), false) as T[];
            if (atts != null && atts.Length > 0)
            {
                list.Add(new MemberInfoField<T>(m, atts[0]));
            }
        }
        return list;
    }

    public static List<MemberInfoProperty<T>> GetPropertiesWithAttribute<T>(Type type, bool inherited)
    {
        return GetPropertiesWithAttribute<T>(GetAllProperties(type, inherited));
    }

    public static List<MemberInfoProperty<T>> GetPropertiesWithAttribute<T>(List<PropertyInfo> members)
    {
        List<MemberInfoProperty<T>> list = new List<MemberInfoProperty<T>>(members.Count);
        foreach (var m in members)
        {
            T[] atts = m.GetCustomAttributes(typeof(T), false) as T[];
            if (atts != null && atts.Length > 0)
            {
                list.Add(new MemberInfoProperty<T>(m, atts[0]));
            }
        }
        return list;
    }

    public static MethodInfo GetMethodWithParameters(List<MethodInfo> members, string name, params Type[] args)
    {
        MethodInfo info = null;
        foreach (var i in members.FindAll(m => m.Name == name))
        {
            bool correct = true;

            ParameterInfo[] param = i.GetParameters();
            if (param.Length == args.Length)
            {
                for (int j = 0; j < param.Length; j++)
                {
                    correct &= param[j].ParameterType == args[j];
                }
            }

            if (correct) info = i;
        }
        return info;
    }

}