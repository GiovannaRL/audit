CREATE TABLE [dbo].[role_pages] (
    [role_name] VARCHAR (100) NOT NULL,
    [page]      VARCHAR (100) NOT NULL,
    [level]     VARCHAR (50)  NULL,
    CONSTRAINT [role_pages_pk] PRIMARY KEY CLUSTERED ([role_name] ASC, [page] ASC)
);

