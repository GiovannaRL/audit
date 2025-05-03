CREATE TABLE [dbo].[inventory_options] (
    [inventory_id] INT             NOT NULL,
    [option_id]    INT             NOT NULL,
    [domain_id]    SMALLINT        NOT NULL,
    [unit_price]   NUMERIC (10, 2) NULL,
    [quantity]     INT             NOT NULL,
	[inventory_domain_id] SMALLINT NULL, -- Only allow null because was added later
	[document_id]	INT NULL,
	[document_domain_id] SMALLINT NULL
    PRIMARY KEY CLUSTERED ([inventory_id] ASC, [option_id] ASC, [domain_id] ASC),
    CONSTRAINT [check_quantity] CHECK ([quantity]>=(1)),
    CONSTRAINT [inventory_fk] FOREIGN KEY ([inventory_id]) REFERENCES [dbo].[project_room_inventory] ([inventory_id]) ON DELETE CASCADE,
    CONSTRAINT [inventory_options_fk] FOREIGN KEY ([option_id], [domain_id]) REFERENCES [dbo].[assets_options] ([asset_option_id], [domain_id]),
	CONSTRAINT [inventory_options_document_fk] FOREIGN KEY ([document_domain_id], [document_id]) REFERENCES [dbo].[domain_document] ([domain_id], [id]) ON DELETE CASCADE ON UPDATE CASCADE
);

GO

CREATE TRIGGER [dbo].[update_none_option]
ON [dbo].[inventory_options] INSTEAD OF INSERT
AS

	UPDATE project_room_inventory SET none_option = 0 WHERE none_option = 1 AND inventory_id IN
		(SELECT inventory_id FROM inserted);

	INSERT INTO inventory_options SELECT * FROM inserted;
GO


CREATE TRIGGER [dbo].[options_ids_profile_delete]
    ON [dbo].[inventory_options]
    AFTER DELETE
    AS
	DECLARE @inventory_id INT, @option_id INTEGER, @unit_price NUMERIC(10, 2), @quantity INTEGER, @detailed_budget BIT;
    BEGIN
		DECLARE io_cursor CURSOR LOCAL FOR SELECT d.inventory_id, option_id, unit_price, quantity, detailed_budget 
			FROM deleted d LEFT JOIN project_room_inventory pri ON d.inventory_id = pri.inventory_id;
		OPEN io_cursor;
		FETCH NEXT FROM io_cursor INTO @inventory_id, @option_id, @unit_price, @quantity, @detailed_budget;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			UPDATE project_room_inventory SET option_ids = REPLACE(option_ids, CONCAT(',', CAST(@option_id as varchar(10))), '') WHERE inventory_id = @inventory_id;
			UPDATE project_room_inventory SET option_ids = REPLACE(option_ids, CONCAT(CAST(@option_id as varchar(10)), ','), '') WHERE inventory_id = @inventory_id;
			UPDATE project_room_inventory SET option_ids = REPLACE(option_ids, CAST(@option_id as varchar(10)), '') WHERE inventory_id = @inventory_id;

			UPDATE project_room_inventory SET option_ids = NULL WHERE inventory_id = @inventory_id AND option_ids = '';

			update project_room_inventory set asset_profile = STUFF(( SELECT distinct ';' + CONCAT(ao.display_code, '(', io.quantity, ')') FROM inventory_options as io
					inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
					where ao.display_code IS NOT NULL AND ao.display_code <> '' AND inventory_id = @inventory_id FOR XML PATH('')),1 ,1, ''),
				asset_profile_budget = CONCAT('(', STUFF(( SELECT ';$' + CAST(io.unit_price AS VARCHAR(10)) FROM inventory_options as io
					inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
					where ao.display_code IS NOT NULL AND ao.display_code <> '' AND inventory_id = @inventory_id order by ao.display_code FOR XML PATH('')),1 ,1, ''),
				')')
				where inventory_id = @inventory_id;

			IF @detailed_budget = 1
				UPDATE project_room_inventory SET options_unit_price = COALESCE(options_unit_price, 0) - (COALESCE(@unit_price, 0) * COALESCE(@quantity, 1)) WHERE inventory_id = @inventory_id;

			FETCH NEXT FROM io_cursor INTO @inventory_id, @option_id, @unit_price, @quantity, @detailed_budget;
		END
		CLOSE io_cursor;  
		DEALLOCATE io_cursor;
    END
GO

CREATE TRIGGER [dbo].[options_ids_profile_insert]
    ON [dbo].[inventory_options]
    AFTER INSERT
    AS
    BEGIN
        DECLARE @inventory_id INTEGER, @unit_price NUMERIC(10, 2), @quantity INTEGER, 
			@detailed_budget BIT, @domain_id SMALLINT, @option_id INTEGER, @inventory_domain_id SMALLINT;
	
		DECLARE cur CURSOR LOCAL FOR SELECT pri.domain_id, i.inventory_id, unit_price, quantity, detailed_budget, i.domain_id, option_id 
			FROM inserted i LEFT JOIN project_room_inventory pri ON i.inventory_id = pri.inventory_id;
		OPEN cur;
		FETCH NEXT FROM cur INTO @inventory_domain_id, @inventory_id, @unit_price, @quantity, @detailed_budget, @domain_id, @option_id;
		WHILE @@FETCH_STATUS = 0
		BEGIN

				/* options budget */
				IF @detailed_budget = 1
					BEGIN 
						UPDATE project_room_inventory SET options_unit_price = COALESCE(options_unit_price, 0) + (COALESCE(@unit_price, 0) * COALESCE(@quantity, 1)) WHERE inventory_id = @inventory_id;
						UPDATE inventory_options SET inventory_domain_id = @inventory_domain_id WHERE domain_id = @domain_id AND option_id = @option_id AND inventory_id = @inventory_id;
					END
				ELSE
					UPDATE inventory_options SET unit_price = 0, inventory_domain_id = @inventory_domain_id WHERE domain_id = @domain_id AND option_id = @option_id AND inventory_id = @inventory_id;

				/* options id and profile */
				update project_room_inventory set option_ids =
					(STUFF(( select ',' + cast(a.option_id as varchar(10)) from inventory_options a 
						where a.inventory_id = @inventory_id order by option_id FOR XML PATH('')),1 ,1, '')),
				asset_profile = STUFF(( SELECT distinct ';' + CONCAT(ao.display_code, '(', io.quantity, ')') FROM inventory_options as io
					inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
					where ao.display_code IS NOT NULL AND ao.display_code <> '' AND inventory_id = @inventory_id FOR XML PATH('')),1 ,1, ''),
				asset_profile_budget = CONCAT('(', STUFF(( SELECT ';$' + CAST(io.unit_price AS VARCHAR(10)) FROM inventory_options as io
					inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
					where ao.display_code IS NOT NULL AND ao.display_code <> '' AND inventory_id = @inventory_id order by ao.display_code FOR XML PATH('')),1 ,1, ''),
				')')
				where inventory_id = @inventory_id;

				FETCH NEXT FROM cur INTO @inventory_domain_id, @inventory_id, @unit_price, @quantity, @detailed_budget, @domain_id, @option_id;
		END
		CLOSE cur;
		DEALLOCATE cur;
    END
GO

CREATE TRIGGER [dbo].[asset_profile_update]
    ON [dbo].[inventory_options]
    AFTER UPDATE
    AS
    BEGIN
         DECLARE @inventory_id INTEGER, @unit_price NUMERIC(10, 2), @quantity INTEGER, 
			@detailed_budget BIT, @domain_id SMALLINT, @option_id INTEGER, @inventory_domain_id SMALLINT;
	
		/*subtract the unit_price of the deleted object*/
		DECLARE deleted_options_cursor CURSOR LOCAL FOR SELECT d.inventory_id, unit_price, quantity, detailed_budget 
			FROM deleted d LEFT JOIN
			project_room_inventory pri ON d.inventory_id = pri.inventory_id;

		OPEN deleted_options_cursor;
		FETCH NEXT FROM deleted_options_cursor INTO @inventory_id, @unit_price, @quantity, @detailed_budget;  
		WHILE @@FETCH_STATUS = 0
		BEGIN

			IF @detailed_budget = 1
				UPDATE project_room_inventory SET options_unit_price = COALESCE(options_unit_price, 0) - (COALESCE(@unit_price, 0) * COALESCE(@quantity, 1)) WHERE inventory_id = @inventory_id;	

			FETCH NEXT FROM deleted_options_cursor INTO @inventory_id, @unit_price, @quantity, @detailed_budget;
		END
		CLOSE deleted_options_cursor;  
		DEALLOCATE deleted_options_cursor; 

		SET @unit_price = NULL;
		SET @quantity = NULL;

		DECLARE cur_prof CURSOR LOCAL FOR SELECT pri.domain_id, i.inventory_id, unit_price, quantity, detailed_budget, i.domain_id, option_id 
			FROM inserted i LEFT JOIN project_room_inventory pri ON i.inventory_id = pri.inventory_id;
		OPEN cur_prof;
		FETCH NEXT FROM cur_prof INTO @inventory_domain_id, @inventory_id, @unit_price, @quantity, @detailed_budget, @domain_id, @option_id;
		WHILE @@FETCH_STATUS = 0
		BEGIN
				
				IF @detailed_budget = 1
					BEGIN
						UPDATE project_room_inventory SET options_unit_price = COALESCE(options_unit_price, 0) + (COALESCE(@unit_price, 0) * COALESCE(@quantity, 1)) WHERE inventory_id = @inventory_id;
						UPDATE inventory_options SET inventory_domain_id = @inventory_domain_id WHERE domain_id = @domain_id AND option_id = @option_id AND inventory_id = @inventory_id;
					END
				ELSE IF @unit_price > 0
					UPDATE inventory_options SET unit_price = 0, inventory_domain_id = @inventory_domain_id WHERE @inventory_id = inventory_id AND option_id = @option_id AND domain_id = @domain_id;

				update project_room_inventory set asset_profile = STUFF(( SELECT distinct ';' + CONCAT(ao.display_code, '(', io.quantity, ')') FROM inventory_options as io
					inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
					where ao.display_code IS NOT NULL AND ao.display_code <> '' AND inventory_id = @inventory_id FOR XML PATH('')),1 ,1, ''),
				asset_profile_budget = CONCAT('(', STUFF(( SELECT ';$' + CAST(io.unit_price AS VARCHAR(10)) FROM inventory_options as io
					inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
					where ao.display_code IS NOT NULL AND ao.display_code <> '' AND inventory_id = @inventory_id order by ao.display_code FOR XML PATH('')),1 ,1, ''),
				')')
				where inventory_id = @inventory_id;

				FETCH NEXT FROM cur_prof INTO @inventory_domain_id, @inventory_id, @unit_price, @quantity, @detailed_budget, @domain_id, @option_id;
		END
		CLOSE cur_prof;
		DEALLOCATE cur_prof;
    END