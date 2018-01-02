
In Biml/Interrogator you'll find [Interrogator.cs] ()  This is the secret sauce of this whole project. It reads whole files and extracts metadata by applying rules to select an appropriate data type for a column in a delimited file.

You'll also find a [BimlExpress] and a [BimlStudio] project that use the interrogator.  Previously you had to use a batch file to keep "shared" code in sync between the two projects.  That was too difficult, so I've eliminated it. 