CREATE TABLE [dbo].[vendor_contact] (
    [name]              VARCHAR (50)   NOT NULL,
    [vendor_id]         INT            NOT NULL,
    [contact_type]      VARCHAR (50)   CONSTRAINT [DF_CONTACT_TYPE] DEFAULT ('Vendor') NULL,
    [title]             VARCHAR (50)   NULL,
    [email]             VARCHAR (50)   NULL,
    [address]           VARCHAR (50)   NULL,
    [city]              VARCHAR (25)   NULL,
    [state]             VARCHAR (2)    NULL,
    [zipcode]           VARCHAR (10)   NULL,
    [phone]             VARCHAR (20)   NULL,
    [fax]               VARCHAR (20)   NULL,
    [date_added]        DATE           NULL,
    [added_by]          VARCHAR (50)   NULL,
    [comment]           VARCHAR (1000) NULL,
    [mobile]            VARCHAR (20)   NULL,
    [vendor_domain_id]  SMALLINT       NOT NULL,
    [domain_id] SMALLINT NOT NULL, 
    [vendor_contact_id] INT		IDENTITY (1, 1) NOT NULL, 
    CONSTRAINT [vendor_contact_pk] PRIMARY KEY CLUSTERED ([vendor_contact_id] ASC, [domain_id] ASC),
    CONSTRAINT [vendor_contact_fk] FOREIGN KEY ([vendor_id], [vendor_domain_id]) REFERENCES [dbo].[vendor] ([vendor_id], [domain_id]) ON DELETE CASCADE,
	CONSTRAINT [vendor_name_unique] UNIQUE NONCLUSTERED([name] ASC, [vendor_id] ASC, [vendor_domain_id] ASC)
);
GO

