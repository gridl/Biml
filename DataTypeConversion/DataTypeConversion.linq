<Query Kind="Program">
  <Reference Relative="..\..\PinTrader\WebScraper\packages\HtmlAgilityPack.1.4.6\Net45\HtmlAgilityPack.dll">C:\Repositories\PinTrader\WebScraper\packages\HtmlAgilityPack.1.4.6\Net45\HtmlAgilityPack.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.VisualBasic.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.dll</Reference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

//first of all MAJOR PROPS to Catherine Wilhelmsen for the article: https://www.cathrinewilhelmsen.net/2014/05/27/sql-server-ssis-and-biml-data-types/
//this class is just a quick and easy way to move back and forth between the datatypes
//I'm making this open source, so if anyone needs to add another conversion to the list, we all benefit!

void Main()
{
	//DataType="<#= col.DataType #>"
	//cu.Convert(SourceSystem.SqlServer, SourceSystem.Biml, col.DataType) 
	
	ConversionUtility cu = new ConversionUtility();
	
	Console.WriteLine(cu.Convert(SourceSystem.SqlServer, SourceSystem.SSIS, "BIGINT"));
	Console.WriteLine(cu.Convert(SourceSystem.SqlServer,SourceSystem.SqlServer, "bigint"));
	
	//i may need a debug function to see the map that has been built
	//List<Conversion> map = BuildMap();
	//Console.WriteLine(map);

}

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
	output.Add( new Conversion("datetime2","DT_DBTIMESTAMP2","DateTime2") );
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
	
	//check for no c found!
	if(c == null) {
		Console.WriteLine("No mapping found for " + fromType + " " + input);
		return null;
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

