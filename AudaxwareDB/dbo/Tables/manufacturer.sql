CREATE TABLE [dbo].[manufacturer] (
    [manufacturer_id]          INT           IDENTITY (1, 1) NOT NULL,
    [manufacturer_description] VARCHAR (100) NOT NULL,
    [date_added]               DATE          NULL,
    [added_by]                 VARCHAR (50)  NULL,
    [comment]                  VARCHAR (250) NULL,
    [domain_id]                SMALLINT      NOT NULL,
    CONSTRAINT [manufacturer_pk] PRIMARY KEY CLUSTERED ([manufacturer_id] ASC, [domain_id] ASC),
    FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]) ON DELETE CASCADE,
    CONSTRAINT [description_unique] UNIQUE NONCLUSTERED ([manufacturer_description] ASC, [domain_id] ASC)
);

GO

CREATE NONCLUSTERED INDEX [DomainIdIndex] ON [dbo].[manufacturer] ([domain_id])

GO
CREATE NONCLUSTERED INDEX [DescriptionIndex] ON [dbo].[manufacturer] ([manufacturer_description])

