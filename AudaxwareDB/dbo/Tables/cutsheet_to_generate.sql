CREATE TABLE [dbo].[cutsheet_to_generate] (
    [asset_id]      INT         NOT NULL,
    [domain_id]		SMALLINT	NOT NULL, 
    CONSTRAINT [cutsheet_to_generate_pk] PRIMARY KEY CLUSTERED ([asset_id] ASC, [domain_id] ASC)
);
