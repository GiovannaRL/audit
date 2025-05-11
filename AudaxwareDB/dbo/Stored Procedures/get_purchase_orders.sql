CREATE PROCEDURE [dbo].[get_purchase_orders](
 @domain_id SMALLINT,
    @project_id INTEGER,
 @phase_id INTEGER = NULL,
    @department_id INTEGER = NULL,
    @room_id INTEGER = NULL)
AS
BEGIN

 select po.domain_id, po.project_id, po.po_id, po.po_number, po.description,
 case po.status  When 'PO Requested' then po.po_requested_date
 when 'PO Issued' then po.po_received_date
 when 'Quote Requested' then po.quote_requested_date
 when 'Quote Received' then po.quote_received_date
 else null end as status_date,
     case po.status  when 'PO Issued' then DATEDIFF(day, po.po_requested_date, po.po_received_date)
     when 'PO Requested' then DATEDIFF(day, po.po_requested_date, GETDATE())
     when 'Quote Requested' then DATEDIFF(day, po.quote_requested_date,  GETDATE())
     when 'Quote Received' then DATEDIFF(day, po.quote_received_date, GETDATE()) end as aging_days,
     coalesce(sum(COALESCE(ipo.po_qty, 0) * coalesce(ipo.po_unit_amt, 0)) + min(coalesce(po.freight,0)) + min(coalesce(po.warehouse,0)) + min(coalesce(po.tax,0)) + min(coalesce(po.misc,0)) + min(coalesce(po.warranty,0)) + min(coalesce(po.install,0)),0) as amount, 
	 po.status, po.date_added, po.added_by, po.comment, po.quote_number, po.po_requested_number, v.name As vendor_name, pa.nickname As ship_to,
  po.quote_file, 
  po.po_file,
  po.freight,  
  coalesce(sum(COALESCE(ipo.po_qty, 0) * coalesce(ipo.po_unit_amt, 0)),0) asset_amount,
  po.quote_expiration_date,
  po.install,
  po.tax,
  po.warranty,
  po.misc, 
  po.quote_discount,
  po.invalid_po,
  po.quote_amount
    From inventory_purchase_order ipo
 right join purchase_order po On ipo.project_id = po.project_id And ipo.po_id = po.po_id
 left join project_room_inventory pri ON pri.inventory_id = ipo.inventory_id
 left Join project_addresses pa ON po.ship_to = pa.id
 inner Join vendor v On v.vendor_id = po.vendor_id And v.domain_id = po.vendor_domain_id
 Where po.domain_id = @domain_id AND po.project_id = @project_id AND (@phase_id IS NULL OR ((pri.phase_id = @phase_id OR po.phase_id = @phase_id)AND (
  @department_id IS NULL OR ((pri.department_id = @department_id OR po.department_id = @department_id) AND (@room_id IS NULL OR 
  pri.room_id = @room_id OR po.room_id = @room_id))
 )))
 Group By po.domain_id, po.project_id, po.po_id, po.po_number, po.description, po.freight,
 case po.status  When 'PO Requested' then po.po_requested_date
 when 'PO Issued' then po.po_received_date
 when 'Quote Requested' then po.quote_requested_date
 when 'Quote Received' then po.quote_received_date
 else null end, 
  case po.status  when 'PO Issued' then DATEDIFF(day, po.po_requested_date, po.po_received_date)
     when 'PO Requested' then DATEDIFF(day, po.po_requested_date, GETDATE())
     when 'Quote Requested' then DATEDIFF(day, po.quote_requested_date,  GETDATE())
     when 'Quote Received' then DATEDIFF(day, po.quote_received_date, GETDATE()) end, 
 po.status, po.date_added, po.added_by, po.comment, po.quote_number, po.po_requested_number, 
 v.name, pa.nickname, po.quote_file, po.po_file, po.quote_expiration_date,po.install,po.tax,po.warranty,po.misc, po.quote_discount, po.invalid_po, po.quote_amount
 order by status_date desc;
END