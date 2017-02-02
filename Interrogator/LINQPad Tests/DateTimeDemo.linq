<Query Kind="Program">
  <Namespace>System.Data.SqlTypes</Namespace>
</Query>

/*Let's try to convert an input to a datetime type:
Date	*new work preferred
Time	*new work preferred
DateTimeOffset *new work preferred
Datetime2 *new work preferred
*/
void Main() {
	//run test x
	int test = 3;
	//all test 1 (datetime2) pass
	//all test 2 (date) pass
	//all test 3 (time) PASS!
	//test 4(datetimeoffset)PASS!

	//now, let's test a lot more data!
	String FileName = @"C:\Repositories\Biml\Interrogator\testdata\DateAndTime.csv";
	
	using (StreamReader reader = new StreamReader(FileName)) {
		//initialize the row number
		int rownumber = 0;
		
		while (!reader.EndOfStream) {
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
					//test1:testing the smalldatetime --> datetime2
					if( DateTimeGuess(fields[0], null) != SqlDbType.DateTime2) {
						Console.WriteLine("Testing " + fields[0] + " " + DateTimeGuess(fields[0], null));
					}
					//all converted to datetime2 except the nulls, it assumed date... pass
					break;
					
					case 2:
					//test2:  date --> date
					if( DateTimeGuess(fields[1], null) != SqlDbType.Date) {
						Console.WriteLine("Testing " + fields[1] + " " + DateTimeGuess(fields[1], null));
					}
					//all values converted to date
					break;
					
					case 3: 
					//test 3: time --> time
					if( DateTimeGuess(fields[2], null, true) != SqlDbType.Time) {
						DateTimeGuess(fields[2], null);
						Console.WriteLine("Testing " + fields[2] + " " + DateTimeGuess(fields[2], null,false));
					}
					//regex FTW! Only the blanks don't return time
					break;
					
					case 4:
					//test datetimeoffset
					if( DateTimeGuess(fields[3], null) != SqlDbType.DateTimeOffset) {
						DateTimeGuess(fields[3], null);
						Console.WriteLine("Testing " + fields[3] + " " + DateTimeGuess(fields[3], null));
					}
					break;	
					
					//default:
					//nothing
				}
			}
		}
	}
	
}

// Define other methods and classes here
SqlDbType DateTimeGuess(string input, string currentDatatype, bool debug = false) {

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