CREATE TABLE [dbo].[document_types]
(
	[id] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY,
	[name] VARCHAR(200) NOT NULL,
	[display_level] INTEGER NULL,
	CONSTRAINT document_type_display_level_fk FOREIGN KEY (display_level)
		REFERENCES [documents_display_levels](id),
	CONSTRAINT document_type_name_unq UNIQUE([name])
)
