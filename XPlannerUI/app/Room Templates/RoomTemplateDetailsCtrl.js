xPlanner.controller('RoomTemplateDetailsCtrl', ['$scope', 'GridService', 'AuthService', 'WebApiService', '$stateParams', '$state', 'toastr',
        'ProgressService', 'DialogService', 'AudaxwareDataService', 'HttpService',
    function ($scope, GridService, AuthService, WebApiService, $stateParams, $state, toastr, ProgressService, DialogService,
        AudaxwareDataService, HttpService) {

        $scope.controllerParams = { domain_id: AuthService.getLoggedDomain() };
        $scope.params = $stateParams;
        $scope.can_modify = AudaxwareDataService.CanModify($stateParams);
        $scope.gridHeight = window.innerHeight - 160;
        ProgressService.blockScreen();

        if ($scope.selectedTab == undefined) {
            $scope.selectedTab = 1;
        };
        

        WebApiService.genericController.query(angular.extend({ controller: "projects", action: "All" }, $scope.controllerParams),
            function (projects) {
                $scope.projects = projects;
            }, function () {
                toastr.error('Error to retrieve scopes from server, please contact the technical support');
            });

        WebApiService.genericController.query(angular.extend({ controller: "departmentType", action: "All" }, $scope.controllerParams),
            function (department_types) {
                $scope.department_types = department_types;
            }, function () {
                toastr.error('Error to retrieve department types from server, please contact the technical support');
            });


        WebApiService.genericController.get(angular.extend({ controller: "templateRoom", action: "Item" }, $stateParams),
            function (template) {
                $scope.isGlobalTemplate = !template.project_domain_id_template || !template.project_id_template;
                $scope.room_template = template;
                ProgressService.unblockScreen();
            }, function () {
                toastr.error('Error to retrieve data from server, please contact the technical support');
                ProgressService.unblockScreen();
            });
        
        
        

        
        //BEGIN LINKED TEMPLATES
        ProgressService.blockScreen();
        var columns = [
                   //{ field: "project", title: "Project", width: 180 },
                   { field: "phase", title: "Phase", width: 180 },
                   { field: "department", title: "Department", width: 180 },
                   { field: "room_number", title: "Room Number", width: 180 },
                   { field: "room_name", title: "Room Name", width: 180 },
        ];

        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("templateRoom", "AllByLinkedIdTemplate", $stateParams.domain_id, $stateParams.project_id,
                        $stateParams.phase_id, $stateParams.department_id, $stateParams.room_id),
                    headers: { Authorization: "Bearer " + AuthService.getAccessToken() }
                },
                error: function () {
                    ProgressService.unblockScreen();
                    toastr.error("Error to retrieve data from server, please contact technical support");
                }
            }
        };

        
        $scope.options = GridService.getStructure(dataSource, columns, null, { groupable: true, noRecords: "No room is linked to this template", height: $scope.gridHeight });

        $scope.dataBound = function () {
            ProgressService.unblockScreen();
            GridService.dataBound($scope.linkedTemplatesListGrid);
        };
        //END LINKED TEMPLATES

        function _back() {
            $state.go('room-templates');
        };



        /* Save the template */
        function _save() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            $scope.detailsForm.$setSubmitted();

            if ($scope.detailsForm.$valid) {
                WebApiService.genericController.update({ controller: "templateRoom", action: "Item", domain_id: $scope.room_template.domain_id },
                    $scope.room_template, function (data) {
                        toastr.success('Template Saved');
                    }, function (error) {
                        error.status === 409 ? toastr.error(error.data) : // + ' Please choose a different name.') :
                        toastr.error('Error to save template, please contact the technical support');
                    });
            } else {
                toastr.error('Please make sure you enter all the required fields');
            }
        };
        /* END - Save the template */

        /* Clone a template */
        function _clone() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            DialogService.openModal('app/Room Templates/Modals/CloneTemplate.html', 'CloneTemplateCtrl',
                { params: $scope.params }, true).then(function (template) {
                    $state.go('room-templates-details', template);
                });
        };
        /* END - Clone a template */

        /* Delete a template */
        function _delete() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            DialogService.Confirm('Are you sure?', 'The template will be deleted permanently').then(function () {
                ProgressService.blockScreen();
                WebApiService.genericController.remove(angular.extend({ controller: 'templateRoom', action: 'Item' }, $stateParams),
                    function () {
                        toastr.success('Template deleted');
                        ProgressService.unblockScreen();
                        _back();
                    }, function (error) {
                        ProgressService.unblockScreen();
                        error.status === 409 ? toastr.error(error.data) :
                            toastr.error('Error to try delete template, please contact the technical support');
                    });
            });
        };
        /* End - Delete a template */

        /* Fab buttons */
        $scope.fab = [{
            open: true, buttons: [{
                label: 'Save',
                icon: 'save',
                click: _save,
                show: $scope.can_modify
            }, {
                label: 'Clone',
                icon: 'content_copy',
                click: _clone,
                show: !$scope.can_modify
            }, {
                label: 'Delete',
                icon: 'delete_forever',
                click: _delete,
                show: $scope.can_modify
            }, {
                label: 'Back to List',
                icon: 'reply',
                click: _back,
                show: true
            }]
        }];
        /* END - Fab buttons */

        $scope.setAssetsButtons = function (buttons) {
            buttons.push({
                label: 'Back to List',
                icon: 'reply',
                click: { func: _back },
                show: true
            });
            $scope.fab.push({ open: false, buttons: buttons });
        };

    }]);