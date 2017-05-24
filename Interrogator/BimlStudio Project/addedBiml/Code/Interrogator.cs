//currently there is a bug in how Biml and DateTime2 work together. Until this is fixed, there is a patch in this code
//see DateTimeGuess() function

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

//class to hold information about the destination column(s)
public class DestinationColumn {
	public string Name {get; set;}
	public string DataType {get; set;}
	public int? MaxLength {get; set;}
	public int? Precision {get; set;}
	public int? Scale {get; set;}
	public bool Nullable {get; set;}
	
	//name only init
	public DestinationColumn (string name){
		Name = name;
		DataType = null;
		MaxLength = null;
		Precision = null;
		Scale = null;
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

public class Interrogator {
	
	//based on http://stackoverflow.com/questions/3776458/split-a-comma-separated-string-with-both-quoted-and-unquoted-strings
	public string[] RegexSplit(string input, string textDelimiter, string columnDelimiter) {
	//build out this expression so we can get any variation of text and column delimiter
	string RegexExpression = "(?:^|" + columnDelimiter + ")(" + textDelimiter + "(?:[^" + textDelimiter + "]+|" + textDelimiter + textDelimiter + ")*" + textDelimiter + "|[^" + columnDelimiter + "]*)";
	//set up the C# regex
	Regex csvSplit = new Regex(RegexExpression, RegexOptions.Compiled);
	//set up the output	
	List<string> list = new List<string>();
	string curr = null;
	foreach (Match match in csvSplit.Matches(input))
	{        
	curr = match.Value;
	if (0 == curr.Length)
	{
	  list.Add("");
	}

	list.Add(curr.TrimStart(','));
	}

	return list.ToArray<string>();
	}
	
	//function to guess which character type the input is
	//This function can handle changes to data type too.
	public SqlDbType CharGuess(string input, string currentDatatype) {
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
	public string DataTypeGuess(string input, string currentDatatype) {
	string output;
	
	//exclude blanks
	if (input.Length < 1)
		return "";
		
	//first try datetimes
	//CurrentDataType must be compatible
	string[] DateTimeCompatibleDataTypes = {null, "Date","Time","DateTimeOffset","DateTime2","DateTime"};
	if( DateTimeCompatibleDataTypes.Contains(currentDatatype) ) {
		
		output = DateTimeGuess(input, currentDatatype).ToString();
		
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
		output = NumericGuess(input, currentDatatype).ToString();
		
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
		output = CharGuess(input, currentDatatype).ToString();
		return output;
	}
	//if all else fails, then resort to binary
	return "VarBinary";
}

	//function to guess what kind of datetime we're dealing with:
	//Date, Time, Datetimeoffset, Datetime2
	SqlDbType DateTimeGuess(string input, string currentDatatype) {

	DateTime givenDateTime = new DateTime();
	SqlDbType output;
	
	if(DateTime.TryParse(input, out givenDateTime)) {
		output = SqlDbType.DateTime;		
	} else {	
		output = SqlDbType.VarBinary;
		//exit early!
		return output;
	}
	
	//since we now know we have some kind of date time the rest of the tests are safe
	//is it just a time?
	try{
			//this pattern should match time and not datetime
			string pattern = @"^([0-9]{1,2}:[0-9]{1,2})$|^([0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2})$|^([0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}\.[0-9]{0,7})$";
			//^([0-9]{1,2}:[0-9]{1,2}.{0,1}[0-9]{0,7})
			Regex r = new Regex(pattern);
				
			if (r.IsMatch(input)) {
				//return early, you found a time!
				return SqlDbType.Time;
			}
		} catch  {
			//do nothing
		}
	
	//is it just a date?	
	try {	
		if(givenDateTime == givenDateTime.Date && output != SqlDbType.Time) {
			output = SqlDbType.Date;
		}
	} catch  {
		//our default (aka, try something else)
		return output;
	}
	
	//do we have datetime offset?
	try {
		//if the DateTime.Kind is something other than unspecified, it's offset
		if(givenDateTime.Kind != System.DateTimeKind.Unspecified && output == SqlDbType.DateTime2) 
			output = SqlDbType.DateTimeOffset;	
	} catch  {
		return SqlDbType.VarBinary;
	}

	return output;
}

	//function to guess what kind of numeric we're dealing with:
	//TinyInt, SmallInt, Int, BigInt, Decimal, Float
	public SqlDbType NumericGuess(string input, string currentDatatype) {
	Boolean givenBoolean;
	Byte givenByte;
	Int16 givenInt16;
	Int32 givenInt32;
	Int64 givenInt64;
	Decimal givenDecimal;
	Double givenDouble;

		
	//try to cast to bit (boolean)
	if(Boolean.TryParse(input, out givenBoolean)) {
		//early exit
		return SqlDbType.Bit;			
	} 

	//try to cast to TinyInt(byte)
	if(Byte.TryParse(input, out givenByte)) {
		//early exit
		return SqlDbType.TinyInt;			
	} 
	
	//try to cast to smallint (int16)
	if(Int16.TryParse(input, out givenInt16)) {
		//early exit
		return SqlDbType.SmallInt;			
	} 
	
	//try to cast to int(int32)
	if(Int32.TryParse(input, out givenInt32)) {
		//early exit
		return SqlDbType.Int;			
	} 
	
	//try to cast to bigint(int64)
	if(Int64.TryParse(input, out givenInt64)) {
		//early exit
		return SqlDbType.BigInt;			
	} 
	
	//try to cast to Decimal
	//should we return the length here, or at the calling function?
	if(Decimal.TryParse(input, out givenDecimal)) {
		//early exit
		return SqlDbType.Decimal;			
	} 
	
	//try to cast to float(double)
	if(Double.TryParse(input, out givenDouble)) {
		//early exit
		return SqlDbType.Float;			
	} 
		
	//if we can't determine the type by the end, default to binary.
	return SqlDbType.VarBinary;

}

	//process a file, return a list of columns
	public List<DestinationColumn> ProcessFile(string FileName, char ColumnDelimiter
		, bool FirstRowHeader = true, int HeaderRowsToSkip = 0, string TextQualifier = null) {
		
		List<DestinationColumn> output = new List<DestinationColumn>();
		//by default do not treat the whole file as unicode
		bool treatWholeFileAsUnicode = false;
			
		using (StreamReader reader = new StreamReader(FileName)) {
			//initialize the rownumber
			int rownumber = 0;
			while (!reader.EndOfStream) {
				//if we have HeaderRowsToSkip, then skip them
				if( rownumber < HeaderRowsToSkip) {
					rownumber++;
				} else {
					//Processing row
					//replacing the simple split with a regex split
					string[] fields = RegexSplit(reader.ReadLine(), TextQualifier.ToString(), ColumnDelimiter.ToString());
					rownumber++;
					
					for(int i=0; i < fields.Count(); i++) {
						//get rid of TextQualifier
						if(TextQualifier != null)
							fields[i] = fields[i].Replace(TextQualifier,"");
						
						//if you get a new column (in first or 101st line) add it's name to output
						if(i + 1 > output.Count ) {
							//handle blank names error
							if(FirstRowHeader && fields[i].ToString().Length > 0 ) {
									output.Add( new DestinationColumn(fields[i]) );
							} else {
								output.Add( new DestinationColumn( "Column" + i.ToString() ) );
							}
						}else {
							if(!FirstRowHeader || rownumber > 1) {
								//if the field value is blank/null, don't guess
								if(fields[i].Trim().Length > 0) {
									//now get the data type
									output[i].DataType = DataTypeGuess(fields[i].Trim(), output[i].DataType);

									//did we just get a unicode column?
									if(output[i].DataType == "NVarChar")
										treatWholeFileAsUnicode = true;
									
									//get the Maxlength //trying it for all data types
									if(fields[i].Length > (output[i].MaxLength ?? 0) )
										output[i].MaxLength = fields[i].Length;
									
									//fix MaxLengths
									switch(output[i].DataType) {
										case "VarChar":
										case "NVarChar":
										case "Char":
										case "NChar":
											break;
										default:
											output[i].MaxLength = null;
											break;
											
									}

									//get precision
									switch(output[i].DataType) {
										case "Bit":
										case "Boolean":
										case "Byte":
										case "Date":
										case "DateTime":
										case "BigInt":
										case "Int":
										case "SmallInt":
										case "TinyInt":
											output[i].Precision = null;
											break;
										case "DateTime2":
											output[i].Precision = 27;
											break;		
										case "DateTimeOffset":
											output[i].Precision = 34;
											break;
										case "Decimal": //could max at 38
										case "Float":
											if(fields[i].Replace(".","").Length > (output[i].Precision ?? 0) )
												output[i].Precision = fields[i].Replace(".","").Length;
											break;
										
										case "Time":
											output[i].Precision = 7;
											break;
									}
									
									//get scale
									switch(output[i].DataType) {
										case "Date":
										case "DateTime":
											output[i].Scale = null;
											break;
										case "DateTime2":
										case "DateTimeOffset":
										case "Time":
											output[i].Scale = 7;
											break;
										case "Decimal":
										case "Float":
											//remember Indexof will "leave the "." in it's length (+1 to ignore the .)
											int Scale = fields[i].Substring(fields[i].IndexOf(".")+1).Length;
											if(Scale > (output[i].Scale ?? 0) )
												output[i].Scale = Scale;
											break;
									}
									
								}else {
									//if the value is black/null the column can be nullable
									output[i].Nullable = true;
								
								}
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
			}
			//if we don't have a datatype at all, default to varbinary
			foreach( DestinationColumn col in output) {
				if( col.DataType == null ) {
					col.DataType = "VarBinary";
				}
			}	
		}
	
		return output;
	}

}

