CREATE VIEW [dbo].[asset_summarized]
	AS
	SELECT
		a.asset_id,
		a.domain_id,
		a.discontinued,
		a.cut_sheet, a.cad_block, a.revit, a.photo,
		a.asset_code,
		a.serial_name, a.serial_number,
		a.asset_description,
		a.min_cost, a.max_cost, a.avg_cost, a.last_cost,
		a.default_resp,
		a.comment,
		a.updated_at,
		a.asset_suffix,
		m.manufacturer_description AS manufacturer_description,
		m.domain_id as manufacturer_domain_id, m.manufacturer_id,
		CASE WHEN CHARINDEX('.', d.[name]) > 0 THEN SUBSTRING(d.[name], 1, CHARINDEX('.', d.[name])-1) ELSE d.[name] END AS owner_name,
		asub.[description] AS asset_subcategory,
		ac.[description] AS asset_category,
		aa.asset_code AS alternate_asset,
		STUFF(( SELECT DISTINCT ';' + CASE WHEN ao.code IS NOT NULL AND ao.code <> '' THEN ao.code ELSE NULL END
			 FROM assets_options AS ao
			 WHERE a.domain_id = ao.domain_id AND a.asset_id = ao.asset_id
			 FOR XML PATH('')),1 ,1, '') AS options_code,
		STUFF(( SELECT DISTINCT ';' + CASE WHEN av.model_number IS NOT NULL AND av.model_number <> '' THEN av.model_number ELSE NULL END
			 FROM assets_vendor AS av
			 WHERE a.domain_id = av.asset_domain_id AND a.asset_id = av.asset_id
			 FOR XML PATH('')),1 ,1, '') AS vendors_model,
		/*Gases*/
		a.medgas, a.medgas_option, 
		a.medgas_oxygen, 
		a.medgas_air, 
		a.medgas_n2o, a.medgas_co2, 
		a.medgas_wag, a.medgas_other,
		a.medgas_nitrogen, a.medgas_vacuum,
		a.medgas_steam, a.medgas_natgas,
		a.gas_liquid_co2, a.gas_liquid_nitrogen, 
		a.gas_instrument_air, a.gas_liquid_propane_gas, 
		a.gas_methane, a.gas_butane, 
		a.gas_propane, a.gas_hydrogen, 
		a.gas_acetylene,
		a.connection_type,
		/*Environmental*/
		a.misc_antimicrobial, 
		a.misc_ecolabel, a.misc_ecolabel_desc, 
		/*Plumbing*/
		a.plumbing, a.plumbing_option, 
		a.plu_hot_water, 
		a.plu_cold_water, 
		a.plu_drain, a.plu_return, 
		a.plu_treated_water,
		a.plu_chilled_water, a.plu_relief,
		/*HVAC*/
		a.water, a.water_option, a.cfm, a.btus,
		/*Electrical*/
		a.electrical, a.electrical_option, a.volts, a.phases, a.hertz, a.amps, a.volt_amps, a.watts, a.plug_type,
		/*Support*/
		a.blocking, a.blocking_option, a.supports, a.supports_option, 
		a.misc_seismic,
		/*Physical*/
		a.misc_ase, a.misc_ada, a.mobile, a.misc_shielding_lead_line, a.misc_shielding_magnetic,
		a.mobile_option, a.height, a.width, a.depth, a.mounting_height, a.clearance_top, a.clearance_bottom,
		a.clearance_right, a.clearance_left, a.clearance_front, a.clearance_back, a.weight, a.loaded_weight, a.ship_weight,
		/*IT*/
		a.data_option, a.data AS data_desc, a.lan, a.network_type, a.network_option,
		a.ports, a.bluetooth, a.cat6, a.displayport, a.dvi, a.hdmi, a.wireless,
		a.class,
		CASE WHEN a.jsn_suffix IS NOT NULL THEN j.jsn_code + '.' + a.jsn_suffix ELSE j.jsn_code END as jsn_code,
		j.description as jsn_description, j.comments as jsn_comments, j.nomenclature as jsn_nomenclature,  
		a.jsn_utility1, a.jsn_utility2, a.jsn_utility3, a.jsn_utility4, a.jsn_utility5, a.jsn_utility6, a.jsn_utility7,
		(select p.project_description from project p where p.project_id = a.imported_by_project_id and p.domain_id = a.domain_id) as imported_by_project,
		CASE WHEN a.approval_pending_domain IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS approval_pending,
		CASE WHEN a.approval_modify_aw_asset = 1 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS approval_modify_aw_asset
	 FROM assets a
	 INNER JOIN manufacturer m ON a.manufacturer_domain_id = m.domain_id AND a.manufacturer_id = m.manufacturer_id
	 INNER JOIN domain d ON d.domain_id = a.domain_id
	 INNER JOIN assets_subcategory asub ON a.subcategory_domain_id = asub.domain_id AND a.subcategory_id = asub.subcategory_id
	 INNER JOIN assets_category ac ON asub.category_domain_id = ac.domain_id AND asub.category_id = ac.category_id
	 LEFT JOIN assets AS aa ON a.alternate_asset is not null AND a.domain_id = aa.domain_id AND a.alternate_asset = aa.asset_id
	 LEFT JOIN jsn as j on j.id = a.jsn_id AND j.domain_id = a.jsn_domain_id;

