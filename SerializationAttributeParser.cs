using System;
using System.Collections.Generic;

public static class SerializationAttributeParser
{
    private const string SerializationAttribute = "Serializable";

    public static ProjectModels CreateModelSerialization(IServiceProvider hostServiceProvider, string projectName)
    {
        return GetAllModelsOfProject(EnvDteUtils.GetProjectWithName(hostServiceProvider, projectName));
    }

    private static ProjectModels GetAllModelsOfProject(EnvDTE.Project project)
    {
        ProjectModels metaData = new ProjectModels();

        try
        {
            T4Utils.Comment("" + project.FullName);

            List<EnvDTE.ProjectItem> files = EnvDteUtils.GetAllScripts(project.ProjectItems);
            foreach (EnvDTE.ProjectItem f in files)
            {
                List<EnvDTE.CodeElement> elements = GetSerializableModels(f.FileCodeModel.CodeElements);
                foreach (EnvDTE.CodeElement e in elements)
                {
                    if (EnvDteUtils.IsClass(e))
                    {
                        metaData.Classes.Add(new ModelProperties(e.Name));
                    }
                    else if (EnvDteUtils.IsStruct(e))
                    {
                        metaData.Structs.Add(new ModelProperties(e.Name));
                    }
                    else if (EnvDteUtils.IsEnum(e))
                    {
                        EnvDTE.CodeEnum en = e as EnvDTE.CodeEnum;
                        if (en != null)
                        {
                            string type = "int";//en.Bases.Count.ToString();
                            //if(en.get_IsDerivedFrom("byte")) type = "byte";
                            metaData.Enums.Add(new EnumProperties(e.Name, type));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            T4Utils.Print(ex.GetType() + ": " + ex.Message);
            throw;
        }

        return metaData;
    }

    private static bool IsSerializable(EnvDTE.CodeElement e)
    {
        if (EnvDteUtils.IsEnum(e))
        {
            return true;
        }
        else if (EnvDteUtils.IsClass(e))
        {
            foreach (EnvDTE.CodeElement att in e.Children)
            {
                if (EnvDteUtils.IsAttribute(att) && att.Name.EndsWith(SerializationAttribute))
                    return true;
            }
        }
        return false;
    }

    private static List<EnvDTE.CodeElement> GetSerializableModels(EnvDTE.CodeElements elements)
    {
        List<EnvDTE.CodeElement> l = new List<EnvDTE.CodeElement>();
        foreach (EnvDTE.CodeElement p in elements)
        {
            if (IsSerializable(p))
                l.Add(p);

            if (p.Children.Count > 0)
            {
                l.AddRange(GetSerializableModels(p.Children));
            }
        }
        return l;
    }
}