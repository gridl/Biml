<Query Kind="Expression">
  <Connection>
    <ID>72c01465-7893-40a6-955e-80db61e5c848</ID>
    <Server>.</Server>
    <Database>AdventureWorks2014</Database>
    <ShowServer>true</ShowServer>
  </Connection>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Namespace>System.Net</Namespace>
</Query>

//https://www.linqpad.net/WhyLINQBeatsSQL.aspx

//Persons.Where (p => p.LastName.StartsWith("a") )
//Persons.First(p => p.LastName.StartsWith("a") )
//Persons.Any(p => p.LastName == "Lowder")
//Persons.Take(10)
//Persons.Skip(10).Take(10)