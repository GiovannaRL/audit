CREATE TABLE [dbo].[user_notification] (
    [id]        INT            IDENTITY (1, 1) NOT NULL,
    [domain_id] SMALLINT       NOT NULL,
    [userId]    NVARCHAR (128) NOT NULL,
    [message]   VARCHAR (300)  NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [notifications_domain_fk] FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]),
    CONSTRAINT [notifications_user_fk] FOREIGN KEY ([userId]) REFERENCES [dbo].[AspNetUsers] ([Id]),

);

GO
CREATE NONCLUSTERED INDEX [user_notification]
    ON [dbo].[user_notification]([domain_id] ASC, [message] ASC, [userId] ASC);

GO

