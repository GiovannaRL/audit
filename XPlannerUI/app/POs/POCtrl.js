xPlanner.controller('POCtrl', ['$scope', 'HttpService', 'AuthService', 'GridService', 'ProgressService', '$stateParams', 'toastr',
    'DialogService', 'WebApiService', '$state', 'UtilsService', 'ProgressService',
    function ($scope, HttpService, AuthService, GridService, ProgressService, $stateParams, toastr, DialogService, WebApiService,
        $state, UtilsService, ProgressService) {


        $scope.$emit('detailsParams', angular.copy($stateParams));

        $scope.params = angular.copy($stateParams);
        $scope.isNotViewer = (AuthService.getLoggedUserType() != "3");

        var allPOs;

        /* Kendo grid configuration */
        var dataSource = {
            pageSize: 50,
            transport: {
                read: {
                    url: HttpService.generic('PurchaseOrders', 'AllGrouped', AuthService.getLoggedDomain(), $scope.params.project_id,
                        $scope.params.phase_id ? $scope.params.phase_id : null,
                        $scope.params.department_id ? $scope.params.department_id : null,
                        $scope.params.room_id ? $scope.params.room_id : null
                    ),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            error: function () {
                ProgressService.unblockScreen();
                toastr.error("Error to retrieve POs from server, please contact technical support");
            },
            schema: {
                model: { id: "po_id" },
                parse: function (data) {
                    return data.map(function (i) {
                        i.comment = UtilsService.GetCommentTemplate(i.comment, true);
                        return i;
                    });
                }
            }
        };

        $scope.gridHeight = window.innerHeight - 200;
        var gridOptions = {
            filterable: true,
            reorderable: true,
            groupable: true,
            height: $scope.gridHeight,
            noRecords: {
                template: "No POs available."
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

        var columns = [
            { headerTemplate: '<md-checkbox class="checkbox" md-indeterminate="allSelected(posGrid)" ng-checked="allPagesSelected(posGrid)" aria-label="checkbox" ng-click="select($event, posGrid, true)"></md-checkbox>', template: '<md-checkbox class="checkbox" ng-click="select($event, posGrid)" ng-checked="isSelected(posGrid, dataItem)" aria-label="checkbox"></md-checkbox>', width: '3em', lockable: false },
            { field: 'invalid_po', title: 'Valid', template: '<div align=center center class="valid-header" ng-if="!dataItem.invalid_po"><md-icon class="md-prymary" title="Valid">check_circle</md-icon></div>', width: '90px', filterable: true },
            { field: 'po_number', title: 'PO No.', width: 160, template: "<span ng-if=\"dataItem.po_file != null && dataItem.po_number != null\" class=\"grid_link\"><section layout=\"row\" layout-align=\"center center\" ng-click=\"downloadFile('#: po_file #', #: domain_id#, 'po')\">#: po_number #</section></span><span ng-if=\"dataItem.po_file == null && dataItem.po_number != null\"><section layout=\"row\" layout-align=\"center center\">#: po_number #</section></span>" },
            { field: 'description', title: 'Description', width: 230, template: '<span>#: description #<md-tooltip>#: description #</md-tooltip></span>' },
            { field: 'status', title: 'Status', width: 140 },
            {
                field: 'status_date', title: 'Status Date', width: 140,
                template: "#: status_date && status_date !== '' ? kendo.toString(kendo.parseDate(status_date), \"MM-dd-yyyy\") : '' #"
            },
            { field: 'vendor_name', title: 'Vendor', width: 150, template: '<span>#: vendor_name #<md-tooltip>#: vendor_name #</md-tooltip></span>' },
            { field: 'amount', title: 'PO Amount', template: '<aw-currency value="#: amount # "></aw-currency>', width: 150 },
            { field: 'asset_amount', title: 'Asset Amount', template: '<aw-currency value="#: asset_amount # "></aw-currency>', width: 160 },
            { field: 'quote_amount', title: 'Quote Amount', template: '<aw-currency value="#: quote_amount # "></aw-currency>', width: 160 },
            { field: 'freight', title: 'Freight Amount', template: '<aw-currency value="#: freight # "></aw-currency>', width: 160 },
            { field: 'install', title: 'Install', template: '<aw-currency value="#: install # "></aw-currency>', width: 130 },
            { field: 'warranty', title: 'Warranty', template: '<aw-currency value="#: warranty # "></aw-currency>', width: 150 },
            { field: 'tax', title: 'Tax', template: '<aw-currency value="#: tax # "></aw-currency>', width: 120 },
            { field: 'contingency', title: 'Contingency', template: '<aw-currency value="#: misc # "></aw-currency>', width: 160 },
            { field: 'quote_discount', title: 'Quote Discount', template: '<aw-currency value="#: quote_discount # "></aw-currency>', width: 180 },

            {
                field: 'quote_expiration_date', title: 'Quote Expiration Date', width: 210,
                template: "#: quote_expiration_date && quote_expiration_date !== '' ? kendo.toString(kendo.parseDate(quote_expiration_date), \"MM-dd-yyyy\") : '' #"
            },
            { field: 'quote_number', title: 'Quote No.', width: 170, template: "<span ng-if=\"dataItem.quote_file != null && dataItem.quote_number != null\" class=\"grid_link\"><section layout=\"row\" layout-align=\"center center\" ng-click=\"downloadFile('#: quote_file #', #: domain_id#, 'quote')\">#: quote_number #</section></span><span ng-if=\"dataItem.quote_file == null && dataItem.quote_number != null\"><section layout=\"row\" layout-align=\"center center\">#: quote_number #</section></span>" },
            { field: 'po_requested_number', title: 'Requisition No.', width: 160, template: '<span ng-if="dataItem.po_requested_number != null">#: po_requested_number #<md-tooltip>#: po_requested_number #</md-tooltip></span>' },
            
           
            {
                headerTemplate:
                    '<div align=center class="comment-header"><i class="material-icons no-button" title="Comment">comment</i></div>', title: 'Comment', field: 'comment.comment', template: '#= comment.comment #', width: 90
            },
            { field: 'ship_to', title: 'Ship To', width: 120 },
            { field: 'aging_days', title: 'Aging (Days)', width: 160, template: "<center>#: aging_days || '' #</center>" },

            {
                field: 'report', title: 'Report', width: 120,
                template: '<section layout="row" layout-align="center center"><md-icon class="md-accent" ng-click="report(dataItem)">description</md-icon></section>', filterable: false
            }
        ];

        var exportConfig = {
            excel: {
                fileName: 'Purchase Order ' + ($stateParams.room_id ? 'Room' : $stateParams.department_id ? 'Depatment' : $stateParams.phase_id ? 'Phase' : 'Project') + ' Details.xlsx',
                allPages: true,
                filterable: true
            },
            pdf: {
                fileName: 'Purchase Order ' + ($stateParams.room_id ? "Room" : $stateParams.department_id ? "Depatment" : $stateParams.phase_id ? "Phase" : "Project") + " Details.pdf",
                allPages: true,
                author: 'audaxware',
                creator: 'audaxware',
                margin: {
                    left: 10,
                    right: '10pt',
                    top: '10mm',
                    bottom: '1in'
                },
            }
        };

        var toolbar = {
            template:
                '<section layout="row">' +
                '<section layout="row" layout-align="start center" flex-gt-sm="20" flex="30" ng-if=\"' + $scope.isNotViewer + '\">' +
                '<button class="md-icon-button md-button" ng-click="openAddModal()">' +
                '<i class="material-icons">add</i><div class="md-ripple-container"></div>' +
                '<md-tooltip md-direction="bottom">Add PO</md-tooltip>' +
                '</button>' +
                '<button class="md-icon-button md-button" ng-click="showDetails()">' +
                '<i class="material-icons">edit</i><div class="md-ripple-container"></div>' +
                '<md-tooltip md-direction="bottom">View purchase order details</md-tooltip>' +
                '</button>' +
                '<button class="md-icon-button md-button" ng-click="deletePOs()">' +
                '<i class="material-icons">delete</i><div class="md-ripple-container"></div>' +
                '<md-tooltip md-direction="bottom">Delete PO(s)</md-tooltip>' +
                '</button>' +
                '</section>' +
                '<section layout="row" layout-align="center center" flex-gt-sm="70" flex="55" layout-fill>' +
                '<md-input-container class="md-block no-md-errors-spacer" style="margin: 0px;" flex="90">' +
                '<label>Search</label>' +
                '<input name="search" ng-model="searchBoxValue" ng-enter="search(searchBoxValue)">' +
                '</md-input-container>' +
                '<button class="md-icon-button md-button gray-color" style="bottom: -0.5em; padding-left: 0px; margin-left: 0px" ng-click="clearAllFilters()">' +
                '<i class="material-icons">delete_sweep</i><div class="md-ripple-container"></div>' +
                '<md-tooltip md-direction="bottom">Clear all filters</md-tooltip>' +
                '</button>' +
                '</section>' +
                '<section layout="row" flex="10" layout-align="end start">' +
                '<button class="md-icon-button md-button" ng-click="collapseExpand(posGrid)">' +
                '<md-icon md-svg-icon="collapse_expand"></md-icon>' +
                '<md-tooltip md-direction="bottom">Collapse/Expand All</md-tooltip>' +
                '</button>' +
                '</section>' +
                '</section>'
        };

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions, null, exportConfig);

        $scope.showDetails = function (item) {

            if (!item) {
                if (!GridService.verifySelected('view details', 'purchase orders', $scope.posGrid, true)) return;
                item = GridService.getSelecteds($scope.posGrid)[0];
            }

            ProgressService.blockScreen();
            //$scope.toggleList(item);
            $state.go('index.' + $scope.getLevel($scope.params) + '_pos_details', angular.extend({}, $scope.params, item));
        };

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var po = grid.dataItem(this);
                    $scope.showDetails(po);
                });
            }
        };

        $scope.dataBound = function () {
            if (!allPOs)
                allPOs = $scope.posGrid.dataSource.data();
            setDbClick($scope.posGrid);
            GridService.dataBound($scope.posGrid);
        };

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;

        $scope.collapseExpand = GridService.collapseExpand;

        /* Fires when Export PDF */
        $scope.pdfExport = function () {
            GridService.exportToPDF_Event($scope.posGrid);
        };

        /* END - Kendo grid configuration */

        /* Search by all fields */
        $scope.search = function (value) {

            var items = allPOs || $scope.posGrid.dataSource.data();

            if (!value) {
                $scope.posGrid.dataSource.data(allPOs);
            } else {
                value = value.toLowerCase();
                allPOs = items;
                var columns = $scope.posGrid.columns;
                $scope.posGrid.dataSource.data(items.filter(function (item) {
                    for (var i = 1; i < columns.length; i++) {
                        if (!columns[i].hidden && String(item[columns[i].field]).toLowerCase().indexOf(value) > -1) {
                            return true;
                        }
                    };
                }));
            }
        };

        /* clear all filters */
        $scope.clearAllFilters = function (name) {
            $scope.searchBoxValue = null;
            GridService.ClearFilters($scope.posGrid);
            $scope.search();
        };

        /* Open Modal to add a new po */
        $scope.openAddModal = function () {
            if (!validateAccess()) {
                return;
            }

            DialogService.openModal('app/POs/Modals/AddPO.html', 'AddPOCtrl', { params: $stateParams }, true).then(function (po) {
                $state.go('index.' + $scope.getLevel($scope.params) + '_pos_details', angular.extend({}, $scope.params, po));
            });
        };

        $scope.deletePOs = function () {
            if (!validateAccess()) {
                return;
            }

            if (GridService.verifySelected('delete', 'purchase order', $scope.posGrid)) {
                DialogService.Confirm('Are you sure?', 'The purchase order(s) will be deleted permanently').then(function () {
                    ProgressService.blockScreen();
                    var deletedPOs = GridService.getSelecteds($scope.posGrid).map(function (po) { return po.po_id; });
                    GridService.deleteItems(WebApiService.genericController, function (i) { return { controller: 'PurchaseOrders', action: 'Item', domain_id: i.domain_id, project_id: i.project_id, phase_id: i.po_id } },
                        $scope.posGrid, null, true).then(function (data) {
                            ProgressService.unblockScreen();
                            toastr.success('Purchase Order(s) Deleted');
                            allPOs = allPOs.filter(function (po) { return deletedPOs.indexOf(po.po_id) === -1; });
                        }, function (error) {
                            ProgressService.unblockScreen();
                            toastr.error('Error to delete one o more purchase orders, please contact the technical support');
                            GridService.unselectAll($scope.posGrid);
                        });
                });
            }
        };

        function _switchComment(data) {
            return data.map(function (item) {
                var tmp = item.comment.originalComment;
                item.comment.originalComment = item.comment.comment;
                item.comment.comment = tmp;
                return item;
            });
        }

        function exportGrid(to) {

            ProgressService.blockScreen();
            $scope.posGrid.dataSource.data(_switchComment($scope.posGrid.dataSource.data()));

            GridService.exportGrid(to, $scope.posGrid);

            $scope.posGrid.dataSource.data(_switchComment($scope.posGrid.dataSource.data()));
            ProgressService.unblockScreen();
        };

        /* download files */
        $scope.downloadFile = function (filename, domainId, container) {
            window.open(HttpService.generic('filestream', 'file', domainId, filename, container), '_self');
        };


        $scope.report = function (dataItem) {
            window.open(HttpService.generic('PurchaseOrders', 'DownloadPOCover', dataItem.domain_id, dataItem.project_id, dataItem.id), '_self');
        };

        function validateAccess() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return false;
            }
            if (AuthService.getProjectStatus($scope.params.project_id) == "L") {
                DialogService.LockedProjectModal();
                return false;
            }

            return true;
        }

        /*Fab button*/
        $scope.buttons = [{
            //    label: 'Export to PDF',
            //    icon: 'file',
            //    click: { func: exportGrid, params: 'pdf' }
            //}, {
            label: 'Export to Excel',
            icon: 'file_simple',
            click: { func: exportGrid, params: 'excel' }
        }];


    }]);