xPlanner.controller('ConnectivityTabCtrl', ['$scope', 'GridService', 'AuthService', 'HttpService', 'WebApiService', 'ProgressService', 'DialogService', 'toastr', '$stateParams',
    function ($scope, GridService, AuthService, HttpService, WebApiService, ProgressService, DialogService, toastr, $stateParams) {

        /* kendo ui grid configurations*/
        $scope.gridHeight = 350;
        var params = angular.copy($stateParams);

        $scope.$emit('detailsParams', params);
        $scope.isNotViewer = (AuthService.getLoggedUserType() != "3");

        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic('ITConnectivity', 'AllConnections', AuthService.getLoggedDomain(), params.project_id),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            }
        };
       
        var columns = [
            { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(connectedAssetGrid)\" ng-checked=\"allPagesSelected(connectedAssetGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, connectedAssetGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, connectedAssetGrid)\" ng-checked=\"isSelected(connectedAssetGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
            { field: 'conn.connectivity_id', title: 'ID', width: 2, hidden: true },
            { field: 'conn.inventory_id_in', title: 'IN', width: 2, hidden: true },
            { field: 'room_name_in', title: 'Room Name In', width: 160 },
            { field: 'room_number_in', title: 'Room No In', width: 150 },
            { field: 'asset_in_description', title: 'Asset Description IN', width: 200 },
            { field: 'conn.port_number', title: 'Port No In', width: 150 },
            { field: 'conn.inventory_id_out', title: 'OUT', width: 2, hidden: true },
            { field: 'room_name_out', title: 'Room Name Out', width: 170 },
            { field: 'room_number_out', title: 'Room No Out', width: 150 },
            { field: 'asset_out_description', title: 'Asset Description OUT', width: 210 },
            { field: 'conn.port_number_out', title: 'Port No Out', width: 150 },
            { field: 'conn.connection_type', title: 'Connection Type', width: 170 },
           {
                headerTemplate:
                   "<div align=center style=\"padding-top: 17px\" class=\"comment-header\"><i class=\"material-icons no-button\" title=\"Comment\">comment</i></div>", width: 70, field: "conn.comment", filterable: false, sortable: false,
               template: "<div align=center class=\"comment-header\"><i class=\"material-icons no-button\" title=\"#: conn.comment #\">comment</i></div>"
            },
        ];

        $scope.options = GridService.getStructure(dataSource, columns, null, { noRecords: "No connected assets available", groupable: true, height: $scope.gridHeight }, null, null);
        /* END - kendo ui grid configurations*/

        $scope.dataBound = GridService.dataBound;

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        $scope.exportGrid = GridService.exportGrid;

        /* delete connected asset */
        $scope.delete = function () {

            if (!validateAccess()) {
                return;
            }

            if (GridService.verifySelected('delete', 'connected asset', $scope.connectedAssetGrid)){
                DialogService.Confirm('Are you sure?', 'The connected asset(s) will be deleted permanently!').then(function () {
                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.genericController, 
                        function (item) { return { controller: 'ITConnectivity', action: "Item", domain_id: item.conn.domain_id, project_id: item.conn.project_id, phase_id: item.conn.connectivity_id }; },
                        $scope.connectedAssetGrid).then(function () {
                            ProgressService.unblockScreen();
                            toastr.success('Connected Asset(s) Deleted');
                        }, function () {
                            ProgressService.unblockScreen();
                        });
                    GridService.unselectAll($scope.connectedAssetGrid);
                });
            }
        };
        /* END - delete connected asset */


        /* Open the connect asset modal */
        $scope.addAsset = function () {

            if (!validateAccess()) {
                return;
            }

            DialogService.openModal('app/Projects/Assets/Modals/ConnectAssetConnTab.html', 'ConnectAssetModalCtrl',
                { items: null, params: params }, true).then(function () {
                    $scope.connectedAssetGrid.dataSource.read();
                }, function () {
                    //KendoGridService.UnselectAll(scope.assetsGrid);
                });
        }

        /* END - Open the connect asset modal*/


        $scope.editAsset = function (item) {

            if (!validateAccess()) {
                return;
            }

            if (!GridService.verifySelected('edit', 'connectivity', $scope.connectedAssetGrid, true)) return;
            item = GridService.getSelecteds($scope.connectedAssetGrid)[0];

            DialogService.openModal('app/Projects/Connectivity/Modals/EditConnectivity.html', 'EditConnectivityCtrl', { data: item }, true)
                .then(function (conn) {
                    ProgressService.blockScreen();
                    WebApiService.genericController.update({ controller: 'ITConnectivity', action: 'Item', domain_id: AuthService.getLoggedDomain(), project_id: conn.project_id, phase_id: conn.connectivity_id }, conn, function (data) {
                        $scope.connectedAssetGrid.dataSource.read();
                        ProgressService.unblockScreen();
                        toastr.success('Asset IT Connectivity Updated');

                    }, function () {
                        ProgressService.unblockScreen();
                        toastr.error("Erro to save connectivity, please contact technical support");
                    });
                });


        };


        function validateAccess() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return false;
            }
            if (AuthService.getProjectStatus(params.project_id) == "L") {
                DialogService.LockedProjectModal(params);
                return false;
            }

            return true;
        }

    }]);