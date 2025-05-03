xPlanner.controller('OCModalCtrl', ['$scope', '$mdDialog', 'WebApiService', 'local', 'toastr',
    function ($scope, $mdDialog, WebApiService, local, toastr) {

        var amt_locations_total;

        $scope.type = local.type;

        $scope.data = {
            asset: local.asset,
            location: {
                total: 0,
                tree: null,
                selected: {}
            },
            options: {
                selected: null,
                availables: null,
                radio: null
            }
        };
        $scope.locations = {};

        function GetSelected() {
            var retorno = {
                none_selected: $scope.data.options.radio === 'none',
                erase_selected: $scope.data.options.radio === 'erase',
                room_ids: $scope.data.location.selected.rooms ? $scope.data.location.selected.rooms : null,
                phase_ids: $scope.data.location.selected.phases ? [] : null,
                department_ids: $scope.data.location.selected.departments ? [] : null,
                option_ids: $scope.data.options.selected
            }

            angular.forEach($scope.data.location.selected.phases, function (item) {
                retorno.phase_ids.push(item.id);
            });

            angular.forEach($scope.data.location.selected.departments, function (item) {
                retorno.department_ids.push(item.id);
            });

            return retorno;
        };

        WebApiService[$scope.type].get({
            equipment_id: local.asset.equipment_id, domain_id: local.asset.equipment_domain_id, project_id: local.params.project_id,
            phase_id: local.params.phase ? local.asset.phase_id : 'null', department_id: local.params.department_id ? local.params.department_id : 'null',
            room_id: local.params.room_id ? local.params.room_id : 'null'
        }, function (data) {

            $scope.data.location.tree = data.tree;
            $scope.data.options.availables = data.options || data.colors;

            angular.forEach($scope.data.location.tree, function (item) {
                $scope.data.location.total += item.total;
            });

            amt_locations_total = $scope.data.location.total;

            $scope.locations = data.locations;

        });

        $scope.updateLocationsTotal = function (level, parent) {

            var amt = 0;

            if ($scope.data.location.selected[level].length) {
                angular.forEach($scope.data.location.selected[level], function (item) {
                    amt += item.total;
                });
            } else if (parent) {
                angular.forEach($scope.data.location.selected[parent], function (item) {
                    amt += item.total;
                });
            }

            $scope.data.location.total = amt ? amt : amt_locations_total;
        }

        $scope.close = function () {
            $mdDialog.cancel();
        };

        $scope.apply = function () {

            $scope.ocForm.$setSubmitted();

            if ($scope.ocForm.$valid) {

                WebApiService[$scope.type + '_save'].update({
                    equipment_id: local.asset.equipment_id, domain_id: local.asset.equipment_domain_id, project_id: local.params.project_id
                }, GetSelected(), function (data) {
                    toastr.success("Save completed");
                }, function (error) {
                    toastr.error("Error to apply");
                });

            } else {
                toastr.error("Please make sure you enter all the required fields");
            }
        };
    }]);