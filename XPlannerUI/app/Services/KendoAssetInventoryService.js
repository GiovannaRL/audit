xPlanner.factory('KendoAssetInventoryService', ['AuthService', 'HttpService', 'ProgressService', 'toastr', '$timeout', '$q',
        'localStorageService', '$stateParams', 'KendoGridService', 'UtilsService', 'WebApiService', 'PlacementList',
    function (AuthService, HttpService, ProgressService, toastr, $timeout, $q, localStorageService, $stateParams,
        KendoGridService, UtilsService, WebApiService, PlacementList) {

        var jsn_labels = UtilsService.GetJSNAttributesLabel();
        var jsn_utilities = UtilsService.GetJSNUtilities();
        var resp_description = "";

        /* Get Columns */
        function $GetColumnsConfig(isTemplate, isGlobalTemplate, grid_name) {

            columns = [
                 {
                     headerTemplate: KendoGridService.GetSelectAllTemplate(grid_name),
                     template: function (dataItem) {
                         return KendoGridService.GetSelectRowTemplate(dataItem.id);
                     },
                     width: "4em", lockable: false
                 },
                 { field: "phase_description", title: "Phase", width: "150px" },
                 { field: "department_description", title: "Department", width: "150px" },
                 { field: "room_number", title: "Room No.", width: "120px" },
                 { field: "room_name", title: "Room Name", width: "150px" },
                 {
                     field: "asset_code", title: "Code", width: "130px",
                     template: function (dataItem) {
                         return KendoGridService.GetAssetCodeLinkTemplate(dataItem);
                     }, lockable: false
                 },
                 { field: "jsn_code", title: jsn_labels['jsn_code'], width: "100px" },
                 { field: "asset_description", title: "Description", width: "250px", template: "#= asset_description + (tag ? '(' + tag + ')' : '') #", lockable: false },
                 { field: "final_room_number", title: "Wayfinding No.", width: "170px", hidden: true },
                 { field: "final_room_name", title: "Wayfinding Name", width: "170px", hidden: true },
                 { field: "room_code", title: "Room Code", width: 150, hidden: true },
                 { field: "blueprint", title: "Blueprint", width: 150, hidden: true },
                 { field: "staff", title: "Staff", width: 150, hidden: true },
                 { field: "room_area", title: "Room Area", width: 150, hidden: true },
                 { field: "functional_area", title: "Functional Area", width: 180, hidden: true },
                 {
                     field: "resp", title: "Resp", width: "110px", headerTemplate: "<div align=center class=\"comment-header\">Resp<i style=\"vertical-align:middle;font-size:18px\" class=\"material-icons no-button\" title=\"" + resp_description + "\">info</i></div>"
                 },
                 { field: "type_resp", title: "Type", width: "120px" },
                 {
                     field: "source_location", title: "Source Location", width: "200px", hidden: true, attributes: { "class": "no-multilines" }, template: function (dataItem) {
                         if (dataItem.source_location == "Multiple")
                             return dataItem.source_location

                         if (dataItem.source_location) {
                             var data = dataItem.source_location.split('||');
                             return KendoGridService.GetLocationLink(data[1], JSON.parse(data[0]));
                         }

                         return '';
                     }, lockable: false
                 },
                 {
                     field: "target_location", title: "Target Location", width: "200px", hidden: true, attributes: { "class": "no-multilines" }, template: function (dataItem) {
                         if (dataItem.target_location == "Multiple") 
                             return dataItem.target_location
                         
                         if (dataItem.target_location) {
                             var data = dataItem.target_location.split('||');
                             return KendoGridService.GetLocationLink(data[1], JSON.parse(data[0]));
                         }

                         return '';
                     }, lockable: false
                 },
                 {
                     field: "source_room", title: "Source Room", width: "200px", hidden: true, attributes: { "class": "no-multilines" }, template: function (dataItem) {
                         if (dataItem.source_room) {
                             var data = dataItem.source_room.split('||');
                             return KendoGridService.GetLocationLink(data[1], JSON.parse(data[0]));
                         }

                         return '';
                     }, lockable: false
                 },
                 {
                     field: "target_room", title: "Target Room", width: "200px", hidden: true, attributes: { "class": "no-multilines" }, template: function (dataItem) {
                         if (dataItem.target_room) {
                             var data = dataItem.target_room.split('||');
                             return KendoGridService.GetLocationLink(data[1], JSON.parse(data[0]));
                         }

                         return '';
                     }, lockable: false
                 },
                { field: "temporary_location", title: "Temporary Location", width: "200px", hidden: true, attributes: { "class": "no-multilines" } },
                { field: "final_disposition", title: "Final Disposition", width: "200px", hidden: true, attributes: { "class": "no-multilines" } },
                 {
                     field: "photo", title: 'Photo', width: "100px", filterable: false,
                     template: function (dataItem) {
                         return dataItem.photo ?
                             '<section align=center>' +
                             '<i onmouseover="showImage(this, ' + dataItem.photo_domain_id + ',' + dataItem.photo_rotate + ', \'' + dataItem.photo + '\')" onmouseout="hideImage(this)" class="material-icons">visibility</i>' +
                                         '<section align=center class="image_popover grid-column">' +
                                             '<img title="picture not found">' +
                                         '</section>' +
                                     '</section>' : '';
                     }
                 },
                 {
                     field: "tag_photo", title: 'Tag Photo', width: "140px", filterable: false,
                     template: function (dataItem) {
                         return dataItem.tag_photo ?
                             '<section align=center>' +
                             '<i onmouseover="showImage(this, ' + dataItem.domain_id + ',' + dataItem.photo_rotate + ', \'' + dataItem.tag_photo + '\')" onmouseout="hideImage(this)" class="material-icons">visibility</i>' +
                                         '<section align=center class="image_popover grid-column">' +
                                             '<img title="picture not found">' +
                                         '</section>' +
                                     '</section>' : '';
                     }
                 },
                 { field: "cad_id", title: "CAD ID", width: "120px" },
                 { field: "model_number", title: "Model No.", width: "130px" },
                 { field: "model_name", title: "Model Name", width: "150px" },
                 { field: "manufacturer_description", title: "Manufacturer", width: "150px", template: "#= manufacturer_description != '--none--' && manufacturer_description != 'Unknown' ? manufacturer_description || '' : '' #", lockable: false },
                 { field: "room_count", title: "Room Count", width: "150px", template: "<center>#: room_count #</center>", lockable: false },
                 { field: "budget_qty", title: "Planned qty", width: "130px", template: "<center>#: budget_qty || 0 #</center>", lockable: false },
                 { field: "medgas", title: "Gases description", width: "180px", hidden: true },
                 { field: "medgas_option", title: "Gases Required", width: "170px", hidden: true },
                 { field: "medgas_oxygen", title: "Oxygen", width: "115px", hidden: true },
                 { field: "medgas_air", title: "MedAir", width: "115px", hidden: true },
                 { field: "medgas_n2o", title: "N2O", width: "100px", hidden: true },
                 { field: "medgas_co2", title: "CO2", width: "100px", hidden: true },
                 { field: "medgas_wag", title: "WAG", width: "100px", hidden: true },
                 { field: "medgas_other", title: "Air Low Pressure", width: "120px", hidden: true },
                 { field: "medgas_high_pressure", title: "Air High Pressure", width: "120px", hidden: true },
                 { field: "medgas_nitrogen", title: "Nitrogen", width: "120px", hidden: true },
                 { field: "medgas_vacuum", title: "Vacuum", width: "110px", hidden: true },
                 { field: "medgas_steam", title: "Steam", width: "115px", hidden: true },
                 { field: "medgas_natgas", title: "NatGas", width: "120px", hidden: true },
                 { field: "gas_liquid_co2", title: "Liquid CO2", width: "120px", hidden: true },
                 { field: "gas_liquid_nitrogen", title: "Liquid Nitrogen", width: "120px", hidden: true },
                 { field: "gas_instrument_air", title: "Instrument Air", width: "120px", hidden: true },
                 { field: "gas_liquid_propane_gas", title: "Liquid Propane Gas", width: "120px", hidden: true },
                 { field: "gas_methane", title: "Methane", width: "120px", hidden: true },
                 { field: "gas_butane", title: "Butane", width: "120px", hidden: true },
                 { field: "gas_propane", title: "Propane", width: "120px", hidden: true },
                 { field: "gas_hydrogen", title: "Hydrogen", width: "120px", hidden: true },
                 { field: "gas_acetylene", title: "Acetylene", width: "120px", hidden: true },
                 { field: "connection_type", title: "Connection Type", width: "200px", hidden: true, template: "<div align=center>#: connection_type ? connection_type : '' #</div>" },
                 { field: "misc_antimicrobial", title: "Antimicrobial", width: "150px", hidden: true },
                 { field: "misc_ecolabel", title: "Eco-Label", width: "130px", hidden: true },
                 { field: "misc_ecolabel_desc", title: "Eco-Label Description", width: "190px", hidden: true },
                 { field: "plumbing", title: "Pumbling Description", width: "200px", hidden: true },
                 { field: "plumbing_option", title: "Plumbing Required", width: "200px", hidden: true },
                 { field: "plu_hot_water", title: "Hot Water", width: "130px", hidden: true },
                 { field: "plu_cold_water", title: "Cold Water", width: "130px", hidden: true },
                 { field: "plu_drain", title: "Drain", width: "110px", hidden: true },
                 { field: "plu_return", title: "Return", width: "110px", hidden: true },
                 { field: "plu_treated_water", title: "Treated Water", width: "150px", hidden: true },
                 { field: "plu_chilled_water", title: "Chilled Water", width: "150px", hidden: true },
                 { field: "plu_relief", title: "Relief", width: "120px", hidden: true },
                 { field: "water", title: "Exhaust Description", width: "180px", hidden: true },
                 { field: "water_option", title: "Exhaust Required", width: "180px", hidden: true },
                 { field: "cfm", title: "CFM", width: "100px", hidden: true, template: "<div align=center>#: cfm ? cfm : '' #</div>" },
                 { field: "btus", title: "BTUs", width: "100px", hidden: true, template: "<div align=center>#: btus ? btus : '' #</div>" },
                 { field: "data_desc", title: "Data Description", width: "180px", hidden: true },
                 { field: "data_option", title: "Data Required", width: "150px", hidden: true },
                 { field: "network_option", title: "Network Required", width: "250px", hidden: true },
                 { field: "network_type", title: "Network Type", width: "250px", hidden: true, template: "<div align=center>#: network_type ? network_type : '' #</div>" },
                 { field: "electrical", title: "Electrical Description", width: "180px", hidden: true },
                 { field: "electrical_option", title: "Electrical Required", width: "180px", hidden: true },
                 { field: "volts", title: "Volts", width: "100px", hidden: true, template: "<div align=center>#: volts ? volts : '' #</div>" },
                 { field: "phases", title: "Phases", width: "100px", hidden: true, template: "<div align=center>#: phases ? phases : '' #</div>" },
                 { field: "hertz", title: "Hertz", width: "100px", hidden: true, template: "<div align=center>#: hertz ? hertz : '' #</div>" },
                 { field: "plug_type", title: "Plug Type", width: "150px", hidden: true, template: "<div align=center>#: plug_type ? plug_type : '' #</div>" },
                 { field: "amps", title: "Amps", width: "100px", hidden: true, template: "<div align=center>#: amps ? amps : '' #</div>" },
                 { field: "volt_amps", title: "VoltAmps", width: "115px", hidden: true, template: "<div align=center>#: volt_amps ? volt_amps : '' #</div>" },
                 { field: "watts", title: "Watts", width: "100px", hidden: true },
                 { field: "blocking", title: "Blocking Description", width: "190px", hidden: true },
                 { field: "blocking_option", title: "Blocking Required", width: "185px", hidden: true },
                 { field: "supports", title: "Structural Description", width: "190px", hidden: true },
                 { field: "supports_option", title: "Structural Required", width: "185px", hidden: true },
                 { field: "misc_seismic", title: "Seismic Rated", width: "150px", hidden: true },
                 { field: "misc_ase", title: "ASE", width: "100px", hidden: true },
                 { field: "misc_ada", title: "ADA", width: "100px", hidden: true },
                 { field: "misc_shielding_lead_line", title: "Shielding, Lead Lined", width: "100px", hidden: true },
                 { field: "misc_shielding_magnetic", title: "Shielding, RF/Magnetic", width: "100px", hidden: true },
                 { field: "mobile", title: "Mobile Description", width: "185px", hidden: true },
                 { field: "mobile_option", title: "Mobile", width: "180px", hidden: true },
                 { field: "height", title: "Height(in)", width: "130px", hidden: true, template: "<div align=center>#: height ? height : '' #</div>" },
                 { field: "width", title: "Width(in)", width: "130px", hidden: true, template: "<div align=center>#: width ? width : '' #</div>" },
                 { field: "depth", title: "Depth(in)", width: "130px", hidden: true, template: "<div align=center>#: depth || '' #</div>" },
                 { field: "mounting_height", title: "Mounting Height(in)", width: "200px", hidden: true, template: "<div align=center>#: mounting_height || '' #</div>" },
                 { field: "clearance_top", title: "Top Clearance(in)", width: "170px", hidden: true, template: "<div align=center>#: clearance_top || '' #</div>" },
                 { field: "clearance_bottom", title: "Bottom Clearance(in)", width: "180px", hidden: true, template: "<div align=center>#: clearance_bottom || '' #</div>" },
                 { field: "clearance_right", title: "Right Clearance(in)", width: "175px", hidden: true, template: "<div align=center>#: clearance_right || '' #</div>" },
                 { field: "clearance_left", title: "Left Clearance(in)", width: "175px", hidden: true, template: "<div align=center>#: clearance_left || '' #</div>" },
                 { field: "clearance_front", title: "Front Clearance(in)", width: "175px", hidden: true, template: "<div align=center>#: clearance_front || '' #</div>" },
                 { field: "clearance_back", title: "Back Clearance(in)", width: "180px", hidden: true, template: "<div align=center>#: clearance_back || '' #</div>" },
                 { field: "weight", title: "Weight(lb)", width: "140px", hidden: true, template: "<div align=center>#: weight || '' #</div>" },
                 { field: "loaded_weight", title: "Loaded Weight(lb)", width: "180px", hidden: true, template: "<div align=center>#: loaded_weight || '' #</div>" },
                 { field: "ship_weight", title: "Ship Weight(lb)", width: "170px", hidden: true, template: "<div align=center>#: ship_weight || '' #</div>" },
                 {
                     headerTemplate:
                     "<div align=center class=\"comment-header\"><i class=\"material-icons no-button\" title=\"Comment\">comment</i></div>", width: 70, field: "comment", title: "Comment", filterable: false, sortable: false,
                     template: function (dataItem) {
                         if (dataItem.comment == null || dataItem.comment.length == 0)
                             return '';

                         return "<i class=\"material-icons no-button\" title=\"" + dataItem.comment + "\">comment</i></div>";
                     }
                 },
                  {
                      field: "cut_sheet", title: "Spec", width: "90px", filterable: false,
                      template: function (dataItem) {
                          return KendoGridService.GetDownloadFileWithDomainTemplate(dataItem.asset_id, dataItem.asset_domain_id, 'fullcutsheet', dataItem.cut_sheet ? 'images/page_attach.png' : 'images/page.png')
                      }, lockable: false, hidden: true
                  },
                  {
                      field: "has_shop_drawing", title: "Shop Drawing", width: "150px", filterable: false,
                      template: function (dataItem) {
                          if (dataItem.has_shop_drawing) {
                              return KendoGridService.GetDownloadFileWithDomainTemplate(dataItem.pdoc_blob_filename, dataItem.domain_id, 'project-documents', 'images/file-pdf.png')
                          }
                          return '';
                      }, lockable: false
                  },
                 {
                     field: "cad_block", title: "CAD Block", width: "120px", filterable: false,
                     template: function (dataItem) {
                         return KendoGridService.GetDownloadFileWithDomainTemplate(dataItem.cad_block, dataItem.asset_domain_id, 'cadblock', dataItem.cad_block ? 'images/icons/autocad.ico' : '');
                     }, lockable: false, hidden: true
                 },
                     {
                         field: "revit", title: "Revit", width: "90px", filterable: false,
                         template: function (dataItem) {
                             return KendoGridService.GetDownloadFileWithDomainTemplate(dataItem.revit, dataItem.asset_domain_id, 'revit', dataItem.revit ? 'images/icons/revit.ico' : '');
                         }, lockable: false, hidden: true
                     },
                     {
                         field: "discontinued", title: "Flag", width: "90px", filterable: false,
                         template: function (dataItem) {
                             if (dataItem.discontinued)
                                 return KendoGridService.GetCheckIconTemplate();
                             return '';
                         }, lockable: false
                     },
                     { field: "lead_time", title: "Lead Time", width: "150px", template: "<center>#: lead_time ? lead_time : '' #</center>", lockable: false },
                     { field: "estimated_delivery_date", title: "Install Date", width: "150px", template: "#: estimated_delivery_date ? kendo.toString(kendo.parseDate(estimated_delivery_date), \"MM/dd/yyyy\") : '' #" },
                      {
                          field: "asset_profile", title: "Profile", width: "200px", attributes: { "class": "no-multilines" }, template: function (dataItem) {
                              if (dataItem.asset_profile === 'Options Pending')
                                  return '<span>Options Pending</span>';

                              if (dataItem.asset_profile) {
                                  if (dataItem.profile_tooltip) {
                                      return "<span class='link' title='" + dataItem.profile_tooltip.replace('&lt;br/&gt;', '\n').replace('"', '\"').replace(/^lt;br\/\&gt;/, "") + "'>" + dataItem.asset_profile.replace('"', '\"') + "</span>";
                                  }

                                  return '<span class="link">' + dataItem.asset_profile.replace('"', '\"') + '</span>';
                              }

                              return '';
                          }, lockable: false
                      },
                    { field: "tag", title: "TAG", width: "100px", hidden: true },
                    {
                        field: "placement", title: "Placement", width: "150px", template: function (dataItem) {
                            if (dataItem.placement) {
                                var data = PlacementList.filter(function (item) { return item.value == dataItem.placement });
                                if (data.length > 0) {
                                    return data[0].name;
                                }
                            }
                            return dataItem.placement;
                        }, hidden: true
                    },
                     { field: "ecn", title: "ECN", width: "100px" },
                    { field: "biomed_check_required", title: "Biomed Check Required", width: "200px", hidden: true }
            ];

            if (!isGlobalTemplate) {
                columns.splice(15, 0, { field: "current_location", title: "Status", width: "120px", template: "#: current_location #", lockable: false },
                    { field: "po_status", title: "PO Status", width: "120px", template: "#: po_status #", lockable: false });
                

                columns.splice(20, 0, { field: "po_status_none", title: "PO Pending Qty", width: "120px", lockable: false },
                    { field: "po_status_open", title: "PO Open Qty", width: "120px", lockable: false },
                    { field: "po_status_issued", title: "PO Issued Qty", width: "130px", lockable: false },
                    { field: "po_status_requested", title: "PO Req. Qty", width: "150px", lockable: false },
                    { field: "po_status_qrequested", title: "PO Q. Req. Qty", width: "150px", lockable: false },
                    { field: "po_status_qreceived", title: "PO Q. Rcv. Qty", width: "150px", lockable: false },
                 { field: "lease_qty", title: "Lease qty", width: "120px", template: "<center>#: lease_qty || 0 #</center>", lockable: false },
                 { field: "dnp_qty", title: "DNP qty", width: "120px", template: "<center>#: dnp_qty || 0 #</center>", lockable: false },
                 { field: "net_new", title: "Net New", width: "120px", template: "<center>#: net_new #</center>" },
                 { field: "total_unit_budget", title: "Unit Budget (Net)", width: "170px", lockable: false, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.total_unit_budget); } },
                 { field: "total_budget_amt", title: "Total Budget (Net)", width: "170px", lockable: false, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.total_budget_amt); } },
                 { field: "po_qty", title: "PO qty", width: "100px", template: "<center>#: po_qty #</center>" },
                 { field: "total_po_amt", title: "PO amt", width: "150px", template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.total_po_amt); } },
                 { field: "buyout_delta", title: "PO delta", width: "120px", template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.buyout_delta); } },
                 { field: "quote_number", title: "Quote Number", width: "150px", lockable: false, hidden: true },
                 { field: "po_requested_number", title: "Requisition Number", width: "150px", lockable: false, hidden: true },
                 { field: "vendor", title: "Vendor", width: "150px", lockable: false, hidden: true },
                 { field: "unit_markup", title: "Unit Markup (%)", width: "150px", lockable: false, hidden: true },
                    { field: "unit_markup_calc", title: "Unit Markup ($)", width: "150px", lockable: false, hidden: true, template: function (dataItem) { return GetCurrency(dataItem.unit_markup_calc); } },
                { field: "unit_escalation", title: "Unit Escalation (%)", width: "170px", lockable: false, hidden: true },
                    { field: "unit_escalation_calc", title: "Unit Escalation ($)", width: "170px", lockable: false, hidden: true, template: function (dataItem) { return GetCurrency(dataItem.unit_escalation_calc); } },
                    { field: "unit_budget_adjusted", title: "Unit Budget(Adjusted)", width: "190px", lockable: false, hidden: true, template: function (dataItem) { return GetCurrency(dataItem.unit_budget_adjusted); } },
                { field: "unit_tax", title: "Unit Tax (%)", width: "150px", lockable: false, hidden: true },
                    { field: "unit_tax_calc", title: "Unit Tax ($)", width: "150px", lockable: false, hidden: true, template: function (dataItem) { return GetCurrency(dataItem.unit_tax_calc); } },
                    { field: "unit_install_net", title: "Unit Install (Net)", width: "170px", lockable: false, hidden: true, template: function (dataItem) { return GetCurrency(dataItem.unit_install_net); } },
                { field: "unit_install_markup", title: "Unit Install Markup (%)", width: "200px", lockable: false, hidden: true },
                    { field: "unit_install", title: "Unit Install", width: "150px", lockable: false, hidden: true, template: function (dataItem) { return GetCurrency(dataItem.unit_install); } },
                    { field: "unit_freight_net", title: "Unit Freight (Net)", width: "170px", lockable: false, hidden: true, template: function (dataItem) { return GetCurrency(dataItem.unit_freight_net); } },
                { field: "unit_freight_markup", title: "Unit Freight Markup (%)", width: "200px", lockable: false, hidden: true },
                    { field: "unit_freight", title: "Unit Freight", width: "150px", lockable: false, hidden: true, template: function (dataItem) { return GetCurrency(dataItem.unit_freight); } },
                    { field: "unit_budget_total", title: "Unit Budget", width: "150px", lockable: false, hidden: true, template: function (dataItem) { return GetCurrency(dataItem.unit_budget_total); } },
                { field: "total_budget_adjusted", title: "Total Budget (Adjusted)", width: "200px", lockable: false, hidden: true, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.total_budget_adjusted); } },
                { field: "total_tax", title: "Total Tax ($)", width: "150px", lockable: false, hidden: true, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.total_tax); } },
                { field: "total_install_net", title: "Total Install (Net)", width: "170px", lockable: false, hidden: true, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.total_install_net); } },
                { field: "total_install", title: "Total Install", width: "150px", lockable: false, hidden: true, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.total_install); } },
                { field: "total_freight_net", title: "Total Freight (Net)", width: "170px", lockable: false, hidden: true, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.total_freight_net); } },
                { field: "total_freight", title: "Total Freight", width: "150px", lockable: false, hidden: true, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.total_freight); } },
                { field: "total_budget", title: "Total Budget", width: "150px", lockable: false, hidden: true, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.total_budget); } },
                { field: "po_number", title: "PO Number", width: "150px" },
                { field: "delivered_date", title: "Delivered Date", width: "160px", template: "#: delivered_date ? kendo.toString(kendo.parseDate(delivered_date), \"MM/dd/yyyy\") : '' #" },
                { field: "received_date", title: "Received Date", width: "160px", template: "#: received_date ? kendo.toString(kendo.parseDate(received_date), \"MM/dd/yyyy\") : '' #" },
                { field: "date_added", title: "Date Added", width: "160px", template: "#: date_added ? kendo.toString(kendo.parseDate(date_added), \"MM/dd/yyyy\") : '' #" },
                { field: "date_modified", title: "Date Modified", width: "160px", template: "#: date_modified ? kendo.toString(kendo.parseDate(date_modified), \"MM/dd/yyyy\") : '' #" },
                { field: "option_codes", title: "Options Code", width: "200px", attributes: { "class": "no-multilines" }, template: "#: option_codes || '' #", lockable: false, hidden: true },
                { field: "option_descriptions", title: "Options Description", width: "200px", attributes: { "class": "no-multilines" }, template: "#: option_descriptions || '' #", lockable: false, hidden: true },
                {
                    field: "class", title: "Class", width: "185px", hidden: true
                },
                { field: "clin", title: "CLIN", width: "150px", hidden: true },
                { field: "jsn_nomenclature", title: "JSN Nomenclature", width: "185px", hidden: true },
                { field: "jsn_description", title: jsn_labels['jsn_description'], width: 150, filterable: false, sortable: false, hidden: true, template: "#= jsn_description ? '<div align=center><i class=\"material-icons\" title=\"' + jsn_description + '\">comment</i></div>' : '' #" },
                { field: "jsn_comments", title: "JSN Comments", width: 150, filterable: false, sortable: false, hidden: true, template: "#= jsn_comments ? '<div align=center><i class=\"material-icons\" title=\"' + jsn_comments + '\">comment</i></div>' : '' #" },
                {
                    headerTemplate:
                    "<div align=center class=\"comment-header\">" + jsn_labels['jsn_utility1'] + "<i style=\"vertical-align:middle;font-size:18px\" class=\"material-icons no-button\" title=\"" + jsn_utilities['u1'] + "\">info</i></div>", field: "jsn_utility1", title: jsn_labels['jsn_utility1'], width: "100px", hidden: true
                },
                {
                    headerTemplate:
                    "<div align=center class=\"comment-header\">" + jsn_labels['jsn_utility2'] + "<i style=\"vertical-align:middle;font-size:18px\" class=\"material-icons no-button\" title=\"" + jsn_utilities['u2'] + "\">info</i></div>", field: "jsn_utility2", title: jsn_labels['jsn_utility2'], width: "100px", hidden: true
                },
                {
                    headerTemplate:
                    "<div align=center class=\"comment-header\">" + jsn_labels['jsn_utility3'] + "<i style=\"vertical-align:middle;font-size:18px\" class=\"material-icons no-button\" title=\"" + jsn_utilities['u3'] + "\">info</i></div>", field: "jsn_utility3", title: jsn_labels['jsn_utility3'], width: "100px", hidden: true
                },
                {
                    headerTemplate:
                    "<div align=center class=\"comment-header\">" + jsn_labels['jsn_utility4'] + "<i style=\"vertical-align:middle;font-size:18px\" class=\"material-icons no-button\" title=\"" + jsn_utilities['u4'] + "\">info</i></div>", field: "jsn_utility4", title: jsn_labels['jsn_utility4'], width: "100px", hidden: true
                },
                {
                    headerTemplate:
                    "<div align=center class=\"comment-header\">" + jsn_labels['jsn_utility5'] + "<i style=\"vertical-align:middle;font-size:18px\" class=\"material-icons no-button\" title=\"" + jsn_utilities['u5'] + "\">info</i></div>", field: "jsn_utility5", title: jsn_labels['jsn_utility5'], width: "100px", hidden: true
                },
                {
                    headerTemplate:
                    "<div align=center class=\"comment-header\">" + jsn_labels['jsn_utility6'] + "<i style=\"vertical-align:middle;font-size:18px\" class=\"material-icons no-button\" title=\"" + jsn_utilities['u6'] + "\">info</i></div>", field: "jsn_utility6", title: jsn_labels['jsn_utility6'], width: "100px", hidden: true
                }
                );

                columns.splice(columns.length - 1, 0, { field: "cost_center", title: "Cost Center", width: "140px" }),
                    columns.splice(columns.length - 1, 0, { field: "inventory_id", title: "Inventory ID", width: "130px", hidden: true, template: "#: inventory_id > 0 ? inventory_id : inventory_ids #" });
            }

            return columns;
        };
        /* END - Get Columns */

        /* Get the name of the local where the state is on localStorage */
        function $GetLocalStorageName(isTemplate, isGlobalTemplate) {
            return isTemplate ? (isGlobalTemplate ? 'asset-grid-state-global-template' : 'asset-grid-state-template') : 'asset-grid-state-' + ($stateParams.room_id ? 'room' :
                $stateParams.department_id ? 'department' : $stateParams.phase_id ? 'phase' : 'project');
        };
        /* END - Get the name of the local where the state is on localStorage */

        /* Get URL */
        function _GetGridUrl(isTemplate, consolidated, consolidatedColumns, selectOnlyMode, docLinkMode, isPOPage, showApprovedAssets) {
            
            let service_name = 'asset_inventory_consolidated';
            
            if (docLinkMode) {
                service_name = 'asset_inventory_doc_link';
            } else if (isPOPage && !consolidated) {
                service_name = 'asset_inventory_available_for_po';
            } else if (!isPOPage && (isTemplate || selectOnlyMode || !(consolidated && consolidatedColumns !== undefined))) {
                service_name = 'asset_inventory';
            }

            const parameters = [
                isTemplate ? $stateParams.domain_id : AuthService.getLoggedDomain(),
                $stateParams.project_id,
                docLinkMode || $stateParams.phase_id,
                $stateParams.department_id,
                $stateParams.room_id
            ];

            if (consolidated) 
                parameters.push(isPOPage, showApprovedAssets, consolidatedColumns);
            else if (isPOPage) 
                parameters.push(showApprovedAssets);

            return HttpService[service_name](...parameters);


        };
        /* END - Get URL */

        function GetCurrency(data) {

            if (data == "Multiple") 
                return data;
            else
                return KendoGridService.GetCurrencyTemplate(data);
        }

        function _saveState(grid, isTemplate, isGlobalTemplate, isConsolidated, timeout, selectOnlyMode) {
            if (!selectOnlyMode) {
                $timeout(function () {
                    var x = angular.extend(grid.getOptions(), { isConsolidated: isConsolidated });
                    x.dataSource.data = [];
                    x.dataSource.filter = null;
                    if (localStorageService.isSupported) {
                        localStorageService.set($GetLocalStorageName(isTemplate, isGlobalTemplate), x);
                    } else if (localStorageService.cookie.isSupported) {
                        localStorageService.cookie.set($GetLocalStorageName(isTemplate, isGlobalTemplate), x);
                    }
                }, timeout || 0);
            }
        };

        function _getDataSource() {

            var dataSource = {
                pageSize: 50,
                transport: {
                    read: {
                        url: null,
                        headers: {
                            Authorization: "Bearer " + AuthService.getAccessToken()
                        }
                    },
                },
                schema: {
                    model: {
                        id: "inventory_id",
                        fields: {
                            asset_code: { type: "string" },
                            photo: { type: 'string' },
                            cad_id: { type: "string" },
                            model_number: { type: "string" },
                            model_name: { type: "string" },
                            manufacturer_description: { type: "string" },
                            asset_description: { type: "string" },
                            current_location: { type: "string" },                            
                            budget_qty: { type: "number" },
                            lease_qty: { type: "number" },
                            dnp_qty: { type: "number" },
                            net_new: { type: "number" },
                            total_unit_budget: { type: "number" },
                            total_budget_amt: { type: "number" },
                            po_qty: { type: "number" },
                            total_po_amt: { type: "number" },
                            buyout_delta: { type: "number" },
                            none_option: { type: "boolean" },
                            total_assets_options: { type: "numbers" },
                            option_codes: { type: "string" },
                            option_descriptions: { type: "string" },
                            asset_profile: { type: "string" },
                            cut_sheet: { type: "string" },
                            cad_block: { type: "string" },
                            revit: { type: "string" },
                            discontinued: { type: "string" },
                            cost_center: { type: "string" },
                            tag: { type: "string" },
                            quantity: { type: "number" },
                            phases_qty: { type: "number" },
                            departments_qty: { type: "number" },
                            rooms_qty: { type: "number" },
                            medgas: { type: "string" },
                            medgas_option: { type: "string" },
                            medgas_oxygen: { type: "string" },
                            medgas_air: { type: "string" },
                            medgas_n2o: { type: "string" },
                            medgas_co2: { type: "string" },
                            medgas_wag: { type: "string" },
                            medgas_other: { type: "string" },
                            medgas_high_pressure: { type: "string" },
                            medgas_nitrogen: { type: "string" },
                            medgas_vacuum: { type: "string" },
                            medgas_steam: { type: "string" },
                            medgas_natgas: { type: "string" },
                            gas_acetylene: { type: "string" },
                            gas_butane: { type: "string" },
                            gas_hydrogen: { type: "string" },
                            gas_instrument_air: { type: "string" },
                            gas_liquid_co2: { type: "string" },
                            gas_liquid_nitrogen: { type: "string" },
                            gas_liquid_propane_gas: { type: "string" },
                            gas_methane: { type: "string" },
                            gas_propane: { type: "string" },
                            misc_antimicrobial: { type: "string" },
                            misc_ecolabel: { type: "string" },
                            misc_ecolabel_desc: { type: "string" },
                            plumbing: { type: "string" },
                            plumbing_option: { type: "string" },
                            plu_hot_water: { type: "string" },
                            plu_cold_water: { type: "string" },
                            plu_drain: { type: "string" },
                            plu_return: { type: "string" },
                            plu_treated_water: { type: "string" },
                            plu_chilled_water: { type: "string" },
                            plu_relief: { type: "string" },
                            water: { type: "string" },
                            water_option: { type: "string" },
                            cfm: { type: "string" },
                            btus: { type: "string" },
                            data_desc: { type: "string" },
                            data_option: { type: "string" },
                            electrical: { type: "string" },
                            electrical_option: { type: "string" },
                            volts: { type: 'string' },
                            phases: { type: 'string' },
                            hertz: { type: "string" },
                            amps: { type: "string" },
                            volt_amps: { type: "string" },
                            watts: { type: "string" },
                            blocking: { type: "string" },
                            blocking_option: { type: "string" },
                            supports: { type: "string" },
                            supports_option: { type: "string" },
                            misc_seismic: { type: "string" },
                            misc_ase: { type: "string" },
                            misc_ada: { type: "string" },
                            misc_shielding_lead_line: { type: "string" },
                            misc_shielding_magnetic: { type: "string" },
                            mobile: { type: "string" },
                            mobile_option: { type: "string" },
                            height: { type: "string" },
                            width: { type: "string" },
                            depth: { type: "string" },
                            clearance_top: { type: "string" },
                            clearance_bottom: { type: "string" },
                            clearance_right: { type: "string" },
                            clearance_left: { type: "string" },
                            clearance_front: { type: "string" },
                            clearance_back: { type: "string" },
                            weight: { type: "string" },
                            loaded_weight: { type: "string" },
                            ship_weight: { type: "string" },
                            linked_template: { type: "number" },
                            room_count: { type: "number" },
                            jsn_code: { type: "string" },
                            jsn_nomenclature: { type: "string" },
                            jsn_comments: { type: "string" },
                            jsn_description: { type: "string" },
                            jsn_utility1: { type: "string" },
                            jsn_utility2: { type: "string" },
                            jsn_utility3: { type: "string" },
                            jsn_utility4: { type: "string" },
                            jsn_utility5: { type: "string" },
                            jsn_utility6: { type: "string" },
                            unit_budget_total: { type: "string" },
                            unit_markup: { type: "string" },
                            unit_markup_calc: { type: "string" },
                            unit_escalation: { type: "string" },
                            unit_escalation_calc: { type: "string" },
                            unit_budget_adjusted: { type: "string" },
                            unit_tax: { type: "string" },
                            unit_tax_calc: { type: "string" },
                            unit_install_net: { type: "string" },
                            unit_install_markup: { type: "string" },
                            unit_install: { type: "string" },
                            unit_freight_net: { type: "string" },
                            unit_freight_markup: { type: "string" },
                            unit_freight: { type: "string" },
                            total_budget_adjusted: { type: "number" },
                            total_tax: { type: "number" },
                            total_install_net: { type: "number" },
                            total_install: { type: "number" },
                            total_freight_net: { type: "number" },
                            total_freight: { type: "number" },
                            total_budget: { type: "number" },
                            po_status: { type: "string" },
                            po_status_none: { type: "number" },
                            po_status_open: { type: "number" },
                            po_status_issued: { type: "number" },
                            po_status_requested: { type: "number" },
                            po_status_qrequested: { type: "number" },
                            po_status_qreceived: { type: "number" }
                        }
                    },
                    parse: function (data) {

                        return UtilsService.GetAssetAttributesLabel(data);
                    }
                },
                error: function () {
                    ProgressService.unblockScreen();
                    toastr.error("Error to retrieve assets from server, please contact technical support");
                },
                change: function () {
                    ProgressService.unblockScreen();
                }
            };

            return dataSource;
        };

        function getResp() {
            return $q(function (resolve, reject) {
                WebApiService.genericController.get({ controller: "Responsability", action: "Description", domain_id: AuthService.getLoggedDomain() }, function (data) {
                    resp_description = data.text;
                    resolve();
                }, function () {
                    toastr.error('Error to retrieve responsability description from server, please contact technical support');
                    reject();
                });
            });

        }

        /* Set Saved State */
        function _SetSavedState(grid, isTemplate, isGlobalTemplate, height, view, grid_name, dataBoundFn, pdfExportFn, saveGridStateFn, consolidatedColumns, selectOnlyMode, docLinkMode, isPOPage, showApprovedAssets) {
            return $q(function (resolve, reject) {
                /* Export Grid configurations */
                var exportConfig = {
                    excel: {
                        fileName: ($stateParams.room_id ? "Room" : $stateParams.department_id ? "Department" : $stateParams.phase_id ? "Phase" : "Project") + " Details.xlsx",
                        allPages: true,
                        filterable: true
                    },
                    pdf: {
                        fileName: ($stateParams.room_id ? "Room" : $stateParams.department_id ? "Department" : $stateParams.phase_id ? "Phase" : "Project") + " Details.pdf",
                        allPages: true,
                        author: "audaxware",
                        creator: "audaxware",
                        margin: {
                            left: 10,
                            right: "10pt",
                            top: "10mm",
                            bottom: "1in"
                        },
                    }
                };
                /* END - Export Grid configurations */

                /* Grid Options */
                height = height || (window.innerHeight - 220);
                var gridOptions = {
                    reorderable: true,
                    groupable: true,
                    height: height,
                    noRecords: "No assets available",
                    columnMenu: {
                        columns: false,
                        sortable: false,
                        messages: {
                            columns: "Columns",
                            filter: "Filter",
                            lock: 'Freeze',
                            unlock: 'Unfreeze',
                        }
                    }
                };
                /* END - Grid Options */

                /* DataSource */
                var dataSource = _getDataSource();
                /* END - DataSource*/
                getResp().then(function () {

                    var columns = $GetColumnsConfig(isTemplate, isGlobalTemplate, grid_name);

                    var getFrom = selectOnlyMode ? null : $GetLocalStorageName(isTemplate, isGlobalTemplate);

                    KendoGridService.GetSavedState(view, getFrom,
                        { controller: "GridView", action: "Item", domain_id: isTemplate ? (isGlobalTemplate ? 'assets_inventory_global_template' : 'assets_inventory_template') : 'assets_inventory', project_id: '(Default)', phase_id: AuthService.getLoggedDomain() },
                        height, columns, dataSource.schema)
                        .then(function (options) {

                            var saveInDB = false;

                            if (!options) {
                                options = KendoGridService.GetStructure(dataSource, columns, null, gridOptions, null, exportConfig);
                                saveInDB = true;
                            } else {
                                options.excel = exportConfig.excel;
                                options.pdf = exportConfig.pdf;
                            }

                            var isConsolidated = options.isConsolidated;
                            options.dataSource.schema.model.id = "inventory_id";
                            options.dataSource.transport.read ? options.dataSource.transport.read.url = _GetGridUrl(isTemplate, isConsolidated, consolidatedColumns, selectOnlyMode, docLinkMode, isPOPage, showApprovedAssets)
                                : options.dataSource.transport.options.read.url = _GetGridUrl(isTemplate, isConsolidated, consolidatedColumns, selectOnlyMode, docLinkMode, isPOPage, showApprovedAssets);

                            if (!options.dataSource.filter)
                                options.dataSource.filter = null;

                            /* Add functions to options */
                            options.dataBound = dataBoundFn;
                            options.pdfExport = pdfExportFn;
                            if (saveGridStateFn) {
                                options.columnHide = saveGridStateFn;
                                options.columnShow = saveGridStateFn;
                                options.columnReorder = saveGridStateFn;
                                options.columnLock = saveGridStateFn;
                                options.columnUnlock = saveGridStateFn;
                            }
                            /* END - Add functions to options */

                            grid.setOptions(options);
                            //grid.dataSource.page(1);// always start on page 1

                            if (saveInDB) {
                                options = angular.extend(grid.getOptions(), { isConsolidated: isConsolidated });
                                KendoGridService.SaveGridState(options, isTemplate ? isGlobalTemplate ? 'assets_inventory_global_template' : 'assets_inventory_template' : 'assets_inventory', '(Default)')
                                    .then(function () {
                                        _saveState(grid, isTemplate, isGlobalTemplate, isConsolidated, null, selectOnlyMode);
                                        resolve(isConsolidated);
                                    }, function () { reject(); });
                            } else {
                                _saveState(grid, isTemplate, isGlobalTemplate, isConsolidated, null, selectOnlyMode);
                                resolve(isConsolidated);
                            }
                        }, function () {
                            reject();
                        });
                });
            });
        };
        /* END - Set Saved State */

        /* DataBound */
        function _dataBound(grid, isTemplate, isGlobalTemplate, isConsolidated, selectOnlyMode) {
            if (grid) {
                KendoGridService.DataBound(grid);
                _saveState(grid, isTemplate, isGlobalTemplate, isConsolidated, null, selectOnlyMode);
            }
        };
        /* END - DataBound*/

        function _setSelecteds(grid, items) {
            if (grid) {
                grid.selecteds = items;
            }
        }

        function _getNotHiddenColumns(isTemplate, isGlobalTemplate, grid_name) {
            return $GetColumnsConfig(isTemplate, isGlobalTemplate, grid_name).filter(function (item) {
                return !item.hidden;
            });
        };

        function _deleteItems(assets) {

            if (assets == null || assets.length <= 0)
                return;

            assets = assets.map(function (item) {
                return {
                    inventory_ids: item.inventory_id,
                    domain_id: AuthService.getLoggedDomain(),
                    project_id: $stateParams.project_id,
                    phase_id: item.phase_id || -1,
                    department_id: item.department_id || -1,
                    room_id: item.room_id || -1
                };
            });

            return WebApiService.genericController.remove_with_data({
                controller: 'assetsInventory', action: 'Multiple',
                domain_id: AuthService.getLoggedDomain(),
                project_id: $stateParams.project_id
            }, assets).$promise;
        }

        function _getNotConsolidatedFields() {
            var fields = ["phase_description", "department_description", "room_number", "room_name", "room_area", "po_status", "lease_qty", "dnp_qty", "net_new", "total_budget_amt", "po_qty",
                "total_po_amt", "buyout_delta", "unit_markup_calc", "unit_escalation_calc", "unit_budget_adjusted", "unit_tax_calc", "unit_install", "unit_freight", "unit_budget_total",
                "total_budget_adjusted", "total_tax", "total_install_net", "total_install", "total_freight_net", "total_freight", "total_budget", "po_number",
                "option_codes", "option_descriptions", "room_count", "budget_qty", "inventory_id", "final_room_number", "final_room_name", "network_option", "network_type",
                "ecn", "po_status_none", "po_status_open", "po_status_issued", "po_status_requested", "po_status_qrequested", "po_status_qreceived"];

            return fields;
        }

        function _getMandatoryFields() {
            var fields = [ "inventory_id", "jsn_code", "asset_code" ];
            return fields;
        };

        /* return functions */
        return {
            SetSavedState: _SetSavedState,
            DataBound: _dataBound,
            SaveState: _saveState,
            GetGridUrl: _GetGridUrl,
            SetSelecteds: _setSelecteds,
            GetNotHiddenColumns: _getNotHiddenColumns,
            GetDataSource: _getDataSource,
            DeleteItems: _deleteItems,
            GetNotConsolidatedFields: _getNotConsolidatedFields,
            GetMandatoryFields: _getMandatoryFields
        };

    }]);