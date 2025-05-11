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

update assets set placement = 'On Cart' where placement = 'OnCart'
update assets set placement = 'Other Equipment' where placement = 'OE'
update assets set placement = 'Under-Counter' where placement = 'UC'
update project_room_inventory set placement = 'On Cart' where placement = 'OnCart'
update project_room_inventory set placement = 'Other Equipment' where placement = 'OE'
update project_room_inventory set placement = 'Under-Counter' where placement = 'UC'

GO

exec sp_set_session_context 'domain_id', null;