xPlanner.controller('AssetsListCtrl', ['$scope', 'AssetsService', 'GridService', 'DialogService',
    'ProgressService', 'WebApiService', 'toastr', 'AuthService', 'HttpService', '$state', 'GridViewService',
    'KendoGridService', '$filter', 'UtilsService', '$q',
    function ($scope, AssetsService, GridService, DialogService, ProgressService, WebApiService, toastr,
        AuthService, HttpService, $state, GridViewService, KendoGridService, $filter, UtilsService, $q) {

        $scope.$emit('initialTab', 'assets');
        var gridName = 'assetsListGrid';
        var jsn_labels = UtilsService.GetJSNAttributesLabel();
        var jsn_utilities = UtilsService.GetJSNUtilities();
        $scope.logged_domain = AuthService.getLoggedDomain();
        $scope.grid_content_height = window.innerHeight - 200;

        $scope.isLoggedAsAudaxware = AuthService.isAudaxwareDomainAndEmail();

        /* download files */
        window.downloadFile = function (filename, asset_domain_id, container) {
            window.open(HttpService.generic('filestream', 'file', asset_domain_id, filename, container), '_self');
        };

        /* grid configuration */
        var columns = [
            {
                headerTemplate: KendoGridService.GetSelectAllTemplate(gridName),
                template: function (dataItem) {
                    return KendoGridService.GetSelectRowTemplate(dataItem.asset_id);
                },
                width: "4em"
            }, { field: "asset_code", title: "Code", width: 110 },
            {
                field: "photo", title: 'Photo', width: 100, filterable: false,
                template: function (dataItem) {
                    return dataItem.photo ?
                        '<section align=center>' +
                        '<i onmouseover="showPhoto(this,' + dataItem.domain_id + ', \'' + dataItem.photo + '\')" onmouseout="hidePhoto(this)" class="material-icons">visibility</i>' +
                        '<section align=center class="image_popover grid-column">' +
                        '<img title="loading..." src="images/loading_picture.png" class="loading">' +
                        '</section>' +
                        '</section>' : '';
                }
            },
            { field: "model_number", title: "Model No.", width: 120 },
            { field: "model_name", title: "Model Name", width: 140 },
            { field: "asset_description", title: "Description", width: 250 },
            { field: "default_resp", title: "Resp", width: 150 },
            { field: "manufacturer_description", title: "Manufacturer", width: 150 },
            { field: "asset_category", title: "Category", width: 120, filterable: { multi: true, search: true } },
            { field: "asset_subcategory", title: "Subcategory", width: 140, filterable: { multi: true, search: true } },
            { field: "owner_name", title: "Owner", width: 110 },
            {
                headerTemplate:
                    "<div align=center class=\"comment-header\"><i class=\"material-icons no-button\" title=\"Comment\">comment</i></div>", width: 70, field: "comment", filterable: false, sortable: false,
                template: "#= comment #"
            },
            {
                field: "cut_sheet", title: "Spec", width: "90px", filterable: false,
                template: "#= cut_sheet #"
            },
            {
                field: "cad_block", title: "CAD block", width: "120px", filterable: false,
                template: "#= cad_block #"
            },
            {
                field: "revit", title: "Revit", width: "90px", filterable: false,
                template: "#= revit #"
            },
            { field: "updated_at", title: "Last Update", width: 150, template: "#: updated_at ? kendo.toString(kendo.parseDate(updated_at), \"MM/dd/yyyy\") : '' #" },
            {
                field: "discontinued", title: "Disc", width: 80, filterable: false, template: function (dataItem) {
                    if (dataItem.discontinued)
                        return KendoGridService.GetCheckIconTemplate();
                    return '';
                }, hidden: true
            },
            { field: "alternate_asset", title: "Alternate Asset", width: 180, hidden: true },
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
            { field: "connection_type", title: "Connection Type", width: "150px", hidden: true, template: "<div align=center>#: connection_type ? connection_type : '' #</div>" },
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
            { field: "bluetooth", title: "Bluetooth", width: "115px", hidden: true, template: "<div align=center>#: bluetooth ? 'Yes' : '--' #</div>" },
            { field: "cat6", title: "Cat6", width: "115px", hidden: true, template: "<div align=center>#: cat6 ? 'Yes' : '--' #</div>"  },
            { field: "displayport", title: "Displayport", width: "115px", hidden: true, template: "<div align=center>#: displayport ? 'Yes' : '--' #</div>"  },
            { field: "dvi", title: "DVI", width: "115px", hidden: true, template: "<div align=center>#: dvi ? 'Yes' : '--' #</div>"  },
            { field: "hdmi", title: "HDMI", width: "115px", hidden: true, template: "<div align=center>#: hdmi ? 'Yes' : '--' #</div>"  },
            { field: "wireless", title: "Wireless", width: "115px", hidden: true, template: "<div align=center>#: wireless ? 'Yes' : '--' #</div>"  },
            { field: "ports", title: "Ports", width: "100px", hidden: true, template: "<div align=center>#: ports ? ports : '--' #</div>" },
            { field: "electrical", title: "Electrical Description", width: "180px", hidden: true },
            { field: "electrical_option", title: "Electrical Required", width: "180px", hidden: true },
            { field: "volts", title: "Volts", width: "100px", hidden: true, template: "<div align=center>#: volts ? volts : '' #</div>" },
            { field: "phases", title: "Phases", width: "100px", hidden: true, template: "<div align=center>#: phases ? phases : '' #</div>" },
            { field: "hertz", title: "Hertz", width: "100px", hidden: true, template: "<div align=center>#: hertz ? hertz : '' #</div>" },
            { field: "amps", title: "Amps", width: "100px", hidden: true, template: "<div align=center>#: amps ? amps : '' #</div>" },
            { field: "volt_amps", title: "VoltAmps", width: "115px", hidden: true, template: "<div align=center>#: volt_amps ? volt_amps : '' #</div>" },
            { field: "plug_type", title: "Plug Type", width: "150px", hidden: true, template: "<div align=center>#: plug_type ? plug_type : '' #</div>" },
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
            { field: "mounting_height", title: "Mounting Height(in)", width: "190px", hidden: true, template: "<div align=center>#: mounting_height || '' #</div>" },
            { field: "clearance_top", title: "Top Clearance(in)", width: "170px", hidden: true, template: "<div align=center>#: clearance_top || '' #</div>" },
            { field: "clearance_bottom", title: "Bottom Clearance(in)", width: "180px", hidden: true, template: "<div align=center>#: clearance_bottom || '' #</div>" },
            { field: "clearance_right", title: "Right Clearance(in)", width: "175px", hidden: true, template: "<div align=center>#: clearance_right || '' #</div>" },
            { field: "clearance_left", title: "Left Clearance(in)", width: "175px", hidden: true, template: "<div align=center>#: clearance_left || '' #</div>" },
            { field: "clearance_front", title: "Front Clearance(in)", width: "175px", hidden: true, template: "<div align=center>#: clearance_front || '' #</div>" },
            { field: "clearance_back", title: "Back Clearance(in)", width: "180px", hidden: true, template: "<div align=center>#: clearance_back || '' #</div>" },
            { field: "weight", title: "Weight(lb)", width: "140px", hidden: true, template: "<div align=center>#: weight || '' #</div>" },
            { field: "loaded_weight", title: "Loaded Weight(lb)", width: "180px", hidden: true, template: "<div align=center>#: loaded_weight || '' #</div>" },
            { field: "ship_weight", title: "Ship Weight(lb)", width: "170px", hidden: true, template: "<div align=center>#: ship_weight || '' #</div>" },
            { field: "class", title: "Class", width: "150px", filterable: true, hidden: true },
            { field: "imported_by_project", title: "Imported From Project", width: "220px", filterable: true, hidden: true },
            { field: "jsn_code", title: jsn_labels['jsn_code'], width: "185px", hidden: true },
            { field: "jsn_nomenclature", title: "JSN Nomenclature", width: "185px", hidden: true },
            {
                field: "jsn_description", title: jsn_labels['jsn_description'], width: 150, filterable: false, sortable: false, hidden: true,
                template: "#= jsn_description #"
            },
            {
                field: "jsn_comments", title: "JSN Comments", width: 150, filterable: false, sortable: false, hidden: true,
                template: "#= jsn_comments #"
            },

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
        ];

        if ($scope.isLoggedAsAudaxware) {
            columns.splice(1, 0, {
                field: "approval_pending", title: "Approval Pending", width: 200, template: function (dataItem) {
                    if (dataItem.approval_pending)
                        return KendoGridService.GetCheckIconTemplate();
                    return '';
                }
            });
        }

        var dataSource = {
            data: [], schema: {
                model: {
                    id: "id", fields: {
                        comment: { type: "string" },
                        cut_sheet: { type: "string" },
                        cad_block: { type: "string" },
                        revit: { type: "string" },
                        photo: { type: "string" },
                        "owner_name": { type: "string" },
                        asset_description: { type: "string" },
                        updated_at: { type: "string" },
                        jsn_description: { type: "string" },
                        jsn_comments: { type: "string" }
                    }
                }
            },
            //group: [{ field: 'asset_category' }, { field: 'asset_subcategory' }]
        };

        var exportConfig = {
            excel: {
                fileName: 'Assets Catalog.xlsx',
                allPages: true,
                filterable: true
            }
        };

        var current_assets = null;
        var current_assets_export = null;
        var current_assets_search = null;

        ProgressService.blockScreen();

        function ParseAssetsExport(assets) {
            return assets.map(function (asset) {
                asset.owner_name = $filter('capitalize')(asset.owner_name);
                asset.id = asset.domain_id.toString() + asset.asset_id.toString();
                asset.asset_description = asset.asset_suffix;
                asset.comment = asset.comment ? asset.comment.replace('<div align=center><i class="material-icons" title="', '').replace('">comment</i></div>', '') : "";
                asset.photo = asset.photo ? "TRUE" : "FALSE";
                asset.cut_sheet = asset.cut_sheet ? "TRUE" : "FALSE";
                asset.cad_block = asset.cad_block ? "TRUE" : "FALSE";
                asset.revit = asset.revit ? "TRUE" : "FALSE";
                asset.discontinued = asset.discontinued ? "TRUE" : "FALSE";
                asset.jsn_description = asset.jsn_description ? asset.jsn_description.replace('<div align=center><i class="material-icons" title="', '').replace('">comment</i></div>', '') : "";
                asset.jsn_comments = asset.jsn_comments ? asset.jsn_comments.replace('<div align=center><i class="material-icons" title="', '').replace('">comment</i></div>', '') : "";
                return asset;
            });
        }

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var asset = grid.dataItem(this);
                    $scope.showDetails(asset);
                });
            }
        };

        function dataBound() {
            setDbClick($scope.assetsListGrid);
            KendoGridService.DataBound($scope.assetsListGrid, 'assetList-grid-state');
            KendoGridService.UnselectAll($scope.assetsListGrid);
        };

        $scope.assetsListGrid = $("#assetsListGrid").kendoGrid().data("kendoGrid");

        function ReloadAssets(refresh) {
            AssetsService.GetDomainAssets(refresh).then(function (assets) {
                current_assets = UtilsService.GetAssetAttributesLabel(assets, true);
                current_assets_export = ParseAssetsExport(JSON.parse(JSON.stringify(assets)));

                KendoGridService.SetSavedState($scope.assetsListGrid, current_assets, 'assetList-grid-state', { controller: 'GridView', action: 'Item', domain_id: 'assets_database', project_id: '(Default)', phase_id: AuthService.getLoggedDomain() }, $scope.grid_content_height,
                    null, null, angular.extend({}, { dataBound: dataBound },
                        KendoGridService.GetStructure(dataSource, columns, null,
                            {
                                groupable: true, reorderable: true, noRecords: "No assets available",
                                height: window.innerHeight - 130
                            }, { pageSizeDefault: 500, pageSizes: [100, 500, 1000, 5000, 10000, 20000] }, exportConfig)), null, dataBound)
                    .then(function () {

                        KendoGridService.InitiateGridEvents($scope.assetsListGrid, gridName);

                        WebApiService.genericController.query({ controller: "GridView", action: "All", domain_id: 'assets_database', project_id: AuthService.getLoggedDomain() }, function (data) {
                            $scope.views = data;
                        });

                    }, function () { toastr.error('Error to try retrieve the grid state, please contact the technical support'); });
                ProgressService.unblockScreen();
            }, function () {
                ProgressService.unblockScreen();
                toastr.error("Error to retrieve assets from server, please contact technical support");
            });
        }

        ReloadAssets();

        /* Views */
        function _isSystemView(name, showMessage, isSave) {
            if (name && name.charAt(0) === '(' && name.charAt(name.length - 1) === ')') {
                if (showMessage)
                    toastr.error("The views inside () are reserved views of the system and cannot be saved or deleted." + (isSave ? 'Please choose a name without the () to your customized view' : ''));
                return true;
            }
            return false;
        };

        $scope.loadView = function (view) {
            GridViewService.loadView($scope.assetsListGrid, view, $scope.grid_content_height, current_assets).then(function () {
                $scope.viewSelected = view;
                $scope.viewName = GridViewService.isSystemView(view.name) ? null : view.name;
            });
        }

        $scope.saveView = function (name) {
            GridViewService.saveView($scope.assetsListGrid, 'assets_database', name, $scope.views).then(function (data) {
                if (data.updatedView) {
                    $scope.views[$scope.views.indexOf(data.updatedView)].grid_state = data.newView.grid_state;
                    $scope.viewSelected = $scope.views[$scope.views.indexOf(data.updatedView)];
                } else {
                    $scope.views.push(data.newView);
                    $scope.viewSelected = data.newView;
                }
                $scope.viewName = data.newView.name;
            });
        };

        $scope.deleteView = function (view) {
            GridViewService.deleteView(view).then(function () {
                $scope.views.splice($scope.views.indexOf(view), 1);
                $scope.viewName = null;
            });
        };
        /* END - Views */

        /* Search box */
        $scope.search = function (value) {
            var items = current_assets || $scope.assetsListGrid.dataSource.data();
            KendoGridService.UnselectAll($scope.assetsListGrid);

            if (!value) {
                $scope.assetsListGrid.dataSource.data(current_assets);
                current_assets_search = null;
            } else {
                value = value.toLowerCase();
                var columns = $scope.assetsListGrid.columns;

                current_assets_search = current_assets_export.filter(function (item) {
                    for (var i = 1; i < columns.length; i++) {
                        if (columns[i].field != "comment" && columns[i].field != "photo" && columns[i].field != "cut_sheet"
                            && columns[i].field != "revit" && columns[i].field != "cad_block" && columns[i].field != "jsn_description" && columns[i].field != "jsn_comments" &&
                            !columns[i].hidden && String(item[columns[i].field]).toLowerCase().indexOf(value) > -1) {
                            return true;
                        }
                    };
                });

                $scope.assetsListGrid.dataSource.data(items.filter(function (item) {
                    for (var i = 1; i < columns.length; i++) {
                        if (columns[i].field != "comment" && columns[i].field != "photo" && columns[i].field != "cut_sheet"
                            && columns[i].field != "revit" && columns[i].field != "cad_block" && columns[i].field != "jsn_description" && columns[i].field != "jsn_comments" &&
                            !columns[i].hidden && String(item[columns[i].field]).toLowerCase().indexOf(value) > -1) {
                            return true;
                        }
                    };
                }));
            }
        };
        /* END - Search box */

        /* BEGIN - Clear all filter*/
        $scope.clearAllFilters = function (name) {
            $scope.searchBoxValue = null;
            KendoGridService.ClearFilters($scope.assetsListGrid);
            $scope.search();
        };
        /* END - Clear all filters */

        $scope.showDetails = function (asset) {

            if (!asset) {
                if (!KendoGridService.VerifySelected('view details', 'asset', $scope.assetsListGrid, true)) return;
                asset = KendoGridService.GetSelecteds($scope.assetsListGrid)[0];
            }

            ProgressService.blockScreen();
            $state.go('assetsWorkspace.assetsDetails', { domain_id: asset.domain_id, asset_id: asset.asset_id });
        };

        /* BEGIN - Choose columns to display */
        $scope.chooseDisplayedColumns = function () {

            ProgressService.blockScreen();
            var columns = $scope.assetsListGrid.columns;
            DialogService.openModal('app/Utils/Modals/DisplayColumns.html', 'DisplayColumnsCtrl', { columns: columns, isCatalog: true })
                .then(function (gridColumns) {

                    gridColumns.display.forEach(function (item, index) {

                        if (item.hidden != columns[index].hidden) {
                            if (item.hidden) {
                                $scope.assetsListGrid.hideColumn(index);
                            } else {
                                $scope.assetsListGrid.showColumn(index);
                            }
                        }
                    });
                    ProgressService.unblockScreen();
                });
        }
        /* END- Choose columns to display */

        /* END - grid configuration */

        // Add a new asset
        $scope.openAddModal = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            DialogService.openModal('app/Assets/Modals/AddAsset.html', 'AddAssetCtrl', null, true);
        };

        // delete assets
        $scope.delete = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            if (KendoGridService.VerifySelected('delete', 'asset', $scope.assetsListGrid)) {
                DialogService.Confirm('Are you sure?', 'The asset(s) will be deleted permanently!').then(function () {
                    ProgressService.blockScreen();
                    AssetsService.DeleteAssetsFromGrid($scope.assetsListGrid, GridService.getSelecteds($scope.assetsListGrid)).then(function (data) {
                        ProgressService.unblockScreen();
                        toastr.success('Asset(s) Deleted');
                        KendoGridService.UnselectAll($scope.assetsListGrid);
                    }, function (error) {
                        ProgressService.unblockScreen();
                        if (error.status == 409) toastr.info('Some assets are assigned to inventory and cannot be deleted');
                        KendoGridService.UnselectAll($scope.assetsListGrid);
                    });
                });
            }
        };



        /* upload files */
        function _filesUpload() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            DialogService.openModal('app/Assets/Modals/UploadAssets.html', 'UploadAssetsCtrl', null).then(function () {
                ReloadAssets(true);
                dataBound();
            }, function (error) {
            });

        };

        /* Create all cutsheet */
        function _createAllCutsheets(domain_id) {
            DialogService.Confirm('CutSheets Generation', 'Do you want to regenerate the cutsheets again?')
                .then(function () {
                    ProgressService.blockScreen();
                    WebApiService.genericController.update({ controller: 'CutSheet', action: "All", domain_id: domain_id },
                        null, function () {
                            toastr.success("The cutsheets were marked for regeneration");
                            ProgressService.unblockScreen();
                        }, function () {
                            toastr.error('Error trying to mark cutsheets for regeneration');
                            ProgressService.unblockScreen();
                        });
                });
        }


        /* END - upload files */



        function _exportGrid(to) {
            ProgressService.blockScreen();

            var assets = JSON.parse(JSON.stringify($scope.assetsListGrid.dataSource.data()));
            var assets_to_export;
            if (current_assets_search == null)
                assets_to_export = JSON.parse(JSON.stringify(current_assets_export));
            else
                assets_to_export = JSON.parse(JSON.stringify(current_assets_search));


            if ($scope.logged_domain != 1) {
                assets_to_export = assets_to_export.filter(function (item) {
                    return item.domain_id == $scope.logged_domain;
                });
            }

            $scope.assetsListGrid.dataSource.data(assets_to_export);

            GridService.exportGrid(to, $scope.assetsListGrid);

            $scope.assetsListGrid.dataSource.data(assets);

            ProgressService.unblockScreen();
        };

        $scope.collapseExpand = KendoGridService.collapseExpand;

        $scope.closeToolbars = function () { $scope.showViewsOptions = false; };

        /*Fab button*/
        $scope.buttons = [{
            label: 'Export to Excel (Will only export assets from your own enterprise)',
            icon: 'file_simple',
            click: { func: _exportGrid, params: 'excel' },
            only_audaxware: false
        },
        {
            label: 'Import from Excel (Will only import assets to your own enterprise)',
            icon: 'file_upload',
            click: { func: _filesUpload },
            only_audaxware: false
        },
        {
            label: 'Regenerate all cutsheets',
            icon: 'regenerate',
            click: { func: _createAllCutsheets, params: AuthService.getLoggedDomain() }
        }];

        window.assetImgLoaded = function (elem) {
            $(elem).parent().children('img.loading').remove();
            if ($(elem).attr('toShow') == 'true')
                $(elem).show();
        }

        window.showPhoto = function (elem, domain_id, photo) {
            if ($(elem).closest('section').children('.image_popover').children('img.loading').length > 0) {
                if ($(elem).closest('section').children('.image_popover').children('img.loaded').length <= 0) {
                    $(elem).closest('section').children('.image_popover').append('<img class="loaded" toShow="true" title="No picture" onload="assetImgLoaded(this)">');
                    $(elem).closest('section').children('.image_popover').children('img.loaded').attr('src', HttpService.generic('filestream', 'file', domain_id, photo, 'photo'));
                }
                $(elem).closest('section').children('.image_popover').children('img.loading').show();
            } else if ($(elem).closest('section').children('.image_popover').children('img.loading').length <= 0) {
                $(elem).closest('section').children('.image_popover').children('img.loaded').show();
            }
            $(elem).closest('section').children('.image_popover').children('img').attr('toShow', 'true');
        };

        window.hidePhoto = function (elem) {
            $(elem).closest('section').children('.image_popover').children('img').hide();
            $(elem).closest('section').children('.image_popover').children('img').attr('toShow', 'false');
        };

    }]);