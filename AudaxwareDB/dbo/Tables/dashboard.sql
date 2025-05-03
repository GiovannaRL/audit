
CREATE TABLE [dbo].[dashboard](
	[dashboard_id] [int] IDENTITY(1,1) NOT NULL,
	[domain_id] [smallint] NOT NULL,
	[workspace_collection] [varchar](50) NOT NULL,
	[workspace_id] [varchar](50) NOT NULL,
	[dataset_id] [varchar](50) NOT NULL,
	[name] [varchar](30) NOT NULL,
	[report_id] [varchar](50) NOT NULL,
  [linked_dashboard_id] INT NULL, 
    [pb_filename] VARCHAR(50) NULL, 
    CONSTRAINT [PK_dashboard] PRIMARY KEY ([dashboard_id]), 
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[dashboard]  WITH CHECK ADD  CONSTRAINT [dashboard_domain_fk] FOREIGN KEY([domain_id])
REFERENCES [dbo].[domain] ([domain_id])
GO

ALTER TABLE [dbo].[dashboard] CHECK CONSTRAINT [dashboard_domain_fk]
GO
ALTER TABLE [dbo].[dashboard] ADD CONSTRAINT UQ_Dashboard_DataSetID UNIQUE (dataset_id)
go


