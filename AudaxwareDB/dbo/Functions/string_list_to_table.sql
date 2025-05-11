


CREATE FUNCTION [dbo].[string_list_to_table]
(
    @List varchar(MAX)
)
RETURNS 
@ParsedList table
(
    item int
)
AS
BEGIN
    DECLARE @item varchar(800), @Pos int

    SET @List = LTRIM(RTRIM(@List))+ ','
    SET @Pos = CHARINDEX(',', @List, 1)

    WHILE @Pos > 0
    BEGIN
        SET @item = LTRIM(RTRIM(LEFT(@List, @Pos - 1)))
        IF @item <> ''
        BEGIN
            INSERT INTO @ParsedList (item) 
            VALUES (CAST(@item AS int))
        END
        SET @List = RIGHT(@List, LEN(@List) - @Pos)
        SET @Pos = CHARINDEX(',', @List, 1)
    END

    RETURN
END





