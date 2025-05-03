CREATE PROCEDURE [dbo].[update_all_valid_po]
AS
    BEGIN
        DECLARE @project_id INT, @domain_id SMALLINT, @po_id INTEGER;
        DECLARE [cursor] CURSOR LOCAL FOR SELECT project_id, domain_id, po_id FROM purchase_order po
        WHERE 
        (po.invalid_po = 0 AND (CONVERT(DECIMAL(10, 2), (COALESCE(po.quote_amount, 0) - COALESCE(po.freight, 0) - COALESCE(po.tax, 0) - COALESCE(po.install, 0) - COALESCE(po.warranty, 0) - COALESCE(po.misc, 0) - COALESCE(po.quote_discount,0)))) <> (CONVERT(DECIMAL(10, 2), COALESCE((SELECT SUM(po_qty * po_unit_amt) FROM inventory_purchase_order ipo WHERE ipo.po_id = po.po_id),0)))) OR
        (po.invalid_po = 1 AND (CONVERT(DECIMAL(10, 2), (COALESCE(po.quote_amount, 0) - COALESCE(po.freight, 0) - COALESCE(po.tax, 0) - COALESCE(po.install, 0) - COALESCE(po.warranty, 0) - COALESCE(po.misc, 0) - COALESCE(po.quote_discount,0)))) = (CONVERT(DECIMAL(10, 2), COALESCE((SELECT SUM(po_qty * po_unit_amt) FROM inventory_purchase_order ipo WHERE ipo.po_id = po.po_id),0))))
        OPEN [cursor]
        FETCH NEXT FROM [cursor] INTO @project_id, @domain_id, @po_id;
        WHILE @@FETCH_STATUS = 0
	        BEGIN

                EXEC [dbo].[update_valid_po] @domain_id, @project_id, @po_id;
		        
	        FETCH NEXT FROM [cursor] INTO @project_id, @domain_id, @po_id;
	        END
        CLOSE [cursor];
        DEALLOCATE [cursor];
END;


