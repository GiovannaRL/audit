CREATE PROCEDURE [dbo].[get_all_category_subcategories](
	@domain_id INTEGER,
	@categories_domain_id SMALLINT,
    @categories_id VARCHAR(MAX))
AS
BEGIN

	SELECT sc.*, c.description AS category_description, d.name AS domain_name FROM assets_subcategory sc
	LEFT JOIN assets_category c ON sc.category_domain_id = c.domain_id AND sc.category_id = c.category_id 
	LEFT JOIN domain d ON sc.domain_id = d.domain_id
	WHERE (sc.domain_id = @domain_id OR (sc.domain_id = 1 AND d.show_audax_info = 1)) AND 
		category_domain_id = @categories_domain_id AND sc.category_id IN 
		(SELECT CAST(value AS INTEGER) AS category_id FROM STRING_SPLIT(@categories_id, ';'));


END