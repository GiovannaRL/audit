CREATE TABLE [dbo].[profile]
(
	[profile_id] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY,
	[asset_domain_id] SMALLINT NOT NULL,
	[asset_id] INTEGER NOT NULL,
	[profile] VARCHAR(3000) NOT NULL,
	[label] VARCHAR(100) NULL,
	CONSTRAINT Unq_asset_profile UNIQUE(asset_domain_id, asset_id, [profile]),
	CONSTRAINT [asset_domain2_fk] FOREIGN KEY ([asset_id], [asset_domain_id]) REFERENCES [dbo].[assets] ([asset_id], [domain_id]) ON DELETE CASCADE ON UPDATE CASCADE
)
