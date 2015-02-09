
using System.Collections.Generic;

public class RpcInterfaceCollection
{
    public List<RpcInterfaceView> RoomEvents { get; private set; }
    public List<RpcInterfaceView> RoomOperations { get; private set; }
    public List<RpcInterfaceView> ServerEvents { get; private set; }
    public List<RpcInterfaceView> ServerOperations { get; private set; }

    public RpcInterfaceCollection()
    {
        RoomEvents = new List<RpcInterfaceView>();
        RoomOperations = new List<RpcInterfaceView>();
        ServerEvents = new List<RpcInterfaceView>();
        ServerOperations = new List<RpcInterfaceView>();
    }
}

public class RpcInterfaceView
{
    public string Name { get; private set; }
    public List<NetworkMethod> NetworkMethods { get; private set; }
    public string Interface { get; set; }
    public int MethodIdOffset { get; set; }
    public bool IsApplicationInterface { get; set; }

    public RpcInterfaceView(string name)
    {
        Name = name;
        NetworkMethods = new List<NetworkMethod>();
        IsApplicationInterface = false;
    }

    public string GetClassPrefix()
    {
        return Name.Replace("Events", "").Replace("Operations", "").Substring(1);
    }

    public string GetInterfacePrefix()
    {
        return Name.Replace("Events", "").Replace("Operations", "");
    }

    public string GetFieldPrefix()
    {
        string n = GetClassPrefix();
        string firstLetter = n[0].ToString().ToLower();
        return n.Substring(1).Insert(0, firstLetter);
    }
}