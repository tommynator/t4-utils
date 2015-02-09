using System;
using System.IO;
using System.Xml.Linq;

public static class WebServiceAppConfig
{
    const string projectName = "UberStrike.DataCenter.WebService";
    const string binding = "basicHttpBinding";

    public static void Update(IServiceProvider hostServiceProvider, string configFileName, string baseUrl = "")
    {
        EnvDTE.Project project = EnvDteUtils.GetProjectWithName(hostServiceProvider, projectName);
        foreach (EnvDTE.ProjectItem p in project.ProjectItems)
        {
            if (p.Name.EndsWith(configFileName, true, null))
            {
                string path = p.get_FileNames(1);
                string content = string.Empty;

                //open app.config
                TextReader r = new StreamReader(path);
                content = r.ReadToEnd();
                r.Close();

                //clear old system.serviceModel
                XElement e = XElement.Parse(content);

                foreach (var n in e.Elements())
                {
                    if (n.Name.LocalName == "system.serviceModel")
                    {
                        XComment comment = n.PreviousNode as XComment;
                        if (comment != null) comment.Remove();//.Value = XmlHeader();
                        n.Remove();
                    }
                }

                XElement main = new XElement("system.serviceModel");
                XElement serviceHostingEnvironment = new XElement("serviceHostingEnvironment", new XAttribute("multipleSiteBindingsEnabled", "true"));
                main.Add(serviceHostingEnvironment);

                //SERVICES
                XElement services = new XElement("services");
                var interfaces = WebServiceAttributeParser.GetProjectInterfaces(EnvDteUtils.GetProjectWithName(hostServiceProvider, projectName));
                //Here we declare the interfaces for IIS
                foreach (var i in interfaces)
                {
                    if (i.UseBinarySerialization)
                    {
                        services.Add(CreateWebServiceElement(i.ClassName, i.Name, baseUrl, "basicHttpBinding", ""));
                    }
                    else
                    {
                        services.Add(CreateWebServiceElement(i.ClassName, i.Name, baseUrl, "webHttpBinding", "DescriptorBehaviour", false));
                    }
                }

                main.Add(services);

                //BINDINGS
                XElement bindingsElement = new XElement("bindings");
                XElement basicHttpBindingElement = new XElement("basicHttpBinding");
                XElement bindingElement = new XElement("binding"
                    , new XAttribute("name", "basicHttpBinding")
                    , new XAttribute("closeTimeout", "00:01:00")
                    , new XAttribute("openTimeout", "00:01:00")
                    , new XAttribute("receiveTimeout", "00:01:00")
                    , new XAttribute("sendTimeout", "00:01:00")
                    , new XAttribute("maxReceivedMessageSize", WebServiceConstants.MaxReceivedMessageSize)
                    , new XAttribute("maxBufferSize", WebServiceConstants.MaxBufferSize)
                    , new XAttribute("maxBufferPoolSize", WebServiceConstants.MaxBufferPoolSize)
                    );
                XElement readerQuotasElement = new XElement("readerQuotas"
                    , new XAttribute("maxDepth", WebServiceConstants.MaxDepth)
                    , new XAttribute("maxStringContentLength", WebServiceConstants.MaxStringContentLength)
                    , new XAttribute("maxArrayLength", WebServiceConstants.MaxArrayLength)
                    , new XAttribute("maxBytesPerRead", WebServiceConstants.MaxBytesPerRead)
                    , new XAttribute("maxNameTableCharCount", WebServiceConstants.MaxNameTableCharCount)
                    );
                bindingElement.Add(readerQuotasElement);
                basicHttpBindingElement.Add(bindingElement);
                bindingsElement.Add(basicHttpBindingElement);
                main.Add(bindingsElement);

                //BEHAVIOURS
                XElement behaviorsElement = new XElement("behaviors");
                XElement serviceBehaviorsElement = new XElement("serviceBehaviors");
                XElement behaviorElement1 = new XElement("behavior",
                    new XElement("serviceMetadata", new XAttribute("httpGetEnabled", "True")),
                    new XElement("serviceDebug", new XAttribute("includeExceptionDetailInFaults", "True")),
                    new XElement("serviceThrottling", new XAttribute("maxConcurrentCalls", "64")));
                serviceBehaviorsElement.Add(behaviorElement1);
                behaviorsElement.Add(serviceBehaviorsElement);

                XElement endpointBehaviorsElement = new XElement("endpointBehaviors");
                XElement behaviorElement2 = new XElement("behavior", new XAttribute("name", "DescriptorBehaviour"));
                behaviorElement2.Add(new XElement("webHttp"));
                endpointBehaviorsElement.Add(behaviorElement2);
                behaviorsElement.Add(endpointBehaviorsElement);

                main.Add(behaviorsElement);

                e.Add(new XComment(T4Utils.XmlHeader()));
                e.Add(main);

                TextWriter w = new StreamWriter(path);

                try
                {

                    w.Write(e.ToString());
                }
                finally
                {
                    w.Close();
                }

                //T4Utils.Comment(path + " was updated " + DateTime.Now);
            }
        }
    }

    private static XElement CreateWebServiceElement(string service, string contract, string baseUrl, string binding, string behaviour, bool serializedWebService = true)
    {
        string serviceUrl = String.Empty;
        if (serializedWebService)
        {
            contract = projectName + "." + WebServiceConstants.InternalWebServiceNamespace + "." + contract + WebServiceConstants.InternalWebServiceSuffix;
            service = projectName + "." + WebServiceConstants.InternalWebServiceNamespace + "." + service + WebServiceConstants.InternalWebServiceSuffix;
            serviceUrl = baseUrl + "/" + service + WebServiceConstants.WebServiceEndpointSuffix + "/";
        }
        else
        {
            contract = projectName + ".Interfaces." + contract;
            service = projectName + "." + service;
            serviceUrl = baseUrl + "/" + service + WebServiceConstants.WebServiceEndpointSuffix + "/";
        }


        //Descriptor Webservice
        XElement serviceElement = new XElement("service", new XAttribute("name", service));
        XElement endpoint = null;
        if (string.IsNullOrEmpty(behaviour))
        {
            endpoint = new XElement("endpoint", new XAttribute("address", ""), new XAttribute("binding", binding), new XAttribute("bindingConfiguration", binding), new XAttribute("contract", contract));
        }
        else
        {
            endpoint = new XElement("endpoint", new XAttribute("address", ""), new XAttribute("behaviorConfiguration", behaviour), new XAttribute("binding", binding), new XAttribute("contract", contract));
        }
        serviceElement.Add(endpoint);

        XElement host = new XElement("host");
        XElement baseAddresses = new XElement("baseAddresses");
        baseAddresses.Add(new XElement("add", new XAttribute("baseAddress", service.Contains("CrossDomain") ? baseUrl : serviceUrl)));
        host.Add(baseAddresses);
        serviceElement.Add(host);

        return serviceElement;
    }
}