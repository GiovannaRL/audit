CREATE PROCEDURE [dbo].[delete_asset_option_picture]
	@domain_id SMALLINT,
	@asset_option_id INT
AS
	DECLARE @document_domain_id SMALLINT, @document_id INT;

	SELECT @document_domain_id = document_domain_id, @document_id = document_id
	FROM assets_options WHERE asset_option_id = @asset_option_id AND domain_id = @domain_id;

	IF (@document_domain_id IS NOT NULL)
		BEGIN
			UPDATE assets_options SET document_domain_id = NULL, document_id = NULL
				WHERE asset_option_id = @asset_option_id AND domain_id = @domain_id;

			IF (NOT EXISTS (SELECT 1 FROM assets_options WHERE document_domain_id = @document_domain_id
				AND document_id = @document_id) AND 
			NOT EXISTS(SELECT 1 FROM inventory_options WHERE document_domain_id = @document_domain_id
				AND document_id = @document_id))
					DELETE FROM domain_document WHERE domain_id = @document_domain_id AND id = @document_id AND type_id = 5;
		END
