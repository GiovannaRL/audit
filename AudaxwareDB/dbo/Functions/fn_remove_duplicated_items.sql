CREATE FUNCTION [dbo].[fn_remove_duplicated_items]
(
	@data AS VARCHAR(MAX),
	@charToSplit as char(1) = ',' --, or ;
)
RETURNS VARCHAR(MAX)
as
BEGIN
	DECLARE @distinctTable TABLE(columnData VARCHAR(300))

	INSERT INTO @distinctTable
	SELECT DISTINCT TRIM(value) FROM STRING_SPLIT(@data, @charToSplit) WHERE value is not null AND TRIM(value) != '' ORDER BY 1

	return (SELECT STRING_AGG(columnData, @charToSplit) FROM @distinctTable)
END
