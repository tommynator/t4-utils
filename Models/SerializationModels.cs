using System.Collections.Generic;

public class ProjectModels
{
    public List<ModelProperties> Classes = new List<ModelProperties>();
    public List<ModelProperties> Structs = new List<ModelProperties>();
    public List<EnumProperties> Enums = new List<EnumProperties>();
}

public class ModelProperties
{
    public ModelProperties(string name)
    {
        Name = name;
    }

    public string Name { get; private set; }
}

public class EnumProperties
{
    public EnumProperties(string name, string baseType)
    {
        Name = name;
        BaseType = baseType;
    }

    public string Name { get; private set; }
    public string BaseType { get; private set; }
}