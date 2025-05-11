CREATE TABLE [dbo].[documents_display_levels]
(
	[id] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	[description] VARCHAR(200) NOT NULL,
	CONSTRAINT documents_display_levels_desc_unq UNIQUE([description])
)
