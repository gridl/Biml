<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

void Main()
{

	Vehicle myVehicle = new Vehicle();
	//Vehicle myVehicle = new Vehicle(1, false);
	
	Console.WriteLine(myVehicle);
	
	//turn off the privateMethod in vehicle.constructors first
	//Unicycle myUnicycle = new Unicycle();
	//Console.WriteLine(myUnicycle);
	
	//value and reference
	//value
	/*
	ValueDemo a = new ValueDemo();
	a.myInt = 1;
	ValueDemo b = a;
	b.myInt++;
	Console.WriteLine(a);
	Console.WriteLine(b);
	*/
	//reference
	/*
	ReferenceDemo c = new ReferenceDemo();
	c.myInt = 1;
	ReferenceDemo d = c;
	d.myInt = 2;
	
	Console.WriteLine(c);
	Console.WriteLine(d);
	*/
}

public class Vehicle
{
    public int? WheelCount;
	public bool? HasEngine;


    public Vehicle() {
	  WheelCount = null;
	  HasEngine = null;
	  PrivateMethod();
	}
	
	public Vehicle(int wheelCount, bool hasEngine) {
	  WheelCount = wheelCount;
	  HasEngine = hasEngine;	
	  PrivateMethod();
	}
	private void PrivateMethod() {
		Console.WriteLine("private method");
	}
	protected void ProtectedMethod() {
		Console.WriteLine("Protected Method");
	}

}

public class Unicycle : Vehicle {

	public Unicycle () {
		WheelCount = 1;
		HasEngine = false;
		//PrivateMethod();
		//ProtectedMethod();
	}
	
}

public class ReferenceDemo {
	public int myInt;
}
public struct ValueDemo {
	public int myInt;
}
