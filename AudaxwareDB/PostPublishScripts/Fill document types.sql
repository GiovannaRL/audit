/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
EXEC sp_set_session_context 'domain_id', '1';

GO

DELETE FROM document_types;

GO

INSERT INTO document_types VALUES('Custom');
INSERT INTO document_types VALUES('Shop Drawing');

GO

EXEC sp_set_session_context 'domain_id', null;