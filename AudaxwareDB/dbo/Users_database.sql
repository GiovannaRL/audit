CREATE ROLE rls_users AUTHORIZATION db_securityadmin;
GO
GRANT SELECT on AudaxWarePowerBI TO rls_users; 
GO
DENY VIEW DEFINITION ON __MigrationHistory TO rls_users;
GO
DENY VIEW DEFINITION ON user_notification TO rls_users;
GO
DENY VIEW DEFINITION ON inventory_options TO rls_users;
GO
DENY VIEW DEFINITION ON Sessions TO rls_users;
GO
DENY VIEW DEFINITION ON AspNetUsers TO rls_users;
GO
DENY VIEW DEFINITION ON responsability TO rls_users;
GO
DENY VIEW DEFINITION ON AspNetRoles TO rls_users;
GO
DENY VIEW DEFINITION ON project TO rls_users;
GO
DENY VIEW DEFINITION ON AspNetUserRoles TO rls_users;
GO
DENY VIEW DEFINITION ON purchase_order TO rls_users;
GO
DENY VIEW DEFINITION ON AspNetUserLogins TO rls_users;
GO
DENY VIEW DEFINITION ON dashboard TO rls_users;
GO
DENY VIEW DEFINITION ON user_project_mine TO rls_users;
GO
DENY VIEW DEFINITION ON AspNetUserClaims TO rls_users;
GO
--DENY VIEW DEFINITION ON room_it_connectivity_boxes TO rls_users;
GO
DENY VIEW DEFINITION ON assets_category TO rls_users;
GO
DENY VIEW DEFINITION ON cost_center TO rls_users;
GO
DENY VIEW DEFINITION ON manufacturer_contact TO rls_users;
GO
DENY VIEW DEFINITION ON project_department TO rls_users;
GO
DENY VIEW DEFINITION ON department_type TO rls_users;
GO
DENY VIEW DEFINITION ON domain TO rls_users;
GO
DENY VIEW DEFINITION ON manufacturer_contact_all TO rls_users;
GO
DENY VIEW DEFINITION ON vendor_contact_all TO rls_users;
GO
DENY VIEW DEFINITION ON assets TO rls_users;
GO
DENY VIEW DEFINITION ON project_room TO rls_users;
GO
DENY VIEW DEFINITION ON report_type TO rls_users;
GO
DENY VIEW DEFINITION ON project_report TO rls_users;
GO
DENY VIEW DEFINITION ON project_addresses TO rls_users;
GO
DENY VIEW DEFINITION ON asset_it_connectivity TO rls_users;
GO
DENY VIEW DEFINITION ON assets_measurement TO rls_users;
GO
DENY VIEW DEFINITION ON manufacturer TO rls_users;
GO
DENY VIEW DEFINITION ON project_phase TO rls_users;
GO
DENY VIEW DEFINITION ON inventory_po_qty_v TO rls_users;
GO
DENY VIEW DEFINITION ON project_room_inventory TO rls_users;
GO
DENY VIEW DEFINITION ON assets_codes TO rls_users;
GO
DENY VIEW DEFINITION ON report_location TO rls_users;
GO
DENY VIEW DEFINITION ON item_status_v TO rls_users;
GO
DENY VIEW DEFINITION ON phase_documents TO rls_users;
GO
DENY VIEW DEFINITION ON ancillary_v TO rls_users;
GO
DENY VIEW DEFINITION ON assets_options TO rls_users;
GO
DENY VIEW DEFINITION ON matching_values TO rls_users;
GO
DENY VIEW DEFINITION ON assets_subcategory TO rls_users;
GO
DENY VIEW DEFINITION ON global_contact TO rls_users;
GO
DENY VIEW DEFINITION ON inventory_purchase_order TO rls_users;
GO
DENY VIEW DEFINITION ON inventory_tab_display_prefs TO rls_users;
GO
DENY VIEW DEFINITION ON inventory_w_relo_v TO rls_users;
GO
DENY VIEW DEFINITION ON facility TO rls_users;
GO
DENY VIEW DEFINITION ON project_contact TO rls_users;
GO
DENY VIEW DEFINITION ON assets_project TO rls_users;
GO
DENY VIEW DEFINITION ON client TO rls_users;
GO
DENY VIEW DEFINITION ON bundle TO rls_users;
GO
DENY VIEW DEFINITION ON project_user TO rls_users;
GO

DENY VIEW DEFINITION ON related_assets TO rls_users;
GO
DENY VIEW DEFINITION ON role_pages TO rls_users;
GO
DENY VIEW DEFINITION ON User_gridView TO rls_users;
GO
DENY VIEW DEFINITION ON asset_inventory TO rls_users;
GO
DENY VIEW DEFINITION ON users_track TO rls_users;
GO
DENY VIEW DEFINITION ON vendor TO rls_users;
GO
DENY VIEW DEFINITION ON vendor_contact TO rls_users;
GO
DENY VIEW DEFINITION ON assets_vendor TO rls_users;
GO
DENY VIEW DEFINITION ON bundle_asset TO rls_users;
GO
DENY VIEW DEFINITION ON bundle_inventory TO rls_users;
GO
DENY VIEW DEFINITION ON [profile] TO rls_users;
GO
DENY VIEW DEFINITION ON fn_SUM_UNIT_PRICE TO rls_users;
GO
DENY VIEW DEFINITION ON Security.enterpriseAccessPredicate TO rls_users;
GO
DENY VIEW DEFINITION ON Security.enterpriseAccessPredicateWithAudaxwareDomain TO rls_users;
GO
DENY VIEW DEFINITION ON string_list_to_table TO rls_users;
GO
DENY VIEW DEFINITION ON project_documents TO rls_users;
GO
DENY VIEW DEFINITION ON document_types TO rls_users;
GO
DENY VIEW DEFINITION ON documents_associations TO rls_users;
GO
DENY VIEW DEFINITION ON asset_summarized TO rls_users;
GO
DENY VIEW DEFINITION ON joined_category_subcategory TO rls_users;
GO
DENY VIEW DEFINITION ON po_vendor_contact TO rls_users;
GO

CREATE USER [audaxware_1] FROM LOGIN [audaxware]
	WITH DEFAULT_SCHEMA = dbo

GO

GRANT CONNECT TO [audaxware_1]
GO

sp_addrolemember 'rls_users', 'audaxware_1';
GO


CREATE USER [dell_2] FROM LOGIN [dell]
	WITH DEFAULT_SCHEMA = dbo

GO

GRANT CONNECT TO [dell_2]
GO

sp_addrolemember 'rls_users', 'dell_2';

GO


CREATE USER [hsginc_3] FROM LOGIN [hsginc]
	WITH DEFAULT_SCHEMA = dbo

GO

GRANT CONNECT TO [hsginc_3]
GO

sp_addrolemember 'rls_users', 'hsginc_3';

GO



CREATE USER [acme_5] FROM LOGIN [acme]
	WITH DEFAULT_SCHEMA = dbo

GO

GRANT CONNECT TO [acme_5]
GO

sp_addrolemember 'rls_users', 'acme_5';

GO

CREATE USER [surgicalsolutionsintl_4] FROM LOGIN [surgicalsolutionsintl]
	WITH DEFAULT_SCHEMA = dbo

GO

GRANT CONNECT TO [surgicalsolutionsintl_4]
GO
sp_addrolemember 'rls_users', 'surgicalsolutionsintl_4';

GO


CREATE USER [sunagmed_15] FROM LOGIN [sunagmed]
	WITH DEFAULT_SCHEMA = dbo

GO

GRANT CONNECT TO [sunagmed_15]
GO
sp_addrolemember 'rls_users', 'sunagmed_15';

GO


CREATE USER [CRTKL_20] FROM LOGIN [CRTKL]
	WITH DEFAULT_SCHEMA = dbo

GO

GRANT CONNECT TO [CRTKL_20]
GO
sp_addrolemember 'rls_users', 'CRTKL_20';

GO

CREATE USER [vizientinc_19] FROM LOGIN [vizientinc]
	WITH DEFAULT_SCHEMA = dbo

GO

GRANT CONNECT TO [vizientinc_19]
GO

sp_addrolemember 'rls_users', 'vizientinc_19';

GO

CREATE USER [aecom_21] FROM LOGIN [aecom]
	WITH DEFAULT_SCHEMA = dbo

GO
GRANT CONNECT TO [aecom_21]
GO
sp_addrolemember 'rls_users', 'aecom_21';

GO



CREATE USER [abcdrives_22] FROM LOGIN [abcdrives]
	WITH DEFAULT_SCHEMA = dbo

GO

GRANT CONNECT TO [abcdrives_22]
GO

sp_addrolemember 'rls_users', 'abcdrives_22';
GO


CREATE USER [emtsolutions_23] FROM LOGIN [emtsolutions]
	WITH DEFAULT_SCHEMA = dbo

GO

GRANT CONNECT TO [emtsolutions_23]
GO

sp_addrolemember 'rls_users', 'emtsolutions_23';
GO

CREATE USER [millcreek_24] FROM LOGIN [millcreek]
	WITH DEFAULT_SCHEMA = dbo

GO

GRANT CONNECT TO [millcreek_24]
GO

sp_addrolemember 'rls_users', 'millcreek_24';
GO


