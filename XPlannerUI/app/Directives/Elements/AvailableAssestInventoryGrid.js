xPlanner.directive('awAvailableAssetsInventoryGrid', ['CostFieldList', 'WebApiService', 'AuthService', 'toastr', 'ProgressService',
        'AssetsService', 'KendoGridService', 'HttpService', 'localStorageAwService', 'KendoAssetInventoryService',
        'CostFieldListWithSource',
    function (CostFieldList, WebApiService, AuthService, toastr, ProgressService, AssetsService, KendoGridService, HttpService,
        localStorageAwService, KendoAssetInventoryService, CostFieldListWithSource) {

        return {
            restrict: 'E',
            scope: {
                selecteds: '=',
                height: '=',
                params: '=',
                replace: "=",
                projectAction: "=?",
                selectedCostField: "=?",
                isToGlobalTemplate: "=?",
                isRelocate: "=?",
                isLink: "=?",
                isTemplate: "=?",
                linkBudgetQty: "=?"
            },
            link: function (scope, elem, attrs, ctrl) {

                var allAssets;
                var gridName = scope.isRelocate ? 'relocateGrid' : 'addAssetsGrid';
                var gridLoaded = false;

                function resetCostFieldList(withBudgets) {
                    if (withBudgets) {
                        scope.costField = CostFieldListWithSource;
                    } else {
                        scope.costField = CostFieldList;
                    }

                    scope.selected_costField = scope.selectedCostField = scope.costField[0].value;
                }

                resetCostFieldList();
                scope.isRelocate = false;

                /* Set Catalog Options */
                var loggedDomain = AuthService.getLoggedDomainFull();
                scope.catalogOptions = [];
                if (!scope.isLink) {
                    scope.catalogOptions = loggedDomain.show_audax_info ? [{ name: 'Audaxware', domain_id: 1, status: 'A' }] : [];
                    if (loggedDomain.domain_id != 1) {
                        scope.catalogOptions.push({ name: loggedDomain.name.charAt(0).toUpperCase() + loggedDomain.name.slice(1), domain_id: loggedDomain.domain_id, status: 'A' });
                    }
                }
                
                scope.actionChanged = function () {
                    scope.clearAllFilters();
                    scope.projectAction = scope.action;
                    scope.isRelocate = scope.action == 2;
                    scope.assetsGrid.setOptions(GetInventoryGridConfiguration());

                    if (scope.action == 2) {
                        scope.assetsGrid.table.on("click", ".k-checkbox", { grid: scope.assetsGrid, grid_name: gridName }, selectRow);
                        $('#select-all-' + gridName).on('change', { grid: scope.assetsGrid }, selectAll);
                    }
                    else {
                        scope.selecteds = [];
                    }

                    _bindItemsGrid();
                    gridLoaded = true;
                }

                scope.$watch('action', function (new_value, old_value) {
                    if (new_value == old_value) 
                        return

                    scope.actionChanged();
                });

                scope.catalogOptionChanged = function () {
                    ProgressService.blockScreen('availableAssetInventory');
                    if (!scope.assetsGrid)
                        scope.assetsGrid = $('#addAssetsGrid').kendoGrid().data('kendoGrid');

                    scope.clearAllFilters();
                    scope.selecteds = [];

                    localStorageAwService.set('add-asset-inventory-catalog-option', scope.selectedCatalog);
                    if (scope.selectedCatalog.project_id) {
                        var old_action = scope.action;

                        if ((scope.selectedCatalog.status == 'I' || scope.isLink) && scope.selectedCatalog.project_id != scope.params.project_id) {
                            scope.action = 2;
                        } else {
                            scope.action = 1;
                        }
                  
                        if (old_action === scope.action) {
                            // If catalog change causes an action change, it will be caught by action watch
                            _bindItemsGrid();
                            gridLoaded = true;
                            resetCostFieldList(true);
                        }

                        ProgressService.unblockScreen('availableAssetInventory');

                    } else {
                        scope.action = 1;
                        AssetsService.GetDomainAssets(true).then(function (data) {

                            // If it's template all assets should be displaied
                            if (!scope.isTemplate) {
                                data = data.filter(function (item) {
                                    return item.domain_id == scope.selectedCatalog.domain_id;
                                });
                            }

                            data.sort(function (a, b) {
                                return a.asset_category > b.asset_category ? 1 : a.asset_category < b.asset_category ? -1 : 0;
                            });

                            scope.assetsGrid.setOptions(GetGridConfiguration(data));
                            _bindItemsGrid();
                            gridLoaded = true;
                            ProgressService.unblockScreen('availableAssetInventory');
                            resetCostFieldList();
                        });
                    }
                    scope.actionChanged();
                    allAssets = null;

                    if (scope.selectedCatalog.project_id == scope.params.project_id) {
                        scope.selected_costField = "source";
                    }
                }
                /* END - Set Catalog Options */

                window.showPhoto = function (elem) {
                    $(elem).closest('section').children('.image_popover').children('img').show();
                }

                window.hidePhoto = function (elem) {
                    $(elem).closest('section').children('.image_popover').children('img').hide();
                }

                function GetInventoryGridConfiguration() {
                    var columns = KendoAssetInventoryService.GetNotHiddenColumns(false, false, 'addAssetsGrid');
                    var unit_budget_column, code_column_idx, qty_column;
                    for (var i = 0; i < columns.length; i++) {
                        if (columns[i].headerTemplate != undefined && (!columns[i].headerTemplate.includes("checkbox") || (columns[i].headerTemplate.includes("checkbox") && scope.action != 2))) {
                            columns.splice(i, 1);
                            --i;
                        }
                        else if (columns[i].field === 'total_budget_amt' || columns[i].field === 'current_location' || columns[i].field === 'po_status') {
                            columns.splice(i, 1);
                            --i;
                        }
                        else if (columns[i].field == 'budget_qty') {
                            columns[i].template = "<div align=center>#: budget_qty || 0 #</div>";
                            columns[i].title = "Qty";
                            if (scope.action != 2)
                                columns[i].attributes = { "class": "editable-cell" };
                            columns[i].width = "80px";
                            qty_column = columns[i];
                            columns.splice(i, 1);
                        } else if (columns[i].field == 'asset_profile') {
                            columns[i].template = null;
                        } else if (columns[i].field == 'asset_code') {
                            code_column_idx = i;
                        } else if (columns[i].field == 'total_unit_budget') {
                            unit_budget_column = columns[i];
                            if (scope.action == 2)
                                unit_budget_column.attributes = { "class": "editable-cell" };
                            columns.splice(i, 1);
                        }
                    }
                    
                    columns.splice(code_column_idx + 3, 0, unit_budget_column, qty_column);

                    var gridOptions = {
                        groupable: true, noRecords: "No assets on inventory", reorderable: true, editable: true,
                        height: scope.height
                    };

                    var dataSource = KendoAssetInventoryService.GetDataSource();
                    dataSource.schema.model.fields['budget_qty'].validation = { min: 0 };
                    
                    for (var key in dataSource.schema.model.fields) {
                        dataSource.schema.model.fields[key].editable = false;
                        if (key == 'budget_qty' && scope.action != 2)
                            dataSource.schema.model.fields[key].editable = true;
                        if (key == 'total_unit_budget' && scope.action != 1)
                            dataSource.schema.model.fields[key].editable = true;
                    }

                    if (scope.selectedCatalog.project_id != undefined) {
                        dataSource.transport.read.url = HttpService.asset_inventory_filter_relocate(scope.selectedCatalog.domain_id, scope.selectedCatalog.project_id, scope.linkBudgetQty);
                        var oldParse = angular.copy(dataSource.schema.parse);
                        dataSource.schema.parse = function (data) {
                            var newData = oldParse(data);

                            return newData.map(function (item) {
                                if (scope.action == 2)
                                    item.total_unit_budget = 0;
                                else
                                    item.budget_qty = 0;
                                return item;
                            });
                        }
                    }
                    
                    return KendoGridService.GetStructure(dataSource, columns, null, gridOptions);
                };

                function GetGridConfiguration(assets) {

                    subcategories = assets.map(function (item) { return item.asset_subcategory }).sort();
                    subcategories = subcategories.filter(function (elem, index, self) {
                        return index == self.indexOf(elem);
                    }).map(function (s) { return { 'asset_subcategory': s } });

                    var dataSource = new kendo.data.DataSource({
                        pageSize: 50,
                        data: assets,
                        schema: {
                            model: {
                                id: "asset_id",
                                fields: {
                                    discontinued: { editable: false },
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
                                    jsn_code: { editable: false },
                                    options_code: { editable: false },
                                    vendors_model: { editable: false },
                                    budget_qty: { type: "number", validation: { min: 0 } },
                                    photo: { editable: false },
                                    phase_description: { editable: false },
                                    department_description: { editable: false },
                                    unit_budget: {
                                        validation: {
                                            unitbudgetvalidation: function (input) {
                                                if (input.is("[name='unit_budget']") && input.val() != "") {
                                                    input.attr("data-unitbudgetvalidation-msg", "Only currency values are allowed");
                                                    return /^\-?[0-9]+(\.[0-9]{1,2}){0,1}$/.test(input.val());
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
                        selectable: scope.replace ? 'single row' : null,
                        height: scope.height
                    };

                    var columns = [
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
                                return KendoGridService.GetDownloadFileWithDomainTemplate(dataItem.asset_id, dataItem.domain_id, 'fullcutsheet', dataItem.cut_sheet ? 'images/page_attach.png' : 'images/page.png')
                            }, filterable: false
                        },
                        { field: "model_number", title: "Model No.", width: 120 },
                        { field: "model_name", title: "Model Name", width: 140 },
                        { field: "manufacturer_description", title: "Manufacturer", width: 150 },
                        { field: "discontinued", title: "Disc", width: 80, filterable: false, columnMenu: false },
                        {
                            field: "asset_category", title: "Category", width: 130, filterable: {
                                multi: true,
                                //dataSource: new kendo.data.DataSource({ data: categories })
                            }
                        },
                        {
                            field: "asset_subcategory", title: "Subcategory", width: 150, filterable: {
                                multi: true,
                                dataSource: new kendo.data.DataSource({ data: subcategories })
                            }
                        },
                        { field: "options_code", title: "Avail. Options", width: 250, template: "<span style='white-space: nowrap' title='#: options_code ? options_code : '' #'>#: options_code ? options_code : '' #</span>" },
                        { field: "vendors_model", title: "Vendor's Models", width: 250, template: "<span title='#: vendors_model ? vendors_model : '' #'>#: vendors_model ? vendors_model : '' #</span>" }
                    ];

                    if (!scope.isToGlobalTemplate) {
                        columns.splice(5, 0, {
                            field: "unit_budget", title: "Budget", width: 100, template: GetUnitBudgetTemplate, attributes: {
                                "class": "editable-cell"
                            }, filterable: false
                        },
                        {
                            field: "default_resp", title: "Resp", width: 100, editor: AssetsService.respDropDown, attributes: {
                                "class": "editable-cell"
                            }, filterable: false
                        });
                    }

                    if (!scope.replace) {
                        columns.splice(6, 0, {
                            field: "budget_qty", title: "Qty", width: 80, template: "<div align=center>#: data.budget_qty ? budget_qty : 0 #</div>", attributes: {
                                "class": "editable-cell"
                            }, filterable: false
                        })
                        //columns.unshift({
                        //    headerTemplate: KendoGridService.GetSelectAllTemplate(grid_name),
                        //    template: function (dataItem) {
                        //        return KendoGridService.GetSelectRowTemplate(dataItem.id);
                        //    },
                        //    width: "4em", lockable: false
                        //});
                    } else {
                        columns.splice(10, 0,
                            { field: "min_cost", title: "Min", width: 130, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.min_cost); }, hidden: true },
                            { field: "max_cost", title: "Max", width: 130, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.max_cost); }, hidden: true },
                            { field: "last_cost", title: "Last", width: 130, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.last_cost); }, hidden: true },
                            {
                                field: "avg_cost", title: "Avg", width: 130, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.avg_cost); }, hidden: true
                            });
                    }

                    return KendoGridService.GetStructure(dataSource, columns, null, gridOptions);
                };

                function saveGridState() {
                    KendoGridService.SaveState(scope.assetsGrid, scope.isToGlobalTemplate ? 'add-asset-template-grid-state' : 'add-asset-grid-state');
                }

                function dataBound() {
                    KendoGridService.DataBound(scope.assetsGrid, scope.isToGlobalTemplate ? 'add-asset-template-grid-state' : 'add-asset-grid-state');
                    gridLoaded = true;
                    

                }

                function selectRow(ev) {
                    KendoGridService.SelectRow(ev, this);
                    scope.selecteds = KendoGridService.GetSelecteds(scope.assetsGrid);
                    scope.$apply();
                }

                function selectAll(ev) {
                    KendoGridService.SelectAllChange(ev);
                    scope.selecteds = KendoGridService.GetSelecteds(scope.assetsGrid);
                    scope.$apply();
                }

                function _bindItemsGrid() {

                    scope.assetsGrid.dataSource.bind("change", function (e) {
                        if (e.action == 'itemchange' && e.field == 'budget_qty') {
                            if (scope.selecteds) {
                                var item = scope.selecteds.find(function (i) { return i.id == e.items[e.items.length - 1].id });
                                if (e.items[e.items.length - 1].budget_qty > 0) {
                                    if (!item) {
                                        scope.selecteds.push(e.items[e.items.length - 1]);
                                        scope.$apply();
                                    }
                                } else if (item) {
                                    var index = scope.selecteds.indexOf(item);
                                    scope.selecteds.splice(index, 1);
                                    scope.$apply();
                                }
                            } else if (e.items[e.items.length - 1].budget_qty > 0) {
                                scope.selecteds = [e.items[e.items.length - 1]];
                                scope.$apply();
                            }
                        }
                    });
                    
                    scope.assetsGrid.bind('dataBound', dataBound);
                }

                function errorToRetrieveData() {
                    toastr.error('Error to retrieve data from server, please contact the technical support.');
                    ProgressService.unblockScreen('availableAssetInventory');
                }

                ProgressService.blockScreen('availableAssetInventory');
                // Get the available costFields
                WebApiService.genericController.get({
                    controller: 'projects', action: 'CostField', domain_id: AuthService.getLoggedDomain(),
                    project_id: scope.params.project_id
                }, function (data) {
                    scope.default_cost_field = data.cost_field;

                    /* Set catalog options */
                    scope.favProjectOptions = [];
                    scope.projectOptions = [];
                    var allProjects = [];

                    WebApiService.genericController.query({ controller: 'Projects', action: 'All', domain_id: loggedDomain.domain_id },
                        function (data) {
                            data = data.sort(function (a, b) {
                                return (a.project_description > b.project_description ? 1 : -1);
                            });

                            allProjects = allProjects.concat(data.map(function (item) {
                                item.name = 'Project: ' + item.project_description;
                                item.status = item.status;
                                if ((item.project_id != scope.params.project_id && scope.isLink && item.status == "I") || !scope.isLink) {
                                    return item;
                                }
                            }));

                            allProjects = allProjects.filter(function (el) {
                                return (el != null && el != undefined);
                            });

                            allProjects.forEach(function (item) {
                                if (item.user_project_mine.length > 0) {
                                    for (var i = 0; i < item.user_project_mine.length; i++) {
                                        if (item.user_project_mine[i].userId === loggedDomain.user_id) {
                                            scope.favProjectOptions.push(item);
                                            return;
                                        }
                                    }
                                }
                                scope.projectOptions.push(item);
                            })

                            // SELECT FIRST CATALOG
                            var selectedBefore = localStorageAwService.get("add-asset-inventory-catalog-option");
                            var lastProjectSelected = null;

                            if (selectedBefore) {
                                allProjects.forEach(function (item) {
                                    if (selectedBefore.domain_id == item.domain_id && selectedBefore.project_id == item.project_id)
                                        lastProjectSelected = item;
                                })
                            };

                            if (!scope.isToGlobalTemplate) {
                                if (scope.isLink) {
                                    scope.selectedCatalog = lastProjectSelected;
                                    if (!scope.selectedCatalog)
                                        scope.selectedCatalog = allProjects[0];
                                    scope.catalogOptionChanged();

                                } else {
                                    if (selectedBefore) {
                                        scope.selectedCatalog = selectedBefore;
                                        if (lastProjectSelected)
                                            scope.selectedCatalog = lastProjectSelected;
                                    }
                                    else {
                                        scope.selectedCatalog = scope.catalogOptions[0];
                                    }
                                    scope.catalogOptionChanged();

                                }
                            } else {
                                scope.selectedCatalog = scope.catalogOptions[0];
                                scope.catalogOptionChanged();
                            }

                        }, function () {
                            errorToRetrieveData();
                        });
                }, function () {
                    errorToRetrieveData();
                });

                function GetUnitBudgetTemplate(dataItem) {
                    var value = scope.action == 2 ? 0 : (dataItem.unit_budget || dataItem[scope.selected_costField === 'default' ? scope.default_cost_field : scope.selected_costField] || 0);
                    return KendoGridService.GetCurrencyTemplate(value);
                }

                // Set the function to expand and collapse the grid
                scope.collapseExpand = KendoGridService.CollapseExpand;

                scope.searchAssets = function (value) {
                    if (gridLoaded) {
                        var items = allAssets || scope.assetsGrid.dataSource.data();

                        if (!value) {
                            scope.assetsGrid.dataSource.data(items);
                        } else {
                            value = value.toLowerCase();
                            allAssets = items;
                            var columns = scope.assetsGrid.columns;
                            var filteredItems = items.filter(function (item) {
                                for (var i = 0; i < columns.length; i++) {
                                    if (!columns[i].hidden && String(item[columns[i].field]).toLowerCase().indexOf(value) > -1) {
                                        return true;
                                    }
                                };
                            });
                            scope.assetsGrid.dataSource.data(new kendo.data.ObservableArray(filteredItems));
                            if (filteredItems.length > 0) {
                                scope.assetsGrid.dataSource.page(1);
                            }
                        }
                    }
                }

                scope.clearAllFilters = function () {
                    scope.searchBoxValue = null;
                    KendoGridService.ClearFilters(scope.assetsGrid);
                    scope.searchAssets();
                };

                /* When the cost field changes reload the grid data to update the unit budget */
                scope.$watch('selected_costField', function (new_value, old_value) {
                    if (scope.assetsGrid)
                        scope.assetsGrid.dataSource.data(scope.assetsGrid.dataSource.data());

                    scope.selectedCostField = new_value;
                });

            },
            templateUrl: 'app/Directives/Elements/AvailableAssetsInventoryGrid.html'
        };

    }]);