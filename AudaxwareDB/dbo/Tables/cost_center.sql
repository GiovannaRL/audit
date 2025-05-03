CREATE TABLE [dbo].[cost_center] (
    [id]          INT           IDENTITY (1, 1) NOT NULL,
    [code]        VARCHAR (20)  NOT NULL,
    [description] VARCHAR (100) NULL,
    [project_id]  INT           NOT NULL,
    [is_default]  BIT           DEFAULT ((0)) NULL,
    [domain_id]   SMALLINT      NOT NULL,
    CONSTRAINT [cost_center_pkey] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [project_id1_fk] FOREIGN KEY ([project_id], [domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id]) ON DELETE CASCADE,
	CONSTRAINT [code_unique] UNIQUE NONCLUSTERED ([code] ASC, [project_id] ASC, [domain_id] ASC)
);


GO
CREATE TRIGGER check_default_project_cost_center
ON cost_center
AFTER INSERT, UPDATE 
AS 
	DECLARE @project_id int, @domain_id smallint, @is_default bit, @id int

	select top 1 @project_id = project_id, @domain_id = domain_id, @is_default = is_default, @id = id FROM inserted;

	--IF (select count(*) from cost_center where project_id = @project_id and domain_id = @domain_id) = 1
	--	UPDATE cost_center set is_default = 1 where id = @id
	--ELSE
	--	BEGIN
			IF @is_default = 1
				update cost_center set is_default = 0 where project_id = @project_id AND domain_id = @domain_id and id <> @id;
		--END

GO

CREATE INDEX [idx_cost_center_domain_project_code] ON [dbo].[cost_center] ([domain_id], [project_id], [code])
