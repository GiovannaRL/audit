
CREATE VIEW [dbo].[matching_values] AS 
 SELECT a.domain_id, a.project_id,
    format(sum(b.total_budget_amt) + sum(- b.buyout_delta) + (min(a.install_projected) + min(a.freight_projected) + min(a.warehouse_projected) + min(a.tax_projected) + min(a.warranty_projected) + min(a.misc_projected)), 'C', 'en-us') AS total_projected,
    format(min(COALESCE(c.medial_budget, 0)), 'C', 'en-us') AS project_budget,
    format(sum(b.total_budget_amt) + sum(- b.buyout_delta) + (min(a.install_projected) + min(a.freight_projected) + min(a.warehouse_projected) + min(a.tax_projected) + min(a.warranty_projected) + min(a.misc_projected)) - (sum(b.total_budget_amt) + (min(a.install_budget) + min(a.freight_budget) + min(a.warehouse_budget) + min(a.tax_budget) + min(a.warranty_budget) + min(a.misc_budget))), 'C', 'en-us') AS projected_delta, --
    format(sum(b.total_budget_amt) + (min(a.install_budget) + min(a.freight_budget) + min(a.warehouse_budget) + min(a.tax_budget) + min(a.warranty_budget) + min(a.misc_budget)), 'C', 'en-us') AS total_planned,
    format(sum(b.total_budget_amt) + sum(- b.buyout_delta) + (min(a.install_projected) + min(a.freight_projected) + min(a.warehouse_projected) + min(a.tax_projected) + min(a.warranty_projected) + min(a.misc_projected)) - min(COALESCE(c.medial_budget, 0)), 'C', 'en-us') AS cliente_budget_delta
   FROM ancillary_v a,
    inventory_po_qty_v b,
    project c
  WHERE a.project_id = b.project_id AND a.project_id = c.project_id
  GROUP BY a.domain_id, a.project_id;


