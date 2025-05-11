xPlanner.controller('CategoriesListCtrl', ['$scope', 'AuthService', 'HttpService', 'GridService', 'DialogService',
        'ProgressService', 'WebApiService', 'toastr', 'SettingsDropDown', '$q', 'AudaxwareDataService', 'CheckboxEditor',
        'FileService',
    function ($scope, AuthService, HttpService, GridService, DialogService, ProgressService, WebApiService, toastr,
        SettingsDropDown, $q, AudaxwareDataService, CheckboxEditor, FileService) {

        $scope.$emit('initialTab', 'categories');

        /* grid configuration */

        $scope.grid_content_height = window.innerHeight - 180;
        $scope.logged_domain = AuthService.getLoggedDomain();

        var columns = [
            { headerTemplate: '<md-checkbox class="checkbox" md-indeterminate="allSelected(categoryListGrid)" ng-checked="allPagesSelected(categoryListGrid)" aria-label="checkbox" ng-click="select($event, categoryListGrid, true)"></md-checkbox>', template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, categoryListGrid)\" ng-checked=\"isSelected(categoryListGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
            { field: 'description', title: 'Description', width: 200 },
            { field: 'domain.name', title: 'Owner', width: 150 },
            { field: 'hvac', title: 'HVAC', width: 100, filterable: true, template: "#: hvac === 'E' ? 'Enabled' : hvac === 'D' ? 'Disabled' : hvac === 'R' ? 'Required' : '' #", editor: SettingsDropDown },
            { field: 'plumbing', title: 'Plumbing', width: 120, filterable: true, template: "#: plumbing === 'E' ? 'Enabled' : plumbing === 'D' ? 'Disabled' : plumbing === 'R' ? 'Required' : '' #", editor: SettingsDropDown },
            { field: 'gases', title: 'Gases', width: 100, filterable: true, template: "#: gases === 'E' ? 'Enabled' : gases === 'D' ? 'Disabled' : gases === 'R' ? 'Required' : '' #", editor: SettingsDropDown },
            { field: 'it', title: 'IT', width: 100, filterable: true, template: "#: it === 'E' ? 'Enabled' : it === 'D' ? 'Disabled' : it === 'R' ? 'Required' : '' #", editor: SettingsDropDown, hidden: true },
            { field: 'electrical', title: 'Electrical', width: 110, filterable: true, template: "#: electrical === 'E' ? 'Enabled' : electrical === 'D' ? 'Disabled' : electrical === 'R' ? 'Required' : '' #", editor: SettingsDropDown, hidden: true },
            { field: 'support', title: 'Support', width: 100, filterable: true, template: "#: support === 'E' ? 'Enabled' : support === 'D' ? 'Disabled' : support === 'R' ? 'Required' : '' #", editor: SettingsDropDown, hidden: true },
            { field: 'physical', title: 'Physical', width: 100, filterable: true, template: "#: physical === 'E' ? 'Enabled' : physical === 'D' ? 'Disabled' : physical === 'R' ? 'Required' : '' #", editor: SettingsDropDown, hidden: true },
            { field: 'environmental', title: 'Environmental', width: 100, filterable: true, template: "#: environmental === 'E' ? 'Enabled' : environmental === 'D' ? 'Disabled' : environmental === 'R' ? 'Required' : '' #", editor: SettingsDropDown, hidden: true }
            //{ field: "placement", title: "Placement", width: 110, filterable: false, template: "#: placement === 'E' ? 'Enabled' : placement === 'D' ? 'Disabled' : placement === 'R' ? 'Required' : '' #", editor: SettingsDropDown, hidden: true }
        ];

        function _getToolbar(type) {
            return {
                template:
                    '<section layout="row" ng-cloak>' +
                        '<section layout="row" layout-align="start center" flex="85">' +
                            "<button class=\"md-icon-button md-button\" ng-click=\"add('" + type + "')\"><i class=\"material-icons\">add</i><div class=\"md-ripple-container\"></div>" +
                                '<md-tooltip md-direction="bottom">Add ' + type.charAt(0).toUpperCase() + type.substring(1) + '</md-tooltip>' +
                            '</button>' +
                            "<button class=\"md-icon-button md-button\" ng-click=\"saveChanges('" + type + "')\" ng-disabled=\"getChangedItems(" + type + "ListGrid).length === 0\"><i class=\"material-icons\">save</i><div class=\"md-ripple-container\"></div>" +
                                '<md-tooltip md-direction="bottom">Save Changes</md-tooltip>' +
                            '</button>' +
                            "<button class=\"md-icon-button md-button\" ng-click=\"delete('" + type + "')\" ng-disabled=\"!hasNoAudaxwareItems(" + type + "ListGrid)\"><i class=\"material-icons\">delete</i><div class=\"md-ripple-container\"></div>" +
                                '<md-tooltip md-direction="bottom">Delete ' + type.charAt(0).toUpperCase() + type.substring(1) + '</md-tooltip>' +
                            '</button>' +
                            "<button class=\"md-icon-button md-button\" ng-click=\"showDetails('" + type + "')\" ng-disabled=\"!hasNoAudaxwareItems(" + type + "ListGrid)\"><i class=\"material-icons\">edit</i><div class=\"md-ripple-container\"></div>" +
                                '<md-tooltip md-direction="bottom">Edit ' + type.charAt(0).toUpperCase() + type.substring(1) + '</md-tooltip>' +
                            '</button>' +
                        '</section>' +
                        '<section layout="row" layout-align="end center" flex="15">' +
                            '<button class="md-icon-button md-button" ng-click="collapseExpand(' + type + 'ListGrid)">' +
                                '<md-icon md-svg-icon="collapse_expand"></md-icon>' +
                                '<md-tooltip md-direction="bottom">Collapse/Expand All</md-tooltip>' +
                            '</button>' +
                        '</section>' +
                    '</section>'
            }
        };

        /*Get the data to dataSource*/
        function _getDataSource(type) { //type = subcategories ou type = categories

            if (type === 'category') {
                return {
                    transport: {
                        read: {
                            url: HttpService.generic('categories', 'All', $scope.logged_domain),
                            headers: {
                                Authorization: 'Bearer ' + AuthService.getAccessToken()
                            }
                        }
                    },
                    schema: { model: { fields: { description: { type: 'string', editable: false }, "domain.name": { type: 'string', editable: false } } } },
                    error: function () { toastr.error('Error to retrieve data from server, please contact technical support'); },
                    sort: { field: 'description', dir: 'asc' }//,
                    //group: { field: "domain.name" }
                };
            } else {
                return {
                    data: [],
                    schema: {
                        model: {
                            fields: {
                                description: { type: 'string', editable: false },
                                "category_description": { type: 'string', editable: false },
                                use_category_settings: { type: 'boolean' },
                            }
                        }
                    },
                    sort: [{ field: 'category_description', dir: 'asc' }, { field: 'description', dir: 'asc' }]//,
                    //group: { field: "assets_category.description" }
                };
            }
        }

        var gridOptions = {
            columnMenu: {
                columns: true,
                sortable: false,
                messages: {
                    columns: 'Columns',
                    filter: 'Filter'
                }
            },
            noRecords: 'No categories available', groupable: true, height: $scope.grid_content_height, editable: true
        };

        $scope.categoryOptions = GridService.getStructure(_getDataSource('category'), angular.copy(columns), _getToolbar('category'), gridOptions, false);
        columns.splice(2, 1, { field: 'domain_name', title: 'Owner', width: 150 });
        columns.splice(0, 1, { headerTemplate: '<md-checkbox class="checkbox" md-indeterminate="allSelected(subcategoryListGrid)" ng-checked="allPagesSelected(subcategoryListGrid)" aria-label="checkbox" ng-click="select($event, subcategoryListGrid, true)"></md-checkbox>', template: '<md-checkbox class="checkbox" ng-click="select($event, subcategoryListGrid)" ng-checked="isSelected(subcategoryListGrid, dataItem)" aria-label="checkbox"></md-checkbox>', width: '3em' }, { field: 'category_description', title: 'Category', width: 200 });
        columns.push({ field: 'use_category_settings', title: 'Use Category Settings', template: '<section layout="column" layout-align="none center"><md-checkbox style="margin-top: 0px; margin-bottom: 0px" class="checkbox" ng-checked="#: use_category_settings #" aria-label="checkbox"></md-checkbox></section>', width: 200, editor: CheckboxEditor });
        gridOptions.noRecords = 'No subcategories available'
        $scope.subcategoryOptions = GridService.getStructure(_getDataSource('subcategory'), columns, _getToolbar('subcategory'), gridOptions, false);

        function _getSubcategories() {

            var params = angular.extend({ controller: 'subcategories', domain_id: $scope.logged_domain },
                $scope.clickedCategory ? { action: 'All', project_id: $scope.clickedCategory.domain_id, phase_id: $scope.clickedCategory.category_id } : { action: 'Multi' });

            var method = 'query';

            if (!$scope.clickedCategory) {
                ProgressService.blockScreen();
                method = 'save';
            }

            WebApiService.genericController[method](params, GridService.getSelecteds($scope.categoryListGrid).map(function (i) { return { domain_id: i.domain_id, category_id: i.category_id } }),
                function (data) {
                    $scope.clickedCategory ? $scope.subcategoryListGrid.dataSource.data(data) : $scope.subcategoryListGrid.dataSource.data(data.subcategories || []);
                    ProgressService.unblockScreen();
                }, function () {
                    toastr.error('Error to retrieve data from server, please contact the technical support');
                    ProgressService.unblockScreen();
                });
        };

        function setDbClick(grid, type) {
            if (grid) {
                grid.tbody.find('tr').dblclick(function () {
                    var item = grid.dataItem(this);
                    $scope.showDetails(type, item);
                });

                if (grid === $scope.categoryListGrid) {
                    grid.tbody.find("tr td:nth-child(2), tr td:nth-child(3)").click(function () {
                        if (GridService.getSelecteds($scope.categoryListGrid).length === 0) {
                            $scope.clickedCategory = grid.dataItem($(this).closest('tr'));
                            _getSubcategories();
                        }
                    });

                    grid.tbody.find("tr td:nth-child(2), tr td:nth-child(3)").hover(function () {
                        GridService.getSelecteds($scope.categoryListGrid).length === 0 ? $(this).css('cursor', 'pointer')
                            : $(this).css('cursor', 'default');
                    });
                }
            }
        };

        function _reloadData(type) {
            if (type === 'category') {
                $scope.categoryListGrid.dataSource.read();
                $scope.subcategoryListGrid.dataSource.data([])
            } else {
                _getSubcategories();
            }
        };

        function _saveChangesAux(items, type, type_plural) {
            ProgressService.blockScreen();
            $q.all(AudaxwareDataService.RemoveAudaxwareItems(items).map(function (item) {
                return WebApiService.genericController.update({ controller: type_plural, action: "Item", domain_id: $scope.logged_domain, project_id: item[type + '_id'] },
                item).$promise
            })).then(function () {
                _reloadData(type);
                toastr.success(type.charAt(0).toUpperCase() + type_plural.substring(1) + ' Updated');
                ProgressService.unblockScreen();
            }, function () {
                toastr.error('Error to updated one or more ' + type_plural + ', please contact the tecnical support');
                ProgressService.unblockScreen();
            });
        };

        $scope.hasNoAudaxwareItems = function (grid) {
            return AudaxwareDataService.HasNoAudaxwareItems(GridService.getSelecteds(grid));
        };

        $scope.getChangedItems = function (grid) {
            return grid.dataSource.data().filter(function (item) {
                return item.dirty;
            });
        }

        $scope.saveChanges = function (type) {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            var type_plural = type === 'category' ? 'categories' : 'subcategories';

            var items = $scope.getChangedItems($scope[type + 'ListGrid']);

            if (items.length > 0) {
                var canEdit = AudaxwareDataService.CanModifyAny(items);

                if (canEdit === 1) {
                    _saveChangesAux(items, type, type_plural);
                } else {
                    AudaxwareDataService.ShowCantModifyDialog(type_plural, true, canEdit === -1).then(function () {
                        canEdit === -1 ? $scope[type + 'ListGrid'].refresh() : _saveChangesAux(items, type, type_plural);
                    });
                }
            } else {
                toastr.info('There is no changes to be save in the ' + type_plural + ' grid');
            };
        };

        $scope.showDetails = function (type, item) {
            if (!item) {
                if (!GridService.verifySelected('edit', type, $scope[type + 'ListGrid'], true)) return;
                item = GridService.getSelecteds($scope[type + 'ListGrid'])[0];
            }

            if (AudaxwareDataService.CanModify(item)) {
                var modal = type === 'category' ? DialogService.openModal('app/Category/Modals/AddCategory.html', 'AddCategoryCtrl', { edit: true, category: item }, true) :
                    DialogService.openModal('app/Category/Modals/AddSubcategory.html', 'AddSubcategoryCtrl', { edit: true, subcategory: item }, true);

                modal.then(function () {
                    _reloadData(type);
                });
            } else {
                AudaxwareDataService.ShowCantModifyDialog(type);
            }
        };

        $scope.add = function (type) {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            var modal = type === 'category' ? DialogService.openModal('app/Category/Modals/AddCategory.html', 'AddCategoryCtrl', null, true) :
                DialogService.openModal('app/Category/Modals/AddSubcategory.html', 'AddSubcategoryCtrl',
                    $scope.clickedCategory ? { category: $scope.clickedCategory } : GridService.getSelecteds($scope.categoryListGrid).length === 1 ?
                        { category: GridService.getSelecteds($scope.categoryListGrid)[0] } : null
                , true);

            modal.then(function () {
                _reloadData(type);
            });

        }

        $scope.dataBound = function (grid, type) {
            setDbClick(grid, type);
            GridService.dataBound(grid);
        }

        $scope.delete = function (type) {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            var type_plural = type === 'category' ? 'Categories' : 'Subcategories';

            if (GridService.verifySelected('delete', type, $scope[type + "ListGrid"])) {
                DialogService.Confirm('Are you sure?',
                    AudaxwareDataService.GetDeleteMessage(type_plural, AudaxwareDataService.HasAudaxwareItems(GridService.getSelecteds($scope[type + "ListGrid"]))))
                    .then(function () {
                        ProgressService.blockScreen();
                        GridService.deleteItems(WebApiService.genericController, function (i) { return { controller: type_plural, action: 'Item', domain_id: i.domain_id, project_id: i.category_domain_id, phase_id: i.category_id, department_id: i.subcategory_id } },
                            $scope[type + "ListGrid"], null, true).then(function (data) {
                                ProgressService.unblockScreen();
                                toastr.success(type_plural + ' Deleted');
                                type === 'subcategory' ? _getSubcategories() : GridService.unselectAll($scope.categoryListGrid);
                            }, function (error) {
                                ProgressService.unblockScreen();
                                if (error.status === 409) toastr.info("Some, or all, of the " + type_plural + " could not be deleted -- there is assigned asset(s)");
                                else toastr.error('Error to delete one o more ' + type_plural + ', please contact the technical support');
                                type === 'subcategory' ? _getSubcategories() : GridService.unselectAll($scope.categoryListGrid);
                            });
                    });
            }
        }

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = function (e, grid, all) {
            $scope.clickedCategory = null;
            GridService.select(e, grid, all);
        }

        $scope.$watch(function () { return GridService.getSelecteds($scope.categoryListGrid).length }, function (newValue) {
            _getSubcategories();
        });

        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        $scope.collapseExpand = GridService.collapseExpand;
        
        function _exportGrid() {
            FileService.downloadFile(HttpService.generic('categories', 'Export', $scope.logged_domain), "categories_" + $scope.logged_domain + "_export.xlsx");
        };

        function _importFile() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            DialogService.openModal('app/Category/Modals/ImportFile.html', 'ImportCategoriesFileCtrl')
                .then(function () {
                    _reloadData('category');
                });
        };

        /* Fab buttons */
        $scope.buttons = [{
            label: 'Export to Excel',
            icon: 'file_simple',
            click: { func: _exportGrid }
        },
        {
            label: 'Import data from Excel',
            icon: 'file_upload',
            click: { func: _importFile }
        }];
        /* END - Fab buttons */

    }]);