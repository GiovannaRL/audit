CREATE TABLE [dbo].[project_addresses] (
    [id]          INT           IDENTITY (1, 1) NOT NULL,
    [domain_id]   SMALLINT      NOT NULL,
    [nickname]    VARCHAR (20)  NOT NULL,
    [description] VARCHAR (100) NULL,
    [project_id]  INT           NOT NULL,
    [address1]    VARCHAR (50)  NOT NULL,
    [address2]    VARCHAR (50)  NULL,
    [city]        VARCHAR (25)  NOT NULL,
    [state]       VARCHAR (2)   NOT NULL,
    [zip]         VARCHAR (10)  NOT NULL,
    [is_default]  BIT           DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [project_id_domain_fk] FOREIGN KEY ([project_id], [domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [project_addresses_unique] UNIQUE NONCLUSTERED ([project_id] ASC, [domain_id] ASC, [nickname] ASC)
);


GO
CREATE TRIGGER check_default_project_addresses
ON project_addresses
AFTER INSERT, UPDATE 
AS 
	DECLARE @project_id int, @domain_id smallint, @is_default bit, @id int

	select top 1 @project_id = project_id, @domain_id = domain_id, @is_default = is_default, @id = id FROM inserted;

	IF (select count(*) from project_addresses where project_id = @project_id and domain_id = @domain_id) = 1
		UPDATE project_addresses set is_default = 1 where id = @id
	ELSE
		BEGIN
			IF @is_default = 1
				update project_addresses set is_default = 0 where project_id = @project_id AND domain_id = @domain_id and id <> @id;
		END
