CREATE PROCEDURE [dbo].[get_budget_data_from_inventories]
	@domainId int,
	@period int = 12,
	@projectId int = 0,
	@assetCode varchar(25) = null
AS


	SELECT top 1000 p.project_description, pp.description as phase_description, pd.description as department_description, pr.drawing_room_name as room_name, pr.drawing_room_number as room_number, a.asset_description, a.asset_code,
		pri.jsn_code, pri.serial_number, pri.serial_name, pri.manufacturer_description, unit_budget, unit_freight, unit_install, unit_tax, unit_markup, unit_escalation, pri.date_modified
	FROM project_room_inventory pri
	INNER JOIN assets a ON a.asset_id = pri.asset_id and a.domain_id = pri.asset_domain_id
	INNER JOIN PROJECT p ON p.PROJECT_ID = pri.PROJECT_ID and p.domain_id = pri.domain_id
	INNER JOIN project_phase PP ON PP.project_id = pri.project_id AND PP.domain_id = pri.domain_id AND PP.phase_id = pri.phase_id
	INNER JOIN project_department PD ON PD.project_id = pri.project_id AND PD.domain_id = pri.domain_id AND PD.phase_id = pri.phase_id AND PD.department_id = pri.department_id
	INNER JOIN project_room PR ON PR.project_id = pri.project_id AND PR.domain_id = pri.domain_id AND PR.phase_id = pri.phase_id AND PR.department_id = pri.department_id AND PR.room_id = pri.room_id 
	WHERE pri.domain_id = @domainId
	AND date_modified >= DATEADD(DAY, 1, EOMONTH(GETDATE(), -1 * @period)) 
	AND ((@projectId > 0 AND pri.project_id = @projectId) OR @projectId = 0) 
	AND (@assetCode is null OR (CONCAT(asset_code, jsn_code, pri.asset_description, manufacturer_description, pri.serial_name, pri.serial_number) LIKE '%' + @assetCode + '%')) 
	ORDER BY pri.date_modified desc
