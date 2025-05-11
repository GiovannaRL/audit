CREATE TABLE [dbo].[user_project_mine] (
    [userId]     NVARCHAR (128) NOT NULL,
    [project_id] INT            NOT NULL,
    [domain_id]  SMALLINT       NOT NULL,
    [user_project_mine_id] INT IDENTITY (1, 1) NOT NULL, 
    PRIMARY KEY CLUSTERED ([project_id] ASC, [domain_id] ASC, [userId] ASC),
    FOREIGN KEY ([userId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY ([project_id], [domain_id]) REFERENCES [dbo].[project] ([project_id], [domain_id]) ON DELETE CASCADE ON UPDATE CASCADE
);

