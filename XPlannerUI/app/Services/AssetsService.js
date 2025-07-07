xPlanner.factory('AssetsService', ['WebApiService', '$q', 'AuthService', 'localStorageService', 'ProgressService', 'toastr',
    'DialogService', 'GridService', 'AudaxwareDataService', 'HttpService', 'KendoGridService',
    function (WebApiService, $q, AuthService, localStorageService, ProgressService, toastr, DialogService,
        GridService, AudaxwareDataService, HttpService, KendoGridService) {

        var cachedDomainAssets = {
            domain_id: null,
            assets: []
        };     

        var _GetToGrid = function () {
            return $q(function (resolve, reject) {
                ProgressService.blockScreen();
                WebApiService.genericController.query({ controller: "Assets", action: "Summarized", domain_id: AuthService.getLoggedDomain() }, function (data) {
                    ProgressService.unblockScreen();
                    resolve(data);
                }, function () {
                    ProgressService.unblockScreen();
                    reject();
                });
            });
        }

        var _GetNetworkAssetInToGrid = function (projectId) {

            return $q(function (resolve, reject) {
                ProgressService.blockScreen();
                WebApiService.genericController.query({ controller: "assetsInventory", action: "AllNetWorkInventoriesWithPortsAndITConnections", domain_id: AuthService.getLoggedDomain(), project_id: projectId }, function (data) {
                    ProgressService.unblockScreen();
                    resolve(data);
                }, function () {
                    ProgressService.unblockScreen();
                    reject();
                });
            });
        }   

        var _GetNetworkAssetToGrid = function (projectId, assetIn) {

            return $q(function (resolve, reject) {
                ProgressService.blockScreen();
                WebApiService.genericController.query({ controller: "assetsInventory", action: "AllNetWorkInventories", domain_id: AuthService.getLoggedDomain(), project_id: projectId, phase_id: assetIn }, function (data) {
                    ProgressService.unblockScreen();
                    resolve(data);
                }, function () {
                    ProgressService.unblockScreen();
                    reject();
                });
            });
        } 

        function _respDropDown(container, options) {
            $('<input data-text-field="name" data-value-field="value" data-bind="value:' + options.field + '"/>')
                .appendTo(container)
                .kendoDropDownList({
                    dataSource: {
                        transport: {
                            read: {
                                url: HttpService.generic('responsability', 'All', AuthService.getLoggedDomain()),
                                headers: {
                                    Authorization: "Bearer " + AuthService.getAccessToken()
                                }
                            },
                        },
                        schema: {
                            parse: function (data) {
                                return data.map(function (i) { return { name: i.name, value: i.name }; });
                            }
                        }
                    }
                });
        };

        //If it's not add is replace

        var _GetReplaceParams = function (selectCatalog) {
            var isCatalog = !selectCatalog.project_id;
            return {
                controller: isCatalog ? "Assets" : "assetsInventory",
                action: isCatalog ? "Summarized" : "AllInventories",
                domain_id: AuthService.getLoggedDomain(),
                project_id: isCatalog ? null : selectCatalog.project_id
            }
        };
        
        var projectColumns = [
            { title: "phase", index: 0 },
            { title: "Department", index: 1 },
            { title: "Room No.", index: 2 },
            { title: "Room Name", index: 3 },
            { title: "Planned qty", index: 11 },
            { title: "Shop Drawing", index: 22 },
            { title: "ECN", index: 23 },
            { title: "Cost Center", index: 24 }];

        var catalogColumns = [
            { title: "Disc", index: 19 },
            { title: "Category", index: 20 },
            { title: "Subcategory", index: 21 },
            { title: "Min", index: 15 },
            { title: "Max", index: 16 },
            { title: "Last", index: 17 },
            { title: "Avg", index: 17 }];


        var _SetColumns = function (grid) {
            var isCatalog = !grid.selected_Catalog.project_id
            var show = isCatalog ? catalogColumns : projectColumns;
            var hide = isCatalog ? projectColumns : catalogColumns;

            show.forEach(function (item) {
                grid.assetsGrid.showColumn(item.index);
            });

            hide.forEach(function (item) {
                grid.assetsGrid.hideColumn(item.index);
            });
        };

        var _GetGridReplaceConfiguration = function (isTemplate, height) {
            var dataSource = new kendo.data.DataSource({
                pageSize: 50,
                data: [],
                schema: {
                    model: {
                        id: "asset_id",
                        fields: {
                            discontinued: { editable: false },
                            phase_description: { editable: false },
                            description_description: { editable: false },
                            room_number: { editable: false },
                            room_name: { editable: false },
                            asset_code: { editable: false },
                            cut_sheet: { editable: false },
                            model_number: { editable: false },
                            model_name: { editable: false },
                            manufacturer_description: { editable: false },
                            asset_description: { editable: false },
                            min_cost: { editable: false },
                            max_cost: { editable: false },
                            last_cost: { editable: false },
                            avg_cost: { editable: false },
                            asset_category: { editable: false },
                            asset_subcategory: { editable: false },
                            has_shop_drawing: { editable: false },
                            ecn: { editable: false },
                            cost_center: { editable: false },
                            jsn_code: { editable: false },
                            photo: { editable: false },
                            cut_sheet: { editable: false },
                            budget_qty: { type: "number" },
                            unit_budget: {
                                validation: {
                                    unitbudgetvalidation: function (input) {
                                        if (input.is("[name='unit_budget']") && input.val() != "") {
                                            input.attr("data-unitbudgetvalidation-msg", "Only currency values are allowed");
                                            return /^[0-9]+(\.[0-9]{1,2}){0,1}$/.test(input.val());
                                        }

                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                //group: [
                //    { field: "asset_category" },
                //    { field: "asset_subcategory" },
                //]
            });

            var gridOptions = {
                groupable: true, noRecords: "No assets available", reorderable: true, editable: true, columnMenu: {
                    columns: true,
                    sortable: false
                },
                selectable: 'single row',
                height: height
            };

            var columns = [
                { field: "phase_description", title: "Phase", width: "150px", hidden: true },
                { field: "department_description", title: "Department", width: "150px", hidden: true },
                { field: "room_number", title: "Room No.", width: "120px", hidden: true },
                { field: "room_name", title: "Room Name", width: "150px", hidden: true },
                {
                    field: "asset_code", title: "Code", width: 110, template: function (dataItem) {
                        return KendoGridService.GetAssetCodeLinkTemplate(dataItem);
                    }, lockable: false
                },
                { field: "jsn_code", title: "JSN", width: 150 },
                { field: "asset_description", title: "Description", width: 250 },
                {
                    field: "photo", title: 'Photo', width: 100, filterable: false,
                    template: function (dataItem) {
                        return dataItem.photo ?
                            '<section align=center>' +
                            '<i onmouseover="showPhoto(this)" onmouseout="hidePhoto(this)" class="material-icons">visibility</i>' +
                            '<section align=center class="image_popover grid-column">' +
                            '<img src="' + HttpService.generic('filestream', 'file', dataItem.domain_id, dataItem.photo, 'photo') +
                            '" title="picture not found">' +
                            '</section>' +
                            '</section>' : '';
                    }
                },
                {
                    field: "cut_sheet", title: "Spec", width: 90, template: function (dataItem) {
                        return KendoGridService.GetDownloadFileWithDomainTemplate(dataItem.asset_id, (!dataItem.asset_domain_id ? dataItem.domain_id : dataItem.asset_domain_id), 'fullcutsheet', dataItem.cut_sheet ? 'images/page_attach.png' : 'images/page.png')
                    }, filterable: false
                },
                {
                    field: "default_resp", title: "Resp", width: 100, editor: _respDropDown, attributes: {
                        "class": "editable-cell"
                    }, filterable: false
                },
                {
                    field: "unit_budget", title: "Unit Budget", width: 150, template: "<div align=right>{{ dataItem.unit_budget || (wizardData.selected_costField == '' ? wizardData.default_unit_budget : dataItem[wizardData.selected_costField === 'default' ? default_cost_field : wizardData.selected_costField]) || 0 | currency}}</div>", attributes: {
                        "class": "editable-cell unit-budget"
                    }, filterable: false
                },
                { field: "budget_qty", title: "Planned qty", width: "130px", template: function (dataItem) { return "<center>#:" + dataItem.budget_qty ? dataItem.budget_qty : '' + "#</center>" }, hidden: true },
                { field: "model_number", title: "Model No.", width: 120 },
                { field: "model_name", title: "Model Name", width: 140 },
                { field: "manufacturer_description", title: "Manufacturer", width: 150 },
                { field: "min_cost", title: "Min", width: 130, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.min_cost ? dataItem.min_cost : ''); } },
                { field: "max_cost", title: "Max", width: 130, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.max_cost ? dataItem.max_cost : ''); } },
                { field: "last_cost", title: "Last", width: 130, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.last_cost ? dataItem.last_cost : ''); } },
                { field: "avg_cost", title: "Avg", width: 130, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.avg_cost ? dataItem.avg_cost : ''); } },
                { field: "discontinued", title: "Disc", width: 80, filterable: false, columnMenu: false },
                { field: "asset_category", title: "Category", width: 130, filterable: { multi: true, search: true } },
                { field: "asset_subcategory", title: "Subcategory", width: 150, filterable: { multi: true, search: true } },
                {
                    field: "has_shop_drawing", title: "Shop Drawing", width: "150px", filterable: false, hidden: true,
                    template: function (dataItem) {
                        if (dataItem.has_shop_drawing) {
                            return KendoGridService.GetDownloadFileWithDomainTemplate(dataItem.pdoc_blob_filename, dataItem.domain_id, 'project-documents', 'images/file-pdf.png')
                        }
                        return '';
                    }, lockable: false
                },
                { field: "ecn", title: "ECN", width: "100px", hidden: true },
                { field: "cost_center", title: "Cost Center", width: "130px", hidden: true }

            ];

            

            var _toolbar = {
                template:
                    "<section layout=\"row\" layout-align=\"space-between center\">" +
                    "<section layout=\"row\" layout-align=\"start center\" flex=\"45\">" +
                    "<md-input-container class=\"md-block\" style=\"margin-bottom: 7px;\" flex=\"90\" ng-style=\"isToGlobalTemplate ? {display:'none'} : {display: 'block'}\">" +
                    "<label>Catalog</label>" +
                    "<md-select name=\"catalog\" ng-model=\"wizardData.selected_Catalog\" ng-change=\"catalogOptionChanged()\">" +
                    "<md-optgroup label=\"Catalog\" ng-if=\"!isLink\">" +
                    "<md-option ng-repeat=\"op in catalogOptions\" ng-value=\"op\">" +
                    "{{op.name}}" +
                    "</md-option>" +
                    "</md-optgroup>" +
                    "<md-optgroup label=\"Favorite Projects\" ng-if=\"!isLink\">" +
                    "<md-option ng-repeat=\"op in favProjectOptions\" ng-value=\"op\">" +
                    "{{op.name}}" +
                    "</md-option>" +
                    "</md-optgroup>" +
                    "<md-optgroup label=\"Projects\" ng-if=\"!isLink\">" +
                    "<md-option ng-repeat=\"op in projectOptions\" ng-value=\"op\">" +
                    "{{op.name}}" +
                    "</md-option>" +
                    "</md-optgroup>" +
                    "</md-select>" +
                    "</md-input-container>" +
                    "</section>" +
                    "<section layout=\"row\" layout-align=\"center center\" flex=\"40\">" +
                    "<md-input-container class=\"md-block no-md-errors-spacer\" style=\"margin-bottom: 7px;\" flex=\"90\">" +
                    "<label>Search</label>" +
                    "<input name=\"search\" ng-model=\"wizardData.searchBoxValue\" ng-enter=\"search(wizardData.searchBoxValue)\">" +
                    "</md-input-container>" +
                    "<button class=\"md-icon-button md-button gray-color\" style=\"bottom: -0.5em; padding-left 0px; margin-left 0px\" ng-click=\"clearAllFilters()\">" +
                    "<i class=\"material-icons\">delete_sweep</i><div class=\"md-ripple-container\"></div>" +
                    "<md-tooltip md-direction=\"bottom\">Clear all filters</md-tooltip>" +
                    "</button>" +
                    "</section>" +
                    "<section layout=\"row\" flex=\"15\" layout-align=\"end start\">" +
                    "<md-input-container class=\"md-block\" style=\"margin-bottom: 7px;\">" +
                    "<label>Asset Default Cost</label>" +
                    "<md-select ng-model=\"wizardData.selected_costField\">" +
                    "<md-option ng-repeat=\"cost in costField\" value=\"{{cost.value}}\">" +
                    "{{cost.name}}" +
                    "</md-option>" +
                    "</md-select>" +
                    "</md-input-container>" +
                    "<button class=\"md-icon-button md-button\" ng-click=\"collapseExpand(assetsGrid)\" style=\"bottom: -18px\">" +
                    "<md-icon md-svg-icon=\"collapse_expand\"></md-icon>" +
                    "<md-tooltip md-direction=\"bottom\">Collapse/Expand All</md-tooltip>" +
                    "</button>" +
                    "</section>" +
                    "</section>"
            };

            return GridService.getStructure(dataSource, columns, _toolbar, gridOptions);
        };

        var _getDomainAssets = function (refresh) {

            return $q(function (resolve, reject) {
                const domainId = AuthService.getLoggedDomain();
                if (cachedDomainAssets.domain_id === domainId) {
                    resolve(cachedDomainAssets.assets);
                    return;
                }
                WebApiService.genericController.query({ controller: "assets", action: "Summarized", domain_id: domainId }, function (data) {
                    cachedDomainAssets = {
                        domain_id: domainId,
                        assets: data
                    };
                    resolve(data);
                }, function () {
                    reject();
                });
            });
        };

        function _filterAssetInformation(assetServer, assetView) {

            return {
                asset_id: assetServer.asset_id,
                domain_id: assetServer.domain_id,
                descontinued: assetServer.discontinued,
                cut_sheet: assetServer.cut_sheet,
                cad_block: assetServer.cad_block,
                revit: assetServer.revit,
                asset_code: assetServer.asset_code,
                model_number: assetServer.model_number,
                model_name: assetServer.model_name,
                asset_description: assetServer.asset_description,
                min_cost: assetServer.min_cost,
                max_cost: assetServer.max_cost,
                avg_cost: assetServer.avg_cost,
                last_cost: assetServer.last_cost,
                default_resp: assetServer.default_resp,
                manufacturer_description: assetServer.manufacturer ? assetServer.manufacturer.manufacturer_description : assetView.manufacturer.manufacturer_description,
                owner: { domain_id: assetServer.domain_id, name: assetServer.domain_id == 1 ? 'audaxware' : AuthService.getLoggedDomainName() },
                asset_category: assetView.assets_subcategory.category_description,
                asset_subcategory: assetServer.assets_subcategory ? assetServer.assets_subcategory.description : assetView.assets_subcategory.description,
                comment: assetServer.comment || null,
                updated_at: assetServer.updated_at
            };
        }

        function _updateAtLocalStorage(asset, assetView) {
            if (localStorageService.isSupported) {
                var assets = localStorageService.get("assets");
            } else if (localStorageService.cookie.isSupported) { // otherwise we use cookie
                var assets = localStorageService.cookie.get("assets");
            }

            if (assets) {
                assets = assets.map(function (item) {
                    if (item.domain_id == asset.domain_id && item.asset_id == asset.asset_id) item = _filterAssetInformation(asset, assetView);
                    return item;
                });
                //} else { assets = [_filterAssetInformation(asset, assetView)] }
                if (localStorageService.isSupported) {
                    var assets = localStorageService.set("assets", assets);
                } else if (localStorageService.cookie.isSupported) { // otherwise we use cookie
                    var assets = localStorageService.cookie.set("assets", assets);
                }
            }
            else {
                localStorage.removeItem("assets");
            }
        };

        var _updateAsset = function (params, asset, succesMessage) {
            return $q(function (resolve, reject) {
                ProgressService.blockScreen();
                params.controller = "assets";
                params.action = "Item";
                params.domain_id = AuthService.getLoggedDomain();
                WebApiService.genericController.update(params, asset,
                    function (data) {
                        const domainId = AuthService.getLoggedDomain();
                        if (succesMessage) toastr.success(succesMessage);
                        if (cachedDomainAssets.domain_id === domainId) {domainId
                            _getSingleSummarizedAsset(data).then(function (value) {
                                if (value) {
                                    var cachedAssetIndex = cachedDomainAssets.assets.findIndex(function (val) {
                                        return val.asset_id === value.asset_id;
                                    });
                                    cachedDomainAssets.assets[cachedAssetIndex] = value.toJSON();
                                }
                            });
                        }

                        ProgressService.unblockScreen();
                        resolve(data);
                    }, function () {
                        ProgressService.unblockScreen();
                        toastr.error('Error to save asset, please contact the technical support');
                        reject();
                    });
            });
        };

        function _deleteAssetsFromGrid(grid, assets) {

            var promisseArray = [];

            if (assets && grid) {
                angular.forEach(AudaxwareDataService.RemoveAudaxwareItems(assets), function (item) {
                    promisseArray.push(WebApiService.genericController.remove({ controller: "assets", action: "Item", domain_id: item.domain_id, project_id: item.asset_id }, function () {
                        grid.dataSource.remove(item);

                        const loggedDomainId = AuthService.getLoggedDomain();
                        if (cachedDomainAssets.domain_id === loggedDomainId) {
                            cachedDomainAssets.assets = cachedDomainAssets.assets.filter(function (val) {
                                return val.asset_id !== item.asset_id
                            })
                        }
                    }).$promise)
                });
            }

            return $q.all(promisseArray);
        };

        function _deleteAsset(asset) {
            return $q(function (resolve, reject) {
                DialogService.Confirm('Are you sure?', 'The asset will be deleted permanently!').then(function () {
                    ProgressService.blockScreen();

                    WebApiService.genericController.remove({ controller: "assets", action: "Item", domain_id: asset.domain_id, project_id: asset.asset_id },
                        function () {
                            const loggedDomainId = AuthService.getLoggedDomain();
                            ProgressService.unblockScreen();
                            toastr.success("Asset deleted");
                            if (cachedDomainAssets.domain_id === loggedDomainId) {
                                cachedDomainAssets.assets = cachedDomainAssets.assets.filter(function (val) {
                                    return val.asset_id !== asset.asset_id
                                });
                            }
                            resolve();
                        }, function (error) {
                            ProgressService.unblockScreen();
                            if (error.status == 409) {
                                toastr.info(error.data);
                            } else {
                                toastr.error('Error to try delete asset, please contact the technical support');
                            }
                            reject();
                        });
                }, function () {
                    reject('cancel');
                });
            });
        };

        function _addAsset(asset) {
            return $q(function (resolve, reject) {
                WebApiService.genericController.save({ controller: "assets", action: "Item", domain_id: AuthService.getLoggedDomain() },
                    asset, function (data) {
                        toastr.success('Asset Added');

                        const loggedDomainId = AuthService.getLoggedDomain();
                        if (cachedDomainAssets.domain_id === loggedDomainId) {
                            _getSingleSummarizedAsset(data).then(function (value) {
                                if (value) {
                                    cachedDomainAssets.assets.push(value.toJSON());
                                }
                            });
                        }
                        resolve(data);
                    }, function () {
                        toastr.error("Error to save asset, please contact technical support");
                        reject();
                    });
            });
        };

        function _duplicateAsset(asset, linkDuplicatedAsset) {
            return $q(function (resolve, reject) {
                WebApiService.genericController.update({ controller: "assets", action: "Duplicate", domain_id: asset.domain_id, project_id: asset.asset_id, phase_id: AuthService.getLoggedDomain(), department_id: linkDuplicatedAsset }, null,
                    function (data) {
                        const loggedDomainId = AuthService.getLoggedDomain();
                        if (cachedDomainAssets.domain_id === loggedDomainId) {
                            _getSingleSummarizedAsset(data).then(function (value) {
                                if (value) {
                                    cachedDomainAssets.assets.push(value.toJSON());
                                }
                            });
                        }
                        toastr.success('Asset Duplicated');
                        resolve(data);
                    }, function () {
                        toastr.error("Error to duplicate asset, please contact technical support");
                        reject();
                    });
            });
        };

        function _getSingleSummarizedAsset(asset) {
            // Function to get assets on the same model of the grid to keep cached data up to date
            return $q(function (resolve, reject) {
                WebApiService.genericController.get({ controller: "assets", action: "SummarizedSingle", domain_id: asset.domain_id, project_id: asset.asset_id },
                    function (data) {
                        resolve(data);
                    },
                    function () {
                        reject();
                    }
                );
            });
        }

        window.showPhoto = function (elem) {
            $(elem).closest('section').children('.image_popover').children('img').show();
        }

        window.hidePhoto = function (elem) {
            $(elem).closest('section').children('.image_popover').children('img').hide();
        }

        return {
            GetToGrid: _GetToGrid,
            GetNetworkAssetInToGrid: _GetNetworkAssetInToGrid,
            GetNetworkAssetToGrid: _GetNetworkAssetToGrid,
            GetDomainAssets: _getDomainAssets,
            UpdateAsset: _updateAsset,
            DeleteAsset: _deleteAsset,
            AddAsset: _addAsset,
            DeleteAssetsFromGrid: _deleteAssetsFromGrid,
            GetGridReplaceConfiguration: _GetGridReplaceConfiguration,
            respDropDown: _respDropDown,
            DuplicateAsset: _duplicateAsset,
            GetReplaceParams: _GetReplaceParams,
            SetColumns: _SetColumns
        }
    }]);