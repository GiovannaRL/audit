xPlanner.controller('AuditedListCtrl', ['$scope', 'GridService', 'HttpService', 'AuthService', 'DialogService', 'ProgressService',
    'toastr', '$state',
    function ($scope, GridService, HttpService, AuthService, DialogService, ProgressService, toastr, $state) {

        if (!AuthService.isAuthenticated()) {

            AuthService.clearLocalStorage();
            $state.go('login');
            return;
        }
        ProgressService.blockScreen();

        /* kendo ui grid configurations*/
        var toolbar = {
            template:
                "<section layout=\"row\" ng-cloack>" +
                "<section layout=\"row\" layout-align=\"start center\">" +
                "<button class=\"md-icon-button md-button\" style=\"color: black;\" ng-click=\"openAddEditModal(true)\"><i class=\"material-icons\">visibility</i><div class=\"md-ripple-container\"></div>" +
                "<md-tooltip md-direction=\"bottom\">View Audited Data</md-tooltip>" +
                "</button>" +
                "</section>" +
                "</section>"
        };
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("Audit", "All", AuthService.getLoggedDomain()),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            schema: {
                model: {
                    id: "audit_log_id",
                    fields: {
                        modified_date: { type: 'date' }
                    }
                }
            },
            error: function () {
                toastr.error('Error to retrieve data from server, please contact technical support');
                ProgressService.unblockScreen();
            }
        };
        var gridOptions = { groupable: true, height: window.innerHeight - 100, noRecords: "No audited data available", };

        var columns = [
            { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(auditGrid)\" ng-checked=\"allPagesSelected(auditGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, auditGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, auditGrid)\" ng-checked=\"isSelected(auditGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "5px" },
            { field: "project_description", title: "Project", width: "15px" },
            { field: "username", title: "User", width: "12px" },
            { field: "operation", title: "Operation", width: "10px" },
            { field: "table_name", title: "Table", width: "10px" },
            { field: "original_fields", title: "Original", width: "20px" },
            { field: "changed_fields", title: "Modified", width: "20px" },
            {
                field: "modified_date", title: "Modified Date", format: "{0:MM/dd/yyyy}", width: "12px", template: "#: modified_date ? kendo.toString(kendo.parseDate(modified_date), \"MM/dd/yyyy\") : '' #"
            }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions);

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").off("dblclick").on("dblclick", function () {
                    var clickedItem = grid.dataItem(this);
                    var selectedItems = GridService.getSelecteds(grid);
                    if (selectedItems.length !== 1) {
                        toastr.error("Please select an audited item to view.");
                        return;
                    }
                    var selectedItem = selectedItems[0];
                    if (clickedItem.audit_log_id !== selectedItem.audit_log_id) {
                        toastr.error("Click only on the selected item to view.");
                        return;
                    }
                    $scope.openAddEditModal(true, clickedItem);
                });
            }
        }


        $scope.dataBound = function () {
            setDbClick($scope.auditGrid);
            GridService.dataBound($scope.auditGrid);
            ProgressService.unblockScreen();
        };

        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (widget === $scope.auditGrid) {
                setDbClick($scope.auditGrid);
            }
        });
        /* END - kendo ui grid configurations*/

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        $scope.openAddEditModal = function (fromDblClick, auditedData) {
            if (fromDblClick && auditedData) {
                DialogService.openModal('app/Audit/Modals/ViewAudited.html', 'ViewAuditedCtrl', { audit: auditedData });
                return;
            }

            if (!GridService.verifySelected('edit', 'audit', $scope.auditGrid, true))
                return;

            var selectedItems = GridService.getSelecteds($scope.auditGrid);

            if (selectedItems.length === 1) {
                var item = selectedItems[0];
                DialogService.openModal('app/Audit/Modals/ViewAudited.html', 'ViewAuditedCtrl', { audit: item });
            }
        };




    }]);