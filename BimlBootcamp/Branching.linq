<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Namespace>System.Net</Namespace>
</Query>

void Main()
{
	/*
	System.Int16 test = new Int16();

	int a = 1;
	
	a++;
	
	if(a == 1) {
		Console.WriteLine("a is one");
	}else {
		Console.WriteLine("a is not one");
	}
	
	*/
	int b = 100; 
	/*
	switch(b) {
		case 1:
			Console.WriteLine("case 1");
			break;
		case 2:
		case 3:
			Console.WriteLine("case " + b.ToString());
			break;
		default:
			Console.WriteLine("default case");
			break;	
	}*/
	
	do {
		b+=1;
		Console.WriteLine(b);
	}
	while (b <200);
	
		
	//} while (true);
	/*
	for(int i = 1; i <= 10; i++) {
		Console.WriteLine ("Guess how we will make package " + i.ToString());
	}
	*/
	
	/*
	string[] packageType = {"extract","transform","load"};
	
	foreach(var pt in packageType) {
		Console.WriteLine(pt + " package");
	}
	*/
	
	/*
	int c = 1;
	Random r = new Random();
	while (c < 10)
	{
        
		Console.WriteLine("building package " + c.ToString());
		c+=r.Next(1,3);
	}
	*/
}

// Define other methods and classes here