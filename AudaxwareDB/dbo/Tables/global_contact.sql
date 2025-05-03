CREATE TABLE [dbo].[global_contact] (
    [contact_id]   INT            IDENTITY (1, 1) NOT NULL,
    [name]         VARCHAR (50)   NOT NULL,
    [contact_type] VARCHAR (50)   NULL,
    [company]      VARCHAR (50)   NULL,
    [title]        VARCHAR (50)   NULL,
    [email]        VARCHAR (50)   NULL,
    [address]      VARCHAR (50)   NULL,
    [city]         VARCHAR (25)   NULL,
    [state]        VARCHAR (2)    NULL,
    [zip]          VARCHAR (10)   NULL,
    [phone]        VARCHAR (20)   NULL,
    [fax]          VARCHAR (20)   NULL,
    [app_access]   VARCHAR (1)    NULL,
    [date_added]   DATE           NULL,
    [added_by]     VARCHAR (50)   NOT NULL,
    [comment]      VARCHAR (1000) NULL,
    [mobile]       VARCHAR (20)   NULL,
    [domain_id]    SMALLINT       NOT NULL,
    CONSTRAINT [contact_pk] PRIMARY KEY CLUSTERED ([contact_id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [unq_email]
    ON [dbo].[global_contact]([email] ASC);


GO

CREATE TRIGGER [dbo].[global_contact_insert]
ON [dbo].[global_contact]
INSTEAD OF INSERT
AS
BEGIN
	DECLARE
		@email varchar(50),
		@exist int

	SELECT @email = email FROM inserted;

	IF @email is null OR @email = ''
		BEGIN
			SET IDENTITY_INSERT global_contact ON
			INSERT INTO global_contact(contact_id, name, contact_type, company, title, email, address, city, state, zip, phone, fax, app_access, date_added, added_by, comment, mobile, domain_id) SELECT * FROM inserted;
			SET IDENTITY_INSERT global_contact OFF
		END
	ELSE
		BEGIN
			SELECT @exist = COUNT(contact_id) FROM global_contact WHERE email = @email;
			IF @exist = 0
			BEGIN
				SET IDENTITY_INSERT global_contact ON
				INSERT INTO global_contact(contact_id, name, contact_type, company, title, email, address, city, state, zip, phone, fax, app_access, date_added, added_by, comment, mobile, domain_id) SELECT * FROM inserted;
				SET IDENTITY_INSERT global_contact OFF
			END
		END
END

