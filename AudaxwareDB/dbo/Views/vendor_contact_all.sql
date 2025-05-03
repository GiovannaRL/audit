
CREATE VIEW vendor_contact_all AS
	SELECT CONCAT(vendor_domain_id, vendor_id, domain_id, name) AS Id, * FROM vendor_contact;