CREATE TABLE [dbo].[responsability] (
    [name]  VARCHAR (10) NOT NULL,
    [isNew] BIT      DEFAULT ((0)) NOT NULL,
    [domain_id] SMALLINT NOT NULL DEFAULT ((1)), 
    [description] VARCHAR(100) NULL, 
    PRIMARY KEY CLUSTERED ([name] ASC, [domain_id] ASC),
    CONSTRAINT [fk_responsability_domain] FOREIGN KEY ([domain_id]) REFERENCES [domain]([domain_id]) ON DELETE CASCADE
);

