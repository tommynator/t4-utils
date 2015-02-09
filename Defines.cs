
public enum ParameterKind
{
    Int16,
    Int32,
    Int64,
    Single,
    String,
    Boolean,
    Byte,
    Decimal,
    Enum,
    Class,
    Struct,
    List,
    Dictionary,
    Array,
    Void,
    UInt16,
}

public static class ParameterUtils
{
    public static bool IsValueType(ParameterKind kind)
    {
        switch (kind)
        {
            case ParameterKind.Boolean:
            case ParameterKind.Int16:
            case ParameterKind.Int32:
            case ParameterKind.Int64:
            case ParameterKind.Single:
            case ParameterKind.String:
            case ParameterKind.Byte:
            case ParameterKind.Decimal:
            case ParameterKind.Enum:
            case ParameterKind.Struct:
                return true;
            default:
                return false;
        }
    }
}