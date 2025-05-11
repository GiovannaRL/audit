CREATE PROCEDURE [dbo].[get_associated_manufacturers](@domain_id SMALLINT)
AS
BEGIN
	SELECT [manufacturer_domain_id], [manufacturer_id] FROM [domain_manufacturer] WHERE [domain_manufacturer].[domain_id] = @domain_id
END