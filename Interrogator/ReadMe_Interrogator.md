
In Biml/Interrogator you'll find [Interrogator.cs](https://github.com/shannonlowder/Biml/tree/master/Interrogator/Interrogator.cs)  This is the secret sauce of this whole project. It reads whole files and extracts metadata by applying rules to select an appropriate data type for a column in a delimited file.

You'll also find a [BimlExpress Project](https://github.com/shannonlowder/Biml/tree/master/Interrogator/BimlExpress%20Project) and a [BimlStudio Project](https://github.com/shannonlowder/Biml/tree/master/Interrogator/BimlStudio%20Project) project that use the interrogator.  Previously you had to use a batch file to keep "shared" code in sync between the two projects.  That was too difficult, so I've eliminated it. 

[LINQPad Tests](https://github.com/shannonlowder/Biml/tree/master/Interrogator/LINQPad%20Tests) -- when I was developing the interrogator, I found it easier to use LINQPad to quickly scratch out some code.  This is my original work.

[testdata](https://github.com/shannonlowder/Biml/tree/master/Interrogator/testdata) -- here are some sample files I've used to test my code.
