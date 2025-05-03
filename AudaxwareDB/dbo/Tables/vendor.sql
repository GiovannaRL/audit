CREATE TABLE [dbo].[vendor] (
    [vendor_id]  INT            IDENTITY (1, 1) NOT NULL,
    [name]       VARCHAR (50)   NULL,
    [territory]  VARCHAR (50)   NULL,
    [hospitals]  VARCHAR (100)  NULL,
    [date_added] DATE           NULL,
    [added_by]   VARCHAR (50)   NULL,
    [comment]    VARCHAR (1000) NULL,
    [domain_id]  SMALLINT       NOT NULL,
    CONSTRAINT [vendor_pk] PRIMARY KEY CLUSTERED ([vendor_id] ASC, [domain_id] ASC),
    FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]) ON DELETE CASCADE,
    CONSTRAINT [name_unique] UNIQUE NONCLUSTERED ([name] ASC, [domain_id] ASC)
);

