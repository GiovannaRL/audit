CREATE TABLE [dbo].[audit_log]
(
	[audit_log_id] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY,
    [domain_id] SMALLINT NOT NULL, 
    [user_id] NVARCHAR(128) NOT NULL, 
    [operation] VARCHAR(10) NOT NULL, 
    [table_name] VARCHAR(30) NOT NULL,
    [table_pk] VARCHAR(100) NOT NULL,
    [original] NVARCHAR(MAX) NOT NULL, 
    [modified] NVARCHAR(MAX) NOT NULL,
    [header] VARCHAR(300) NULL,
    [modified_date] DATETIME NOT NULL, 
    [project_id] INT NULL, 
    [asset_id] INT NULL, 
    [asset_domain_id] SMALLINT NULL,

    [comment] NVARCHAR(200) NULL, 
    CONSTRAINT [project_audit_fk] FOREIGN KEY ([project_id], [domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id])

);

GO

CREATE NONCLUSTERED INDEX [UserIdIndex]
ON [dbo].[audit_log] ([domain_id],[user_id])
INCLUDE ([operation],[table_name],[table_pk],[original],[modified],[header],[modified_date],[project_id],[asset_id],[asset_domain_id],[comment])
