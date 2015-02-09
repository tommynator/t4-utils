using System;
using System.Collections.Generic;

public static class WebServiceAttributeParser
{
    private const string WebServiceAttribute = "CmuneWebServiceInterface";
    private const string EncryptionAttribute = "DontEncryptMethodAttribute";
    private const string SerializationProperty = "UseBinaryProtocol";

    public static List<InterfaceProperties> GetProjectInterfaces(IServiceProvider hostServiceProvider, string projectName)
    {
        return GetProjectInterfaces(EnvDteUtils.GetProjectWithName(hostServiceProvider, projectName));
    }

    public static List<InterfaceProperties> GetProjectInterfaces(EnvDTE.Project project)
    {
        List<InterfaceProperties> interfaces = new List<InterfaceProperties>();

        try
        {
            List<EnvDTE.ProjectItem> files = EnvDteUtils.GetAllScripts(project.ProjectItems);

            //find all classes
            List<EnvDTE.CodeElement> classes = new List<EnvDTE.CodeElement>();
            foreach (EnvDTE.ProjectItem f in files)
            {
                classes.AddRange(EnvDteUtils.GetAllInterfaces(f.FileCodeModel.CodeElements));
            }

            //now handle all room operations
            foreach (EnvDTE.CodeElement e in classes)
            {
                if (IsWebServiceInterface(e))
                {
                    interfaces.Add(GetInterfaceProperties(e));
                }
            }

            //make sure we have a unique name for each method
            foreach (var intfc in interfaces)
            {
                foreach (var method in intfc.Methods)
                {
                    if (string.IsNullOrEmpty(method.Suffix))
                    {
                        List<MethodProperties> l = intfc.Methods.FindAll(m => m.Name.Equals(method.Name));
                        if (l.Count > 1)
                        {
                            for (int i = 0; i < l.Count; i++)
                            {
                                l[i].Suffix = "_" + (i + 1);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            T4Utils.Print(ex.GetType() + ": " + ex.Message);
            T4Utils.Print(ex.StackTrace);
            throw;
        }

        return interfaces;
    }

    public static InterfaceProperties GetInterfaceProperties(EnvDTE.CodeElement e)
    {
        InterfaceProperties inter = new InterfaceProperties(e.Name, e.FullName);

        inter.UseBinarySerialization = IsWebServiceBinarySerializationEnabled(e);
        List<EnvDTE.CodeFunction> functions = EnvDteUtils.GetAllFunctions(e);

        foreach (EnvDTE.CodeFunction fu in functions)
        {
            List<ParameterKind> argKind = new List<ParameterKind>();
            List<KeyValuePair<string, string>> arguments = new List<KeyValuePair<string, string>>();
            foreach (EnvDTE.CodeParameter p in fu.Parameters)
            {
                arguments.Add(new KeyValuePair<string, string>(p.Type.AsString, p.Name));
                argKind.Add(EnvDteUtils.GetParameterKind(p.Type));
            }
            if (fu.Type.AsString != "byte[]")
            {
                inter.Methods.Add(new MethodProperties()
                {
                    Name = fu.Name,
                    ReturnType = fu.Type.AsString,
                    ReturnKind = EnvDteUtils.GetParameterKind(fu.Type),
                    ObsoleteMessage = ObsoleteMessage(fu),
                    IsObsolete = IsObsolete(fu),
                    Arguments = arguments,
                    ArgumentKinds = argKind,
                    EnableEncryption = IsWebServiceEncryptionEnabled(fu)
                });
            }
            else
            {
                T4Utils.Comment("Ignored : " + inter.Name + "." + fu.Name + " with return type " + fu.Type.AsString);
            }
        }

        return inter;
    }

    private static bool IsWebServiceEncryptionEnabled(EnvDTE.CodeFunction e)
    {
        foreach (EnvDTE.CodeElement att in e.Children)
        {
            if (EnvDteUtils.IsAttribute(att) && att.Name == EncryptionAttribute)
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsWebServiceBinarySerializationEnabled(EnvDTE.CodeElement e)
    {
        foreach (EnvDTE.CodeElement att in e.Children)
        {
            if (EnvDteUtils.IsAttribute(att) && att.Name == WebServiceAttribute)
            {
                var attribute = (EnvDTE.CodeAttribute)att;
                if (attribute.Value.Contains(SerializationProperty) && attribute.Value.Contains("false"))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static bool IsObsolete(EnvDTE.CodeFunction e)
    {
        foreach (EnvDTE.CodeElement att in e.Children)
        {
            if (EnvDteUtils.IsAttribute(att) && att.Name.EndsWith("Obsolete"))
            {
                return true;
            }
        }
        return false;
    }

    private static string ObsoleteMessage(EnvDTE.CodeFunction e)
    {
        foreach (EnvDTE.CodeElement att in e.Children)
        {
            if (EnvDteUtils.IsAttribute(att) && att.Name.EndsWith("Obsolete"))
            {
                var attribute = (EnvDTE.CodeAttribute)att;
                return attribute.Value;
            }
        }
        return string.Empty;
    }

    private static bool IsWebServiceInterface(EnvDTE.CodeElement e)
    {
        return EnvDteUtils.HasAttributeWithName(e, WebServiceAttribute);
    }
}