CREATE PROCEDURE [dbo].[update_asset_budget]
	@domainId smallint,
	@projectId int, 
	@unitMarkup decimal(10,2) = null,
	@unitEscalation decimal(10,2) = null,
	@unitTax decimal(10,2) = null,
	@unitFreightMarkup decimal(10,2) = null,
	@unitInstallMarkup decimal(10,2) = null

AS

	UPDATE project_room_inventory SET
	unit_markup = CASE WHEN @unitMarkup is null THEN p.unit_markup else nullif(@unitMarkup, 0) end,
	unit_escalation = CASE WHEN @unitEscalation is null THEN p.unit_escalation else nullif(@unitEscalation, 0) end,
	unit_tax = CASE WHEN @unitTax is null THEN p.unit_tax else nullif(@unitTax, 0) end,
	unit_freight_markup = CASE WHEN @unitFreightMarkup is null THEN p.unit_freight_markup else nullif(@unitFreightMarkup, 0) end,
	unit_install_markup = CASE WHEN @unitInstallMarkup is null THEN p.unit_install_markup else nullif(@unitInstallMarkup, 0) end
	FROM project_room_inventory p WHERE project_id = @projectId AND domain_id = @domainId;

return
