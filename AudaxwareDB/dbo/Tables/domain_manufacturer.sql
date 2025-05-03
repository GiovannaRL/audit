CREATE TABLE [dbo].[domain_manufacturer] (
    [domain_id]					SMALLINT		    NOT NULL,
    [manufacturer_id]           INT					NOT NULL,
    [manufacturer_domain_id]    SMALLINT			NOT NULL,
    PRIMARY KEY CLUSTERED ([domain_id] ASC, [manufacturer_id] ASC, [manufacturer_domain_id] ASC),
    FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]),
    FOREIGN KEY ([manufacturer_id], [manufacturer_domain_id]) REFERENCES [dbo].[manufacturer] ([manufacturer_id], [domain_id])
);
