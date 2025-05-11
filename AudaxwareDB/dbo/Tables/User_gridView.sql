CREATE TABLE [dbo].[User_gridView] (
    [user_id]    NVARCHAR (128) NOT NULL,
    [grid_state] TEXT           NOT NULL,
    [name]       VARCHAR (50)   NOT NULL,
    [type]       VARCHAR (50)   NOT NULL,
    [gridview_id] INT IDENTITY (1, 1) NOT NULL, 
    [sharedby_gridview_id]     INT ,
    [is_shared] BIT NOT NULL DEFAULT ((0)), 
    [domain_id] SMALLINT NOT NULL ,
    [is_private] BIT NOT NULL DEFAULT ((1)), 
    [added_by] VARCHAR(50) NULL, 
    [consolidated_columns] VARCHAR(MAX) NULL,
    CONSTRAINT [check_type] CHECK ([type]='assets_inventory' OR [type]='assets_inventory_template' OR [type]='assets_inventory_global_template' OR [type]='add_asset' OR [type]='assets_database'), 
    CONSTRAINT [PK_User_gridView] PRIMARY KEY ([gridview_id], [domain_id]),
    FOREIGN KEY ([domain_id]) REFERENCES [dbo].[domain] ([domain_id]) ON DELETE CASCADE,
    CONSTRAINT [gv_name_unique] UNIQUE NONCLUSTERED ([name] ASC, [type] ASC, [domain_id] ASC, [added_by] ASC)
    
);

GO


CREATE UNIQUE NONCLUSTERED INDEX [IX__User_gridView.name.type.domain_id.is_private] 
ON [dbo].[User_gridView] ([name],[type],[domain_id],[is_private])
WHERE [is_private] = 0;

GO
