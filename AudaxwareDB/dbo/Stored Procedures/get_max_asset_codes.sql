CREATE PROCEDURE [dbo].[get_max_asset_codes]
	@domain_id int
AS
	
	select SUBSTRING(asset_code, 1,3) as prefix, max(asset_code) as max_asset_code from assets 
	where domain_id = @domain_id and right(asset_code, 1) != 'C'
	group by SUBSTRING(asset_code, 1,3)

RETURN 0
