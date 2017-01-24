<Query Kind="Program" />

void Main()
{
	double output = new double();
	
	Stopwatch stopWatch = new Stopwatch();
	stopWatch.Start();
	//1M test
	for(int i=1; i < 1000000; i++) {
		//Double.TryParse("2.34567",out output);	
		//RunTime 00:00:00.13
		//Double.TryParse("bad",out output);	
		//RunTime 00:00:00.09
		
		//Convert.ToDouble("2.34567");
		//RunTime 00:00:00.13
		try{
			Convert.ToDouble("bad");
		}catch {}
		//RunTime 00:00:26.51
	}
	
	
	stopWatch.Stop();
	
	TimeSpan ts = stopWatch.Elapsed;

        // Format and display the TimeSpan value.
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        Console.WriteLine("RunTime " + elapsedTime);	
}
