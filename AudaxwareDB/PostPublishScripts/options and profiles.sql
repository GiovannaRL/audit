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

--exec sp_set_session_context 'domain_id', '1';

--GO

--UPDATE inventory_options SET unit_price = 0 WHERE inventory_id IN (SELECT inventory_id FROM project_room_inventory WHERE detailed_budget = 0)
--	OR unit_price IS NULL;

--DECLARE @inventory_id INTEGER, @value NUMERIC(10, 2);

--DECLARE options_cursor CURSOR FOR   
--SELECT inventory_id, SUM(COALESCE(unit_price, 0) * COALESCE(quantity, 1)) FROM inventory_options GROUP BY inventory_id; 

--OPEN options_cursor  

--FETCH NEXT FROM options_cursor INTO @inventory_id, @value;  

--WHILE @@FETCH_STATUS = 0
--BEGIN

--	UPDATE project_room_inventory SET options_unit_price = @value WHERE inventory_id = @inventory_id;

--	FETCH NEXT FROM options_cursor INTO @inventory_id, @value;  
--END

--CLOSE options_cursor;
--DEALLOCATE options_cursor;

--UPDATE project_room_inventory SET options_unit_price = 0 WHERE inventory_id not in (SELECT distinct inventory_id FROM inventory_options);

--GO

--DECLARE @inventory_id INTEGER;
--DECLARE inventory_cursor CURSOR FOR SELECT inventory_id FROM project_room_inventory;

--OPEN inventory_cursor;  

--FETCH NEXT FROM inventory_cursor INTO @inventory_id;  
--WHILE @@FETCH_STATUS = 0
--BEGIN

--	DECLARE @profile VARCHAR(3000), @profile_budget VARCHAR(2000);

--	SELECT @profile = STUFF(( SELECT distinct ';' + CONCAT(ao.display_code, '(', io.quantity, ')') FROM inventory_options as io
--					inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
--					where ao.display_code IS NOT NULL AND ao.display_code <> '' AND inventory_id = @inventory_id FOR XML PATH('')),1 ,1, '');

--	SELECT @profile_budget = CONCAT('(', STUFF(( SELECT ';' + CAST(io.unit_price AS VARCHAR(10)) FROM inventory_options as io
--					inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
--					where ao.display_code IS NOT NULL AND ao.display_code <> '' AND inventory_id = @inventory_id order by ao.display_code FOR XML PATH('')),1 ,1, ''),
--				')');

--	UPDATE project_room_inventory SET asset_profile = @profile, asset_profile_budget = @profile_budget WHERE inventory_id = @inventory_id;

--	FETCH NEXT FROM inventory_cursor INTO @inventory_id;
--END

--CLOSE inventory_cursor;
--DEALLOCATE inventory_cursor;

--GO

--DELETE FROM [profile];

--GO

--INSERT INTO [profile](asset_domain_id, asset_id, [profile])
--SELECT DISTINCT asset_domain_id, asset_id, asset_profile FROM project_room_inventory 
--WHERE asset_profile is not null;

GO

exec sp_set_session_context 'domain_id', null;

