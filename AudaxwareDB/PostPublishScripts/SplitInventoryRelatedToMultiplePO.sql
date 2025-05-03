/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
exec sp_set_session_context 'domain_id', '1';
GO
--these are old inventories added by mistake
delete from inventory_purchase_order where po_qty = 0

--these are inventories with dnp_qty wrong because they have PO added with the quantity they set as dnp
update project_room_inventory set dnp_qty = 0 
where inventory_id in(29808,444692,757477,758435) and domain_id = 15

exec split_inventory_purchase_order

/*
PROJECTS THAT ARE GOING TO HAVE A DIFFERENT COMPARATIVE FINANCIALS VALUE

325	15 --WRONG DNP_QTY VALUE
501	15 --WRONG DNP_QTY VALUE
613	24 --PO_QTY > BUDGET_QTY
623	24 --PO_QTY > BUDGET_QTY
624	24 --PO_QTY > BUDGET_QTY
626	5 --PO_QTY > BUDGET_QTY
736	15 --WRONG DNP_QTY VALUE
737	15 --WRONG DNP_QTY VALUE

*/


GO
exec sp_set_session_context 'domain_id', null;