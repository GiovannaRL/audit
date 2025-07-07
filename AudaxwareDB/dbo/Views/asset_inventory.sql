

CREATE VIEW [dbo].[asset_inventory] AS 
	
SELECT 
		a.inventory_id as inventory_id,
		'' as inventory_ids,
		0 as consolidated_view,
		p.project_description,
		PP.description AS phase_description,
		PD.description AS department_description,
		PR.drawing_room_number as room_number,
		PR.drawing_room_name AS room_name,
		PR.final_room_number,
		PR.final_room_name,
		pr.room_quantity as room_count,
		A.resp,
		CASE WHEN a.inventory_source_id is not null THEN 'RELOCATE' WHEN r.isNew = 0 THEN 'EXISTING' ELSE 'NEW' END AS type_resp,
		A.project_id,
        A.phase_id,
        a.department_id,
		a.room_id,
        a.asset_id,
        a.asset_domain_id,
		c.asset_code,
		c.comment as asset_comment,
		coalesce(a.cad_id, '') as cad_id,
		a.asset_description,
        a.manufacturer_description,
        (coalesce(a.budget_qty,0) * IIF(pr.room_quantity > 0, pr.room_quantity, 1)) as budget_qty,
        (COALESCE(A.dnp_qty,0) * IIF(pr.room_quantity > 0, pr.room_quantity, 1)) AS dnp_qty,
        (COALESCE(A.lease_qty, 0) * IIF(pr.room_quantity > 0, pr.room_quantity, 1)) AS lease_qty,
		po_info.quote_number,
		po_info.vendor,
		po_info.po_requested_number,		
        COALESCE(po_info.po_qty, 0) AS po_qty,		
		--CASE
  --          WHEN r.isNew = 0 THEN 0
		--	ELSE COALESCE(a.unit_budget, 0)
  --      END AS unit_cost,
		CASE
            WHEN r.isNew = 0 THEN 0
			ELSE a.asset_total_budget
        END AS asset_total_budget,
		CASE
            WHEN r.isNew = 0 THEN '0'
			ELSE CAST(COALESCE(a.options_price, 0) AS VARCHAR(15))
        END AS options_price,
		CASE
            WHEN r.isNew = 0 THEN 0
             ELSE (coalesce(a.total_budget_amt,0) * IIF(pr.room_quantity > 0, pr.room_quantity, 1))
        END AS total_budget_amt,
		(COALESCE(po_info.total_po_amt ,0)) as total_po_amt,
		(COALESCE(a.unit_budget, 0) * COALESCE(po_info.po_qty_with_quote, 0)) - COALESCE(po_info.total_po_amt, 0) as buyout_delta,
        c.cut_sheet,
        c.cad_block,
        a.model_number,
        a.model_name,
		a.serial_number,
		c.revit,
		asset_photo_doc.rotate AS photo_rotate,
		COALESCE(asset_photo_doc.blob_file_name, c.photo) AS photo,
		CASE WHEN asset_photo_doc.blob_file_name is null THEN a.asset_domain_id ELSE a.domain_id END as photo_domain_id,
		tag_photo_doc.blob_file_name as tag_photo,
        (CASE WHEN C.discontinued = 1 AND p.status IN('A','P') THEN 'D' ELSE '' END) discontinued,
        net_new,
		cc.code AS cost_center,
		a.cost_center_id,
		COALESCE(STUFF(( SELECT distinct '; ' + CASE WHEN PO2.status IS NULL THEN 'None'
        WHEN PO2.status = 'Open' THEN PO2.status
        WHEN PO2.status = 'Quote Requested' THEN 'Qreq'
        WHEN PO2.status = 'Quote Received' THEN 'Qrec'
        WHEN PO2.status = 'PO Requested' THEN 'PO Req'
        WHEN PO2.status = 'PO Issued' THEN 'PO Issued'
        END FROM inventory_purchase_order as ipo
		inner join purchase_order po2 on po2.po_id = ipo.po_id and  ipo.project_id = po2.project_id
		WHERE ipo.inventory_id = a.inventory_id and ipo.po_qty > 0
		FOR XML PATH('')),1 ,1, ''), 'None')  as po_status,
		coalesce(a.budget_qty,0) - coalesce(po_info.po_qty,0) as po_status_none, 
		coalesce(po_info.po_status_open, 0) as po_status_open,
		coalesce(po_info.po_status_issued, 0) as po_status_issued,
		coalesce(po_info.po_status_requested, 0) as po_status_requested,
		coalesce(po_info.po_status_qreceived, 0) as po_status_qreceived,
		coalesce(po_info.po_status_qrequested, 0) as po_status_qrequested,
		A.current_location,
		coalesce(a.tag, '') as tag,
		coalesce(a.unit_budget,0) as unit_budget,
		CAST(a.estimated_delivery_date as varchar(10)) as estimated_delivery_date,
		coalesce(a.comment, '') as comment,
		a.domain_id,
		a.none_option,
		/*Gases*/
		c.medgas, c.medgas_option, 
		CAST(c.medgas_oxygen as varchar(4)) as medgas_oxygen, 
		CAST(c.medgas_air as varchar(4)) as medgas_air, 
		CAST(c.medgas_n2o as varchar(4)) as medgas_n2o,CAST(c.medgas_co2 as varchar(4)) as medgas_co2, 
		CAST(c.medgas_wag as varchar(4)) as medgas_wag, CAST(c.medgas_other as varchar(4)) as medgas_other,
		CAST(c.medgas_nitrogen as varchar(4)) as medgas_nitrogen, CAST(c.medgas_vacuum as varchar(4)) as medgas_vacuum, 
		CAST(c.medgas_steam as varchar(4)) as medgas_steam, CAST(c.medgas_natgas as varchar(4)) as medgas_natgas,
		CAST(c.gas_liquid_co2 as varchar(4)) as gas_liquid_co2, CAST(c.gas_liquid_nitrogen as varchar(4)) as gas_liquid_nitrogen, 
		CAST(c.gas_instrument_air as varchar(4)) as gas_instrument_air, CAST(c.gas_liquid_propane_gas as varchar(4)) as gas_liquid_propane_gas, 
		CAST(c.gas_methane as varchar(4)) as gas_methane, CAST(c.gas_butane as varchar(4)) as gas_butane, 
		CAST(c.gas_propane as varchar(4)) as gas_propane, CAST(c.gas_hydrogen as varchar(4)) as gas_hydrogen, 
		CAST(c.gas_acetylene as varchar(4)) as gas_acetylene, CAST(c.medgas_high_pressure as varchar(4)) as medgas_high_pressure, CAST(c.misc_shielding_magnetic as varchar(4)) as misc_shielding_magnetic,
		/*Environmental*/
		CAST(c.misc_antimicrobial as varchar(4)) as misc_antimicrobial, 
		CAST(c.misc_ecolabel as varchar(4)) as misc_ecolabel, c.misc_ecolabel_desc, CAST(c.misc_shielding_lead_line as varchar(4)) as misc_shielding_lead_line,
		/*Plumbing*/
		c.plumbing, c.plumbing_option, 
		CAST(c.plu_hot_water as varchar(4)) as plu_hot_water, 
		CAST(c.plu_cold_water as varchar(4)) as plu_cold_water, 
		CAST(c.plu_drain as varchar(4)) as plu_drain, CAST(c.plu_return as varchar(4)) as plu_return, 
		CAST(c.plu_treated_water as varchar(4)) as plu_treated_water,
		CAST(c.plu_chilled_water as varchar(4)) as plu_chilled_water, CAST(c.plu_relief as varchar(4)) as plu_relief,
		/*HVAC*/
		c.water, c.water_option, c.cfm, c.btus,
		/*Electrical*/
		c.electrical, c.electrical_option, a.volts, a.volts_ow, c.phases, c.hertz, a.amps, a.amps_ow, c.volt_amps, c.watts,
		a.connection_type, a.connection_type_ow, a.plug_type, a.plug_type_ow,
		/*Support*/
		c.blocking, c.blocking_option, c.supports, c.supports_option as supports_option, 
		CAST(c.misc_seismic as varchar(4)) as misc_seismic,
		/*Physical*/
		CAST(c.misc_ase as varchar(4)) as misc_ase, CAST(c.misc_ada as varchar(4)) as misc_ada, c.mobile, 
		c.mobile_option, a.height, a.height_ow, a.width, a.width_ow, a.depth, a.depth_ow,
		a.mounting_height, a.mounting_height_ow, c.clearance_top, c.clearance_bottom,
		c.clearance_right, c.clearance_left, c.clearance_front, c.clearance_back, c.weight, c.loaded_weight, c.ship_weight,
		/*IT*/
		a.lan, a.lan_ow,
		a.network_type, a.network_type_ow,
		a.network_option, a.network_option_ow,
		coalesce(a.ports, 0) as ports, a.ports_ow,
		CAST(a.bluetooth as varchar(4)) as bluetooth, a.bluetooth_ow,
		CAST(a.cat6 as varchar(4)) as cat6, a.cat6_ow,
		CAST(a.displayport as varchar(4)) as displayport, a.displayport_ow,
		CAST(a.dvi as varchar(4)) as dvi, a.dvi_ow,
		CAST(a.hdmi as varchar(4)) as hdmi, a.hdmi_ow,
		CAST(a.wireless as varchar(4)) as wireless, a.wireless_ow,
		(select count(connectivity_id) from asset_it_connectivity aic where aic.inventory_id_in = a.inventory_id or aic.inventory_id_out = a.inventory_id) as it_connections,
		a.option_ids,
		a.lead_time,
		COALESCE(c.data, '') AS data_desc, c.data_option,
		STUFF(( SELECT distinct '; ' + ao.code FROM inventory_options as io
		inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
		WHERE a.inventory_id = io.inventory_id
		FOR XML PATH('')),1 ,1, '')  as option_codes,
		 STUFF(( SELECT distinct '; ' + ao.description FROM inventory_options as io
		inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
		WHERE a.inventory_id = io.inventory_id
		 FOR XML PATH('')),1 ,1, '')  as option_descriptions,
		 (select count(asset_option_id) from assets_options ao where ao.asset_id = a.asset_id and ao.domain_id = a.asset_domain_id) as total_assets_options,
		 case when a.linked_id_template is null then 0 else 1 end as linked_template,
		 CASE WHEN asset_profile IS NULL OR asset_profile = '' THEN NULL ELSE CONCAT(asset_profile, CASE WHEN a.detailed_budget = 1 THEN CONCAT(' - ', asset_profile_budget) ELSE '' END) END AS asset_profile,
		 a.asset_profile AS profile_text, a.asset_profile_budget AS profile_budget,
		 a.detailed_budget, cast(coalesce(total_unit_budget,0) as varchar(15)) as total_unit_budget,
		 a.linked_document, pdoc.blob_file_name AS pdoc_blob_filename,
		 CASE WHEN pdoc.file_extension = NULL THEN pdoc.filename ELSE CONCAT(pdoc.filename, '.', pdoc.file_extension) END AS pdoc_filename,
		 CASE WHEN a.linked_document IS NOT NULL AND dt.name = 'Shop Drawing' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS has_shop_drawing,
		 STUFF(( SELECT distinct '<br/>' + CONCAT(ao.description, '(', io.quantity, ')') + CASE WHEN a.detailed_budget = 1 THEN CONCAT(' - $', io.unit_price) ELSE '' END
		 FROM inventory_options as io
		 inner join assets_options as ao on ao.asset_option_id = io.option_id and ao.domain_id = io.domain_id
		 WHERE a.inventory_id = io.inventory_id
		 FOR XML PATH('')),1 ,1, '')  as profile_tooltip,

		 --COALESCE(STUFF(( SELECT distinct '; ' + PO2.PO_NUMBER FROM inventory_purchase_order as ipo
		 --inner join purchase_order po2 on po2.po_id = ipo.po_id and  ipo.project_id = po2.project_id
		 --WHERE ipo.inventory_id = a.inventory_id and ipo.po_qty > 0
		 --FOR XML PATH('')),1 ,1, ''), '')  as po_number,
		 (select coalesce(string_agg(x.po_number, '; '),'') from (SELECT distinct trim(PO2.po_number) as po_number FROM inventory_purchase_order as ipo
		 inner join purchase_order po2 on po2.po_id = ipo.po_id and  ipo.project_id = po2.project_id
		 WHERE ipo.inventory_id = a.inventory_id and ipo.po_qty > 0) as x) as po_number,
		 a.class, a.class_ow, a.clin,
		 a.jsn_code as jsn_code,
		 j.description as jsn_description, j.comments as jsn_comments, j.nomenclature as jsn_nomenclature, 
		 coalesce(a.jsn_utility1, 'N/A') as jsn_utility1, coalesce(a.jsn_utility2, 'N/A') as jsn_utility2, coalesce(a.jsn_utility3, 'N/A') as jsn_utility3, 
		 coalesce(a.jsn_utility4, 'N/A') as jsn_utility4, coalesce(a.jsn_utility5, 'N/A') as jsn_utility5, coalesce(a.jsn_utility6, 'N/A') as jsn_utility6,
		 pr.blueprint, pr.room_area, pr.room_code, pr.staff, pr.functional_area,
		 (select project from full_project_name fpn where fpn.inventory_id = a.inventory_source_id and a.inventory_source_id is not null) as source_location,
		 (select project from full_project_name fpn where fpn.inventory_id = a.inventory_target_id and a.inventory_target_id is not null) as target_location,
		 (select room from full_project_name fpn where fpn.inventory_id = a.inventory_source_id and a.inventory_source_id is not null) as source_room,
		 (select room from full_project_name fpn where fpn.inventory_id = a.inventory_target_id and a.inventory_target_id is not null) as target_room,
		 cast(coalesce(a.unit_markup, 0) as varchar(15)) as unit_markup, cast(coalesce(a.unit_escalation, 0) as varchar(15)) as unit_escalation, cast(coalesce(a.unit_freight_net, 0) as varchar(15)) as unit_freight_net, 
		 cast(coalesce(a.unit_freight_markup, 0) as varchar(15)) as unit_freight_markup, 
		 cast(coalesce(a.unit_install_net, 0) as varchar(15)) as unit_install_net, cast(coalesce(a.unit_install_markup, 0) as varchar(15)) as unit_install_markup, cast(coalesce(a.unit_tax, 0) as varchar(15)) as unit_tax,
		 '1' as departments_qty, '1' as phases_qty, 1 as quantity, '1' as rooms_qty,
		 cast(coalesce(a.unit_markup_calc, 0) as varchar(15)) as unit_markup_calc, cast(coalesce(a.unit_escalation_calc, 0) as varchar(15)) as unit_escalation_calc, cast(coalesce(a.unit_budget_adjusted, 0) as varchar(15)) as unit_budget_adjusted, 
		 cast(coalesce(a.unit_tax_calc, 0) as varchar(15)) as unit_tax_calc, cast(coalesce(a.unit_install, 0) as varchar(15)) as unit_install, cast(coalesce(a.unit_freight, 0) as varchar(15)) as unit_freight, cast(unit_budget_total as varchar(15)) as unit_budget_total, 
		 total_install_net, total_budget_adjusted, total_tax, total_install, total_freight_net, total_freight, total_budget, ECN, a.placement, a.placement_ow, coalesce(a.biomed_check_required, 0) as biomed_check_required,
		 a.asset_description_ow, a.temporary_location, a.jsn_ow, a.manufacturer_description_ow, a.model_number_ow, a.model_name_ow, a.final_disposition,
		 a.delivered_date, a.received_date, a.date_modified, a.date_added, am.eq_unit_desc
    FROM project_room_inventory a with (index(project_room_id_indx1))
		LEFT JOIN (select inventory_id, string_agg(po.quote_number, ', ') as quote_number, string_agg(v.name, ', ') as vendor, string_agg(po.po_requested_number, ', ') as po_requested_number,
			sum(
			-- We only compute PO amount if we have received a quote
				CASE
					WHEN po.quote_received_date is null THEN
						0
					ELSE
						(COALESCE(inv_po.po_qty, 0) * COALESCE(inv_po.po_unit_amt, 0))
				END
			) as total_po_amt,
			sum(
			-- We only compute PO amount if we have received a quote
				CASE
					WHEN po.quote_received_date is null THEN
						0
					ELSE
						COALESCE(inv_po.po_qty, 0)
				END
			) as po_qty_with_quote,
			sum(COALESCE(inv_po.po_qty, 0)) as po_qty, 
			Sum(Case when po.status = 'Open' then inv_po.po_qty end) as po_status_open,
			Sum(Case when po.status = 'PO Issued' then inv_po.po_qty end) as po_status_issued,
			Sum(Case when po.status = 'PO Requested' then inv_po.po_qty end) as po_status_requested,
			Sum(Case when po.status = 'Quote Received' then inv_po.po_qty end) as po_status_qreceived,
			Sum(Case when po.status = 'Quote Requested' then inv_po.po_qty end) as po_status_qrequested
			from inventory_purchase_order as inv_po
		LEFT JOIN purchase_order as po ON inv_po.po_id = po.po_id AND inv_po.project_id = po.project_id  
		LEFT JOIN vendor as v on v.vendor_id = po.vendor_id and v.domain_id = po.vendor_domain_id
		GROUP BY inventory_id) as po_info ON a.inventory_id = po_info.inventory_id
		 JOIN assets c ON a.asset_id = c.asset_id AND a.asset_domain_id = c.domain_id
		 --JOIN manufacturer e ON c.manufacturer_id = e.manufacturer_id AND e.domain_id = c.manufacturer_domain_id
		 INNER JOIN project p ON p.project_id = A.project_id and p.domain_id = a.domain_id
		 INNER JOIN project_phase PP ON PP.project_id = A.project_id AND PP.domain_id = A.domain_id AND PP.phase_id = A.phase_id
		 INNER JOIN project_department PD ON PD.project_id = A.project_id AND PD.domain_id = A.domain_id AND PD.phase_id = A.phase_id AND PD.department_id = A.department_id
		 INNER JOIN project_room PR ON PR.project_id = A.project_id AND PR.domain_id = A.domain_id AND PR.phase_id = A.phase_id AND PR.department_id = A.department_id AND PR.room_id = A.room_id 
		 INNER JOIN responsability r on trim(r.name) = trim(a.resp) AND r.domain_id IN (a.domain_id, 1)
		 LEFT JOIN cost_center cc ON cc.id = a.cost_center_id
		 LEFT JOIN project_documents AS pdoc ON pdoc.id = a.linked_document
		 LEFT JOIN document_types dt ON pdoc.type_id = dt.id
		 LEFT JOIN jsn j on j.Id = c.jsn_id
		 LEFT JOIN (
			select pdoc.blob_file_name, da.inventory_id, pdoc.rotate from documents_associations da
				inner join project_documents pdoc on pdoc.id = da.document_id and  pdoc.project_domain_id = da.project_domain_id
			WHERE pdoc.type_id = 6
		 ) as asset_photo_doc on asset_photo_doc.inventory_id = a.inventory_id
		 LEFT JOIN (
			select pdoc.blob_file_name, da.inventory_id from documents_associations da
				inner join project_documents pdoc on pdoc.id = da.document_id and  pdoc.project_domain_id = da.project_domain_id
			WHERE pdoc.type_id = 7
		 ) as tag_photo_doc on tag_photo_doc.inventory_id = a.inventory_id 
		 LEFT JOIN assets_measurement am ON c.eq_measurement_id = am.eq_unit_measure_id
		 WHERE c.approval_pending_domain IS NULL;
