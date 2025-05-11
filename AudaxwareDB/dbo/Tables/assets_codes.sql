CREATE TABLE [dbo].[assets_codes] (
    [prefix]      VARCHAR (3)  NOT NULL,
    [description] VARCHAR (50) NOT NULL,
    [next_seq]    INT          NULL,
    [domain_id]   SMALLINT     NOT NULL,
    [added_by] VARCHAR(50) NULL, 
    [date_added] DATE NULL, 
    CONSTRAINT [equipment_codes_pk] PRIMARY KEY CLUSTERED ([prefix] ASC, [domain_id] ASC),
    CONSTRAINT [domain4_fk] FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id])
);

