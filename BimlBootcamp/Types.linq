<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

void Main()
{
	string name = "shannon";
	Console.WriteLine(name.GetType());
	
	//name = 1;
	//name = 1.ToString()
	//int age = 39;
	//Console.WriteLine(age/1);
	//Console.WriteLine(name/1);
	//var test = 1;
	//Console.WriteLine(test.GetType());
	//Console.WriteLine(testFunction());
}

string testfunction() {
	return "test";
	//return 1;
}


