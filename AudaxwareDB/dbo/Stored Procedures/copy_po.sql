CREATE PROCEDURE dbo.copy_po
	@domainId smallint,
	@projectId int,
	@newDomainId smallint,
	@newProjectId int,
	@addedBy varchar(100)
AS
BEGIN
	--UPDATE COPY_LINK IN CASE IT'S ZERO IN ORDER TO COPY_PO WORK PROPERLY
	update purchase_order set copy_link = (CASE WHEN copy_link = CAST(CAST(0 AS BINARY) AS UNIQUEIDENTIFIER) THEN NEWID() ELSE copy_link END) where domain_id = @domainId and project_id = @projectId;
	
	-- Copy purchase order on project level
	INSERT INTO purchase_order (project_id, [description], vendor_id, freight, warehouse, tax, warranty,
		misc, [status], date_added, added_by, comment, upd_asset_value, vendor_domain_id, domain_id, ship_to,
		quote_amount, allow_assets_update, install, copy_link,
		po_number, quote_number, quote_requested_date, quote_received_date, po_requested_date, po_received_date, 
		quote_file, po_file, po_requested_number, invalid_po, quote_expiration_date)
	SELECT @newProjectId, [description], vendor_id, freight, warehouse, tax, warranty, misc, [status], GETDATE(),
		@addedBy, comment, upd_asset_value, vendor_domain_id, @newDomainId, ship_to,
		quote_amount, allow_assets_update, install, copy_link,
		po_number, quote_number, quote_requested_date, quote_received_date, po_requested_date, po_received_date, 
		quote_file, po_file, po_requested_number, invalid_po, quote_expiration_date
	FROM purchase_order as po
	WHERE po.domain_id = @domainId AND po.project_id = @projectId AND po.phase_id is null;

	-- Copy purchase order on phase level
	INSERT INTO purchase_order (project_id, [description], vendor_id, freight, warehouse, tax, warranty,
		misc, [status], date_added, added_by, comment, upd_asset_value, vendor_domain_id, domain_id, ship_to, phase_id,
		quote_amount, allow_assets_update, install, copy_link,
		po_number, quote_number, quote_requested_date, quote_received_date, po_requested_date, po_received_date, 
		quote_file, po_file, po_requested_number, invalid_po, quote_expiration_date)
	SELECT @newProjectId, po.[description], vendor_id, freight, warehouse, tax, warranty, misc, [status], GETDATE(),
		@addedBy, po.comment, upd_asset_value, vendor_domain_id, @newDomainId, ship_to, newPhase.phase_id,
		quote_amount, allow_assets_update, install, po.copy_link,
		po_number, quote_number, quote_requested_date, quote_received_date, po_requested_date, po_received_date, 
		quote_file, po_file, po_requested_number, invalid_po, quote_expiration_date
	FROM purchase_order as po
	INNER JOIN project_phase as pp ON po.department_id is null AND po.domain_id = pp.domain_id AND
		po.project_id = pp.project_id AND po.phase_id = pp.phase_id
	INNER JOIN project_phase as newPhase ON newPhase.copy_link = pp.copy_link
	WHERE po.domain_id = @domainId AND po.project_id = @projectId AND newPhase.domain_id = @newDomainId AND
		newPhase.project_id = @newProjectId;

	-- Copy purchase order on department level
	INSERT INTO purchase_order (project_id, [description], vendor_id, freight, warehouse, tax, warranty,
		misc, [status], date_added, added_by, comment, upd_asset_value, vendor_domain_id, domain_id, ship_to, phase_id,
		quote_amount, allow_assets_update, install, copy_link, department_id,
		po_number, quote_number, quote_requested_date, quote_received_date, po_requested_date, po_received_date, 
		quote_file, po_file, po_requested_number, invalid_po, quote_expiration_date)
	SELECT @newProjectId, po.[description], vendor_id, freight, warehouse, tax, warranty, misc, [status], GETDATE(),
		@addedBy, po.comment, upd_asset_value, vendor_domain_id, @newDomainId, ship_to, newDept.phase_id,
		quote_amount, allow_assets_update, install, po.copy_link, newDept.department_id,
		po_number, quote_number, quote_requested_date, quote_received_date, po_requested_date, po_received_date, 
		quote_file, po_file, po_requested_number, invalid_po, quote_expiration_date
	FROM purchase_order as po
	INNER JOIN project_department as pd ON po.room_id is null AND po.domain_id = pd.domain_id AND
		po.project_id = pd.project_id AND po.phase_id = pd.phase_id AND po.department_id = pd.department_id
	INNER JOIN project_department as newDept ON pd.copy_link = newDept.copy_link
	WHERE po.domain_id = @domainId AND po.project_id = @projectId AND newDept.domain_id = @newDomainId AND
		newDept.project_id = @newProjectId;

	-- Copy purchase order on room level
	INSERT INTO purchase_order (project_id, [description], vendor_id, freight, warehouse, tax, warranty,
		misc, [status], date_added, added_by, comment, upd_asset_value, vendor_domain_id, domain_id, ship_to, phase_id,
		quote_amount, allow_assets_update, install, copy_link, department_id, room_id,
		po_number, quote_number, quote_requested_date, quote_received_date, po_requested_date, po_received_date, 
		quote_file, po_file, po_requested_number, invalid_po, quote_expiration_date)
	SELECT @newProjectId, po.[description], vendor_id, freight, warehouse, tax, warranty, misc, [status], GETDATE(),
		@addedBy, po.comment, upd_asset_value, vendor_domain_id, @newDomainId, ship_to, newRoom.phase_id,
		quote_amount, allow_assets_update, install, po.copy_link, newRoom.department_id, newRoom.room_id,
		po_number, quote_number, quote_requested_date, quote_received_date, po_requested_date, po_received_date, 
		quote_file, po_file, po_requested_number, invalid_po, quote_expiration_date
	FROM purchase_order as po
	INNER JOIN project_room as pr ON po.domain_id = pr.domain_id AND
		po.project_id = pr.project_id AND po.phase_id = pr.phase_id AND po.department_id = pr.department_id AND
		po.room_id = pr.room_id
	INNER JOIN project_room as newRoom ON newRoom.copy_link = pr.copy_link
	WHERE po.domain_id = @domainId AND po.project_id = @projectId AND newRoom.domain_id = @newDomainId AND
		newRoom.project_id = @newProjectId;

	EXEC copy_po_inventory @domainId, @projectId, @domainId, @newProjectId, @addedBy;
END