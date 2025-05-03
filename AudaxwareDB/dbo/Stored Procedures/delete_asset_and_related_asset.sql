CREATE PROCEDURE [dbo].[delete_asset_and_related_asset]
	@asset_id int,
    @domain_id smallint
AS
BEGIN

	DELETE FROM related_assets WHERE asset_id = @asset_id and domain_id = @domain_id;

	DELETE FROM assets WHERE asset_id = @asset_id and domain_id = @domain_id;

END