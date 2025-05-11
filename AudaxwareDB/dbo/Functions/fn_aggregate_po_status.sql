CREATE FUNCTION [dbo].[fn_aggregate_po_status]
(
	@poStatus AS varchar(max) --SHOULD RECEIVE STATUS,PO_QTY;STATUS,PO_QTY
)
RETURNS varchar(max)
as
BEGIN
	DECLARE @poDataTable table(poStatus varchar(10), poQty int)
	DECLARE @poStatusAggregated varchar(max)

	insert into @poDataTable
	select substring(value, 1, CHARINDEX(',', value) - 1), cast(substring(value, CHARINDEX(',', value)+1, len(value)) as int)  from string_split(@poStatus, ';')


	select @poStatusAggregated = STRING_AGG(poStatus, ' ,') from
	(select poStatus + '(' + cast(sum(poQty) as varchar(10)) + ')' as poStatus from @poDataTable group by poStatus) as x

	return @poStatusAggregated

END
