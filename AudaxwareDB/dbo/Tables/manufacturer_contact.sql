CREATE TABLE [dbo].[manufacturer_contact] (
    [name]                   VARCHAR (50)   NOT NULL,
    [manufacturer_id]        INT            NOT NULL,
    [contact_type]           VARCHAR (50)   NULL,
    [title]                  VARCHAR (50)   NULL,
    [email]                  VARCHAR (50)   NULL,
    [address]                VARCHAR (50)   NULL,
    [city]                   VARCHAR (25)   NULL,
    [state]                  VARCHAR (2)    NULL,
    [phone]                  VARCHAR (20)   NULL,
    [fax]                    VARCHAR (20)   NULL,
    [zipcode]                VARCHAR (10)   NULL,
    [date_added]             DATE           NULL,
    [added_by]               VARCHAR (50)   NOT NULL,
    [comment]                VARCHAR (1000) NULL,
    [mobile]                 VARCHAR (20)   NULL,
    [manufacturer_domain_id] SMALLINT       NOT NULL,
    [contact_domain_id]      SMALLINT       NOT NULL,
    CONSTRAINT [manufacturer_contact_pk] PRIMARY KEY CLUSTERED ([name] ASC, [manufacturer_id] ASC, [manufacturer_domain_id] ASC),
    CONSTRAINT [manufacturer_contact_fk] FOREIGN KEY ([manufacturer_id], [manufacturer_domain_id]) REFERENCES [dbo].[manufacturer] ([manufacturer_id], [domain_id]) ON DELETE CASCADE
);

