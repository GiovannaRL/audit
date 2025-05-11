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
exec sp_set_session_context 'domain_id', '1';

GO

DELETE FROM AspNetUserClaims WHERE ClaimType = 'Enterprise';

INSERT INTO AspNetUserClaims(UserId, ClaimType, ClaimValue) SELECT Id, 'Enterprise', domain_id FROM AspNetUsers;

GO

exec sp_set_session_context 'domain_id', null;
