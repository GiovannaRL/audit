CREATE TABLE [dbo].[department_type] (
    [department_type_id] INT          IDENTITY (1, 1) NOT NULL,
    [description]        VARCHAR (50) NULL,
    [domain_id]          SMALLINT     NOT NULL,
    CONSTRAINT [department_type_pk] PRIMARY KEY CLUSTERED ([department_type_id] ASC, [domain_id] ASC),
    CONSTRAINT [domain5_fk] FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]),
    CONSTRAINT [UQ_domain_description] UNIQUE NONCLUSTERED ([domain_id] ASC, [description] ASC)
);

