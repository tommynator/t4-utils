using System;
using System.Collections.Generic;

public static class EnvDteUtils
{
    public static ParameterKind GetParameterKind(EnvDTE.CodeTypeRef type)
    {
        switch (type.TypeKind)
        {
            case EnvDTE.vsCMTypeRef.vsCMTypeRefCodeType:
                {
                    if (type.CodeType.Kind == EnvDTE.vsCMElement.vsCMElementEnum)
                        return ParameterKind.Enum;
                    else if (type.CodeType.Kind == EnvDTE.vsCMElement.vsCMElementStruct)
                        return ParameterKind.Struct;
                    else if (type.CodeType.Kind == EnvDTE.vsCMElement.vsCMElementClass)
                    {
                        if (type.CodeType.Name == "List") return ParameterKind.List;
                        else if (type.CodeType.Name == "Dictionary") return ParameterKind.Dictionary;
                        else return ParameterKind.Class;
                    }
                    else
                        throw new System.Exception("GetParameterKind failed with CodeType: " + type.CodeType.Kind + " -> " + type.CodeType.Name);
                }
            case EnvDTE.vsCMTypeRef.vsCMTypeRefString: return ParameterKind.String;
            case EnvDTE.vsCMTypeRef.vsCMTypeRefByte: return ParameterKind.Byte;
            case EnvDTE.vsCMTypeRef.vsCMTypeRefShort: return ParameterKind.Int16;
            case EnvDTE.vsCMTypeRef.vsCMTypeRefInt: return ParameterKind.Int32;
            case EnvDTE.vsCMTypeRef.vsCMTypeRefLong: return ParameterKind.Int64;
            case EnvDTE.vsCMTypeRef.vsCMTypeRefFloat: return ParameterKind.Single;
            case EnvDTE.vsCMTypeRef.vsCMTypeRefDecimal: return ParameterKind.Decimal;
            case EnvDTE.vsCMTypeRef.vsCMTypeRefBool: return ParameterKind.Boolean;
            case EnvDTE.vsCMTypeRef.vsCMTypeRefArray: return ParameterKind.Array;
            case EnvDTE.vsCMTypeRef.vsCMTypeRefVoid: return ParameterKind.Void;
            default:
                {
                    if ((int)type.TypeKind == 18)
                        return ParameterKind.UInt16;
                    else
                        throw new System.Exception("GetParameterKind failed with: " + type.TypeKind + " -> " + type.AsFullName);
                }
        }
    }

    public static EnvDTE.Project GetProjectContainingFile(IServiceProvider hostServiceProvider, string item)
    {
        EnvDTE.DTE dte = (EnvDTE.DTE)hostServiceProvider.GetService(typeof(EnvDTE.DTE));

        var projectItem = dte.Solution.FindProjectItem(item);
        if (projectItem == null) throw new Exception(string.Format("Project containing file '{0}' not found!", item));

        return projectItem.ContainingProject;
    }

    public static EnvDTE.Project GetProjectWithName(IServiceProvider hostServiceProvider, string projectName)
    {
        EnvDTE.DTE dte = (EnvDTE.DTE)hostServiceProvider.GetService(typeof(EnvDTE.DTE));

        var project = GetProjectWithName(dte, projectName);
        if (project == null) throw new Exception(string.Format("Project with name '{0}' not found!", projectName));

        return project;
    }

    private static EnvDTE.Project GetProjectWithName(EnvDTE.DTE dte, string name)
    {
        foreach (EnvDTE.Project p in dte.Solution.Projects)
        {
            if (IsSolutionItem(p))
            {
                foreach (EnvDTE.ProjectItem item in p.ProjectItems)
                {
                    if (string.Equals(item.Name, name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return item.SubProject;
                    }
                }
            }
            else if (IsProjectItem(p))
            {
                if (string.Equals(p.Name, name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return p;
                }
            }
        }

        return null;
    }

    public static List<EnvDTE.CodeFunction> GetAllFunctions(EnvDTE.CodeElement element)
    {
        List<EnvDTE.CodeFunction> functions = new List<EnvDTE.CodeFunction>();

        foreach (EnvDTE.CodeElement f in element.Children)
        {
            if (IsFunction(f))
            {
                functions.Add(f as EnvDTE.CodeFunction);
            }
        }

        return functions;
    }

    public static List<EnvDTE.CodeElement> GetAllInterfaces(EnvDTE.CodeElements elements)
    {
        List<EnvDTE.CodeElement> interfaces = new List<EnvDTE.CodeElement>();
        foreach (EnvDTE.CodeElement i in elements)
        {
            if (IsInterface(i))
            {
                interfaces.Add(i);
            }

            if (i.Children.Count > 0)
            {
                interfaces.AddRange(GetAllInterfaces(i.Children));
            }
        }
        return interfaces;
    }

    public static List<EnvDTE.ProjectItem> GetAllScripts(EnvDTE.ProjectItems projectItems)
    {
        List<EnvDTE.ProjectItem> scripts = new List<EnvDTE.ProjectItem>();
        foreach (EnvDTE.ProjectItem item in projectItems)
        {
            if (item.ProjectItems.Count > 0)
            {
                scripts.AddRange(GetAllScripts(item.ProjectItems));
            }
            else
            {
                if (IsCodeItem(item))
                {
                    scripts.Add(item);
                }
            }
        }
        return scripts;
    }

    public static bool HasAttributeWithName(EnvDTE.CodeElement e, string name)
    {
        if (IsInterface(e))
        {
            foreach (EnvDTE.CodeElement att in e.Children)
            {
                if (IsAttribute(att) && att.Name == name)
                    return true;
            }
        }

        return false;
    }


    public static bool IsClass(EnvDTE.CodeElement e)
    {
        return e != null && e.Kind == EnvDTE.vsCMElement.vsCMElementClass;
    }

    public static bool IsFunction(EnvDTE.CodeElement e)
    {
        return e != null && e.Kind == EnvDTE.vsCMElement.vsCMElementFunction;
    }

    public static bool IsEnum(EnvDTE.CodeElement e)
    {
        return e != null && e.Kind == EnvDTE.vsCMElement.vsCMElementEnum;
    }

    public static bool IsStruct(EnvDTE.CodeElement e)
    {
        return e != null && e.Kind == EnvDTE.vsCMElement.vsCMElementStruct;
    }

    public static bool IsInterface(EnvDTE.CodeElement e)
    {
        return e != null && e.Kind == EnvDTE.vsCMElement.vsCMElementInterface;
    }

    public static bool IsAttribute(EnvDTE.CodeElement e)
    {
        return e != null && e.Kind == EnvDTE.vsCMElement.vsCMElementAttribute;
    }


    public static bool IsCodeItem(EnvDTE.ProjectItem item)
    {
        return item.Kind == "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}" && (int)item.Properties.Item(6).Value == 1;
    }


    public static bool IsSolutionItem(EnvDTE.Project item)
    {
        return item.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
    }

    public static bool IsProjectItem(EnvDTE.Project item)
    {
        return item.Kind == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
    }
}