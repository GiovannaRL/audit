CREATE PROCEDURE [dbo].[copy_po_inventory]
	@domainId smallint,
	@projectId int,
	@newDomainId smallint,
	@newProjectId int,
	@addedBy varchar(100)
AS
BEGIN
	INSERT INTO inventory_purchase_order (project_id, po_id, po_qty, po_unit_amt, po_status, date_added, added_by, 
		inventory_id, asset_id, asset_domain_id, po_domain_id)
	SELECT @newProjectId, newPo.po_id, po_qty, po_unit_amt, po_status, GETDATE(), @addedBy, pri1.inventory_id, ipo.asset_id, 
		ipo.asset_domain_id, @newDomainId
	FROM inventory_purchase_order as ipo
		INNER JOIN purchase_order as po ON ipo.project_id = po.project_id AND ipo.po_id = po.po_id
		INNER JOIN purchase_order as newPo ON po.copy_link = newPo.copy_link
		INNER JOIN project_room_inventory as pri ON ipo.inventory_id = pri.inventory_id
		INNER JOIN project_room_inventory as pri1 ON pri.copy_link = pri1.copy_link
	WHERE ipo.po_domain_id = @domainId AND ipo.project_id = @projectId AND newPo.domain_id = @newDomainId AND 
		newPo.project_id = @newProjectId AND pri1.domain_id = @newDomainId AND pri1.project_id = @newProjectId
END