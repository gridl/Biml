<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.VisualBasic.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.dll</Reference>
  <Namespace>System.Net</Namespace>
</Query>

//using Microsoft.VisualBasic.FileIO;

void Main()
{
	//run test x
	int test = 1;
	//test 1:bit All pass!
	//test 2:tinyint all pass!
	//test 3:smallint pass
	//test 4:pass
	//test 5:pass
	//test 6:pass
	//test 7: defaults to decimal.
	
	//yes, the test file is hardcoded
	//using (TextFieldParser parser = new TextFieldParser(@"C:\Repositories\Biml\Interrogator\testdata\Numerics.csv"))
	String FileName = @"C:\Repositories\Biml\Interrogator\testdata\Numerics.csv";
	
	using (StreamReader reader = new StreamReader(FileName))
	{
		//initialize the rownumber
		int rownumber = 0;
		while (!reader.EndOfStream) 
		{
			//Processing row
			string[] fields = reader.ReadLine().Split(',');
			rownumber++;
			//quick spin through the fields to trim the " and spaces
			for(int i=0; i < fields.Count(); i++) {
				//get rid of leading "
				if(fields[i].Trim().StartsWith("\""))
					fields[i] = fields[i].Trim().Substring(1);
				
				//get rid of trailing "
				if(fields[i].Trim().EndsWith("\""))
					fields[i] = fields[i].Trim().Substring(0, fields[i].Trim().Length -1);
			}
			
			//skip the header row for this test
			if(rownumber > 2) {
				switch(test) {
					case 1:
					//test1:bit //test case ignores "" now
					if( NumericGuess(fields[0], false) != SqlDbType.Bit && fields[0] != "") {
						NumericGuess(fields[0], true);
						Console.WriteLine("Testing " + fields[0] + " " + NumericGuess(fields[0]), false);
					}
					break;
					
					case 2:
					//test2:TinyInt
					if( NumericGuess(fields[1]) != SqlDbType.TinyInt && fields[1] != "") {
						Console.WriteLine("Testing " + fields[1] + " " + NumericGuess(fields[1]));
					}
					break;
					
					case 3:
					//test3:SmallInt
					if( NumericGuess(fields[2]) != SqlDbType.SmallInt && fields[2] != "") {
						//NumericGuess(dr["3_SmallInt"].ToString(), true);
						Console.WriteLine("Testing " + fields[2] + " " + NumericGuess(fields[2]));
					}
					break;
					
					case 4:
					//test4:Int
					if( NumericGuess(fields[3]) != SqlDbType.Int && fields[3] != "") {
						Console.WriteLine("Testing " + fields[3] + " " + NumericGuess(fields[3]));
					}
					break;
					
					case 5:
					//test5:BigInt
					if( NumericGuess(fields[4]) != SqlDbType.BigInt && fields[4] != "") {
						Console.WriteLine("Testing " + fields[4] + " " + NumericGuess(fields[4]));
					}
					break;
					
					case 6:
					//test6:Decimal
					if( NumericGuess(fields[5]) != SqlDbType.Decimal && fields[5] != "") {
						Console.WriteLine("Testing " + fields[5] + " " + NumericGuess(fields[5]));
					}
					break;
					
					case 7:
					//test7:Float
					if( NumericGuess(fields[6]) != SqlDbType.Float && fields[6] != "") {
						Console.WriteLine("Testing " + fields[6] + " " + NumericGuess(fields[6]));
					}
					break;
				}
			}
		}
	}
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
	return SqlDbType.VarBinary;

}