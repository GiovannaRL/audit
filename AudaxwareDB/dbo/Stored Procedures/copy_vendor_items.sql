CREATE PROCEDURE [dbo].[copy_vendor_items](
	@from_domain_id SMALLINT,
    @from_vendor_id INTEGER,
	@to_domain_id SMALLINT,
    @to_vendor_id INTEGER)
AS
BEGIN

	INSERT INTO vendor_contact(vendor_domain_id, vendor_id, name, title, email, address, city, state, zipcode, phone, fax, date_added, 
		comment, mobile) SELECT @to_domain_id, @to_vendor_id, name, title, email, address, city, state, zipcode, phone, 
		fax, date_added, comment, mobile FROM vendor_contact WHERE vendor_domain_id = @from_domain_id AND vendor_id = @from_vendor_id;
	
END