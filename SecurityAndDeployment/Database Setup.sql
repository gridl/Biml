--if the DB user exists, drop it first
Use [AdventureWorks2014]
GO
IF EXISTS (SELECT loginname FROM master.dbo.syslogins 
    WHERE name = 'Biml' AND dbname = 'AdventureWorks2014')
DROP USER Biml
GO
USE [master]
GO
--you'll have to kill any sessions that show up here:
SELECT session_id FROM sys.dm_exec_sessions WHERE login_name = 'Biml'
--kill 58

IF EXISTS (SELECT loginname FROM master.dbo.syslogins 
    WHERE name = 'Biml' )
DROP LOGIN Biml
GO

CREATE LOGIN [Biml] WITH PASSWORD=N'Password101!'
    , DEFAULT_DATABASE=[master]
    , DEFAULT_LANGUAGE=[us_english]
    , CHECK_EXPIRATION=OFF
    , CHECK_POLICY=OFF
GO
ALTER LOGIN [Biml] ENABLE
GO


USE [AdventureWorks2014]
GO
CREATE USER [Biml] FOR LOGIN [Biml]

GO
GRANT VIEW DEFINITION TO [Biml]


USE [BimlExtract]
GO
CREATE USER [Biml] FOR LOGIN [Biml]
GO
ALTER ROLE [db_owner] ADD MEMBER [Biml]
GO

GO


