CREATE PROCEDURE [dbo].[update_inventories_option]
	@domain_id SMALLINT,
	@inventories_id VARCHAR(5000),
    @option_id INTEGER,
	@quantity INTEGER,
	@unit_price NUMERIC(10, 2)
AS
BEGIN

	UPDATE inventory_options SET quantity = CASE WHEN @quantity > 0 THEN @quantity ELSE quantity END, 
		unit_price = COALESCE(@unit_price , unit_price)
		WHERE domain_id = @domain_id AND option_id = @option_id AND inventory_id IN 
		(SELECT CAST(value AS INTEGER) AS inventory_id FROM STRING_SPLIT(@inventories_id, ';'));

	INSERT INTO inventory_options(inventory_id, option_id, domain_id, unit_price, quantity, inventory_domain_id) 
	SELECT CAST(value AS INTEGER), @option_id, @domain_id, COALESCE(@unit_price, 0),
		COALESCE(@quantity, 1), @domain_id FROM STRING_SPLIT(@inventories_id, ';') WHERE CAST([value] AS INTEGER)
		NOT IN (SELECT inventory_id FROM inventory_options WHERE domain_id = @domain_id AND
			option_id = @option_id);
END