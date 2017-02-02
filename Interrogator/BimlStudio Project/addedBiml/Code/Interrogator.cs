
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
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

public class Interrogator
{

	//function to guess which character type the input is
	//This function can handle changes to data type too.
	public SqlDbType CharGuess(string input, string currentDatatype, bool debug = false) {
		//if any character is "after" the 255th character it's in the unicode space
		if(input.Any(c => c > 255)) 
			return SqlDbType.NVarChar;
		else {
			//if the current data type is NVarChar, we can't go back to VarChar
			if(currentDatatype != "NVarChar")
				return SqlDbType.VarChar;
			else
				return SqlDbType.NVarChar;
		}

	}
	
	//"main" guess function -- this one calls the guess functions in order
	// 1. DateTime, 2. Numerics, 3. Character, if none of the above, then VarBinary
	public string DataTypeGuess(string input, string currentDatatype, bool debug = false) {
	string output;
	
	//exclude blanks
	if (input.Length < 1)
		return "";
		
	//first try datetimes
	//CurrentDataType must be compatible
	string[] DateTimeCompatibleDataTypes = {null, "Date","Time","DateTimeOffset","DateTime2"};
	if( DateTimeCompatibleDataTypes.Contains(currentDatatype) ) {
		
		output = DateTimeGuess(input, currentDatatype, debug).ToString();
		
		if(debug) {
			Console.WriteLine("CurrentDataType: " + currentDatatype + " output: " + output);
		}
		//if you have a mixed column and the new value isn't null only DateTime2 can hold all
		if(currentDatatype != null && currentDatatype != output && output != "VarBinary" ){
			output = "DateTime2";
		}
		if(output != "VarBinary")
			return output;
	}
	
	//then numerics
	string[] NumericCompatibleDateTypes = {null, "Bit", "TinyInt", "SmallInt", "Int", "BigInt","Decimal","Float"};
	if( NumericCompatibleDateTypes.Contains(currentDatatype)) {
		output = NumericGuess(input, currentDatatype, debug).ToString();
		
		//if changed
		if(currentDatatype != output) {
			switch(currentDatatype) {
				case "TinyInt":
					//if it was a TinyInt, it cannot be an bit
					if(output == "Bit")
						output = currentDatatype;
					break;
				case "SmallInt":
					//SmallInt can't get smaller
					if(output == "Bit" || output == "TinyInt")
						output = currentDatatype;
					break;
				case "Int":
					//Int can't get smaller
					if(output == "Bit" || output == "TinyInt" || output == "SmallInt")
						output = currentDatatype;
					break;
				case "BigInt":
					//BitInt can't get smaller
					if(output == "Bit" || output == "TinyInt"|| output == "SmallInt" || output == "Int")
						output = currentDatatype;
					break;
				case "Decimal":
					//Decimal can't convert to int, precision would be lost
					if(output == "Bit" || output == "TinyInt"|| output == "SmallInt" || output == "Int" || output == "BigInt")
						output = currentDatatype;
					break;
				case "Float":
					//float's stay floats
					output = currentDatatype;
					break;
				//case "bit": 
					//any datatype is ok
				//default:
					//any datatype is ok
			}
		}
	
		if(output != "VarBinary")
			return output;
	}
	//then character strings, the only compatibility check is ! Binary
	if(currentDatatype != "VarBinary") {
		output = CharGuess(input, currentDatatype, debug).ToString();
		return output;
	}
	//if all else fails, then resort to binary
	return "VarBinary";
}

	//function to guess what kind of datetime we're dealing with:
	//Date, Time, Datetimeoffset, Datetime2
	public SqlDbType DateTimeGuess(string input, string currentDatatype, bool debug = false) {

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
		//durations do not cast to datetime in C#, but are valid times...test for duration/time here
		try{
			if(debug) 
				Console.WriteLine("regex check for time.");	
			//this pattern should match time and not datetime
			string pattern = @"^([0-9]{1,2}:[0-9]{1,2})$|^([0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2})$|^([0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}\.[0-9]{0,7})$";
			//^([0-9]{1,2}:[0-9]{1,2}.{0,1}[0-9]{0,7})
			Regex r = new Regex(pattern);
			if(debug)
				Console.WriteLine(pattern);
				
			if (r.IsMatch(input)) {
				if(debug)	
					Console.WriteLine("you have a time.");	
				//return early, you found a time!
				return SqlDbType.Time;
			}
		} catch (Exception e) {
			//our default (aka, try something else)
			Console.WriteLine("{0} Exception caught.", e);
			//on exception return varbinary (default)
			return SqlDbType.VarBinary;
		}
	
		if(debug)
			Console.WriteLine("cannot convert" + input + " to a datetime.");
		output = SqlDbType.VarBinary;
		//exit early!
		return output;
	}
	
	//since we now know we have some kind of date time the rest of the tests are safe
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

	//function to guess what kind of numeric we're dealing with:
	//TinyInt, SmallInt, Int, BigInt, Decimal, Float
	public SqlDbType NumericGuess(string input, string currentDatatype, bool debug = false) {
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
	return SqlDbType.VarBinary;

}

	//process a file, return a list of columns
	public List<DestinationColumn> ProcessFile(string FileName, char[] delimiter, bool FirstRowHeader = true, bool debug = false) {
	List<DestinationColumn> output = new List<DestinationColumn>();
	//by default do not treat the whole file as unicode
	bool treatWholeFileAsUnicode = false;
	using (StreamReader reader = new StreamReader(FileName))
	{
		//initialize the rownumber
		int rownumber = 0;
		while (!reader.EndOfStream) 
		{
			//Processing row
			string[] fields = reader.ReadLine().Split(delimiter);
			rownumber++;
			
			for(int i=0; i < fields.Count(); i++) {
				//get rid of leading "
				if(fields[i].Trim().StartsWith("\""))
					fields[i] = fields[i].Trim().Substring(1);
				
				//get rid of trailing "
				if(fields[i].Trim().EndsWith("\""))
					fields[i] = fields[i].Trim().Substring(0, fields[i].Trim().Length -1);
				
				//if you get a new column (in first or 101st line) add it's name to output
				if(i + 1 > output.Count ) {
					output.Add(new DestinationColumn(fields[i]));
				}else {
					if(!FirstRowHeader || rownumber > 1) {
						//if the field value is blank/null, don't guess
						if(fields[i].Trim().Length > 0) {
							//now get the data type
							output[i].DataType = DataTypeGuess(fields[i], output[i].DataType, debug);
							//did we just get a unicode column?
							if(output[i].DataType == "NVarChar")
								treatWholeFileAsUnicode = true;
							if(debug)
								Console.WriteLine("DataTypeGuess returned: " + output[i].DataType);
							
							//get the Maxlength //trying it for all data types
							//if(output[i].DataType == "VarChar" || output[i].DataType == "NVarChar" || output[i].DataType == "VarBinary") {
								if(fields[i].Length > output[i].MaxLength)
									output[i].MaxLength = fields[i].Length;
							//}
							
							//get precision
							switch(output[i].DataType) {
								case "BigInt":
									output[i].Precision = 19;
									break;
								case "Bit":
									output[i].Precision = 1;
									break;							
								case "Date":
								case "Int":
									output[i].Precision = 10;
									break;
								case "DateTime2":
									output[i].Precision = 27;
									break;	
								case "DateTimeOffset":
									output[i].Precision = 34;
									break;
									
								case "Decimal": //could max at 38
								case "Float":
									if(fields[i].Replace(".","").Length > output[i].Precision)
										output[i].Precision = fields[i].Replace(".","").Length;
									break;
								case "SmallInt":
									output[i].Precision = 5;
									break;
								case "Time":
									output[i].Precision = 16;
									break;
								case "TinyInt":
									output[i].Precision = 3;
									break;
								default:
									output[i].Precision = 0;
									break;
							}
							
							//get scale
							switch(output[i].DataType) {
								case "DateTime2":
								case "DateTimeOffset":
								case "Time":
									output[i].Scale = 7;
									break;
								case "Decimal":
									//remember Indexof will "leave the "." in it's length (+1 to ignore the .)
									int Scale = fields[i].Substring(fields[i].IndexOf(".")+1).Length;
									if(Scale > output[i].Scale)
										output[i].Scale = Scale;
									break;
								default:
									output[i].Scale = 0;
									break;
							}
							
						}else {
							//if the value is black/null the column can be nullable
							output[i].Nullable = true;
						
						}
					}
				}
		}
			//for SSIS, if any column is unicode (nvarchar), then all text has to be read as unicode.
			if(treatWholeFileAsUnicode) {
				foreach( DestinationColumn col in output) {
					if(col.DataType == "VarChar")
						col.DataType = "NVarChar";
				}
			}
			//Console.WriteLine(output);
		}
	}
	return output;
}

}

//class to hold information about the destination column(s)
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