/*
	while developing SSIS Packages, the referenced objects need to exist in the dev instance.
	otherwise you can get errors executing or building BimlScripts

	Assumption, the current MetadataServer is also the same server you use for development

*/
using Varigence.DynamicObjects;
using Varigence.Biml.CoreLowerer.SchemaManagement;
using Varigence.Biml.Extensions;
using Varigence.Biml.Extensions.SchemaManagement;
using Varigence.Flow.FlowFramework;
using Varigence.Flow.FlowFramework.Utility;
using Varigence.Languages.Biml;
using Varigence.Languages.Biml.Connection;
using Varigence.Languages.Biml.Cube;
using Varigence.Languages.Biml.Cube.Action;
using Varigence.Languages.Biml.Cube.Aggregation;
using Varigence.Languages.Biml.Cube.Calculation;
using Varigence.Languages.Biml.Cube.Partition;
using Varigence.Languages.Biml.Dimension;
using Varigence.Languages.Biml.Fact;
using Varigence.Languages.Biml.FileFormat;
using Varigence.Languages.Biml.LogProvider;
using Varigence.Languages.Biml.Measure;
using Varigence.Languages.Biml.MeasureGroup;
using Varigence.Languages.Biml.Metadata;
using Varigence.Languages.Biml.Project;
using Varigence.Languages.Biml.Platform;
using Varigence.Languages.Biml.Principal;
using Varigence.Languages.Biml.Script;
using Varigence.Languages.Biml.Table;
using Varigence.Languages.Biml.Task;
using Varigence.Languages.Biml.Transformation;
using Varigence.Languages.Biml.Transformation.Destination;
using Varigence.Utility.RemoteExecution;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Varigence.Flow.FlowFramework.Validation;


public class DevelopmentHelper
{
    public string DeveloperConnectionString { get; set; }

    //constructor
    public DevelopmentHelper(string developerConnectionString)
    {
        DeveloperConnectionString = developerConnectionString;

    }

    public bool deployDBObject(string dDlQuery)
    {
        try
        {
            //c# doesn't like to use go statements, so we have to split them, and then iterate through
            List<string> statements = Regex.Split(
                dDlQuery,
                @"^\s*GO\s*\d*\s*($|\-\-.*$)",
                RegexOptions.Multiline |
                RegexOptions.IgnorePatternWhitespace |
                RegexOptions.IgnoreCase).ToList();

            using (SqlConnection Conn = new SqlConnection(this.DeveloperConnectionString))
            {
                Conn.Open();
                foreach (string statement in statements.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim(' ', '\r', '\n')))
                {

                    SqlCommand Cmd = new SqlCommand(statement, Conn);
                    Cmd.ExecuteNonQuery();

                }
                Conn.Close();
                return true;
            }
        }
        catch
        {
            return false;
        }

    }

    /* when dealing with project parameters in Biml Express you have to take extra steps. See:
     * https://www.cathrinewilhelmsen.net/2015/11/25/create-ssis-project-parameters-biml-bids-helper/   
     * This function will write the parameters to your current Project.params file
    */
    public void AddProjectParameters(AstRootNode RootNode)
    {
        Varigence.Flow.FlowFramework.Validation.ValidationReporter ValidationReporter = new Varigence.Flow.FlowFramework.Validation.ValidationReporter();

        var project = RootNode.PackageProjects.FirstOrDefault();
        if (project == null)
        {
            ValidationReporter.Report(Severity.Error, "<PackageProject> does not exist");
        }
        else
        {
            var projectPath = project.GetTag("ProjectParametersPath");
            if (projectPath == "")
            {
                ValidationReporter.Report(project, Severity.Error, "Annotation ProjectParametersPath does not exist", @"Add <Annotation Tag=""ProjectParametersPath"">C:\SSIS\TestProject\Project.params</Annotation> to <PackageProject>");
            }
            else
            {

                try
                {

                    var fileAttributes = File.GetAttributes(projectPath);
                    if ((fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(projectPath, (fileAttributes & ~FileAttributes.ReadOnly));
                    }

                    StringBuilder parameters = new StringBuilder();
                    parameters.AppendLine("<?xml version=\"1.0\"?>");
                    parameters.AppendLine("<SSIS:Parameters xmlns:SSIS=\"www.microsoft.com/SqlServer/SSIS\">");
                    foreach (var parameter in project.Parameters)
                    {
                        parameters.AppendFormat("<SSIS:Parameter SSIS:Name=\"{0}\">", parameter.Name).AppendLine();
                        parameters.AppendLine("<SSIS:Properties>");
                        parameters.AppendFormat("<SSIS:Property SSIS:Name=\"ID\">{{{0}}}</SSIS:Property>", (parameter.Id == Guid.Empty ? Guid.NewGuid() : parameter.Id)).AppendLine();
                        parameters.AppendFormat("<SSIS:Property SSIS:Name=\"DataType\">{0}</SSIS:Property>", Convert.ToByte(parameter.DataType)).AppendLine();
                        parameters.AppendFormat("<SSIS:Property SSIS:Name=\"Value\">{0}</SSIS:Property>", parameter.Value).AppendLine();
                        parameters.AppendFormat("<SSIS:Property SSIS:Name=\"Sensitive\">{0}</SSIS:Property>", Convert.ToByte(parameter.IsSensitive)).AppendLine();
                        parameters.AppendFormat("<SSIS:Property SSIS:Name=\"Required\">{0}</SSIS:Property>", Convert.ToByte(parameter.IsRequired)).AppendLine();
                        parameters.AppendFormat("<SSIS:Property SSIS:Name=\"IncludeInDebugDump\">{0}</SSIS:Property>", Convert.ToByte(parameter.IncludeInDebugDump)).AppendLine();
                        parameters.AppendFormat("<SSIS:Property SSIS:Name=\"Description\">{0}</SSIS:Property>", parameter.GetTag("Description")).AppendLine();
                        parameters.AppendFormat("<SSIS:Property SSIS:Name=\"CreationName\">{0}</SSIS:Property>", parameter.GetTag("CreationName")).AppendLine();
                        parameters.AppendLine("</SSIS:Properties>");
                        parameters.AppendLine("</SSIS:Parameter>");
                    }
                    parameters.AppendLine("</SSIS:Parameters>");

                    File.WriteAllText(projectPath, parameters.ToString());

                }
                catch (Exception e)
                {
                    ValidationReporter.Report(project, Severity.Error, "Error writing Project Parameters to Project.params", String.Format("Make sure the path \"{0}\" is correct and that this project uses the Project Deployment Model", projectPath));
                }

            }
        }
    }
}
