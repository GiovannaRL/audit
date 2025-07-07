CREATE PROCEDURE [dbo].[get_budget_summary_values]
	@domainId int,
	@period int = 12,
	@projectId int = 0,
	@assetCode varchar(25) = null
AS


SELECT min_unit_budget, min_unit_freight, min_unit_install, min_unit_tax, min_unit_markup, min_unit_escalation,
max_unit_budget, max_unit_freight, max_unit_install, max_unit_tax, max_unit_markup, max_unit_escalation,
avg_unit_budget, avg_unit_freight, avg_unit_install, avg_unit_tax, avg_unit_markup, avg_unit_escalation,
last_unit_budget, last_unit_freight, last_unit_install, last_unit_tax, last_unit_markup, last_unit_escalation
FROM
(
	SELECT 1 as inventory, 
	coalesce(min(unit_budget),0) as min_unit_budget, coalesce(min(unit_freight),0) as min_unit_freight, coalesce(min(unit_install),0) as min_unit_install, 
	coalesce(min(unit_tax),0) AS min_unit_tax, coalesce(min(unit_markup),0) AS min_unit_markup, coalesce(min(unit_escalation),0) as min_unit_escalation,
	coalesce(max(unit_budget),0) as max_unit_budget, coalesce(max(unit_freight),0) as max_unit_freight, coalesce(max(unit_install),0) as max_unit_install, 
	coalesce(max(unit_tax),0) AS max_unit_tax, coalesce(max(unit_markup),0) AS max_unit_markup, coalesce(max(unit_escalation),0) as max_unit_escalation,
	coalesce(avg(unit_budget),0) as avg_unit_budget, coalesce(avg(unit_freight),0) as avg_unit_freight, coalesce(avg(unit_install),0) as avg_unit_install, 
	coalesce(avg(unit_tax),0) AS avg_unit_tax, coalesce(avg(unit_markup),0) AS avg_unit_markup, coalesce(avg(unit_escalation),0) as avg_unit_escalation
	FROM project_room_inventory pri
	INNER JOIN assets a ON a.asset_id = pri.asset_id and a.domain_id = pri.asset_domain_id
	WHERE pri.domain_id = @domainId
	AND date_modified >= DATEADD(DAY, 1, EOMONTH(GETDATE(), -1 * @period)) 
	AND ((@projectId > 0 AND project_id = @projectId) OR @projectId = 0) 
	AND (@assetCode is null OR (CONCAT(asset_code, jsn_code, a.asset_description, manufacturer_description, pri.model_name, pri.model_number) LIKE '%' + @assetCode + '%')) 
) as minMaxValues
INNER JOIN
( 
	SELECT top 1 1 as inventory, 
	coalesce(LAST_VALUE(unit_budget) OVER (ORDER BY  date_modified desc),0) as last_unit_budget, coalesce(LAST_VALUE(unit_freight) OVER (ORDER BY  date_modified desc),0) as last_unit_freight,
	coalesce(LAST_VALUE(unit_install) OVER (ORDER BY  date_modified desc),0) as last_unit_install, coalesce(LAST_VALUE(unit_tax) OVER (ORDER BY  date_modified desc),0) as last_unit_tax,
	coalesce(LAST_VALUE(unit_markup) OVER (ORDER BY  date_modified desc),0) as last_unit_markup, coalesce(LAST_VALUE(unit_escalation) OVER (ORDER BY  date_modified desc),0) as last_unit_escalation
	FROM project_room_inventory pri
	INNER JOIN assets a ON a.asset_id = pri.asset_id and a.domain_id = pri.asset_domain_id
	WHERE pri.domain_id = @domainId
	AND date_modified >= DATEADD(DAY, 1, EOMONTH(GETDATE(), -1 * @period)) 
	AND ((@projectId > 0 AND project_id = @projectId) OR @projectId = 0) 
	AND (@assetCode is null OR (CONCAT(asset_code, jsn_code, a.asset_description, manufacturer_description, pri.model_name, pri.model_number) LIKE '%' + @assetCode + '%')) 
) as lastValues
on minMaxValues.inventory = lastValues.inventory