using System;
using System.Xml.Linq;

public static class WebServiceDescriptor
{
    public static string Create(IServiceProvider hostServiceProvider, string projectName)
    {
        var interfaces = WebServiceAttributeParser.GetProjectInterfaces(EnvDteUtils.GetProjectWithName(hostServiceProvider, projectName));

        XElement xml = new XElement("services");

        //Here we declare the interfaces for IIS
        foreach (var interfac in interfaces)
        {
            if (interfac.UseBinarySerialization)
            {
                XElement xmlService = new XElement("service"
                    , new XAttribute("name", interfac.ClassName)
                    , new XAttribute("endpoint", projectName + "." + WebServiceConstants.InternalWebServiceNamespace + "." + interfac.ClassName + WebServiceConstants.InternalWebServiceSuffix + WebServiceConstants.WebServiceEndpointSuffix)
                    , new XAttribute("contract", interfac.Name + WebServiceConstants.InternalWebServiceSuffix));
                XElement xmlOperations = new XElement("operations");

                foreach (var method in interfac.Methods)
                {
                    XElement xmlOperation = new XElement("operation",
                        new XAttribute("name", method.Name),
                        new XAttribute("returnType", method.ReturnType));

                    XElement xmlParameters = new XElement("parameters");

                    foreach (var parameter in method.Arguments)
                    {
                        xmlParameters.Add(new XElement("parameter",
                            new XAttribute("name", parameter.Value),
                            new XAttribute("type", parameter.Key)));
                    }

                    xmlOperation.Add(xmlParameters);
                    xmlOperations.Add(xmlOperation);
                }

                xmlService.Add(xmlOperations);
                xml.Add(xmlService);
            }
        }

        return xml.ToString();
    }
}