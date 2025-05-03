CREATE PROCEDURE [dbo].[get_related_assets](@RELATED_ASSET_ID integer, @RELATED_DOMAIN_ID smallint) 
AS	
BEGIN
	-- Returns the info about all the rooms that one (or more) assets belongs

	SET NOCOUNT ON;
	 
	SELECT a.asset_code, a.asset_id, a.domain_id 
	FROM related_assets ra
	INNER JOIN assets a ON ra.asset_id = a.asset_id and ra.domain_id = a.domain_id
	where (cast(related_asset_id as varchar(10)) + cast(related_domain_id as varchar(2))) = (select cast(asset_id as varchar(10)) + cast(domain_id as varchar(2)) 
		from related_assets where related_asset_id = @RELATED_ASSET_ID and related_domain_id = @RELATED_DOMAIN_ID)
END

