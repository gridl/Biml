<Query Kind="Program">
  <Reference Relative="..\..\PinTrader\WebScraper\packages\HtmlAgilityPack.1.4.6\Net45\HtmlAgilityPack.dll">C:\Repositories\PinTrader\WebScraper\packages\HtmlAgilityPack.1.4.6\Net45\HtmlAgilityPack.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.VisualBasic.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.dll</Reference>
  <NuGetReference>WindowsAzure.Storage</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>Microsoft.VisualBasic.FileIO</Namespace>
  <Namespace>Microsoft.WindowsAzure.Storage</Namespace>
  <Namespace>Microsoft.WindowsAzure.Storage.Auth</Namespace>
  <Namespace>Microsoft.WindowsAzure.Storage.Blob</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

void Main()
{
	string demoFile = @"C:\Repositories\Biml\Interrogator\testdata\Numerics.csv";
	string demoDelimiter = ",";
	
	Console.WriteLine(ProcessFile(demoFile, demoDelimiter));
}

// Define other methods and classes here
public class DestinationColumn {

	public string Name {get; set;}
	public string DataType {get; set;}
	public int MaxLength {get; set;}
	public int Precision {get; set;}
	public int Scale {get; set;}
	public bool Nullable {get; set;}
	
	//name only init
	public DestinationColumn (string name){
		Name = name;
		DataType = null;
		MaxLength = 0;
		Precision = 0;
		Scale = 0;
		Nullable = false;
	}
	//full init
	public DestinationColumn (string name, string datatype, int maxlength, int precision, int scale, bool nullable){
		Name = name;
		DataType = datatype;
		MaxLength = maxlength;
		Precision = precision;
		Scale = scale;
		Nullable = nullable;
	}
}

//process a file, return a list of columns
List<DestinationColumn> ProcessFile(string FileName, string delimiter) {
	List<DestinationColumn> output = new List<DestinationColumn>();
	
	using (TextFieldParser parser = new TextFieldParser(FileName))
	{
		parser.TextFieldType = FieldType.Delimited;
		parser.SetDelimiters(delimiter);
		
		while (!parser.EndOfData) 
		{
			//Processing row
			string[] fields = parser.ReadFields();
			
			for(int i=0; i < fields.Count(); i++) {
				//if you get a new column (in first or 101st line) add it's name to output
				if(i + 1 > output.Count ) {
					output.Add(new DestinationColumn(fields[i]));
				}else {
					//now get the data type
					output[i].DataType = DataTypeGuess(fields[i], false).ToString();
					//!!!!!handle datatype changing!!!!!
				}
			}
			
			Console.WriteLine(output);
			Console.WriteLine(fields);
			
		}
	}
	return output;
}

SqlDbType DataTypeGuess(string input, bool debug = false) {
	SqlDbType output;
	//first try datetimes
	output = DateTimeGuess(input, debug);
	
	if(output != SqlDbType.Binary)
		return output;
	
	//then numerics
	output = NumericGuess(input, debug);
	
	if(output != SqlDbType.Binary)
		return output;
	
	//then character strings
	output = CharGuess(input, debug);
	
	//if all else fails, then resort to binary
	return output;
}

SqlDbType DateTimeGuess(string input, bool debug = false) {
	
	DateTime givenDateTime = new DateTime();
	SqlDbType output;
	
	if(debug) 
		Console.WriteLine("input: " + input);

	//first see if we can cast it to any kind of datetime
	if(debug) 
		Console.WriteLine("testing if we can cast input to datetime.");	
	
	if(DateTime.TryParse(input, out givenDateTime)) {
		if(debug) 
			Console.WriteLine(givenDateTime);	
		output = SqlDbType.DateTime2;		
	} else {
		Console.WriteLine("cannot convert" + input + " to a datetime.");
		output = SqlDbType.Binary;
		//exit early!
		return output;
	}
	
	
	//since we now know we have some kind of date time the rest of the tests are safe
	//do we have a time?
	try{
		if(debug) 
			Console.WriteLine("regex check for time.");	
		//this pattern should match time and not datetime
		string pattern = @"^([0-9]{1,2}:[0-9]{1,2}.{0,1}[0-9]{0,7})";
		//^([0-9]{1,2}:[0-9]{1,2}.{0,1}[0-9]{0,7})
		Regex r = new Regex(pattern);
		if(debug)
			Console.WriteLine(pattern);
			
		if (r.IsMatch(input))
			output = SqlDbType.Time;
		if(debug)	
			Console.WriteLine("you have a time.");	
		
	} catch (Exception e) {
		//our default (aka, try something else)
		Console.WriteLine("{0} Exception caught.", e);
		//return whatever we discovered before
		return output;
	}
	
	//is it just a date?	
	try {	
		if(debug) 
			Console.WriteLine("is it just a date?");	
		if(givenDateTime == givenDateTime.Date && output != SqlDbType.Time) {
			output = SqlDbType.Date;
		}
	} catch (Exception e) {
		//our default (aka, try something else)
		Console.WriteLine("{0} Exception caught.", e);
		return output;
	}
	
	//do we have datetime offset?
	try {
		if(debug) 
			Console.WriteLine("do we have datetime offset?");	
		//if the DateTime.Kind is something other than unspecified, it's offset
		if(givenDateTime.Kind != System.DateTimeKind.Unspecified && output == SqlDbType.DateTime2) 
			output = SqlDbType.DateTimeOffset;	
	} catch (Exception e) {
		//our default (aka, try something else)
		Console.WriteLine("{0} Exception caught.", e);
		return SqlDbType.VarBinary;
	}
	
	if(debug)
		Console.WriteLine(input + " is " +output);
		
	return output;
}

SqlDbType NumericGuess(string input, bool debug = false) {
	Boolean givenBoolean;
	Byte givenByte;
	Int16 givenInt16;
	Int32 givenInt32;
	Int64 givenInt64;
	Decimal givenDecimal;
	Double givenDouble;
	
	if(debug)
		Console.WriteLine("input: " + input);
		
	//try to cast to bit (boolean)
	if(Boolean.TryParse(input, out givenBoolean)) {
		if(debug) 
			Console.WriteLine(givenBoolean);	
		//early exit
		return SqlDbType.Bit;			
	} 
	if(debug) 
		Console.WriteLine("cannot convert " + input + " to a bit.");

	//try to cast to TinyInt(byte)
	if(Byte.TryParse(input, out givenByte)) {
		if(debug)
			Console.WriteLine(givenByte);	
		//early exit
		return SqlDbType.TinyInt;			
	} 
	if(debug) 
		Console.WriteLine("cannot convert " + input + " to a tinyint.");
	
	//try to cast to smallint (int16)
	if(Int16.TryParse(input, out givenInt16)) {
		if(debug) 
			Console.WriteLine(givenInt16);	
		//early exit
		return SqlDbType.SmallInt;			
	} 
	if(debug) 
		Console.WriteLine("cannot convert " + input + " to a smallint.");
	
	//try to cast to int(int32)
	if(Int32.TryParse(input, out givenInt32)) {
		if(debug) 
			Console.WriteLine(givenInt32);	
		//early exit
		return SqlDbType.Int;			
	} 
	if(debug) 
		Console.WriteLine("cannot convert " + input + " to a Int.");
	
	//try to cast to bigint(int64)
	if(Int64.TryParse(input, out givenInt64)) {
		if(debug) 
			Console.WriteLine(givenInt64);	
		//early exit
		return SqlDbType.BigInt;			
	} 
	if(debug) 
		Console.WriteLine("cannot convert " + input + " to a bigint.");
	
	//try to cast to Decimal
	//should we return the length here, or at the calling function?
	if(Decimal.TryParse(input, out givenDecimal)) {
		if(debug) 
			Console.WriteLine(givenDecimal);	
		//early exit
		return SqlDbType.Decimal;			
	} 
	if(debug) 
		Console.WriteLine("cannot convert " + input + " to a decimal.");
	
	//try to cast to float(double)
	if(Double.TryParse(input, out givenDouble)) {
		if(debug) 
			Console.WriteLine(givenDouble);	
		//early exit
		return SqlDbType.Float;			
	} 
	if(debug) 
		Console.WriteLine("cannot convert " + input + " to a float.");
		
	//if we can't determine the type by the end, default to binary.
	return SqlDbType.Binary;

}

SqlDbType CharGuess(string input, bool debug = false) {
	//since the source is already a string, our job is to just return char or nchar
	//the calling function can see all the values in a column to make the (variable) length guess
	
	if(input.Any(c => c > 255)) 
		return SqlDbType.NChar;
	else
		return SqlDbType.Char;

}