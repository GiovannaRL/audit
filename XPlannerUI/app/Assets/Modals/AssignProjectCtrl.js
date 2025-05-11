xPlanner.controller('AssignProjectCtrl', ['$scope', 'WebApiService', 'AuthService', 'local', 'toastr', '$mdDialog',
    function ($scope, WebApiService, AuthService, local, toastr, $mdDialog) {

        var asset = angular.copy(local.asset);
        var allProjects = { domain_id: -1, project_id: -1, project_description: 'All Projects' };

        WebApiService.genericController.query({
            controller: 'assetsProject', action: 'All', domain_id: local.asset.domain_id, project_id: local.asset.asset_id
        }, function (data) {
            data.length === 0 || data.length > 1 ? $scope.projectSelected = allProjects : $scope.projectSelected = data[0];

            WebApiService.genericController.query({ controller: 'projects', action: 'All', domain_id: AuthService.getLoggedDomain() },
            function (projects) {
                projects.unshift(allProjects);
                $scope.projectSelected = projects.find(function (p) {
                    return p.domain_id === $scope.projectSelected.domain_id && p.project_id === $scope.projectSelected.project_id;
                });
                $scope.projects = projects;
            });
        });

        $scope.assign = function () {

            $scope.assignProjectForm.$setSubmitted();

            if ($scope.assignProjectForm.$valid) {

                if ($scope.projectSelected.project_id == -1 && $scope.projectSelected.domain_id == -1) {
                    WebApiService.genericController.remove({ controller: 'assetsProject', action: 'All', domain_id: asset.domain_id, project_id: asset.asset_id },
                        function () {
                            toastr.success('Asset assigned to projects');
                            $mdDialog.hide();
                        }, function () {
                            toastr.error('Error to assign asset to project, please contact the technical support');
                        });
                } else {
                    WebApiService.genericController.save({ controller: 'assetsProject', action: 'Assign', domain_id: asset.domain_id, project_id: asset.asset_id, phase_id: $scope.projectSelected.domain_id, department_id: $scope.projectSelected.project_id },
                       { asset_domain_id: asset.domain_id, asset_id: asset.asset_id, domain_id: $scope.projectSelected.domain_id, project_id: $scope.projectSelected.project_id },
                       function (data) {
                           toastr.success('Asset assigned to project');
                           $mdDialog.hide();
                       }, function (error) {
                           if (error.status === 409) {
                               toastr.error(error.data);
                           } else {
                               toastr.error('Error to assign asset to project, please contact the technical support');
                           }
                       });
                }

            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };

    }]);