CREATE TABLE [dbo].[client] (
    [id]        INT           IDENTITY (1, 1) NOT NULL,
    [name]      VARCHAR (100) NOT NULL,
    [domain_id] SMALLINT      NOT NULL,
    CONSTRAINT [pk_client_id_domain] PRIMARY KEY CLUSTERED ([id] ASC, [domain_id] ASC),
    CONSTRAINT [client_domain_fk] FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id])
);

