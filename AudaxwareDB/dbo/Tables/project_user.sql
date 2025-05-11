CREATE TABLE [dbo].[project_user] (
	[project_domain_id] SMALLINT NOT NULL,
    [project_id] INT            NOT NULL,
    [user_pid]   NVARCHAR (128) NOT NULL,
    [project_user_id] INT IDENTITY (1, 1) NOT NULL, 
    CONSTRAINT [project_user_pk] PRIMARY KEY CLUSTERED ([project_domain_id] ASC, [project_id] ASC, [user_pid] ASC),
    CONSTRAINT [user_pid_fk] FOREIGN KEY ([user_pid]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [project_user_proj_fk] FOREIGN KEY ([project_id], [project_domain_id]) REFERENCES [dbo].[project] (project_id, domain_id) ON DELETE CASCADE
);

