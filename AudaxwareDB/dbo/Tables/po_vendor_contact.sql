CREATE TABLE [dbo].[po_vendor_contact]
(
	[po_id] INT NOT NULL , 
    [po_domain_id] SMALLINT NOT NULL, 
    [project_id] INT NOT NULL, 
    [vendor_contact_id] INT NOT NULL, 
    [vendor_contact_domain_id] SMALLINT NOT NULL, 
    PRIMARY KEY ([po_id], [po_domain_id], [project_id], [vendor_contact_id], [vendor_contact_domain_id]), 
	CONSTRAINT [po_vendor_contact_po_fk] FOREIGN KEY ([po_id], [po_domain_id], [project_id]) REFERENCES [purchase_order]([po_id], [domain_id], [project_id]) ON DELETE CASCADE,
	CONSTRAINT [po_vendor_contact_vendor_contact_fk] FOREIGN KEY ([vendor_contact_id], [vendor_contact_domain_id]) REFERENCES [vendor_contact]([vendor_contact_id], [domain_id])
)
