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

using System;
using System.Collections.Generic;
using System.Linq;

public enum SourceSystem {
	SqlServer, SSIS, Biml
}
// Define other methods and classes hereConv
public class Conversion {
	public string SqlServer;
	public string SSIS;
	public string Biml;
	
	public Conversion() {
		SqlServer = null;
		SSIS = null;
		Biml = null;
	}
	public Conversion( string sqlServer, string ssis, string biml) {
		SqlServer = sqlServer;
		SSIS = ssis;
		Biml = biml;
	}
}

public class ConversionUtility {
	private List<Conversion> _privateMap = new List<Conversion>();
	
	public ConversionUtility() {
		_privateMap = BuildMap();
	}
	
	private List<Conversion> BuildMap() {
	List<Conversion> output = new List<Conversion>();

	//Conversion test = new Conversion("bigint","DT_I8","Int64");
	output.Add( new Conversion("bigint","DT_I8","Int64") );
	output.Add( new Conversion("binary","DT_BYTES","Binary") );
	output.Add( new Conversion("bit","DT_BOOL","Boolean") );
	output.Add( new Conversion("char","DT_STR","AnsiStringFixedLength") );
	output.Add( new Conversion("date","DT_DBDATE","Date") );
	output.Add( new Conversion("datetime","DT_DBTIMESTAMP","DateTime") );
	//SSIS behaives oddly when using DT_DBTIMESTAMP2
	//output.Add( new Conversion("datetime2","DT_DBTIMESTAMP2","DateTime2") );
	//but treating it in Biml as a DateTime, should get it to emit packages with DT_DBTIMESTAMP 
	output.Add( new Conversion("datetime2","DT_DBTIMESTAMP","DateTime") );
	output.Add( new Conversion("datetimeoffset","DT_DBTIMESTAMPOFFSET","DateTimeOffset") );
	output.Add( new Conversion("decimal","DT_NUMERIC","Decimal") );
	output.Add( new Conversion("float","DT_R8","Double") );
	output.Add( new Conversion("geography","DT_IMAGE","Object") );
	output.Add( new Conversion("geometry","DT_IMAGE","Object") );
	output.Add( new Conversion("hierarchyid","DT_BYTES","Object") );
	output.Add( new Conversion("image","DT_IMAGE","Binary") );
	output.Add( new Conversion("int","DT_I4","Int32") );
	output.Add( new Conversion("money","DT_CY","Currency") );
	output.Add( new Conversion("nchar","DT_WSTR","StringFixedLength") );
	output.Add( new Conversion("ntext","DT_NTEXT","String") );
	output.Add( new Conversion("numeric","DT_NUMERIC","Decimal") );
	output.Add( new Conversion("nvarchar","DT_WSTR","String") );
	//how should we handle nvarchar Max versus determinate length? 
	//output.Add( new Conversion("nvarchar","DT_NTEXT","String") );
	output.Add( new Conversion("real","DT_R4","Single") );
	output.Add( new Conversion("rowversion","DT_BYTES","Binary") );
	output.Add( new Conversion("smalldatetime","DT_DBTIMESTAMP","DateTime") );
	output.Add( new Conversion("smallint","DT_I2","Int16") );
	output.Add( new Conversion("smallmoney","DT_CY","Currency") );
	output.Add( new Conversion("sql_variant","DT_WSTR","Object") );
	output.Add( new Conversion("text","DT_TEXT","AnsiString") );
	output.Add( new Conversion("time","DT_DBTIME2","Time") );
	output.Add( new Conversion("timestamp","DT_BYTES","Binary") );
	output.Add( new Conversion("tinyint","DT_UI1","Byte") );
	output.Add( new Conversion("uniqueidentifier","DT_GUID","Guid") );
	output.Add( new Conversion("varbinary","DT_BYTES","Binary") );
	//how should we handle varbinary max versus determinate length
	//output.Add( new Conversion("varbinary","DT_IMAGE","Binary") );
	output.Add( new Conversion("varchar","DT_STR","AnsiString") );
	//how should we handle varchar Max versus determinate length
	//output.Add( new Conversion("varchar","DT_TEXT","AnsiString") );
	output.Add( new Conversion("xml","DT_NTEXT","Xml") );

	return output;	
}

	public string Convert(SourceSystem fromType, SourceSystem toType, string input) {
		
		Conversion c = null;
		try {
			switch(fromType) {
				case SourceSystem.SqlServer:
					c = _privateMap.FirstOrDefault (m => m.SqlServer.ToLower() == input.ToLower());
					break;
				case SourceSystem.SSIS:
					c = _privateMap.FirstOrDefault (m => m.SSIS.ToLower() == input.ToLower());
					break;
				case SourceSystem.Biml:
					c = _privateMap.FirstOrDefault (m => m.Biml.ToLower() == input.ToLower());
					break;
				default:
					Console.WriteLine("Unknown FromType: " + fromType);
					break;
			}
		} catch (Exception e) {
			//check for no c found!
			if(c == null) {
				Console.WriteLine("No mapping found for " + fromType + " " + input);
				return null;
			}
			Console.WriteLine(e);
		}
		
		switch(toType) {
			case SourceSystem.SqlServer:
				return c.SqlServer;
			case SourceSystem.SSIS:
				return c.SSIS;
			case SourceSystem.Biml:
				return c.Biml;
			default:
				Console.WriteLine("Unknown ToType: " + toType);
				return null;
		}
	}
	
}


