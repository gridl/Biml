@ECHO OFF
SET "RepositoryPath=%1"

IF NOT defined RepositoryPath GOTO Err

CD %1\BimlExpress Project

mklink BimlExpress\00_Configuration.biml "%1\BimlStudio Project\addedBiml\BimlScripts\00_Configuration.biml"
mklink BimlExpress\01_Connections-ADONET.biml "%1\BimlStudio Project\addedBiml\BimlScripts\01_Connections-ADONET.biml"
mklink BimlExpress\01_Connections-OLEDB.biml "%1\BimlStudio Project\addedBiml\BimlScripts\01_Connections-OLEDB.biml
mklink BimlExpress\01_FileFormats.biml "%1\BimlStudio Project\addedBiml\BimlScripts\01_FileFormats.biml"
mklink BimlExpress\02_Connections-FlatFiles.biml "%1\BimlStudio Project\addedBiml\BimlScripts\02_Connections-FlatFiles.biml"
mklink BimlExpress\02_Databases.biml "%1\BimlStudio Project\addedBiml\BimlScripts\02_Databases.biml"
mklink BimlExpress\03_Schemas.biml "%1\BimlStudio Project\addedBiml\BimlScripts\03_Schemas.biml"
mklink BimlExpress\04_Tables_Excel.biml "%1\BimlStudio Project\addedBiml\BimlScripts\04_Tables_Excel.biml"
mklink BimlExpress\04_Tables_FlatFiles.biml "%1\BimlStudio Project\addedBiml\BimlScripts\04_Tables_FlatFiles.biml"
mklink BimlExpress\04_Tables_SQL-ALL.biml "%1\BimlStudio Project\addedBiml\BimlScripts\04_Tables_SQL-ALL.biml"
mklink BimlExpress\05_Packages_CreateObjects.biml "%1\BimlStudio Project\addedBiml\BimlScripts\05_Packages_CreateObjects.biml"
mklink BimlExpress\06_Packages_ExtractFlatFiles.biml "%1\BimlStudio Project\addedBiml\BimlScripts\06_Packages_ExtractFlatFiles.biml" 
mklink /d "Code\" "%1\BimlStudio Project\addedBiml\Code"

GOTO End

:Err
ECHO You must pass the path to Interrogator in the repository in the form "c:\repositories\Biml\Interrogator"

:End