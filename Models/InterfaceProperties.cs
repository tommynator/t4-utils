using System.Collections.Generic;

public class InterfaceProperties
{
    public string Name { get; private set; }
    public string FullName { get; private set; }
    public bool UseBinarySerialization { get; set; }
    public List<MethodProperties> Methods { get; private set; }

    public InterfaceProperties(string name, string fullName)
    {
        Name = name;
        FullName = fullName;
        Methods = new List<MethodProperties>();
    }

    public string ClassName
    {
        get { return Name.Substring(1); }
    }
}