CREATE TABLE [dbo].[users_track] (
    [id]         INT           IDENTITY (1, 1) NOT NULL,
    [username]   VARCHAR (255) NULL,
    [created_at] DATETIME2 (7) NULL,
    [deleted_at] DATETIME2 (7) NULL,
    CONSTRAINT [id_pk] PRIMARY KEY CLUSTERED ([id] ASC)
);

