using System;
using System.Collections.Generic;

public static class RealtimeAttributeParser
{
    public static RpcInterfaceCollection GetRpcInterfaces(IServiceProvider hostServiceProvider, string projectName)
    {
        return GetRpcInterfaces(EnvDteUtils.GetProjectWithName(hostServiceProvider, projectName));
    }

    static RpcInterfaceCollection GetRpcInterfaces(EnvDTE.Project project)
    {
        RpcInterfaceCollection metaData = null;

        try
        {
            metaData = new RpcInterfaceCollection();

            List<EnvDTE.ProjectItem> files = EnvDteUtils.GetAllScripts(project.ProjectItems);

            //find all classes
            List<EnvDTE.CodeElement> classes = new List<EnvDTE.CodeElement>();
            foreach (EnvDTE.ProjectItem f in files)
            {
                classes.AddRange(EnvDteUtils.GetAllInterfaces(f.FileCodeModel.CodeElements));
            }

            int gloablEventId = 0;
            //now handle all room operations
            foreach (EnvDTE.CodeElement e in classes)
            {
                if (IsRoomEventsInterface(e))
                {
                    metaData.RoomEvents.Add(GetRpcInterfaceView(e, ref gloablEventId));
                }
                else if (IsServerEventsInterface(e))
                {
                    metaData.ServerEvents.Add(GetRpcInterfaceView(e, ref gloablEventId));
                }
                else if (IsRoomOperationsInterface(e))
                {
                    int operationId = 0;
                    metaData.RoomOperations.Add(GetRpcInterfaceView(e, ref operationId));
                }
                else if (IsServerOperationsInterface(e))
                {
                    int operationId = 0;
                    metaData.ServerOperations.Add(GetRpcInterfaceView(e, ref operationId));
                }
                else
                {
                    //T4Utils.WriteLine("//CodeElement => " + e.Name);
                }
            }
        }
        catch (Exception ex)
        {
            T4Utils.WriteLine(ex.GetType() + ": " + ex.Message);
            T4Utils.WriteLine(ex.StackTrace);
        }

        return metaData;
    }

    static RpcInterfaceView GetRpcInterfaceView(EnvDTE.CodeElement e, ref int methodId)
    {
        RpcInterfaceView roomInterface = new RpcInterfaceView(e.Name);

        EnvDTE.CodeInterface interfaceElement = e as EnvDTE.CodeInterface;
        if (interfaceElement != null && interfaceElement.Bases.Count > 0)
        {
            roomInterface.Interface = interfaceElement.Bases.Item(1).Name;
        }

        List<EnvDTE.CodeFunction> functions = EnvDteUtils.GetAllFunctions(e);
        foreach (EnvDTE.CodeFunction fu in functions)
        {
            List<KeyValuePair<string, string>> arguments = new List<KeyValuePair<string, string>>();
            List<ParameterKind> argKind = new List<ParameterKind>();
            foreach (EnvDTE.CodeParameter p in fu.Parameters)
            {
                arguments.Add(new KeyValuePair<string, string>(p.Type.AsString, p.Name));
                argKind.Add(EnvDteUtils.GetParameterKind(p.Type));
            }

            roomInterface.NetworkMethods.Add(new NetworkMethod(fu.Name, ++methodId)
                {
                    ArgumentKinds = argKind,
                    Arguments = arguments,
                    SendUnreliable = SendUnreliable(fu),
                    SendEncrypted = SendEncrypted(fu),
                });
        }

        roomInterface.MethodIdOffset = methodId;

        return roomInterface;
    }

    private static bool SendEncrypted(EnvDTE.CodeFunction e)
    {
        foreach (EnvDTE.CodeElement att in e.Children)
        {
            if (EnvDteUtils.IsAttribute(att) && att.Name == "SendEncrypted")
            {
                return true;
            }
        }
        return false;
    }

    private static bool SendUnreliable(EnvDTE.CodeFunction e)
    {
        foreach (EnvDTE.CodeElement att in e.Children)
        {
            if (EnvDteUtils.IsAttribute(att) && att.Name == "SendUnreliable")
            {
                return true;
            }
        }
        return false;
    }

    static bool IsRoomOperationsInterface(EnvDTE.CodeElement e)
    {
        return EnvDteUtils.HasAttributeWithName(e, "RoomOperations");
    }

    static bool IsRoomEventsInterface(EnvDTE.CodeElement e)
    {
        return EnvDteUtils.HasAttributeWithName(e, "RoomEvents");
    }

    static bool IsServerOperationsInterface(EnvDTE.CodeElement e)
    {
        return EnvDteUtils.HasAttributeWithName(e, "PeerOperations");
    }

    static bool IsServerEventsInterface(EnvDTE.CodeElement e)
    {
        return EnvDteUtils.HasAttributeWithName(e, "PeerEvents");
    }

    static string Receiver(string name)
    {
        if (name.StartsWith("I"))
            name = name.Remove(0, 1);
        return name + "Receiver";
    }

    static string Client(string name)
    {
        if (name.StartsWith("I"))
            name = name.Remove(0, 1);
        if (name.StartsWith("Client"))
            name = name.Remove(0, 6);
        return "Client" + name;
    }

    static string Server(string name)
    {
        if (name.StartsWith("I"))
            name = name.Remove(0, 1);
        if (name.StartsWith("Server"))
            name = name.Remove(0, 6);
        return "Server" + name;
    }

    static string Clean(string name)
    {
        if (name.StartsWith("Server"))
            name = name.Remove(0, 6);
        if (name.StartsWith("Client"))
            name = name.Remove(0, 6);
        if (name.StartsWith("On"))
            name = name.Remove(0, 2);
        return name;
    }
}