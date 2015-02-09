using System.Collections.Generic;

public class MethodProperties
{
    public string ReturnType { get; set; }
    public string Name { get; set; }
    public string Suffix { get; set; }
    public string UniqueName { get { return Name + Suffix; } }
    public bool EnableEncryption { get; set; }
    public ParameterKind ReturnKind { get; set; }
    public List<KeyValuePair<string, string>> Arguments { get; set; }
    public List<ParameterKind> ArgumentKinds { get; set; }
    public string ObsoleteMessage { get; set; }
    public bool IsObsolete { get; set; }

    public string GetObsoleteHeader()
    {
        if (string.IsNullOrEmpty(ObsoleteMessage))
            return "[System.Obsolete(\"\")]";
        else
            return string.Format("[System.Obsolete({0})]", ObsoleteMessage);
    }

    public string DebugArguments()
    {
        if (Arguments.Count > 0)
        {
            var debugFormat = new System.Text.StringBuilder();
            var debugArgs = new System.Text.StringBuilder();

            for (int i = 0; i < Arguments.Count; i++)
            {
                debugFormat.Append(Arguments[i].Value + "={" + i + "}" + (i == Arguments.Count - 1 ? "" : "&"));

                if (ParameterUtils.IsValueType(ArgumentKinds[i]))
                    debugArgs.Append((i < Arguments.Count ? ", " : "") + "_" + Arguments[i].Value);
                else
                    debugArgs.Append((i < Arguments.Count ? ", " : "") + "(_" + Arguments[i].Value + "!=null ? " + "_" + Arguments[i].Value + ".ToString()" + " : \"null\")");
            }

            return "string.Format(\"" + debugFormat.ToString() + "\"" + debugArgs.ToString() + ")";
        }
        else
        {
            return "\"\"";
        }
    }

    public bool IsVoid
    {
        get { return ReturnKind == ParameterKind.Void; }
    }

    public string PrintArgumentDeclaration(bool appendColon = false)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < Arguments.Count; i++)
        {
            builder.Append(Arguments[i].Key + " " + Arguments[i].Value + (appendColon || i < Arguments.Count - 1 ? ", " : ""));
        }
        return builder.ToString();
    }

    public string PrintArgumentCall(bool appendColon = false)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < Arguments.Count; i++)
        {
            builder.Append(Arguments[i].Value + (appendColon || i < Arguments.Count - 1 ? ", " : ""));
        }
        return builder.ToString();
    }
}