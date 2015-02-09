using System.Collections.Generic;

public class NetworkMethod
{
    public int MethodId { get; set; }
    public string Name { get; private set; }
    public List<KeyValuePair<string, string>> Arguments { get; set; }
    public List<ParameterKind> ArgumentKinds { get; set; }
    public bool SendUnreliable { get; set; }
    public bool SendEncrypted { get; set; }

    public NetworkMethod(string name, int methodId)
    {
        Name = name;
        MethodId = methodId;
    }

    public string NamePrint(string prefix = "")
    {
        string arg = string.Empty;
        foreach (var v in Arguments)
        {
            arg += v.Value + ", ";
        }
        return Arguments.Count > 0 ? prefix + arg.Trim(", ".ToCharArray()) : arg.Trim(", ".ToCharArray());
    }

    public string TypePrint()
    {
        string arg = string.Empty;
        foreach (var v in Arguments)
        {
            arg += v.Key + ", ";
        }
        return arg.Trim(", ".ToCharArray());
    }

    public string ArgPrint(string prefix = "", bool useDeltaCompression = false)
    {
        string arg = string.Empty;
        for (int i = 0; i < Arguments.Count && i < ArgumentKinds.Count; i++)
        {
            arg += Serialization.PrintArgument(Arguments[i].Key, ArgumentKinds[i], useDeltaCompression) + " " + Arguments[i].Value + ", ";
        }
        return Arguments.Count > 0 ? prefix + arg.Trim(", ".ToCharArray()) : arg.Trim(", ".ToCharArray());
    }
}