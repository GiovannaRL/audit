xPlanner.controller('CheckRoomModalCtrl', ['$scope', 'HttpService', 'AuthService', 'local', '$mdDialog', '$mdMedia', 'GridService',
    function ($scope, HttpService, AuthService, local, $mdDialog, $mdMedia, GridService) {

        $scope.params = local.params;
        $scope.largeMedia = $mdMedia('gt-sm');
        $scope.gridHeight = 440;

        function mountAssetParam(assets) {

            var ret = '';

            assets.forEach(function (item) {
                ret += item.asset_id + item.asset_domain_id.toString() + ',';
            });

            return ret.slice(0, -1);
        }

        /* kendo ui grid configurations*/
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.search_room_asset(
                        AuthService.getLoggedDomain(),
                        $scope.params.project_id, mountAssetParam(local.assets),
                        $scope.params.phase_id ? $scope.params.phase_id : -1,
                        $scope.params.department_id ? $scope.params.department_id : -1,
                        $scope.params.room_id ? $scope.params.room_id : -1
                    ),
                    headers: {
                        Authorization: 'Bearer ' + AuthService.getAccessToken()
                    }
                }
            },
            schema: { model: { fields: { dnp_qty: {type: 'number'}}}}
        };
        var gridOptions = { reorderable: true, groupable: true, height: $scope.gridHeight };
        var columns = [
                { field: 'asset_code', title: 'Code', width: '110px' },
                { field: 'asset_description', title: 'Description', width: '200px', template: '#: asset_description +  (tag ? "(" + tag + ")" : "") #' },
                { field: 'serial_number', title: 'Model No.', width: '120px' },
                { field: 'serial_name', title: 'Model Name', width: '140px' },
                { field: 'drawing_room_name', title: 'Room', width: '170px', template: '<a ng-click=\"close()\" ui-sref=\"index.room({project_id: #: project_id #, phase_id: #: phase_id #, department_id: #: department_id #, room_id: #: room_id #})\">#: drawing_room_name #</a>' },
                { field: 'dept_desc', title: 'Department', width: '200px' },
                { field: 'phase_desc', title: 'Phase', width: '200px' },
                { field: 'budget_qty', title: 'Planned Qty', width: '120px', template: '<center>#: budget_qty#</center>' },
                { field: 'lease_qty', title: 'Lease Qty', width: '120px', template: '<center>#: lease_qty#</center>' },
                { field: 'dnp_qty', title: 'DNP Qty', width: '120px', template: '<center>#: dnp_qty || 0 #</center>' },
                { field: 'resp', title: 'Resp', width: '100px' },
                { field: 'current_location', title: 'EQ Status', width: '120px' },
                { field: 'po_status', title: 'PO Status', width: '130px' },
                { field: 'total_budget_amt', title: 'Budget', width: '130px', template: '<aw-currency value="#: total_budget_amt # "></aw-currency>' }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, null, gridOptions);
        /* END - kendo ui grid configurations*/

        $scope.dataBound = GridService.dataBound;

        // close the modal
        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);