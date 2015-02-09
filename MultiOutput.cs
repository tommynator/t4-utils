using System;
using System.IO;
using System.Text;

public static class MultiOutput
{
    public static void SaveOutput(string outputFileName, StringBuilder builder, IServiceProvider sp, string templateFile)
    {
        EnvDTE.DTE dte = (EnvDTE.DTE)sp.GetService(typeof(EnvDTE.DTE));
        EnvDTE.ProjectItem templateProjectItem = dte.Solution.FindProjectItem(templateFile);

        //string filename = Path.GetFileNameWithoutExtension(templateProjectItem.FileNames[1]);
        string outputFilePath = Path.Combine(Path.GetDirectoryName(templateProjectItem.FileNames[1]), outputFileName);

        File.WriteAllText(outputFilePath, builder.ToString());
        templateProjectItem.ProjectItems.AddFromFile(outputFilePath);
        builder.Clear();
    }
}