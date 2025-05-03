CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (128) NOT NULL,
    [UserName]             NVARCHAR (256) NOT NULL,
    [Email]                NVARCHAR (256) NOT NULL,
    [Comment]              VARCHAR (128)  NULL,
    --[Password]             VARCHAR (128)  NOT NULL,
    [LastActivityDate]     DATETIME2 (7)  NULL,
    [LastLoginDate]        DATETIME2 (7)  NULL,
    [CreationDate]         DATETIME2 (7)  NULL,
    [IsOnLine]             BIT            NULL,
    [LockoutEnabled]       BIT            NOT NULL,
    [LockoutEndDateUtc]    DATETIME       NULL,
    [IsPasswordTemporary]  BIT            DEFAULT ((1)) NOT NULL,
    [accept_user_license]  BIT            DEFAULT ((0)) NOT NULL,
    [first_name]           VARCHAR (30)   NULL,
    [last_name]            VARCHAR (30)   NULL,
    [Hometown]             NVARCHAR (MAX) NULL,
    [EmailConfirmed]       BIT            DEFAULT ((1)) NOT NULL,
    [PasswordHash]         NVARCHAR (MAX) NULL,
    [PhoneNumber]          NVARCHAR (MAX) NULL,
    [PhoneNumberConfirmed] BIT            DEFAULT ((1)) NOT NULL,
    [TwoFactorEnabled]     BIT            DEFAULT ((0)) NOT NULL,
    [AccessFailedCount]    INT            DEFAULT ((0)) NOT NULL,
    [SecurityStamp]        NVARCHAR (MAX) NULL,
    [domain_id]            SMALLINT       NULL,
    CONSTRAINT [users_pkey] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [user_domain_fk] FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]),
    UNIQUE NONCLUSTERED ([Email] ASC),
    UNIQUE NONCLUSTERED ([UserName] ASC)
);


GO
ALTER TABLE [dbo].[AspNetUsers] NOCHECK CONSTRAINT [user_domain_fk];


GO

CREATE TRIGGER [dbo].[REMOVE_REPORTS]
    ON [dbo].[AspNetUsers]
    FOR DELETE
    AS
    BEGIN
        SET NoCount ON

		DELETE FROM project_report WHERE generated_by in (SELECT Id FROM deleted) AND isPrivate = 1;

		UPDATE project_report SET generated_by = 'removed user' WHERE generated_by in (SELECT Id FROM deleted) AND isPrivate = 0;
    END