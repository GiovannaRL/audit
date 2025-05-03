
CREATE VIEW [dbo].[ancillary_v] AS
 SELECT a.domain_id, a.project_id,
    min(COALESCE(a.freight_budget, 0 )) AS freight_budget,
    sum(COALESCE(b.freight, 0)) AS freight_charges,
        CASE
            WHEN (min(COALESCE(a.freight_budget, 0)) > sum(COALESCE(b.freight, 0))) THEN min(COALESCE(a.freight_budget, 0))
            ELSE sum(COALESCE(b.freight, 0))
        END AS freight_projected,
    min(COALESCE(a.warehouse_budget, 0)) AS warehouse_budget,
    sum(COALESCE(b.warehouse, 0)) AS warehouse_charges,
        CASE
            WHEN (min(COALESCE(a.warehouse_budget, 0)) > sum(COALESCE(b.warehouse, 0))) THEN min(COALESCE(a.warehouse_budget, 0))
            ELSE sum(COALESCE(b.warehouse, 0))
        END AS warehouse_projected,
    min(COALESCE(a.tax_budget, 0)) AS tax_budget,
    sum(COALESCE(b.tax, 0)) AS tax_charges,
        CASE
            WHEN (min(COALESCE(a.tax_budget, 0)) > sum(COALESCE(b.tax, 0))) THEN min(COALESCE(a.tax_budget, 0))
            ELSE sum(COALESCE(b.tax, 0))
        END AS tax_projected,
    min(COALESCE(a.misc_budget, 0)) AS misc_budget,
    sum(COALESCE(b.misc, 0)) AS misc_charges,
        CASE
            WHEN (min(COALESCE(a.misc_budget, 0)) > sum(COALESCE(b.misc, 0))) THEN min(COALESCE(a.misc_budget, 0))
            ELSE sum(COALESCE(b.misc, 0))
        END AS misc_projected,
    min(COALESCE(a.warranty_budget, 0)) AS warranty_budget,
    sum(COALESCE(b.warranty, 0)) AS warranty_charges,
        CASE
            WHEN (min(COALESCE(a.warranty_budget, 0)) > sum(COALESCE(b.warranty, 0))) THEN min(COALESCE(a.warranty_budget, 0))
            ELSE sum(COALESCE(b.warranty, 0))
        END AS warranty_projected,
	min(COALESCE(a.install_budget, 0)) AS install_budget,
	sum(COALESCE(b.install, 0)) AS install_charges,
        CASE
            WHEN (min(COALESCE(a.install_budget, 0)) > sum(COALESCE(b.install, 0))) THEN min(COALESCE(a.install_budget, 0))
            ELSE sum(COALESCE(b.install, 0))
        END AS install_projected,
    sum(COALESCE(b.quote_discount, 0)) as quote_discount
   FROM project a
     LEFT JOIN purchase_order b ON a.project_id = b.project_id AND a.domain_id = b.domain_id and b.invalid_po = 0
  GROUP BY a.domain_id, a.project_id;


