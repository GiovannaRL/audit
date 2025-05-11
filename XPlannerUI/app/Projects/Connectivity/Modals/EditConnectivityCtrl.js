xPlanner.controller('EditConnectivityCtrl', ['$scope', '$mdDialog', 'toastr', 'AuthService', 'local', 'ConnectionTypeList',
    function ($scope, $mdDialog, toastr, AuthService, local, ConnectionTypeList) {

        $scope.conn = angular.copy(local.data);
        $scope.connTypeList = loadConnectionTypeDropDown();

        $scope.controllerParams = {
            domain_id: AuthService.getLoggedDomain(), project_id: $scope.conn.conn.project_id, phase_id: $scope.conn.conn.connectivity_id, department_id: $scope.conn.conn.inventory_id_in
        }


        function loadConnectionTypeDropDown() {

            var connectionTypeListFiltered = [];
            var inventoryIn = $scope.conn.inventoryIn;
            var inventoryOut = $scope.conn.inventoryOut;
            for (var i = 0; i < ConnectionTypeList.length; i++) {
                var connectionTypeData = {};
                var columnName = "";
                columnName = ConnectionTypeList[i].value.toLowerCase().replace(/\s/g, '');
                if (inventoryOut[columnName] && inventoryOut[columnName] == inventoryIn[columnName]) {
                    connectionTypeData.name = ConnectionTypeList[i].name;
                    connectionTypeData.value = ConnectionTypeList[i].value;
                    connectionTypeListFiltered.push(connectionTypeData);
                }
            }

            return connectionTypeListFiltered;
        }

        $scope.save = function () {
            $scope.editConnForm.$setSubmitted();

            if ($scope.editConnForm.$valid) {
                var assetOut = $scope.conn.conn.inventory_id_out;
                if (assetOut.ports && assetOut.it_connections >= assetOut.ports) {
                    toastr.error(assetOut.asset_description + " does not have available ports to connect.");
                    return;
                }
                $scope.conn.conn.inventory_id_out = assetOut.inventory_id;
                $scope.conn.conn.project_room_inventory1 = assetOut;
                $mdDialog.hide($scope.conn.conn);
            } else {
                toastr.error("Please make sure you entered correctly all the fields");
            }
        };


        $scope.close = function () {
            $mdDialog.cancel();
        };

    }]);