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


    UPDATE User_gridView 
    SET grid_state = REPLACE(CAST(grid_state AS VARCHAR(MAX)), 'serial_', 'model_')
    UPDATE project_room_inventory SET serial_number = null;


--IF EXISTS (SELECT 1 FROM assets WHERE model_number IS NULL) 
--BEGIN
--    UPDATE assets 
--    SET model_number = serial_number, model_name = serial_name;
    
--    UPDATE project_room_inventory 
--    SET model_number = serial_number, model_name = serial_name, model_number_ow = serial_number_ow, model_name_ow = serial_name_ow;

--END 

/*
TODO LATER: REMOVE COLUMNS AND DATA FROM TABLE
ALTER TABLE project_room_inventory DROP COLUMN serial_name, serial_number_ow, serial_name_ow;
ALTER TABLE assets DROP COLUMN serial_name, serial_number;
*/

exec sp_set_session_context 'domain_id', null;
