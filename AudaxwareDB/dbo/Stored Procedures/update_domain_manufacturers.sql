CREATE PROCEDURE [dbo].[update_domain_manufacturers]
	@domain_id SMALLINT,
	@manufacturers_domain_id SMALLINT,
	@manufacturers_ids VARCHAR(5000) -- Format: manufacturer_id;manufacturer_id....
AS
BEGIN
	-- Only enterprises of the type M (Manufacturer) can have manufacturers associated to them
	IF EXISTS (SELECT * FROM domain WHERE domain_id = @domain_id AND type = 'M')
		BEGIN
			DECLARE @manufacturers TABLE (id INTEGER);

			INSERT @manufacturers (id) SELECT [value] FROM STRING_SPLIT(@manufacturers_ids, ';');

			INSERT INTO domain_manufacturer (domain_id, manufacturer_domain_id, manufacturer_id)
			SELECT @domain_id, @manufacturers_domain_id, id
			FROM @manufacturers
			WHERE id NOT IN 
				(SELECT manufacturer_id FROM domain_manufacturer WHERE domain_id = @domain_id 
					AND manufacturer_domain_id = @manufacturers_domain_id)
	
			DELETE FROM domain_manufacturer WHERE domain_id = @domain_id AND manufacturer_domain_id = @manufacturers_domain_id
				AND manufacturer_id NOT IN (SELECT id FROM @manufacturers);
		END
	ELSE IF EXISTS (SELECT * FROM domain WHERE domain_id = @domain_id AND type = 'E')
		BEGIN
			DELETE FROM domain_manufacturer WHERE domain_id = @domain_id;
		END
END