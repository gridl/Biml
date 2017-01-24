<Query Kind="Program">
  <Reference Relative="..\..\..\PinTrader\WebScraper\packages\HtmlAgilityPack.1.4.6\Net45\HtmlAgilityPack.dll">C:\Repositories\PinTrader\WebScraper\packages\HtmlAgilityPack.1.4.6\Net45\HtmlAgilityPack.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.VisualBasic.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.dll</Reference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>Microsoft.VisualBasic.FileIO</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

void Main()
{
	//process each file in a source folder
	string sourceFolder = @"C:\Repositories\Biml\Interrogator\testdata";
	string[] acceptedFilePatterns = new string[]{".csv",".txt"};
	
	DirectoryInfo dir = new DirectoryInfo(sourceFolder);
	
	foreach (var file in dir.GetFiles()){
		//Console.WriteLine(file.Extension);
		if( acceptedFilePatterns.Contains(file.Extension)) {
			Console.WriteLine(file.FullName);
		}
	}
}

// Define other methods and classes here
