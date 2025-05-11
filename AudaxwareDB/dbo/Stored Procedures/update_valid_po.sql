CREATE PROCEDURE [dbo].[update_valid_po]
	@domain_id SMALLINT,
	@project_id INT,
	@po_id INTEGER
AS
	BEGIN
		UPDATE purchase_order
		SET invalid_po = 
			CASE
				WHEN(
					CONVERT(
						DECIMAL(10, 2), 
							COALESCE(quote_amount, 0) 
							- COALESCE(freight, 0) 
							- COALESCE(tax, 0) 
							- COALESCE(install, 0) 
							- COALESCE(warranty, 0) 
							- COALESCE(misc, 0) 
							- COALESCE(quote_discount,0)
							) 
					= 
						CONVERT(
							DECIMAL(10, 2), 
								COALESCE(
									(SELECT SUM(po_qty * po_unit_amt) 
										FROM inventory_purchase_order ipo 
										WHERE ipo.po_domain_id = @domain_id AND ipo.project_id = @project_id AND ipo.po_id = @po_id )
									,0))
						)
				THEN 0 
				ELSE 1
			END
		WHERE domain_id = @domain_id AND project_id = @project_id AND po_id = @po_id;
	END
