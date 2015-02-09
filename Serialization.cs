using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class Serialization
{
    public static List<PropertyInfo> GetFields(Type t)
    {
        //get all props and KEEP the ones that should be hidden because they might be autoproperties (compiler generated fields)
        List<PropertyInfo> props = ReflectionHelper.GetAllProperties(t, true);
        props.RemoveAll(p => p.GetCustomAttributes(typeof(NonSerializedAttribute), true).Length > 0);

        props.Sort(new Comparison<PropertyInfo>((a, b) => a.Name.CompareTo(b.Name)));
        int counter = 0;
        foreach (var p in props)
        {
            if (!p.PropertyType.IsValueType) counter++;
        }

        if (counter > 32)
            throw new Exception(string.Format("GenericClassProxy<{0}> Exception: More fields defined than allowed! (maximum field count: 31, current field count: {1})", t.Name, props.Count));

        return props;
    }

    public static string GetProxyName(Type type)
    {
        if (type.IsEnum)
        {
            return "EnumProxy<" + type.FullName.Replace('+', '.') + ">";
        }
        else
        {
            return type.Name + "Proxy";
        }
    }

    public static string GetSerializerName(Type type)
    {
        string args;
        return GetSerializerName(type, out args);
    }

    public static bool IsNullable(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static string GetTypeName(Type type)
    {
        if (type.GetGenericArguments().Length == 0)
        {
            return type.FullName;
        }
        var genericArguments = type.GetGenericArguments();
        var typeDefinition = type.FullName;
        var unmangledName = typeDefinition.Substring(0, typeDefinition.IndexOf("`"));
        return unmangledName + "<" + String.Join(",", genericArguments.Select(GetTypeName)) + ">";
    }

    public static string GetSerializerName(Type type, out string arguments)
    {
        arguments = string.Empty;

        if (type.IsEnum)
        {
            return "EnumProxy<" + type.FullName.Replace('+', '.') + ">";
        }
        else if (type.IsArray)
        {
            arguments = ", " + GetProxyName(type.GetElementType()) + ".Serialize";
            return "ListProxy<" + type.GetElementType().FullName.Replace('+', '.') + ">";
        }
        else if (IsNullable(type))
        {
            type = Nullable.GetUnderlyingType(type);
            return type.Name + "Proxy";
        }
        else if (type.IsGenericType)
        {
            Type[] types = type.GetGenericArguments();
            if (types.Length == 1)
            {
                arguments = ", " + GetProxyName(types[0]) + ".Serialize";
                return "ListProxy<" + types[0].FullName.Replace('+', '.') + ">";
            }
            if (types.Length == 2)
            {
                if (type.Name == "KeyCollection")
                {
                    arguments = ", " + GetProxyName(types[0]) + ".Serialize";
                    return "ListProxy<" + types[0].FullName.Replace('+', '.') + ">";
                }
                else if (type.Name == "ValueCollection")
                {
                    arguments = ", " + GetProxyName(types[1]) + ".Serialize";
                    return "ListProxy<" + types[1].FullName.Replace('+', '.') + ">";
                }
                else
                {
                    arguments = ", " + GetProxyName(types[0]) + ".Serialize, " + GetProxyName(types[1]) + ".Serialize";
                    return "DictionaryProxy<" + types[0].FullName.Replace('+', '.') + ", " + types[1].FullName.Replace('+', '.') + ">";
                }
            }
            else
            {
                return type.Name;
            }
        }
        else
        {
            return type.Name + "Proxy";
        }
    }

    public static string GetDeserializerName(Type type, out string arguments)
    {
        arguments = string.Empty;

        if (type.IsEnum)
        {
            return "EnumProxy<" + type.FullName.Replace('+', '.') + ">";
        }
        else if (type.IsArray)
        {
            arguments = ", " + GetProxyName(type.GetElementType()) + ".Deserialize";
            return "ListProxy<" + type.GetElementType().FullName.Replace('+', '.') + ">";
        }
        else if (IsNullable(type))
        {
            type = Nullable.GetUnderlyingType(type);
            return type.Name + "Proxy";
        }
        else if (type.IsGenericType)
        {
            Type[] types = type.GetGenericArguments();
            if (types.Length == 1)
            {
                arguments = ", " + GetProxyName(types[0]) + ".Deserialize";
                return "ListProxy<" + types[0].FullName.Replace('+', '.') + ">";
            }
            if (types.Length == 2)
            {
                if (type.Name == "KeyCollection")
                {
                    arguments = ", " + GetProxyName(types[0]) + ".Deserialize";
                    return "ListProxy<" + types[0].FullName.Replace('+', '.') + ">";
                }
                else if (type.Name == "ValueCollection")
                {
                    arguments = ", " + GetProxyName(types[1]) + ".Deserialize";
                    return "ListProxy<" + types[1].FullName.Replace('+', '.') + ">";
                }
                else
                {
                    arguments = ", " + GetProxyName(types[0]) + ".Deserialize, " + GetProxyName(types[1]) + ".Deserialize";
                    return "DictionaryProxy<" + types[0].FullName.Replace('+', '.') + ", " + types[1].FullName.Replace('+', '.') + ">";
                }
            }
            else
            {
                return type.Name;
            }
        }
        else
        {
            return type.Name + "Proxy";
        }
    }


    //public static string GetProxyName(string type, ParameterKind kind, bool useDeltaCompression = false)
    //{
    //    switch (kind)
    //    {
    //        case ParameterKind.Int32: return "Int32Proxy";
    //        case ParameterKind.String: return "StringProxy";
    //        case ParameterKind.Boolean: return "BooleanProxy";
    //        case ParameterKind.Byte: return "ByteProxy";
    //        case ParameterKind.Single: return "SingleProxy";
    //        case ParameterKind.Int16: return "Int16Proxy";
    //        case ParameterKind.Enum: return "EnumProxy<" + type + ">";
    //        case ParameterKind.List:
    //            {
    //                string[] token = type.Split(new char[] { '<', '>' });
    //                if (useDeltaCompression)
    //                    return "ListProxy<" + token[1] + ">";
    //                else
    //                    return "ListProxy<" + token[1] + ">";
    //            }
    //        case ParameterKind.Dictionary:
    //            {
    //                string[] token = type.Split(new char[] { '<', '>', ',' });
    //                if (useDeltaCompression)
    //                    return "DictionaryProxy<" + token[1] + "," + token[2] + ">";
    //                else
    //                    return "DictionaryProxy<" + token[1] + "," + token[2] + ">";
    //            }
    //        default:
    //            {
    //                if (useDeltaCompression)
    //                    return StripNamespace(type) + "DeltaProxy";
    //                else
    //                    return StripNamespace(type) + "Proxy";
    //            }
    //    }
    //}

    public static string PrintArgument(string type, ParameterKind kind, bool useDeltaCompression, bool useICollection = false)
    {
        if (useDeltaCompression)
        {
            switch (kind)
            {
                case ParameterKind.Byte:
                case ParameterKind.Int16:
                case ParameterKind.Int32:
                case ParameterKind.String:
                case ParameterKind.Boolean:
                case ParameterKind.Single:
                case ParameterKind.Enum: return type;
                case ParameterKind.List:
                    {
                        string[] token = type.Split(new char[] { '<', '>' });
                        return token[0] + "<" + PrintArgument(token[1], TypeHelper.GuessParameterkind(token[1]), useDeltaCompression) + ">";
                    }
                case ParameterKind.Dictionary:
                    {
                        string[] token = type.Split(new char[] { '<', '>', ',' });
                        return token[0] + "<" + PrintArgument(token[1], TypeHelper.GuessParameterkind(token[1]), useDeltaCompression) + ", " + PrintArgument(token[2], TypeHelper.GuessParameterkind(token[2]), useDeltaCompression) + ">";
                    }
                default:
                    {
                        return type + "Delta";
                    }
            }
        }
        else
        {
            if (useICollection && kind == ParameterKind.List)
            {
                string[] token = type.Split(new char[] { '<', '>' });
                return "System.Collections.Generic.ICollection<" + PrintArgument(token[1], TypeHelper.GuessParameterkind(token[1]), useDeltaCompression) + ">";
            }
            else
            {
                return type;
            }
        }
    }


    //public static string GetProxyName(string type, bool useDeltaCompression = false)
    //{
    //    if (type == "int")
    //    {
    //        return "Int32Proxy";
    //    }
    //    else if (type == "float")
    //    {
    //        return "SingleProxy";
    //    }
    //    else if (type == "byte")
    //    {
    //        return "ByteProxy";
    //    }
    //    else if (type == "ushort")
    //    {
    //        return "UInt16Proxy";
    //    }
    //    else if (type == "short")
    //    {
    //        return "Int16Proxy";
    //    }
    //    else if (type == "string")
    //    {
    //        return "StringProxy";
    //    }
    //    else if (type == "bool")
    //    {
    //        return "BooleanProxy";
    //    }
    //    else if (type.Contains("List<"))
    //    {
    //        string[] token = type.Split(new char[] { '<', '>' });
    //        return "ListProxy<" + token[1] + ">";
    //    }
    //    else if (type.Contains("Dictionary<"))
    //    {
    //        string[] token = type.Split(new char[] { '<', '>', ',' });
    //        return "DictionaryProxy<" + token[1] + "," + token[2] + ">";
    //    }
    //    else
    //    {
    //        if (type.EndsWith("Type"))
    //        {
    //            return "EnumProxy<" + type + ">";
    //        }
    //        else if (type.EndsWith("MemberRegistrationResult"))
    //        {
    //            return "EnumProxy<" + type + ">";
    //        }
    //        else if (type.EndsWith("MemberOperationResult"))
    //        {
    //            return "EnumProxy<" + type + ">";
    //        }
    //        else
    //        {
    //            int idx = type.LastIndexOf(".");
    //            if (idx > 0)
    //            {
    //                return type.Substring(idx + 1) + (useDeltaCompression ? "DeltaProxy" : "Proxy");
    //            }
    //            else
    //            {
    //                return type + (useDeltaCompression ? "DeltaProxy" : "Proxy");
    //            }
    //        }
    //    }
    //}

    public static string GetDeserializeArguments(string type, bool useDeltaCompression = false)
    {
        var kind = TypeHelper.GuessParameterkind(type);

        if (kind == ParameterKind.List)
        {
            string[] token = TypeHelper.GetGenericArgs(type);
            return ", " + TypeHelper.GetProxyType(token[1], useDeltaCompression) + ".Deserialize";
        }
        else if (kind == ParameterKind.Dictionary)
        {
            string[] token = TypeHelper.GetGenericArgs(type);
            return ", " + TypeHelper.GetProxyType(token[1], useDeltaCompression) + ".Deserialize, " + TypeHelper.GetProxyType(token[2], useDeltaCompression) + ".Deserialize";
        }
        else
        {
            return string.Empty;
        }
    }

    public static string GetSerializeArguments(string type, bool useDeltaCompression = false)
    {
        var kind = TypeHelper.GuessParameterkind(type);

        if (kind == ParameterKind.List)
        {
            string[] token = TypeHelper.GetGenericArgs(type);
            return ", " + TypeHelper.GetProxyType(token[1], useDeltaCompression) + ".Serialize";
        }
        else if (kind == ParameterKind.Dictionary)
        {
            string[] token = TypeHelper.GetGenericArgs(type);
            return ", " + TypeHelper.GetProxyType(token[1], useDeltaCompression) + ".Serialize, " + TypeHelper.GetProxyType(token[2]) + ".Serialize";
        }
        else
        {
            return string.Empty;
        }
    }
}