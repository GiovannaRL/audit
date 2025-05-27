xPlanner.controller('EditAssetModalCtrl', ['$scope', 'StatusListeditMulti', '$mdMedia', '$mdDialog',
    'AuthService', 'local', 'toastr', 'ProgressService', 'WebApiService', 'GridService',
    'HttpService', '$q', 'CostFieldListWithNone', 'PlacementList', 'UtilsService', 'FileService',
    'OptionTypes', 'AssetOptionScope', 'DialogService', 'DocumentTypes', 'FormService', 'AssetClassList', 'KendoGridService',
    function ($scope, StatusListeditMulti, $mdMedia, $mdDialog, AuthService, local, toastr,
        ProgressService, WebApiService, GridService, HttpService, $q, CostFieldListWithNone, PlacementList,
        UtilsService, FileService, OptionTypes, AssetOptionScope, DialogService, DocumentTypes, FormService, AssetClassList, KendoGridService) {

        $scope.costField = CostFieldListWithNone;
        $scope.selected_costField = 'default';
        $scope.image = downloadFile(local.assets[0].photo, local.assets[0].photo_domain_id);
        $scope.style_height = window.innerHeight - 120;
        $scope.grid_height = window.innerHeight - 300;
        $scope.statusList = StatusListeditMulti;
        $scope.largeMedia = $mdMedia('gt-sm');
        $scope.data = {};
        $scope.showDocumentsTab = false;
        $scope.placementList = PlacementList;
        $scope.selectedTemporaryLocation;
        $scope.selectedFinalDisposition;
        $scope.classes = AssetClassList;
        $scope.showZoomPicture = false;

        $scope.newOptionCtrl = {};
        $scope.imgData = [];

        var default_inventory_id;
        var noFirstTimeDetails;
        var usingProfile = false;
        var optionsHaveBeenModified = false;
        var hasOtherAssets = false;
        var usedProfileDetailedBudget;
        var oldProfile = {
            profile_text: local.assets[0].profile_text,
            profile_budget: local.assets[0].profile_budget,
            detailed_budget: local.assets[0].detailed_budget,
        };


        // Disable Attributes Tab
        var showAttributesTab = true;
        if (local.multiple && local.assets[0].consolidated_view == 0) {
            var showAttributesTab = false

        };

        if (local.assets.some(function (a) { return hasDifferentQuantityOrBudget(local.assets[0], a); })) {
            toastr.warning("The selected assets have different quantities / budget, if you change them using edit multiple, the values changed will apply to all assets, making them the same");
        }

        function hasDifferentQuantityOrBudget(a, b) {
            return a.budget_qty !== b.budget_qty || a.lease_qty !== b.lease_qty || a.dnp_qty !== b.dnp_qty || a.unit_budget !== b.unit_budget
        }

        function addEmptyDocument(mustEmpty, idx) {

            if (mustEmpty) { $scope.imgData = []; }

            var emptyDocument = { type: '', label: '', file: {}, idx: idx };

            if (!idx) {
                emptyDocument.idx = $scope.imgData.length;
                $scope.imgData.push(emptyDocument);
            } else if ($scope.imgData.length > idx) {
                $scope.imgData[idx] = emptyDocument;
            }
        };

        addEmptyDocument();

        if (local.multiple) {
            var one_type = true;
            var inventory_ids = [];
            var id = local.assets[0].asset_id;
            var default_profile = local.assets[0].asset_profile;

            if (local.assets[0].consolidated_view == 1) { //consolidated
                if (local.assets.length == 1) {
                    $scope.data = angular.copy(local.assets[0]);
                    $scope.data.budget_qty = null; //need to be zero when consolidated
                    $scope.data.unit_budget = null;
                    $scope.data.dnp_qty = null;
                    $scope.data.lease_qty = null;
                }


                angular.forEach(local.assets, function (asset) {
                    inventory_ids = asset.inventory_ids.split(',');
                    if (asset.asset_id !== id) one_type = false;
                    if (asset.asset_profile != default_profile) $scope.multipleProfiles = true;
                });
            } else { // not consolidated
                inventory_ids = local.assets.map(function (asset) {
                    if (asset.asset_id !== id) one_type = false;
                    if (asset.asset_profile != default_profile) $scope.multipleProfiles = true;

                    return asset.inventory_id;
                });
            }

            $scope.oneType = one_type;

            default_inventory_id = inventory_ids[0];

            if (one_type) {
                $scope.data.asset_code = local.assets[0].asset_code;
                $scope.data.asset_id = local.assets[0].asset_id;
                $scope.data.asset_domain_id = local.assets[0].asset_domain_id;
            }
        } else {


            $scope.showDocumentsTab = true;
            $scope.data = angular.copy(local.assets[0]);

            //THIS IS NECESSARY BECAUSE THIS COLUMNS ARE STRING IN ASSET_INVENTORY AND BOOL IN PROJECT_ROOM_INVENTORY SO WE NEED THIS CONVERSION.
            //IF ERASE THIS, THE EDIT SINGLE ATTRIBUTES WILL STOP WORKING
            $scope.data.bluetooth = $scope.data.bluetooth == '1' || $scope.data.bluetooth == 'True' ? true : false;
            $scope.data.cat6 = $scope.data.cat6 == '1' || $scope.data.cat6 == 'True' ? true : false;
            $scope.data.displayport = $scope.data.displayport == '1' || $scope.data.displayport == 'True' ? true : false;
            $scope.data.dvi = $scope.data.dvi == '1' || $scope.data.dvi == 'True' ? true : false;
            $scope.data.hdmi = $scope.data.hdmi == '1' || $scope.data.hdmi == 'True' ? true : false;
            $scope.data.wireless = $scope.data.wireless == '1' || $scope.data.wireless == 'True' ? true : false;
            $scope.data.network_option = $scope.data.network_option == 1 || $scope.data.network_option == 'Yes' ? 1 : $scope.data.network_option == '--' || $scope.data.network_option == 0 ? 0 : 2;
            //END

            if ($scope.data.temporary_location) {
                $scope.selectedTemporaryLocation = { name: $scope.data.temporary_location }
            }
            if ($scope.data.final_disposition) {
                $scope.selectedFinalDisposition = { name: $scope.data.final_disposition }
            }
            default_inventory_id = $scope.data.inventory_id;
            if (local.assets[0].consolidated_view == 1)  //consolidated
                $scope.data.unit_budget = $scope.data.unit_budget || $scope.data.total_unit_budget;

            if ($scope.data.room_count > 1) {
                $scope.data.budget_qty = ($scope.data.budget_qty / $scope.data.room_count);
                $scope.data.dnp_qty = ($scope.data.dnp_qty / $scope.data.room_count);
                $scope.data.lease_qty = ($scope.data.lease_qty / $scope.data.room_count);
            }
        }

        /* Set class */
        $scope.data.class = null;
        if (!local.multiple && local.assets[0].class) {
            var classItem = $scope.classes.find(function (item) {
                return item.name == local.assets[0].class;
            });

            if (classItem) {
                local.assets[0].class = $scope.data.class = classItem.id;
            }
        }
        /* END - Set class */

        $scope.assetInfo = { asset_domain_id: local.assets[0].asset_domain_id, asset_id: local.assets[0].asset_id };

        toJsnAsset();

        if (local.profile) {
            WebApiService.genericController.get({ controller: 'assetsInventory', action: 'profile', domain_id: AuthService.getLoggedDomain(), project_id: local.params.project_id, phase_id: default_inventory_id },
                function (profile) {
                    hasOtherAssets = local.multiple && profile.qty_assets_project > inventory_ids || !local.multiple && profile.qty_assets_project > 1;
                });
        }

        if (!local.multiple || one_type) {
            WebApiService.genericController.query({
                controller: 'profile', action: 'AllNotProject',
                domain_id: AuthService.getLoggedDomain(), project_id: local.params.project_id,
                phase_id: local.assets[0].asset_domain_id, department_id: local.assets[0].asset_id
            }, function (data) {
                data.sort();
                $scope.globalProfiles = data.map(function (item) {
                    item.domain_id = item.asset_domain_id;
                    return item;
                });
                $scope.globalProfiles.unshift({ profile_text: 'None' });
                WebApiService.genericController.query({
                    controller: 'profile', action: 'AllProject',
                    domain_id: AuthService.getLoggedDomain(), project_id: local.params.project_id, phase_id: local.assets[0].asset_domain_id,
                    department_id: local.assets[0].asset_id
                }, function (data) {
                    $scope.profiles = data;
                }, function () {
                    toastr.error('Error to retrieve data from server. Please contact the technical support.');
                });
            }, function () {
                toastr.error('Error to retrieve data from server. Please contact the technical support.');
            });
        }

        if (!local.multiple || !$scope.multipleProfiles) {

            WebApiService.genericController.query({
                controller: 'inventoryOptions', action: 'All', domain_id: AuthService.getLoggedDomain(),
                project_id: default_inventory_id
            },
                function (data) {
                    if (local.profile && data.length > 0 && data.find(function (i) { return i.unit_price > 0 })) {
                        $scope.data.detailed_budget = true;
                    }
                    $scope.optionsAdded = data;
                    $scope.optionsGrid.dataSource.data(data);
                });
        }

        if ($scope.data.biomed_check_required === 'Yes') {
            $scope.data.biomed_check_required = true;
        }

        if ($scope.data.estimated_delivery_date)
            $scope.data.estimated_delivery_date = new Date($scope.data.estimated_delivery_date);

        if ($scope.data.delivered_date)
            $scope.data.delivered_date = new Date($scope.data.delivered_date);

        if ($scope.data.received_date)
            $scope.data.received_date = new Date($scope.data.received_date);

        $scope.data.tags = $scope.data.tag ? $scope.data.tag.split(",") : [];

        $scope.controllerParams = {
            domain_id: AuthService.getLoggedDomain()
        }

        $scope.projectControllerParams = {
            domain_id: AuthService.getLoggedDomain(),
            project_id: local.params.project_id,
        }

        $scope.ccControllerParams = {
            domain_id: AuthService.getLoggedDomain(),
            project_id: local.params.project_id
        };

        
        WebApiService.genericController.get({
            controller: 'projects', action: 'CostField', domain_id: AuthService.getLoggedDomain(),
            project_id: local.params.project_id
        }, function (data) {
            $scope.default_cost_field = data.cost_field;
        }, function () {
            toastr.error('Error to retrieve data from server, please contact the technical support.');
        });

        function downloadFile(filename, asset_domain_id) {
            return HttpService.generic('filestream', 'file', asset_domain_id, filename, 'photo');
        };

        function GetParams(action) {

            var params = angular.copy(local.params);
            params.domain_id = AuthService.getLoggedDomain();
            params.project_id = local.params.project_id;
            params.action = action ? action : (local.multiple ? 'All' : 'EditSingle');

            return params;
        };

        function GetUpdateOptionsParams() {

            return {
                domain_id: AuthService.getLoggedDomain(),
                project_id: local.params.project_id,
                action: 'Options'
            };
        };

        function SetUnitPrice(options) {

            if (options && !$scope.data.detailed_budget)
                return options.map(function (op) { op.unit_price = 0; return op; });

            return options;
        }

        function GetBody() {
            if ($scope.data.tags) {
                $scope.data.tag = $scope.data.tags.join();
            }

            if ($scope.data.empty_cad_id) {
                $scope.data.cad_id = -1;
            }

            //if ($scope.data.class) {
            //    $scope.data.class = $scope.classes.find(function (item) {
            //        if (item.name == $scope.data.class) {
            //            return item;
            //        }
            //    }).id;
            //}

            if ($scope.data.resp && typeof $scope.data.resp === 'object') {
                $scope.data.resp = $scope.data.resp.name;
            }

            if (typeof $scope.data.unit_budget !== "number" && $scope.data.unit_budget != undefined) {
                $scope.data.unit_budget = $scope.data.unit_budget.toString().replace('$', '');
            }

            $scope.data.temporary_location = $scope.selectedTemporaryLocation && $scope.selectedTemporaryLocation.name;

            $scope.data.final_disposition = $scope.selectedFinalDisposition && $scope.selectedFinalDisposition.name;

            if (local.multiple) {
                if ($scope.data.unit_budget < 0) {
                    $scope.data.unit_budget = null;
                }
            }

            return local.multiple ? {
                edited_data: $scope.data, inventories: inventory_ids
            } : $scope.data;
        };

        function GetUpdateOptionsBody() {

            $scope.optionsAdded = SetUnitPrice($scope.optionsAdded);

            if (!local.multiple) {
                return {
                    inventories_id: [$scope.data.inventory_id],
                    options: $scope.optionsAdded
                };
            }

            return {
                inventories_id: inventory_ids,
                options: !$scope.multipleProfiles ? $scope.optionsAdded : null
            };
        }

        function GetCalculateBudgetBody() {
            return {
                type_resp: $scope.data.type_resp,
                unit_markup_calc: $scope.data.unit_markup_calc,
                unit_escalation_calc: $scope.data.unit_escalation_calc,
                unit_budget_adjusted: $scope.data.unit_budget_adjusted,
                unit_tax_calc: $scope.data.unit_tax_calc,
                unit_install: $scope.data.unit_install,
                unit_freight: $scope.data.unit_freight,
                unit_budget_total: $scope.data.unit_budget_total,
                total_install_net: $scope.data.total_install_net,
                total_budget_adjusted: $scope.data.total_budget_adjusted,
                total_tax: $scope.data.total_tax,
                total_install: $scope.data.total_install,
                total_freight_net: $scope.data.total_freight_net,
                total_freight: $scope.data.total_freight,
                total_budget: $scope.data.total_budget,
                unit_markup: $scope.data.unit_markup,
                total_unit_budget: $scope.data.total_unit_budget,
                unit_escalation: $scope.data.unit_escalation,
                unit_tax: $scope.data.unit_tax,
                unit_install_net: $scope.data.unit_install_net,
                unit_install_markup: $scope.data.unit_install_markup,
                unit_freight_net: $scope.data.unit_freight_net,
                unit_freight_markup: $scope.data.unit_freight_markup,
                net_new: $scope.data.net_new
            }

        }
        /* BEGIN - Grid configurations */
        var dataSource = {
            data: $scope.multipleProfiles ? [] : $scope.optionsAdded || [],
            schema: {
                model: {
                    fields: {
                        data_type: { editable: false },
                        code: { editable: false },
                        description: { editable: false },
                        unit_price: { type: 'number', validation: { min: 0 } },
                        unit_budget: { type: 'number', validation: { min: 0 } },
                        quantity: { type: 'number', validation: { min: 1 }, defaultValue: 1 },
                        assets_options: { type: 'object', editable: false },
                        'assets_options.scope': { editable: false },
                        'assets_options.settings': { editable: false }
                    }
                }
            },
            change: function (e) {
                if (!$scope.addOp && e.action && e.action === "itemchange") {
                    $scope.optionsAdded = $scope.optionsGrid.dataSource.data();
                    optionsHaveBeenModified = true;
                }
            }
        };

        function toJsnAsset() {
            if (!$scope.data) {
                return;
            }
            $scope.jsn_asset = { jsn: {} };
            $scope.jsn_asset.jsn.jsn_code = $scope.data.jsn_code;
            $scope.jsn_asset.jsn_suffix = '';
            var jsn_code = $scope.data.jsn_code;

            if (jsn_code && typeof (jsn_code) == 'string') {
                var startSuffix = jsn_code.indexOf('.');
                if (startSuffix >= 0) {
                    $scope.jsn_asset.jsn.jsn_code = jsn_code.substring(0, startSuffix);
                    $scope.jsn_asset.jsn_suffix = jsn_code.substring(startSuffix + 1);
                }
            }
            $scope.jsn_asset.jsn.utility1 = $scope.data.jsn_utility1;
            $scope.jsn_asset.jsn.utility2 = $scope.data.jsn_utility2;
            $scope.jsn_asset.jsn.utility3 = $scope.data.jsn_utility3;
            $scope.jsn_asset.jsn.utility4 = $scope.data.jsn_utility4;
            $scope.jsn_asset.jsn.utility5 = $scope.data.jsn_utility5;
            $scope.jsn_asset.jsn.utility6 = $scope.data.jsn_utility6;

            $scope.jsn_asset.jsn.jsn_ow = $scope.data.jsn_ow;
        }

        function fromJsnAsset() {
            if (!$scope.data) {
                return;
            }
            var jsnAsset = $scope.jsn_asset;
            $scope.data.jsn_code = jsnAsset.jsn.jsn_code;
            jsnAsset.jsn_suffix = jsnAsset.jsn_suffix.trim();
            if (jsnAsset.jsn_suffix.length > 0) {
                $scope.data.jsn_code += '.' + jsnAsset.jsn_suffix;
            }

            $scope.data.jsn_utility1 = jsnAsset.jsn.utility1;
            $scope.data.jsn_utility2 = jsnAsset.jsn.utility2;
            $scope.data.jsn_utility3 = jsnAsset.jsn.utility3;
            $scope.data.jsn_utility4 = jsnAsset.jsn.utility4;
            $scope.data.jsn_utility5 = jsnAsset.jsn.utility5;
            $scope.data.jsn_utility6 = jsnAsset.jsn.utility6;

            $scope.data.jsn_ow = jsnAsset.jsn.jsn_ow;
        }

        var columns = [{ headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(optionsGrid)\" ng-checked=\"allPagesSelected(optionsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, optionsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, optionsGrid)\" ng-checked=\"isSelected(optionsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "2.8em" },
        { field: "assets_options.scope", title: "Scope", template: "#: assets_options.scope == 2 ? 'Custom' : 'Catalog' #" },
        { field: "data_type", title: "Type", template: "{{getOptionType(dataItem.assets_options ? dataItem.assets_options.data_type : dataItem.data_type)}}" },
        { field: "code", title: "Code", template: "#: assets_options ? (assets_options.code || '') : (code || '') #" },
        { field: "description", title: "Description", template: "#: assets_options ? assets_options.description : description  #" },
        { field: "unit_price", title: "Unit Price", attributes: { "class": "editable-cell" }, template: "<div align=right>{{ dataItem.unit_price || dataItem[selected_costField === 'default' ? default_cost_field : selected_costField] || dataItem.unit_budget || 0 | currency}}</div>", hidden: !$scope.data.detailed_budget },
        { field: "quantity", title: "Quantity per Asset", type: "number", attributes: { "class": "editable-cell" }, template: local.multiple ? "{{addOp ? #: quantity || 1 # : #: quantity #}}" : "#: quantity || 1 #" },
        { field: "assets_options.settings", title: "Settings", attributes: { "class": "no-multilines" } }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, null, { noRecords: "No options", groupable: true, editable: true, height: $scope.grid_height });

        $scope.toggleEditOption = function () {
            if (!GridService.verifySelected('edit', 'option', $scope.optionsGrid, true)) return;
            var option = GridService.getSelecteds($scope.optionsGrid)[0];
            editOption(option);
        }

        function editOption(option) {
            if (AssetOptionScope[option.assets_options.scope] == 'Catalog') {
                toastr.info('You can only edit custom options');
                return;
            }

            $scope.optionToEdit = angular.extend({}, option.assets_options,
                {
                    quantity: option.quantity, unit_price: option.unit_price,
                    domain_document: option.domain_document
                });
            $scope.toggleAddOption();
        }

        /* Grid selecteds*/
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Grid selecteds */

        $scope.dataBound = function () {
            GridService.dataBound($scope.optionsGrid);
        };
        /* END - Grid configurations */

        function GetParamsChangeProfile(selectedProfile) {

            if (selectedProfile.inventory_id) {
                return {
                    controller: 'InventoryOptions',
                    action: 'All',
                    domain_id: AuthService.getLoggedDomain(),
                    project_id: selectedProfile.inventory_id
                };
            }

            // edit profile
            return {
                controller: 'Profile',
                action: 'options',
                domain_id: AuthService.getLoggedDomain(),
                project_id: local.params.project_id,
                phase_id: selectedProfile.profile_id
            };
        }

        function cleanDeveredAndReceivedData() {
            var current_location = $scope.data.current_location;
            if (current_location == 'Delivered') {
                $scope.data.received_date = null;
            }
            else if (current_location != 'Received' && current_location != 'Completed') {
                $scope.data.delivered_date = null;
                $scope.data.received_date = null;
            }
        };

        // Function that reloads the grid that display the preview of the selected options profile
        $scope.changeProfile = function () {

            if ($scope.selectedProfile.profile_text != 'None') {
                WebApiService.genericController.query(GetParamsChangeProfile($scope.selectedProfile), function (data) {
                    $scope.optionsGrid.dataSource.data(data);
                    $scope.optionsAdded = data;
                    usedProfileDetailedBudget = $scope.selectedProfile.detailed_budget;
                    $scope.toggleDetailedBudget(usedProfileDetailedBudget);
                });
                usingProfile = true;
                hasOtherAssets = local.profile ? hasOtherAssets : $scope.selectedProfile.qty_assets_project > 1 ? !local.multiple || $scope.selectedProfile.qty_assets_project > inventory_ids
                    : $scope.selectedProfile.qty_assets_project === 1 && ($scope.selectedProfile.detailed_budget != oldProfile.detailed_budget
                        || $scope.selectedProfile.profile_text != oldProfile.profile_text || $scope.selectedProfile.profile_budget != oldProfile.profile_budget);
                optionsHaveBeenModified = local.profile && oldProfile.profile_text && oldProfile.profile_text != '';
            } else {
                $scope.optionsGrid.dataSource.data([]);
                $scope.optionsAdded = [];
                usingProfile = false;
                hasOtherAssets = local.profile && hasOtherAssets;
                optionsHaveBeenModified = local.profile && oldProfile.profile_text && oldProfile.profile_text != '';
            }
            $scope.multipleProfiles = false;

        };

        $scope.changeQty = function () {
            $scope.data.net_new = ($scope.data.budget_qty || 0) - ($scope.data.dnp_qty || 0);
            $scope.calculateBudgets();
        }

        $scope.canChangeBudgetQty = function () {
            if (local.multiple) {
                return !local.assets.some(function (val) { return !!val.source_location; });
            } else {
                return !$scope.data.source_location;
            }
        }

        $scope.calculateBudgets = function () {
            WebApiService.asset_inventory.update({ action: 'InventoryBudget', domain_id: AuthService.getLoggedDomain(), project_id: local.params.project_id }, GetCalculateBudgetBody(), function (data) {
                updateBudgetValues(data);
            }, function () {
                toastr.error('Error to update budgetary values, please contact technical support');
            });

        }

        function updateBudgetValues(data) {
            $scope.data.total_unit_budget = data.total_unit_budget;
            $scope.data.unit_budget_adjusted = data.unit_budget_adjusted;
            $scope.data.unit_budget_total = data.unit_budget_total;
            $scope.data.unit_markup_calc = data.unit_markup_calc;
            $scope.data.unit_freight = data.unit_freight;
            $scope.data.unit_escalation_calc = data.unit_escalation_calc;
            $scope.data.unit_install = data.unit_install;
            $scope.data.unit_tax_calc = data.unit_tax_calc;
            $scope.data.total_budget_adjusted = data.total_budget_adjusted;
            $scope.data.total_tax = data.total_tax;
            $scope.data.total_install_net = data.total_install_net;
            $scope.data.total_install = data.total_install;
            $scope.data.total_freight_net = data.total_freight_net;
            $scope.data.total_freight = data.total_freight;
            $scope.data.total_budget = data.total_budget;
        }

        $scope.getOptionType = function (data_type) {

            var type = OptionTypes.find(function (item) {
                return item.id == data_type;
            });

            return type ? type.name : '';
        }

        function _buildOption(item) {

            return angular.extend({}, $scope.assetInfo, {
                option_id: item.asset_option_id,
                domain_id: item.domain_id,
                unit_price: item.unit_price || 0,
                quantity: item.quantity || 1,
                assets_options: item,
                code: item.code,
                data_type: item.data_type,
                description: item.description,
                picture: item.picture,
                min_cost: item.min_cost,
                max_cost: item.max_cost,
                scope: item.scope,
                settings: item.settings
            });
        }

        function _removeFromOptionsAdded(option) {
            var item = $scope.optionsAdded.find(function (op) {
                return op.assets_options.asset_option_id == option.asset_option_id;
            });

            if (item) {
                $scope.optionsAdded.splice($scope.optionsAdded.indexOf(item), 1);
            }
        }

        $scope.addNewOptionToGrid = function () {
            if ($scope.newOptionCtrl.validateOption()) {
                $scope.newOptionCtrl.getOption().then(function (item) {
                    if ($scope.optionToEdit) {
                        _removeFromOptionsAdded(item);
                    }

                    $scope.optionsAdded.push(_buildOption(item));

                    optionsHaveBeenModified = true;
                    $scope.toggleAddOption();
                    $scope.optionToEdit = null;
                });
            }
        }

        $scope.toggleAddOption = function () {

            if ($scope.addOp) {
                $scope.optionsGrid.dataSource.data($scope.optionsAdded);
                $scope.optionToEdit = null;
            }

            $scope.addOp = !$scope.addOp;
        };

        $scope.deleteOptions = function () {

            if (!GridService.anySelected($scope.optionsGrid)) {
                toastr.error('You need to select at least one option to delete');
                return;
            }

            var items = GridService.getSelecteds($scope.optionsGrid);
            items.forEach(function (item) {
                $scope.optionsGrid.dataSource.remove(item);
            });

            $scope.optionsAdded = $scope.optionsGrid.dataSource.data();
            GridService.unselectAll($scope.optionsGrid);
            optionsHaveBeenModified = true;
        };

        $scope.confirmSynchronization = function (saveAndSync, item) {
            return $q(function (resolve, reject) {
                if (!saveAndSync) {
                    resolve(false);
                }
                else {
                    //GET TOTAL CHANGED ASSETS
                    var totalAssets = 0;
                    WebApiService.genericController.get({
                        controller: 'assetsInventory', action: 'GetSynchronizedCount',
                        domain_id: item.domain_id, project_id: item.project_id, phase_id: item.inventory_id, department_id: item.jsn_code, room_id: item.resp.trim()
                    }, function (total) {
                        totalAssets = total;
                        ProgressService.unblockScreen();
                        var message = 'Are you sure you want all the assets with JSN ' + item.jsn_code + '  and Responsibility ' + item.resp + ' to be updated with this asset information? A total of ' + totalAssets.text + " item(s) are going to be updated.";
                        DialogService.Confirm('Asset update', message, null, null, true).then(function () {
                            resolve(true);
                        }, function () { resolve(false); });
                    }, function () {
                        ProgressService.unblockScreen();
                        toastr.error('Error when trying to get total synchronized assets. Assets were not updated.');
                    });
                }

            });
        };

        function isDataValid() {
            cleanDeveredAndReceivedData();
            var deliveredDate = $scope.data.delivered_date;
            var receivedDate = $scope.data.received_date;

            if (deliveredDate != null && receivedDate != null) {
                if (deliveredDate >= receivedDate) {
                    toastr.error('Received Date must be higher than Delivered Date');
                    return false;
                }
            }
            else if (local.multiple && (deliveredDate != null || receivedDate != null)) {
                for (var i = 0; i < local.assets.length; i++) {
                    var oldDelivered = new Date(local.assets[i].delivered_date);
                    var oldReceived = new Date(local.assets[i].received_date);
                    var assetCode = local.assets[i].asset_code;
                    var error = false;

                    if (!isNaN(oldDelivered) && oldDelivered >= receivedDate) {
                        error = true;
                    }
                    else if (!isNaN(oldReceived) && deliveredDate >= oldReceived) {
                        error = true;
                    }

                    if (error) {
                        toastr.error('Received Date must be higher than Delivered Date for asset: ' + assetCode);
                        return false;
                    }

                }
            }

            if (!local.multiple && $scope.data.po_qty > 0 && ($scope.data.budget_qty - $scope.data.dnp_qty) != $scope.data.po_qty) {
                const poNumber = $scope.data.po_number > 0 ? '(#' + $scope.data.po_number + ')' : '';
                toastr.error('This asset has already been added to a PO' + poNumber + ' and you cannot change the planned quantity. To edit the planned quantity you must remove the asset from the PO');
                return false;
            }

            if (local.multiple && ($scope.data.budget_qty >= 0 || $scope.data.dnp_qty >= 0)) {
                // Validate quantity values
                var isQuantityValid = local.assets.every(function (val) {
                    var budgetQty = $scope.data.budget_qty != null ? $scope.data.budget_qty : val.budget_qty;
                    var dnpQty = $scope.data.dnp_qty != null ? $scope.data.dnp_qty : val.dnp_qty;

                    return budgetQty - dnpQty >= val.po_qty;
                });
                if (!isQuantityValid) {
                    toastr.error('The quantity values provided are not valid. DNP Qty cannot be greater than planned qty - po qty');
                    return false;
                }

                if ($scope.data.budget_qty >= 0) {
                    const isPOPlannedQtyInvalid = local.assets.every(function (val) {
                        var budgetQty = $scope.data.budget_qty != null ? $scope.data.budget_qty : val.budget_qty;
                        var dnpQty = $scope.data.dnp_qty != null ? $scope.data.dnp_qty : val.dnp_qty;

                        return val.po_qty >= 0 && (budgetQty - dnpQty) != val.po_qty;
                    });
                    if (isPOPlannedQtyInvalid) {
                        toastr.error('One of this assets has already been added to a PO and you cannot change the planned quantity. To edit the planned quantity you must remove the asset from the PO');
                        return false;
                    }
                }
                

            }
            return true;
        }

        $scope.save = function (saveAndSync) {
            if (!isDataValid()) return;
            fromJsnAsset();
            if (!local.profile) $scope.editAssetForm.$setSubmitted();

            if (local.profile || $scope.editAssetForm.$valid) {
                ProgressService.blockScreen();
                var getAllData = GetBody();

                $scope.confirmSynchronization(saveAndSync, getAllData).then(function (shouldSync) {
                    ProgressService.blockScreen();
                    WebApiService.asset_inventory.update(GetParams(), GetBody(), function (data) {
                        WebApiService.asset_inventory.update(GetUpdateOptionsParams(), GetUpdateOptionsBody(), function () {
                            if (shouldSync) {
                                WebApiService.asset_inventory.update(GetParams('Synchronize'), GetBody(), function () {
                                    toastr.success('Asset updated and synchronized. The grid will be reloaded.');
                                    ProgressService.unblockScreen();
                                    $scope.close(true);
                                }, function () {
                                    toastr.info('Asset updated but not synchronized. The grid will be reloaded.');
                                    ProgressService.unblockScreen();
                                    $scope.close(true);
                                });
                            }
                            else {
                                toastr.success('Asset(s) updated. The grid will be reloaded.');
                                ProgressService.unblockScreen();
                                $scope.close(true);
                            }

                        }, function () {
                            ProgressService.unblockScreen();
                            $scope.close(true);
                            toastr.info('It was not possible to update asset\'s options');
                        });
                    }, function (error) {
                            ProgressService.unblockScreen();
                            if (error != undefined) {
                                toastr.error('Error to save asset(s): ' + error.data);
                            }
                            else {
                                toastr.error('Error to save asset(s), please contact technical support');
                            }
                        
                    });
                });
            } else {
                toastr.error('Please make sure you enter correctly all the fields');
            }

        };

        $scope.toggleDetailedBudget = function (newValue) {

            $scope.data.detailed_budget = (newValue != undefined ? newValue ? true : false : $scope.data.detailed_budget);

            if ($scope.data.detailed_budget)
                $scope.optionsGrid.showColumn("unit_price");
            else
                $scope.optionsGrid.hideColumn("unit_price");

        }

        $scope.$watch("data.unit_budget", function (new_value, old_value) {
            new_value = new_value || 0;
            old_value = old_value || 0;
            if (new_value != old_value) {
                $scope.data.total_unit_budget = ($scope.data.total_unit_budget - Number(old_value)) + Number(new_value);
                $scope.calculateBudgets();
            }

        });

        $scope.selectDetailsTab = function () {
            $scope.optionsTab = false;
            if (noFirstTimeDetails && $scope.data.detailed_budget) {
                var items = $scope.optionsGrid.dataSource.data();
                var options_budget = 0;
                items.forEach(function (item) {
                    options_budget += item.unit_price * item.quantity;
                });
                $scope.data.total_unit_budget = Number($scope.data.unit_budget || 0) + Number(options_budget);
                $scope.data.options = options_budget;
            }
            noFirstTimeDetails = true;
        }

        $scope.selectBudgetTab = function () {
            $scope.optionsTab = false;
            $scope.data.options = ($scope.data.total_unit_budget || 0) - ($scope.data.unit_budget || 0);
        }

        $scope.selectGovernmentTab = function () {
            $scope.optionsTab = false;
        }

        function _GetOldProfileInformation() {
            return {
                old_profile: local.profile ? oldProfile.profile_text : $scope.selectedProfile.profile_text || $scope.selectedProfile.profile,
                old_profile_budget: local.profile ? oldProfile.profile_budget : $scope.selectedProfile.profile_budget,
                old_detailed_budget: local.profile ? oldProfile.detailed_budget : usedProfileDetailedBudget
            };
        }

        $scope.close = function (hide) {
            if (hide) {
                $mdDialog.hide($scope.selectedProfile ? angular.extend({
                    profileModified: (local.profile || usingProfile) && (optionsHaveBeenModified || $scope.selectedProfile.detailed_budget != $scope.data.detailed_budget) && hasOtherAssets,
                    inventory_id_new_profile: default_inventory_id
                }, _GetOldProfileInformation()) : local.profile ? angular.extend({
                    profileModified: (optionsHaveBeenModified || oldProfile.detailed_budget != $scope.data.detailed_budget) && hasOtherAssets,
                    new_detailed_budget: $scope.data.detailed_budget, inventory_id_new_profile: default_inventory_id
                }, _GetOldProfileInformation()) : { profileModified: false })
            } else {
                $mdDialog.cancel();
            }
        };

        $scope.selectDocumentsTab = function () {
            $scope.photosFileMatrix = [];
            $scope.artworkFileMatrix = [];
            $scope.optionsTab = false;

            WebApiService.genericController.query({
                controller: 'assetsInventory', action: 'pictures', domain_id: AuthService.getLoggedDomain(),
                project_id: local.params.project_id, phase_id: $scope.data.inventory_id
            }, function (data) {
                if (data && data.length > 0) {
                    var row = -1;
                    var rowArtwork = -1;

                    var imgCount = 0;
                    var imgArtworkCount = 0;

                    data.forEach(function (p) {
                        if (p.isArtwork) {

                            if (imgArtworkCount % 3 === 0) {
                                $scope.artworkFileMatrix.push([]);
                                rowArtwork++;
                            }

                            $scope.artworkFileMatrix[rowArtwork].push({
                                id: p.id, isAssetPhoto: p.isAssetPhoto, isTagPhoto: p.isTagPhoto, isArtwork: p.isArtwork, rotate: p.rotate,
                                file: HttpService.generic('filestream', 'file', AuthService.getLoggedDomain(), p.blobFilename, 'photo'),
                                blobFilename: p.blobFilename,
                                isChecked: false
                            });

                            imgArtworkCount++;
                        }
                        else {
                            if (imgCount % 3 === 0) {
                                $scope.photosFileMatrix.push([]);
                                row++;
                            }
                            $scope.photosFileMatrix[row].push({
                                id: p.id, isAssetPhoto: p.isAssetPhoto, isTagPhoto: p.isTagPhoto, rotate: p.rotate,
                                file: HttpService.generic('filestream', 'file', AuthService.getLoggedDomain(), p.blobFilename, 'photo'),
                                blobFilename: p.blobFilename,
                                isChecked: false
                            });

                            imgCount++;
                        }
                    });
                }
            }, function () {
                toastr.error('Error to try get images files from room');
            });
        };

        function isLocked(field) {
            return !$scope.data[field + "_ow"];
        }

        $scope.getLockIcon = function (field) {
            return isLocked(field) ? "lock" : "lock_open";
        }

        $scope.getLockTooltip = function (field) {
            if (isLocked(field)) {
                return "Unlock to customize the value";
            }
            else {
                return "Lock to pick the value from the catalog";
            }
        }

        function _isOneType() {
            return !local.multiple || $scope.oneType;
        }

        $scope.toggleLock = function (field) {
            var data = $scope.data;
            function restoreField(field) {
                if (data.assetData.hasOwnProperty(field)) {
                    data[field] = data.assetData[field];
                }
                else {
                    toastr.error("Error to retrieve property from catalog, please contact tecnical support");
                }
            }
            var ow = field + "_ow";
            data[ow] = !data[ow];
            if (!data[ow]) {
                if (typeof data.assetData === "undefined" && _isOneType()) {
                    ProgressService.blockScreen();
                    WebApiService.genericController.get({ controller: "assets", action: "item", domain_id: data.asset_domain_id, project_id: data.asset_id },
                        function (assetData) {
                            // adjust fields that do not necessarely map in naming
                            assetData.manufacturer_description = assetData.manufacturer.manufacturer_description;
                            data.assetData = assetData;
                            restoreField(field);
                            ProgressService.unblockScreen();
                        },
                        function () {
                            toastr.error("Error to retrieve asset data");
                            ProgressService.unblockScreen();
                        }
                    );
                }
                else if (typeof data.assetData !== "undefined") {
                    restoreField(field);
                }
            }
        }

        if (!local.multiple) {

            $scope.show_firefox = UtilsService.IsFirefox();

            $scope.fileSizeLimit = FileService.ImageFileSizeLimit;

            var pictures = [];

            function emptyPhotos() {

                pictures = [];
                addEmptyDocument(true);

                if ($scope.uploadForm) {
                    FormService.ResetForm($scope.uploadForm);
                }
            }

            emptyPhotos();

            $scope.toggleEditPicturesMode = function () {
                $scope.editPhotosMode = !$scope.editPhotosMode;
                if (!$scope.editPhotosMode) emptyPhotos();
            };

            $scope.addDocument = function (row) {

                addEmptyDocument();
                row++; // increases the row number to add the new file in the row below

                for (var i = $scope.imgData.length - 1; i > row; i--) {
                    $scope.imgData[i] = $scope.imgData[i - 1];
                    $scope.imgData[i].idx = i;
                }
                addEmptyDocument(false, i);
            }

            $scope.deleteDocument = function (index) {
                if ($scope.imgData.length > 1) {
                    $scope.imgData.splice(index, 1);
                }

                // Update the idx value
                for (var i = index; i < $scope.imgData.length; i++) {
                    $scope.imgData[i].idx = i;
                }
            }

            function getAddedFilesNoFirefox() {
                return $scope.imgData.filter(function (item) { return item.file.picture; });
            }

            function getAddedFilesFirefox() {
                return $scope.imgData.filter(function (item) {
                    var elem = document.getElementById('photoFx' + item.idx);
                    return elem && elem.files && elem.files.length > 0;
                });
            }

            function uploadFiles() {

                WebApiService.genericController.save({
                    controller: 'assetsInventory', action: 'pictures',
                    domain_id: AuthService.getLoggedDomain(), project_id: local.params.project_id,
                    phase_id: default_inventory_id
                }, pictures, function () {
                    toastr.success('Pictures successfully uploaded');
                    ProgressService.unblockScreen();
                    $scope.editPhotosMode = false;
                    emptyPhotos();
                    $scope.selectDocumentsTab();
                }, function (error) {
                    toastr.error('Error to upload one or more pictures. ' + error.data);
                    ProgressService.unblockScreen();
                });
            }

            function uploadFirefox(photosAdded) {

                var finalIdx = photosAdded.length - 1;

                pictures = [];

                photosAdded.forEach(function (data) {
                    FileService.GetBase64FileFirefox('photoFx' + data.idx).then(function (file) {

                        file.label = data.label;
                        file.fileType = data.type;

                        pictures.push(file);

                        if (data.idx === finalIdx) {
                            uploadFiles();
                        }
                    });
                });
            }

            function uploadNoFirefox(photosAdded) {

                var finalIdx = photosAdded.length - 1;

                pictures = [];

                photosAdded.forEach(function (data, idx) {
                    FileService.GetBase64NoFileFirefox(data.file.picture).then(function (file) {
                        file.label = data.label;
                        file.fileType = data.type;

                        pictures.push(file);

                        if (idx === finalIdx) {
                            uploadFiles();
                        }
                    });
                });
            }

            //SHOW IMAGE BIGGER
            $scope.zoomPicture = function (image) {
                $scope.showZoomPicture = true;
                $scope.selectedImg = image;
            }

            $scope.closeZoomPicture = function () {
                $scope.showZoomPicture = false;
            }

            $scope.savePictures = function () {

                $scope.uploadForm.$setSubmitted();

                if (!$scope.uploadForm.$valid) {
                    toastr.error('Please make sure you entered all the required fields');
                    return;
                }

                var photosAdded;
                if ($scope.show_firefox) {
                    photosAdded = getAddedFilesFirefox();
                } else {
                    photosAdded = getAddedFilesNoFirefox();
                }

                if (!photosAdded || photosAdded.length <= 0) {
                    validated = false;
                    toastr.error('Choose at least one file to upload');
                    return;
                }

                ProgressService.blockScreen();
                if ($scope.show_firefox) {
                    uploadFirefox(photosAdded);
                } else {
                    uploadNoFirefox(photosAdded);
                }
            }

            function _getSelectedDocuments() {
                var photos = [];

                $scope.photosFileMatrix.concat($scope.artworkFileMatrix).forEach(function (row) {
                    row.forEach(function (photo) {
                        if (photo.isChecked)
                            photos.push(photo);
                    })
                });
                return photos;
            };

            function _deleteDocumentsPromise() {

                return $q.all(_getSelectedDocuments().map(function (photo) {
                    return WebApiService.genericController.remove({
                        controller: 'assetsInventory', action: 'picture', domain_id: AuthService.getLoggedDomain(),
                        project_id: local.params.project_id, phase_id: $scope.data.inventory_id, department_id: photo.id
                    }).$promise;
                }));
            };

            function _checkOneDocumentSelected() {

                var docs = _getSelectedDocuments();

                if (!docs || docs.length < 1) {
                    toastr.error('You need to select one document');
                } else if (docs.length > 1) {
                    toastr.error('Only one document can be selected');
                } else {
                    return docs[0];
                }

                return null;
            }

            $scope.deleteDocuments = function () {
                ProgressService.blockScreen('deleteDocuments');
                ProgressService.startProgressBar();
                _deleteDocumentsPromise().then(function () {
                    ProgressService.unblockScreen('deleteDocuments');
                    ProgressService.completeProgressBar();
                    toastr.success('Photo(s) Deleted!');
                    $scope.selectDocumentsTab();
                },
                    function () {
                        ProgressService.unblockScreen('deleteDocuments');
                        ProgressService.completeProgressBar();
                        toastr.error('Error to try delete one or more photos, please contact the technical support');
                    });
            }

            function _changeDocumentType(doc, newTypeId) {
                return $q(function (resolve, reject) {
                    if (doc) {
                        ProgressService.blockScreen();
                        WebApiService.genericController.patch({
                            controller: 'projectDocuments', action: 'type', domain_id: AuthService.getLoggedDomain(),
                            project_id: local.params.project_id, phase_id: doc.id
                        }, { type_id: newTypeId }, function () {
                            ProgressService.unblockScreen();
                            $scope.selectDocumentsTab();
                            resolve(doc);
                        }, function (error) {
                            ProgressService.unblockScreen();
                            reject(error);
                        });
                    } else {
                        reject('No document')
                    }
                })
            }

            function _rotatePhoto(pic, newRotate) {
                return $q(function (resolve, reject) {
                    if (pic) {
                        ProgressService.blockScreen();
                        WebApiService.genericController.patch({
                            controller: 'projectDocuments', action: 'rotate', domain_id: AuthService.getLoggedDomain(),
                            project_id: local.params.project_id, phase_id: pic.id
                        }, { rotate: newRotate }, function () {
                            ProgressService.unblockScreen();
                            $scope.selectDocumentsTab();
                            resolve(pic);
                        }, function (error) {
                            ProgressService.unblockScreen();
                            reject(error);
                        });
                    } else {
                        reject('No document')
                    }
                })
            }


            $scope.setAsTagPhoto = function () {
                var doc = _checkOneDocumentSelected();
                if (doc) {
                    _changeDocumentType(doc, DocumentTypes['TagPhoto'])
                        .then(function () { toastr.success('Tag Photo Updated!'); }, function () {
                            toastr.error('Error to try set picture to "Tag Photo"');
                        });
                }
            }

            $scope.setAsAssetPhoto = function () {
                var doc = _checkOneDocumentSelected();
                if (doc) {
                    _changeDocumentType(doc, DocumentTypes['AssetPhoto'])
                        .then(function () {
                            toastr.success('Asset Photo Updated!');
                            $scope.image = downloadFile(doc.blobFilename, local.assets[0].domain_id);
                        }, function () {
                            toastr.error('Error to try set picture to "Asset Photo"');
                        });
                }
            }

            $scope.rotatePhoto = function (value) {
                var pic = _checkOneDocumentSelected();
                var newRotate = pic.rotate + value;
                if (pic) {
                    _rotatePhoto(pic, newRotate)
                        .then(function () {
                            toastr.success('Photo Rotated');
                            $scope.image = downloadFile(pic.blobFilename, local.asset[0].domain_id);
                        }, function () {
                            toastr.error('Error to try rotate photo')
                        })
                }
            }

            //***********************************************************************************************
            //BUDGET COPILOT ACCORDEON
            $scope.favProjectOptions = [];
            $scope.projectOptions = [];
            $scope.catalogOptions = [];
            $scope.searchBoxValue = local.assets[0].asset_code;
            $scope.sourceProject = local.params.project_id;
            $scope.showBudgetCopilot = false;

            var loggedDomain = AuthService.getLoggedDomainFull();
            $scope.catalogOptions = loggedDomain.show_audax_info ? [{ name: 'Audaxware', domain_id: 1, status: 'A' }] : [];
            if (loggedDomain.domain_id != 1) {
                $scope.catalogOptions.push({ name: loggedDomain.name.charAt(0).toUpperCase() + loggedDomain.name.slice(1), domain_id: loggedDomain.domain_id, status: 'A' });
            }


            ////BUDGET COPILOT GRIDS
            //default_inventory_id
            function getCopilotData() {
                ProgressService.blockScreen();
                WebApiService.genericController.get({
                    controller: 'BudgetCopilot', action: 'All', domain_id: AuthService.getLoggedDomain(),
                    project_id: $scope.sourceProject, phase_id: $scope.monthPeriod, department_id: $scope.searchBoxValue
                },
                    function (data) {
                        if (data.budgets == null) {
                            data.budgets = [];
                        }
                            $scope.budgetOptionsGrid.dataSource.data(data.budgets);
                        $scope.sourceAssetsGrid.dataSource.data(data.assets);
                        ProgressService.unblockScreen();
                    });

            }

            /* kendo ui grid configurations*/
            var budgetDataSource = {
                data: [],
                pageSize: 4,
                height: 304,
                schema: {
                    model: {
                        model: { id: "aggregate_type" },
                    }
                }
            };

            var sourceDataSource = {
                data: [],
                pageSize: 5,
                height: 600,
                schema: {
                    model: {
                        model: { id: "inventory_id" },
                    }
                }
            };


            var budgetColumns = [
                {
                    headerTemplate:
                        "", width: 100, filterable: false, sortable: false,
                    template: "<md-button class=\"md-primary\" style=\"min-width:0;padding:0;border:0\" ng-click=\"applyBudget(dataItem)\">Apply</md-button>"
                },
                { field: "aggregate_type", title: "Budget", width: 200 },
                { field: "unit_budget", title: "Unit Budget", width: 190, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_budget); } },
                { field: "unit_install", title: "Unit Install", width: 180, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_install); } },
                { field: "unit_freight", title: "Unit Freight", width: 190, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_freight); } },
                { field: "unit_tax", title: "Unit Tax(%)", width: 180, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_tax); } },
                { field: "unit_markup", title: "Unit Markup(%)", width: 180, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_markup); } },
                { field: "unit_escalation", title: "Unit Escalation(%)", width: 180, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_escalation); } }
            ];

            var sourceColumns = [
                {
                    headerTemplate:
                        "", width: 100, filterable: false, sortable: false,
                    template: "<md-button class=\"md-primary\" style=\"min-width:0;padding:0;border:0\" ng-click=\"applyBudget(dataItem)\">Apply</md-button>"
                },
                { field: "asset_code", title: "Code", width: 120 },
                { field: "jsn_code", title: "JSN", width: 110 },
                { field: "asset_description", title: "Asset Description", width: 180 },
                { field: "project_description", title: "Project", width: 200 },
                { field: "phase_description", title: "Phase", width: 120 },
                { field: "department_description", title: "Department", width: 150 },
                { field: "room_name", title: "Room Name", width: 150 },
                { field: "serial_number", title: "Model No.", width: 150 },
                { field: "serial_name", title: "Model Name", width: 150 },
                { field: "manufacturer_description", title: "Manufacturer", width: 150 },
                { field: "unit_budget", title: "Unit Budget", width: 130, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_budget); } },
                { field: "unit_install", title: "Unit Install", width: 130, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_install); } },
                { field: "unit_freight", title: "Unit Freight", width: 130, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_freight); } },
                { field: "unit_tax", title: "Unit Tax(%)", width: 150, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_tax); } },
                { field: "unit_markup", title: "Unit Markup(%)", width: 160, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_markup); } },
                { field: "unit_escalation", title: "Unit Escalation(%)", width: 180, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unit_escalation); } },
                { field: "date_modified", title: "Date", width: 110, template: "#: date_modified ? kendo.toString(kendo.parseDate(date_modified), \"MM-dd-yyyy\") : '' #" }

            ];

            var gridBudgetOptions = {
                pageSize: 4,
                filterable: true,
                reorderable: false,
                groupable: false,
                height: 304,
                noRecords: {
                    template: "No budgets available."
                },
                columnMenu: {
                    columns: true,
                    sortable: false,
                    messages: {
                        columns: "Columns",
                        filter: "Filter"
                    }
                }
            };

            var gridSourceOptions = {
                pageSize: 10,
                filterable: true,
                reorderable: true,
                groupable: true,                
                height: 600,                
                noRecords: {
                    template: "No source assets available."
                },
                columnMenu: {
                    columns: true,
                    sortable: false,
                    messages: {
                        columns: "Columns",
                        filter: "Filter"
                    }
                }
            };

            $scope.budgetOptions = GridService.getStructure(budgetDataSource, budgetColumns, null, gridBudgetOptions);
            $scope.sourceOptions = GridService.getStructure(sourceDataSource, sourceColumns, null, gridSourceOptions);

            $scope.budgetDataBound = function () {
                GridService.dataBound($scope.budgetOptionsGrid);
            };

            $scope.sourceDataBound = function () {
                GridService.dataBound($scope.sourceAssetsGrid);
            };

            $scope.searchAssets = function () {
                getCopilotData();
            }

            $scope.clearAllFilters = function () {
                $scope.searchBoxValue = "";
                KendoGridService.ClearFilters($scope.sourceAssetsGrid);
                $scope.searchAssets();
            };

            $scope.applyBudget = function (dataItem) {
                var message = 'The selected budget values will be applied to the current asset. Are you sure?';
                DialogService.Confirm('Asset update', message, null, null, true).then(function () {
                    $scope.data.unit_budget = dataItem.unit_budget;
                    $scope.data.unit_freight_net = dataItem.unit_freight;
                    $scope.data.unit_install_net = dataItem.unit_install;
                    $scope.data.unit_tax = dataItem.unit_tax;
                    $scope.data.unit_markup = dataItem.unit_markup;
                    $scope.data.unit_escalation = dataItem.unit_escalation;
                    
                    toastr.info('Unit Budget value will be applied after hit save button.');
                    

                }, function () { });
            }




            WebApiService.genericController.query({ controller: 'Projects', action: 'All', domain_id: loggedDomain.domain_id },
                function (data) {
                    data = data.sort(function (a, b) {
                        return (a.project_description > b.project_description ? 1 : -1);
                    });
                    $scope.projectOptions = $scope.projectOptions.concat(data.map(function (item) {
                        item.name = 'Project: ' + item.project_description;
                        item.status = item.status;
                        return item;
                    }));

                    $scope.projectOptions = $scope.projectOptions.filter(function (el) {
                        return (el != null && el != undefined);
                    });

                    $scope.projectOptions.forEach(function (item) {
                        if (item.user_project_mine.length > 0) {
                            for (var i = 0; i < item.user_project_mine.length; i++) {
                                if (item.user_project_mine[i].userId === loggedDomain.user_id) {
                                    $scope.favProjectOptions.push(item);
                                }
                            }
                        }
                    });

                    $scope.favProjectOptions.forEach(function (item) {
                        $scope.projectOptions.splice($scope.projectOptions.indexOf(item), 1);
                    })

                    $scope.selectedCatalog = $scope.catalogOptions[0];
                }, function () {
                    errorToRetrieveData();
            });

            $scope.projectOptionChanged = function () {
                getCopilotData();
            }

            
            $scope.showHideBudgetCopilot = function () {
                if ($scope.showBudgetCopilot == true) {
                    $scope.showBudgetCopilot = false;
                }
                else {
                    $scope.showBudgetCopilot = true;
                }
            }


            //END BUDGET COPILOT

        }
    }]);
