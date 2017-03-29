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

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

public class DevelopmentHelper
{
	public string DeveloperConnectionString { get; set;}
	
	//constructor
	public DevelopmentHelper(string developerConnectionString) {
		DeveloperConnectionString = developerConnectionString;
		
	}

	public bool deployDBObject(string dDlQuery) {
		try {
			//c# doesn't like to use go statements, so we have to split them, and then iterate through
			List<string> statements = Regex.Split(
	            dDlQuery,
	            @"^\s*GO\s*\d*\s*($|\-\-.*$)",
	            RegexOptions.Multiline |
	            RegexOptions.IgnorePatternWhitespace |
	            RegexOptions.IgnoreCase).ToList();
			
			using (SqlConnection Conn = new SqlConnection(this.DeveloperConnectionString))	{
				Conn.Open();
				foreach( string statement in statements.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim(' ', '\r', '\n'))) {
							
					SqlCommand Cmd = new SqlCommand(statement, Conn);
					Cmd.ExecuteNonQuery();
					
				}
				Conn.Close();
				return true;
			}
		} catch {
			return false;
		}
		
	}
}
