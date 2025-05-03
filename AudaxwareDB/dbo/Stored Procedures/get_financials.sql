CREATE PROCEDURE [dbo].[get_financials](@domain_id integer, @project_id INTEGER = null, @phase_id integer = null, @department_id integer = null, @room_id integer = null) 
--RETURNS TABLE
AS	
BEGIN

	select a.domain_id, a.project_id, c.project_description,
	CASE c.status
		WHEN 'A' THEN 'Active'
		WHEN 'D' THEN 'Canceled'
		WHEN 'C' THEN 'Complete'
		WHEN 'I' THEN 'Inventory'
		WHEN 'L' THEN 'Locked'
		WHEN 'P' THEN 'Pending'
		WHEN 'T' THEN 'Training'
	END status,

	-- PLANNED_BUDGET
	format(isnull(sum(b.total_budget) + 
	CASE WHEN @phase_id is not null THEN 0 
		ELSE (coalesce(c.freight_budget,0) + coalesce(c.warehouse_budget,0) + coalesce(c.tax_budget,0) + coalesce(c.warranty_budget,0) + coalesce(c.misc_budget,0) + coalesce(c.markup_budget,0) + coalesce(c.escalation_budget,0) + coalesce(c.install_budget,0)) 
	END ,0), 'C', 'en-us')  as planned_budget, 

	-- PO_TOTAL
	format(coalesce(sum(b.total_po_amt),0) + 
	case when @phase_id is not null THEN 0 
		else (coalesce(min(a.freight_charges),0) + coalesce(min(a.tax_charges),0) + coalesce(min(a.warranty_charges),0) + coalesce(min(a.misc_charges),0) + coalesce(min(a.install_charges),0) - coalesce(a.quote_discount, 0)) end
	, 'C', 'en-us') as po_total,

	-- PROJECTED_COST
	format(sum(b.total_budget) + sum(- b.buyout_delta) + 
	case when @phase_id is not null then 0 
		else (min(a.freight_projected) + coalesce(min(a.tax_projected),0) + coalesce(min(a.warranty_projected),0) + coalesce(min(a.misc_projected),0) + coalesce(min(a.install_projected),0) - coalesce(min(a.quote_discount), 0)) end
		, 'C', 'en-us') as projected_cost, 

	format(coalesce(sum(b.buyout_delta),0), 'C', 'en-us') as planned_budget_delta,
	format(coalesce(c.medial_budget,0), 'C', 'en-us') as total_budget,
	format(sum(b.total_freight), 'C', 'en-us') as freight_planned,
	format(coalesce(min(a.freight_charges),0), 'C', 'en-us') as freight_actual,
	format(case when min(a.freight_projected) > sum(b.total_freight) then min(a.freight_projected) else sum(b.total_freight) end , 'C', 'en-us') as freight_projected,
	format(coalesce(c.freight_budget,0) - (case when min(a.freight_projected) > sum(b.total_freight) then min(a.freight_projected) else sum(b.total_freight) end), 'C', 'en-us') as freight_delta, 
	format(coalesce(c.freight_budget,0), 'C', 'en-us') as freight_budget,
	format(min(c.warehouse_budget), 'C', 'en-us') as warehouse_planned,
	format(coalesce(min(a.warehouse_charges),0), 'C', 'en-us') as warehouse_actual,
	format(min(a.warehouse_projected), 'C', 'en-us') as warehouse_projected,
	format(min(a.warehouse_projected)-min(c.warehouse_budget), 'C', 'en-us') as warehouse_delta, 
	format(coalesce(c.warehouse_budget,0), 'C', 'en-us') as warehouse_budget,
	format(sum(b.total_tax), 'C', 'en-us') as tax_planned,
	format(coalesce(min(a.tax_charges),0), 'C', 'en-us') as tax_actual,
	format(min(a.tax_projected), 'C', 'en-us') as tax_projected,
	format(min(a.tax_projected)-min(b.total_tax), 'C', 'en-us') as tax_delta, 
	format(coalesce(c.tax_budget,0), 'C', 'en-us') as tax_budget,
	format(sum(c.misc_budget), 'C', 'en-us') as contingency_planned,
	format(coalesce(min(a.misc_charges),0), 'C', 'en-us') as contingency_actual,
	format(min(a.misc_projected), 'C', 'en-us') as contingency_projected,
	format(min(a.misc_projected)-min(c.misc_budget), 'C', 'en-us') as contingency_delta, 
	format(coalesce(c.misc_budget,0), 'C', 'en-us') as contingency_budget,
	format(sum(c.warranty_budget), 'C', 'en-us') as warranty_planned,
	format(coalesce(min(a.warranty_charges),0), 'C', 'en-us') as warranty_actual,
	format(min(a.warranty_projected), 'C', 'en-us') as warranty_projected,
	format(min(a.warranty_projected)-min(c.warranty_budget), 'C', 'en-us') as warranty_delta, 
	format(coalesce(c.warranty_budget,0), 'C', 'en-us') as warranty_budget,
	format(min(b.unit_markup_calc), 'C', 'en-us') as markup_planned,
	format(0, 'C', 'en-us') as markup_actual,
	format(min(b.markup_projected), 'C', 'en-us') as markup_projected,
	format(min(b.markup_projected)-min(b.unit_markup_calc), 'C', 'en-us') as markup_delta, 
	format(coalesce(c.markup_budget,0), 'C', 'en-us') as markup_budget,
	format(sum(b.unit_escalation_calc), 'C', 'en-us') as escalation_planned,
	format(0, 'C', 'en-us') as escalation_actual,
	format(min(b.escalation_projected), 'C', 'en-us') as escalation_projected,
	format(min(b.escalation_projected)-min(b.unit_escalation_calc), 'C', 'en-us') as escalation_delta, 
	format(coalesce(c.escalation_budget,0), 'C', 'en-us') as escalation_budget,
	format(sum(b.total_install), 'C', 'en-us') as install_planned,
	format(coalesce(min(a.install_charges),0), 'C', 'en-us') as install_actual,
	format(min(a.install_projected), 'C', 'en-us') as install_projected,
	format(min(a.install_projected)-min(b.total_install), 'C', 'en-us') as install_delta, 
	format(coalesce(c.install_budget,0), 'C', 'en-us') as install_budget,
	sum(b.budget_qty) as budget_qty,
	sum(b.dnp_qty) as dnp_qty,
	sum(b.po_qty) as po_qty
	FROM ancillary_v a,
		inventory_po_qty_v b,
		project c
	WHERE a.project_id = b.project_id and a.domain_id = b.domain_id and a.project_id = c.project_id and a.domain_id = c.domain_id
	and (@project_id is null or b.project_id = @project_id) and b.domain_id = @domain_id
	and (@phase_id is null or b.phase_id = @phase_id)
	and (@department_id is null or b.department_id = @department_id)
	and (@room_id is null or b.room_id = @room_id)
	GROUP BY a.domain_id, a.project_id, a.quote_discount, c.freight_budget, c.warehouse_budget, c.tax_budget, c.warranty_budget, 
	c.misc_budget, c.markup_budget, c.escalation_budget, c.install_budget, c.medial_budget, c.project_description, c.status;
END
