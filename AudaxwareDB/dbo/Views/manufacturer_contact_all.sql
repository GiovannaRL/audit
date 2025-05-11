
CREATE VIEW manufacturer_contact_all AS
	SELECT CONCAT(manufacturer_domain_id, manufacturer_id, name) AS Id, * FROM manufacturer_contact;