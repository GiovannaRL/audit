CREATE PROCEDURE [dbo].[add_option_picture]
	@domain_id SMALLINT,
	@filename VARCHAR(200),
	@blobFileNameWithExtension VARCHAR(100),
	@extension VARCHAR(10),
	@optionID INTEGER,
	@isInventory BIT = 0,
	@inventoriesIds VARCHAR(5000) = null
AS
BEGIN
	/* To insert on domain's documents */
	IF NOT EXISTS (SELECT 1 from domain_document where domain_id = @domain_id AND blob_file_name = @blobFileNameWithExtension AND type_id = 5)
	BEGIN 
		INSERT INTO domain_document(filename, type_id, date_added, domain_id, blob_file_name, file_extension)
			values(@filename, 5, GETDATE(), @domain_id, @blobFileNameWithExtension, @extension);
	END

	DECLARE @document_id INTEGER = 
		(SELECT TOP 1 id FROM domain_document WHERE domain_id = @domain_id AND 
			type_id = 5 AND filename = @filename AND blob_file_name = @blobFileNameWithExtension);

	/* To associate to option */
	IF (@isInventory = 1)
		UPDATE inventory_options SET document_domain_id = @domain_id, document_id = @document_id
			WHERE option_id = @optionID AND domain_id = @domain_id AND inventory_id IN 
				(SELECT CAST(value AS INTEGER) AS inventory_id FROM STRING_SPLIT(@inventoriesIds, ';'));
	ELSE
		UPDATE assets_options SET document_domain_id = @domain_id, document_id = @document_id
			WHERE domain_id = @domain_id AND asset_option_id = @optionID;
END