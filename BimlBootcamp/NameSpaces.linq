<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

//https://msdn.microsoft.com/en-us/library/system.net.httpwebrequest(v=vs.110).aspx
//https://msdn.microsoft.com/en-us/library/system.web.httprequest(v=vs.110).aspx

void Main()
{
	test1 whichClass = new test1();
	//test2 whichClass = new test2();
	whichClass.output();
}

// Define other methods and classes here
class test1 {

	public void output() {
		Console.WriteLine("Class test1");
	}
	
}
class test2 {

	public void output() {
		Console.WriteLine("Class test2");
	}
}
