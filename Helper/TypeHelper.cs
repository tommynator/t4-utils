
public static class TypeHelper
{
    public static ParameterKind GuessParameterkind(string type)
    {
        if (type == "int")
        {
            return ParameterKind.Int32;
        }
        else if (type == "ushort")
        {
            return ParameterKind.UInt16;
        }
        else if (type == "float")
        {
            return ParameterKind.Single;
        }
        else if (type == "byte")
        {
            return ParameterKind.Byte;
        }
        else if (type == "short")
        {
            return ParameterKind.Int16;
        }
        else if (type == "string")
        {
            return ParameterKind.String;
        }
        else if (type == "bool")
        {
            return ParameterKind.Boolean;
        }
        else if (type.Contains("List<"))
        {
            return ParameterKind.List;
        }
        else if (type.Contains("Dictionary<"))
        {
            return ParameterKind.Dictionary;
        }
        else if (type.EndsWith("Type"))
        {
            return ParameterKind.Enum;
        }
        else
        {
            return ParameterKind.Class;
        }
    }

    public static string GetProxyType(string type, bool useDeltaCompression = false)
    {
        return GetProxyType(type, GuessParameterkind(type), useDeltaCompression);
    }

    public static string GetProxyType(string type, ParameterKind kind, bool useDeltaCompression = false)
    {
        switch (kind)
        {
            case ParameterKind.Int32:
            case ParameterKind.Single:
            case ParameterKind.Byte:
            case ParameterKind.UInt16:
            case ParameterKind.Int16:
            case ParameterKind.String:
            case ParameterKind.Boolean:
                return kind.ToString() + "Proxy";
            case ParameterKind.Enum:
                return "EnumProxy<" + type + ">";
            case ParameterKind.List:
                {
                    string[] token = GetGenericArgs(type);
                    return "ListProxy<" + token[1] + ">";
                }
            case ParameterKind.Dictionary:
                {
                    string[] token = GetGenericArgs(type);
                    return "DictionaryProxy<" + token[1] + "," + token[2] + ">";
                }
        }

        //here it's getting wacky - fix this!
        if (type.EndsWith("Type") || type.EndsWith("MemberRegistrationResult") || type.EndsWith("MemberOperationResult"))
        {
            return "EnumProxy<" + type + ">";
        }
        else
        {
            if (useDeltaCompression)
                return StripNamespace(type) + "DeltaProxy";
            else
                return StripNamespace(type) + "Proxy";

            ////what does that mean?
            //int idx = type.LastIndexOf(".");
            //if (idx > 0)
            //{
            //    return type.Substring(idx + 1) + (useDeltaCompression ? "DeltaProxy" : "Proxy");
            //}
            //else
            //{
            //    return type + (useDeltaCompression ? "DeltaProxy" : "Proxy");
            //}
        }
    }

    public static string[] GetGenericArgs(string type)
    {
        return type.Split(new char[] { '<', '>', ',' });
    }

    public static string StripNamespace(string name)
    {
        int idx = name.LastIndexOf(".");
        return idx > 0 ? name.Substring(idx + 1) : name;
    }

    public static string PrintArguments(NetworkMethod method)
    {
        string arg = string.Empty;
        for (int i = 0; i < method.Arguments.Count && i < method.ArgumentKinds.Count; i++)
        {
            arg += Serialization.PrintArgument(method.Arguments[i].Key, method.ArgumentKinds[i], false, true) + " " + method.Arguments[i].Value + ", ";
        }

        return method.Arguments.Count > 0 ? arg.Trim(", ".ToCharArray()) : arg.Trim(", ".ToCharArray());
    }
}