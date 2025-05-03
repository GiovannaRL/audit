CREATE FUNCTION [dbo].[fn_SUM_UNIT_PRICE]
(
	@inventory_id AS INT
)
RETURNS @prices table
(
	OPTION_PRICES numeric(10,2)
)
as
BEGIN
	declare @OPTION_PRICES numeric(10,2);

	select
		@OPTION_PRICES = sum(coalesce(io.unit_price,0) * coalesce(io.quantity, 0))
	from 
		inventory_options io 
	where 
		io.inventory_id = @inventory_id

	insert @prices select @OPTION_PRICES
	return;
END
GO

