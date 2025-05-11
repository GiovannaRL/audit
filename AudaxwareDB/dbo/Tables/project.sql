CREATE TABLE [dbo].[project] (
    [project_id]              INT             IDENTITY (1, 1) NOT NULL,
    [project_description]     VARCHAR (200)   NOT NULL,
    [project_start]           DATE            NULL,
    [project_end]             DATE            NULL,
    [status]                  VARCHAR (50)    NULL,
    [client_project_number]   VARCHAR (25)    NULL,
    [facility_project_number] VARCHAR (25)    NULL,
    [hsg_project_number]      VARCHAR (25)    NULL,
    [address1]                VARCHAR (50)    NULL,
    [address2]                VARCHAR (50)    NULL,
    [city]                    VARCHAR (25)    NULL,
    [state]                   VARCHAR (2)     NULL,
    [zip]                     VARCHAR (10)    NULL,
    [default_cost_field]      VARCHAR (25)    NULL,
    [medial_budget]           NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [freight_budget]          NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [warehouse_budget]        NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [tax_budget]              NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [warranty_budget]         NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [misc_budget]             NUMERIC (15, 2) DEFAULT ((0)) NULL,
    [date_added]              DATE            NULL,
    [added_by]                VARCHAR (50)    NULL,
    [comment]                 VARCHAR (1000)  NULL,
    [domain_id]               SMALLINT        NOT NULL,
    [client_id]               INT             NOT NULL,
    [client_domain_id]        SMALLINT        NOT NULL,
    [facility_id]             INT             NOT NULL,
    [facility_domain_id]      SMALLINT        NOT NULL,
    [markup] DECIMAL(10, 2) NULL, 
    [escalation] DECIMAL(10, 2) NULL, 
    [tax] DECIMAL(10, 2) NULL, 
    [freight_markup] DECIMAL(10, 2) NULL, 
    [install_markup] DECIMAL(10, 2) NULL, 
    [markup_budget] DECIMAL(10, 2) NULL, 
    [escalation_budget] DECIMAL(10, 2) NULL, 
    [install_budget] DECIMAL(10, 2) NULL, 
    [date_locked] DATE NULL, 
    [from_project_id] INT NULL, 
    [locked_comment] VARCHAR(30) NULL, 
    [locked_date] DATE NULL, 
	[copy_link] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
    CONSTRAINT [project_pk] PRIMARY KEY CLUSTERED ([project_id] ASC, [domain_id] ASC),
    FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]) ON DELETE CASCADE,
    CONSTRAINT [project_client_fk] FOREIGN KEY ([client_id], [client_domain_id]) REFERENCES [dbo].[client] ([id], [domain_id]),
    CONSTRAINT [project_facility_fk] FOREIGN KEY ([facility_id], [facility_domain_id]) REFERENCES [dbo].[facility] ([id], [domain_id])
);


GO
ALTER TABLE [dbo].[project] NOCHECK CONSTRAINT [project_client_fk];


GO
ALTER TABLE [dbo].[project] NOCHECK CONSTRAINT [project_facility_fk];


GO