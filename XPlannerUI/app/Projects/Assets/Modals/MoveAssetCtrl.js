xPlanner.controller('MoveAssetCtrl', ['$scope', 'StatusListProject', '$mdDialog', 'AuthService', 'local', 'WebApiService', 'toastr', 'ProgressService',
    function ($scope, StatusListProject, $mdDialog, AuthService, local, WebApiService, toastr, ProgressService) {

        $scope.data = {
            project_id: local.params.project_id,

        };

        $scope.params = local.params;
        $scope.departments = [];
        $scope.rooms = [];

        var GetParamsCB = function (data, action, phase_id, department_id) {
            return {
                controller: data,
                action: action,
                domain_id: AuthService.getLoggedDomain(),
                project_id: $scope.data.project_id,
                phase_id: (data != 'phases' ? phase_id : null),
                department_id: (data == 'rooms' ? department_id : null)
            };
        };

        $scope.getData = function (data, phase_id, department_id) {
            WebApiService.genericController.query(GetParamsCB(data, "All", phase_id, department_id), function (items) {
                $scope[data] = items;
            });
        };

        $scope.getData('phases', null, null);

        $scope.$watch('data.phase_id', function (newValue) {
            if (newValue) {
                $scope.departments = [];
                $scope.rooms = [];
                $scope.getData('departments', newValue, null);
            }
        });

        $scope.$watch('data.department_id', function (newValue) {
            if (newValue) {
                $scope.rooms = [];
                $scope.getData('rooms', $scope.data.phase_id, newValue);
            }
        });


        function GetParams() {

            var params = {};

            params.controller = "rooms";
            params.action = "MoveAsset";
            params.domain_id = AuthService.getLoggedDomain();
            params.project_id = $scope.data.project_id;
            params.phase_id = $scope.data.phase_id;
            params.department_id = $scope.data.department_id;
            params.room_id = $scope.data.room_id;

            return params;
        };

        $scope.move = function () {
            $scope.moveAssetForm.$setSubmitted();
            ProgressService.blockScreen();
            if ($scope.moveAssetForm.$valid) {

                var inventories = [];
                for (var i = 0; i < local.items.length; i++) {
                    inventories.push(local.items[i].inventory_id);
                }

                WebApiService.genericController.save(GetParams(), inventories, function (data) {
                    ProgressService.unblockScreen();
                    toastr.success('The inventories were successfully moved');
                    $mdDialog.hide(data);
                }, function (error) {
                    toastr.error('Error trying to move inventories, please contact the technical support');
                    ProgressService.unblockScreen();
                    //$mdDialog.cancel();

                });
            } else {
                toastr.error("Please make sure you entered correctly all the fields");
                ProgressService.unblockScreen();
            }

        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);