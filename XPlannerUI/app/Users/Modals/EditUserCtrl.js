xPlanner.controller('EditUserCtrl', ['$scope', '$mdDialog', 'GridService', 'local', 'AuthService', 'HttpService',
        'WebApiService', 'toastr', 'ProgressService', 'StatusListProject', 
    function ($scope, $mdDialog, GridService, local, AuthService, HttpService, WebApiService, toastr, ProgressService, StatusListProject) {

        $scope.style_height = window.innerHeight - 120;
        $scope.user = angular.copy(local.user);
        $scope.edit = local.edit;
        $scope.selectedIndex = 0;
        var allProjects = null;
        var originalEmail = null;
        $scope.statusList = StatusListProject;
        $scope.showProjects = !AuthService.isManufacturerDomain();

        /* Projects tab */
        $scope.gridHeight = 450;

        function _getGenericParams(save) {
            return {
                controller: 'users',
                action: 'Item',
                domain_id: AuthService.getLoggedDomain(),
                project_id: local.user.id
            };
        };

        WebApiService.genericController.query({ controller: 'AspNetRoles', action: 'All' }, function (data) {
            $scope.roles = data;
        });

        if (local.edit) {
            WebApiService.genericController.get(_getGenericParams(), function (data) {
                $scope.user = data;
                originalEmail = angular.copy(data.email);
                if (data.aspNetUserRoles && data.aspNetUserRoles.length > 0) {
                    $scope.user.role_id = data.aspNetUserRoles[0].roleId;
                }
            });
        }
        else {
            $scope.user = {};
        }


        var columns = [
               { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(userProjectsGrid)\" ng-checked=\"allPagesSelected(userProjectsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, userProjectsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, userProjectsGrid)\" ng-checked=\"isSelected(userProjectsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
               { field: "project_description", title: "Project Name", width: 400 }
        ];

        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic('projects', "Summarized", AuthService.getLoggedDomain()),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            schema: { model: { id: "project_id", fields: { project_description: { type: "string" } } } },
            error: function () { toastr.error("Error to retrieve projects from server, please contact technical support"); },
            sort: { field: "project_description", dir: "asc" }
        }

        var gridOptions = {
            noRecords: "This enterprise has no projects", height: $scope.gridHeight
        };

        $scope.options = $scope.showProjects ? GridService.getStructure(dataSource, angular.copy(columns), null, gridOptions, false) : null;

        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = function (e, grid, all) {
            if ($scope.user.role_id == "1") {
                toastr.error("Administrators have access to all projects");
            }
            else {
                GridService.select(e, grid, all);
            }
            
        }
        $scope.allPagesSelected = GridService.allPagesSelected;

        $scope.verifyRole = function () {
            if ($scope.showProjects && $scope.user.role_id == undefined) {
                toastr.warning('Please select a Role before go to projects tab.');
                $scope.selectedIndex = 0;
            }
        }

        $scope.dataBound = function () {
            GridService.dataBound($scope.userProjectsGrid);

            if (local.user != undefined) {
                if (local.user.role == 'Administrators') {
                    $scope.userProjectsGrid.selecteds = $scope.userProjectsGrid.dataSource.data();
                }
                else if (local.user && local.user.aspNetUser.project_user) {
                    $scope.userProjectsGrid.selecteds = $scope.userProjectsGrid.dataSource.data().filter(function (item) {
                        return local.user.aspNetUser.project_user.find(function (i) { return i.project_id === item.project_id });
                    });
                }
            }
        }

        $scope.updateProjects = function () {
            if ($scope.showProjects && $scope.user.role_id != undefined) {
                if($scope.user.role_id == "1")   
                    $scope.userProjectsGrid.selecteds = $scope.userProjectsGrid.dataSource.data();
                else
                    $scope.userProjectsGrid.selecteds = "";
            }
        }

        $scope.$watch('project_status', function (newValue, oldValue) {
            if (newValue != oldValue) {
                allProjects = allProjects == null || allProjects.length == 0 ? $scope.userProjectsGrid.dataSource.data() :  allProjects;

                    $scope.userProjectsGrid.dataSource.data(allProjects);
                    if ($scope.project_status != "0") {
                        $scope.userProjectsGrid.dataSource.data($scope.userProjectsGrid.dataSource.data().filter(function (item) {
                            return $scope.project_status === item.status;
                        }));
                    }
            }
        });
        /* END - Projects tab */


        $scope.save = function () {

            $scope.EditUserForm.$setSubmitted();

            if (!$scope.EditUserForm.$valid) {
                toastr.error("Please make sure you've entered all required fields");
                return;
            }

            ProgressService.blockScreen();
            $scope.user.project_user = GridService.getSelecteds($scope.userProjectsGrid);
            $scope.user.aspNetUserRoles = null;
            $scope.user.AspNetUserRoles = [{ userId: (local.edit ? $scope.user.id : null), roleId: $scope.user.role_id, domain_id: AuthService.getLoggedDomain() }];
            if (!local.edit) {
                var sendData = { user: $scope.user, enterprise: AuthService.getLoggedDomainFull() }
            }
            WebApiService.genericController[local.edit ? 'update' : 'save']({
                controller: 'Users', action: local.edit ? 'Item' : 'InviteUser', domain_id: AuthService.getLoggedDomain(), project_id: local.edit ? $scope.user.id : null
            }, local.edit ? $scope.user : sendData, function (data) {
                toastr.success('User successfully saved.');
                ProgressService.unblockScreen();
                $scope.close(true, data);
            }, function (error) {
                if (error.status === 409) {
                    toastr.error('The enterprise already has an associated user with the informed Email!');
                } else {
                    toastr.error('Error trying to save user, please contact the technical support.');
                }
                ProgressService.unblockScreen();
            });
        };

        $scope.resetPassword = function () {
            ProgressService.blockScreen();
            WebApiService.genericController.save({ controller: 'account', action: 'forgotPassword' }, { email: originalEmail }, function () {
                ProgressService.unblockScreen();
                toastr.success('An email have been sent to the user with the instructions');
                $scope.close();
            }, function (error) {
                ProgressService.unblockScreen();
                toastr.error('Error sending email, please try again. If the error persist, please contact the technical support.');
            });
        }

        $scope.close = function (hide, data) {
            hide ? $mdDialog.hide(data) : $mdDialog.cancel();
        };

    }]);