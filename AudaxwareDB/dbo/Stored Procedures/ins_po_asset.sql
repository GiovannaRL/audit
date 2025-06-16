
-- ========================================================================
-- Author:		Camila Silva
-- Create date: 08/30/2016
-- Description:	Insert an asset to a PO
-- =========================================================================

CREATE PROCEDURE [dbo].[ins_po_asset](
	@domain_id SMALLINT,
    @project_id INTEGER,
	@asset_domain_id SMALLINT,
	@asset_id INTEGER,
    @po_id INTEGER,
	@inventory_ids VARCHAR(8000),
    @po_qty INTEGER,
    @po_unit_amt NUMERIC(10, 2),
	@added_by VARCHAR(200))
AS
BEGIN

	IF @po_qty > 0
		BEGIN
			DECLARE @qty_left INTEGER, @exists INTEGER;
			Declare @id varchar(20) = null;
			DECLARE @delivery_date date;
			DECLARE @phase_id int;

			WHILE LEN(@inventory_ids) > 0 
				BEGIN
					IF PATINDEX('%,%',@inventory_ids) > 0
						BEGIN
							SET @id = SUBSTRING(@inventory_ids, 0, PATINDEX('%,%',@inventory_ids));

							SET @inventory_ids = SUBSTRING(@inventory_ids, LEN(@id + '|') + 1, LEN(@inventory_ids));
						END
					ELSE
						BEGIN
							SET @id = @inventory_ids;
							SET @inventory_ids = NULL;
						END

					--GET DELIVERY DATE
					SELECT @phase_id = phase_id from project_room_inventory where inventory_id = @id;
					SELECT @delivery_date = ofci_delivery from project_phase WHERE project_id = @project_id and domain_id = @domain_id and phase_id = @phase_id;

					--IF STATUS INVENTORY IS PLAN CHANGE TO APPROVED
					UPDATE project_room_inventory SET current_location = 'Approved' WHERE inventory_id = @id AND current_location = 'Plan'

					SELECT @qty_left = budget_qty - coalesce(dnp_qty,0) - SUM(coalesce(po_qty,0)) FROM project_room_inventory pri 
						LEFT JOIN inventory_purchase_order ipo on ipo.inventory_id = pri.inventory_id
						WHERE pri.inventory_id = @id group by budget_qty, dnp_qty;

					IF @qty_left > @po_qty OR @qty_left IS NULL
						BEGIN
							SET @qty_left = @po_qty;
						END

					SELECT @exists = COUNT(*) FROM inventory_purchase_order WHERE po_domain_id = @domain_id AND po_id = @po_id AND
						project_id = @project_id AND inventory_id = @id;

					IF @exists > 0
						BEGIN
							UPDATE inventory_purchase_order SET po_qty = po_qty + @qty_left WHERE po_domain_id = @domain_id AND po_id = @po_id 
								AND project_id = @project_id AND inventory_id = @id;
						END
					ELSE
						BEGIN
							IF @qty_left > 0 BEGIN
								INSERT INTO inventory_purchase_order(project_id, po_id, po_qty, po_unit_amt, po_status, date_added,
									added_by, inventory_id, asset_id, asset_domain_id, po_domain_id) 
								VALUES(@project_id, @po_id, @qty_left, @po_unit_amt, 'Ordered', GETDATE(), 
									@added_by, @id, @asset_id, @asset_domain_id, @domain_id);
							END
						END

					SET @po_qty = @po_qty - @qty_left;
				END
		END
END