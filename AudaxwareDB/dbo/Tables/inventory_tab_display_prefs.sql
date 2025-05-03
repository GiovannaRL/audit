CREATE TABLE [dbo].[inventory_tab_display_prefs] (
    [user_name]  VARCHAR (255) NOT NULL,
    [project]    VARCHAR (500) NULL,
    [phase]      VARCHAR (500) NULL,
    [department] VARCHAR (500) NULL,
    [room]       VARCHAR (500) NULL,
    CONSTRAINT [inventory_tab_display_prefs_pkey] PRIMARY KEY CLUSTERED ([user_name] ASC)
);

