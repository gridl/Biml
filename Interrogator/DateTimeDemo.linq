<Query Kind="Program">
  <Namespace>System.Data.SqlTypes</Namespace>
</Query>

/*Let's try to convert an input to a datetime type:
Date	*new work preferred
Time	*new work preferred
DateTimeOffset *new work preferred
Datetime2 *new work preferred
*/
void Main()
{
	//run test x
	int test = 4;
	//all test 1 (datetime2) pass
	//all test 2 (date) pass
	//all test 3 (time) PASS!
	//test 4(datetimeoffset)PASS!

	//now, let's test a lot more data!
	SqlConnection connection = new SqlConnection("Data Source=localhost;Initial Catalog=BimlDemo;Trusted_Connection=True;");
	SqlCommand cmd = new SqlCommand("SELECT * FROM DateAndTime", connection);
	SqlDataAdapter da = new SqlDataAdapter(cmd);
	DataTable dt = new DataTable();
	da.Fill(dt);
	foreach (DataRow dr in dt.Rows)
	{
		switch(test) {
			case 1:
			//test1:testing the smalldatetime --> datetime2
			if( DateTimeGuess(dr["1_SmallDateTime"].ToString()) != SqlDbType.DateTime2) {
				Console.WriteLine("Testing " + dr["1_SmallDateTime"].ToString() + " " + DateTimeGuess(dr["1_SmallDateTime"].ToString()));
			}
			//all converted to datetime2 except the nulls, it assumed date... pass
			break;
			
			case 2:
			//test2:  date --> date
			if( DateTimeGuess(dr["2_date"].ToString()) != SqlDbType.Date) {
				Console.WriteLine("Testing " + dr["2_date"].ToString() + " " + DateTimeGuess(dr["2_date"].ToString()));
			}
			//all values converted to date
			break;
			
			case 3: 
			//test 3: time --> time
			if( DateTimeGuess(dr["3_time"].ToString()) != SqlDbType.Time) {
				DateTimeGuess(dr["3_time"].ToString());
				Console.WriteLine("Testing " + dr["3_time"].ToString() + " " + DateTimeGuess(dr["3_time"].ToString()));
			}
			//regex FTW! Only the blanks don't return time
			break;
			
			
			case 4:
			//test datetimeoffset
			if( DateTimeGuess(dr["4_DateTimeOffset"].ToString()) != SqlDbType.DateTimeOffset) {
				DateTimeGuess(dr["4_DateTimeOffset"].ToString());
				Console.WriteLine("Testing " + dr["4_DateTimeOffset"].ToString() + " " + DateTimeGuess(dr["4_DateTimeOffset"].ToString(), false));
			}
			break;	
			
			//default:
			
		}
	}
	
}

// Define other methods and classes here
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