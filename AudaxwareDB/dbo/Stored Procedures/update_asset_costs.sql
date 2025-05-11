CREATE PROCEDURE [dbo].[update_asset_costs]
AS
BEGIN	

	UPDATE assets set assets.min_cost = query.min_final_cost, assets.max_cost = query.max_final_cost, assets.avg_cost = query.avg_cost
    FROM (
		SELECT asset_domain_id, asset_id, MIN(po_unit_amt) as min_cost,
       MAX(po_unit_amt) as max_cost,
       STDEV(po_unit_amt) as stddev_cost,
       AVG(po_unit_amt) as avg_cost,
      IIF(MIN(po_unit_amt) < (AVG(po_unit_amt)-2*STDEV(po_unit_amt)), (AVG(po_unit_amt)-2*STDEV(po_unit_amt)), MIN(po_unit_amt)) as min_final_cost,
      IIF(MAX(po_unit_amt) > (AVG(po_unit_amt)+2*STDEV(po_unit_amt)), (AVG(po_unit_amt)+2*STDEV(po_unit_amt)), MAX(po_unit_amt)) as max_final_cost
		FROM inventory_purchase_order ipo
        	WHERE coalesce(ipo.po_unit_amt,0) > 0
        	GROUP BY ipo.asset_id, ipo.asset_domain_id
    ) query
    WHERE assets.domain_id = query.asset_domain_id AND assets.asset_id = query.asset_id

 
	END
