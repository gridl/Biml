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
	int test = 3;
	//all test 1 (datetime2) pass
	//all test 2 (date) pass
	//all test 3 (time) FAIL

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
				DateTimeGuess(dr["3_time"].ToString(), true);
				Console.WriteLine("Testing " + dr["3_time"].ToString() + " " + DateTimeGuess(dr["3_time"].ToString()));
			}
			//checking "06:32:17.5201796", it converted to datetime2
			//after the fix, only the nulls convert to date.
			break;
			
			
			case 4:
			//test datetimeoffset
			if( DateTimeGuess(dr["4_DateTimeOffset"].ToString(), false) != SqlDbType.DateTimeOffset) {
				DateTimeGuess(dr["4_DateTimeOffset"].ToString(), true);
				Console.WriteLine("Testing " + dr["4_DateTimeOffset"].ToString() + " " + DateTimeGuess(dr["4_DateTimeOffset"].ToString(), false));
			}
			break;	
			
			//default:
			
		}
	}
	//Console.Write(DateTimeGuess("06:32:17.5201796", true));	
	//fixed the bug where if you had 2017 in the ms, it detected as datetime
}

// Define other methods and classes here
SqlDbType DateTimeGuess(string input, bool debug = false) {
	
	if(debug) 
		Console.WriteLine("input: " + input);
	SqlDbType output;
	//first do we have something that can be converted to datetime at all?
	try{
		
		DateTime givenDateTime = new DateTime();
		DateTime.TryParse(input, out givenDateTime);
		//need a short date to test for time only
		DateTime shortdate = new DateTime();
		DateTime.TryParse(givenDateTime.ToShortDateString(), out shortdate);
		
		//we have a datetime at least
		output = SqlDbType.DateTime2;
		
		if(debug) 
			Console.WriteLine(givenDateTime);		
				
		//ok, we have a datetime. is it just a date?
		//Console.WriteLine(givenDateTime.Date);
		if(givenDateTime == givenDateTime.Date) {
			//output = SqlDbType.Date;
			//early exit
			return SqlDbType.Date;
		}
		
		//do we have just a time?
		//if we can't find the year, month and day in it, then it's a time only)
		if(debug) {
			Console.WriteLine(input.Contains(givenDateTime.Year.ToString()));
			//Console.WriteLine(givenDateTime.ToShortDateString());
			Console.WriteLine(shortdate);
			Console.WriteLine(shortdate.AddDays(-1).ToString());
			Console.WriteLine(shortdate.AddDays(1).ToString());
			Console.WriteLine(input.Contains(shortdate.Month.ToString()));
			Console.WriteLine(input.Contains(shortdate.AddDays(-1).Month.ToString()));
			Console.WriteLine(input.Contains(shortdate.AddDays(1).Month.ToString()));
		}
		//if the year is not in the input
		if(! input.Contains(givenDateTime.Year.ToString()) ) {
			//and the month (+/-1 day due to offsets) isn't in the input
			if(! (input.Contains(shortdate.Month.ToString()) && input.Contains(shortdate.AddDays(-1).Month.ToString()) && input.Contains(shortdate.AddDays(1).Month.ToString()) ) ) {
				//and the day isn't in the input, then we have a time
				if(! (input.Contains(shortdate.Day.ToString()) && input.Contains(shortdate.AddDays(-1).Day.ToString()) && input.Contains(shortdate.AddDays(1).Day.ToString()) ) ) {
					//output = SqlDbType.Time;
					//early exit 
					return SqlDbType.Time;
				}
			}
		}
		//do we have offset information?
		//Console.WriteLine(givenDateTime.ToLocalTime());
		//Console.WriteLine(givenDateTime.ToUniversalTime());
		
		//if the DateTime.Kind is something other than unspecified, it's offset
		if(givenDateTime.Kind != System.DateTimeKind.Unspecified) 
			output = SqlDbType.DateTimeOffset;
		
		return output;
	
	} catch {
		//our default (aka, try something else)
		return SqlDbType.VarBinary;
	}
	

}