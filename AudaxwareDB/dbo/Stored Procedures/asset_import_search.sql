CREATE PROCEDURE [dbo].[asset_import_search]
	@domain_id smallint,
	@show_audaxware_info bit,
	@asset_code varchar(25),
	@manufacturer varchar(100) = null,
	@model_number varchar(100) = null,
	@model_name varchar(150) = null,
	@asset_description varchar(300) = null,
	@jsn_id INT = null,
	@jsn_domain_id SMALLINT = null,
	@jsn_suffix VARCHAR(4) = null,
	@project_id INT = null
AS
	DECLARE @manufacturer_domain_id SMALLINT,  @manufacturer_id INT = NULL
	SELECT TOP 1  @manufacturer_domain_id = domain_id, @manufacturer_id = manufacturer_id from manufacturer WHERE
		manufacturer_description = @manufacturer AND ((domain_id = 1 AND @show_audaxware_info <> 0) OR domain_id = @domain_id)
		--AND manufacturer_description <> 'TBD' 
		ORDER BY domain_id DESC
    -- We are getting some insertions with duplicates and then re-importing is not working. I am creating this temporary fix for that
	SELECT TOP 1 a.jsn_id as jsn_id, a.asset_id, a.domain_id, a.asset_code, a.default_resp, a.manufacturer_id, a.manufacturer_domain_id, a.model_number,
		a.model_name, a.asset_description FROM assets a
	WHERE (a.domain_id = @domain_id or (a.domain_id = 1 and @show_audaxware_info = 1 ))
	AND
	(
		(a.asset_code = @asset_code)
		OR
		(
			(ISNULL(@asset_code, '') = '')
			AND
			(
				(a.manufacturer_domain_id = @manufacturer_domain_id AND a.manufacturer_id = @manufacturer_id AND
					((ISNULL(a.model_number, '') <> '' AND a.model_number = @model_number)
						 AND (ISNULL(a.model_name, '') <> '' AND a.model_name = @model_name)))
				OR
				(
					a.imported_by_project_id = @project_id
					AND
					(
						(
							 REPLACE(a.asset_description, ' ', '') = REPLACE(@asset_description, ' ', '')
							AND a.manufacturer_id = @manufacturer_id AND a.manufacturer_domain_id = @manufacturer_domain_id
						)
						OR
						(
							(a.jsn_id = @jsn_id AND a.jsn_domain_id = @jsn_domain_id AND a.jsn_suffix = @jsn_suffix )
						)
					)
				)
			)
		)
	)
	AND
	(
		(@jsn_id IS NULL and a.jsn_id is null and a.jsn_domain_id is null) OR
		(@jsn_id = -1 AND a.domain_id = @domain_id) OR -- ASSET EXISTS IN THE DOMAIN WITHOUT ASSOCIATED JSN, we can simply add JSN to it
		 (@jsn_id = a.jsn_id AND @jsn_domain_id = jsn_domain_id)
	)
	ORDER BY a.asset_code;
GO
