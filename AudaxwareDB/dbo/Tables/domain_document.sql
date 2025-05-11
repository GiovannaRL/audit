CREATE TABLE [dbo].[domain_document]
(
	[id] [int] IDENTITY(1,1) NOT NULL,
	[domain_id] [smallint] NOT NULL,
	[filename] [varchar](200) NOT NULL,
	-- Type of document: 
	[type_id] [int] NOT NULL DEFAULT(5),
	[date_added] [date] NOT NULL,
	[blob_file_name] [varchar](100) NULL,
	[file_extension] [varchar](10) NULL,
	CONSTRAINT domain_document_pk PRIMARY KEY CLUSTERED ([domain_id] ASC, [id] ASC),
	CONSTRAINT domain_document_domain_id_fk FOREIGN KEY (domain_id) REFERENCES domain(domain_id),
	CONSTRAINT domain_document_type_id_fk FOREIGN KEY (type_id) REFERENCES document_types(id)
);
