
CREATE PROCEDURE [dbo].[copy_manufacturer_items](
	@from_domain_id SMALLINT,
    @from_manufacturer_id INTEGER,
	@to_domain_id SMALLINT,
    @to_manufacturer_id INTEGER,
	@added_by VARCHAR(50))
AS
BEGIN

	INSERT INTO manufacturer_contact(manufacturer_domain_id, manufacturer_id, name, contact_type, title, email, address, city, state, phone,
		fax, zipcode, date_added, added_by, comment, mobile, contact_domain_id) SELECT @to_domain_id, @to_manufacturer_id, name, 'Manufacturer',
		title, email, address, city, state, phone, fax, zipcode, GETDATE(), COALESCE(@added_by, added_by), comment, mobile, contact_domain_id FROM
		manufacturer_contact WHERE manufacturer_domain_id = @from_domain_id AND manufacturer_id = @from_manufacturer_id;
	
END
