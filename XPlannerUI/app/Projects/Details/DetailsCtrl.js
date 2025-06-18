xPlanner.controller('DetailsCtrl', ['$scope', 'WebApiService', '$stateParams', 'StatusListProject', 'CostFieldList',
    'StatesList', 'CostListProject', '$state', 'toastr', 'AuthService', 'ProgressService', '$mdDialog', 'DialogService', '$q',
    function ($scope, WebApiService, $stateParams, StatusListProject, CostFieldList, StatesList, CostListProject, $state, toastr, AuthService,
        ProgressService, $mdDialog, DialogService, $q) {

        $scope.canChangeStatus = function () {
            return AuthService.isAdmin() || !AuthService.isProjectLocked();
        }

        $scope.$emit('detailsParams', angular.copy($stateParams));

        $scope.params = $stateParams;
        $scope.update_budget = 0;
        $scope.project_default_changed = false;
        $scope.isNotViewer = (AuthService.getLoggedUserType() != "3");

        var ignoreRoomQuantity = false;

        updateFinancials();

        function cloneParams(params) {
            return {
                controller: params.serverController,
                project_id: params.project_id,
                phase_id: params.phase_id,
                department_id: params.department_id,
                room_id: params.room_id,
                domain_id: AuthService.getLoggedDomain()
            };
        }

        function getParams2(action, params) {
            return {
                action: action,
                controller: params.serverController,
                project_id: params.project_id,
                phase_id: params.phase_id,
                department_id: params.department_id,
                room_id: params.room_id,
                domain_id: AuthService.getLoggedDomain()
            };
        }

        function getParams(action, id) {
            return {
                action: action,
                domain_id: id
            };
        }

        var controller = WebApiService.detailsController;
        var controllerParams = cloneParams($stateParams);

        $scope.controllerParams = controllerParams;


        // Store the data
        $scope.data = { status: StatusListProject[0].id };
        $scope.inventory = {};

        // set the select boxes
        $scope.statusList = StatusListProject;
        $scope.costs = CostListProject;
        $scope.states = StatesList;

        if ($scope.params.add) {
            $scope.data.default_cost_field = 'avg_cost';
            $scope.data.room_quantity = '1';
        }

        function load() {

            // Get the Data
            ProgressService.blockScreen(); ProgressService.startProgressBar();
            controller.get(controllerParams, function (data) {
                ProgressService.completeProgressBar();
                //if (!$scope.params.add) {
                if (!controllerParams.department_id && controllerParams.phase_id) {
                    data.start_date = new Date(data.start_date.valueOf());
                    data.end_date = new Date(data.end_date.valueOf());
                    data.equip_move_in_date = data.equip_move_in_date ? new Date(data.equip_move_in_date.valueOf()) : null;
                    data.occupancy_date = data.occupancy_date ? new Date(data.occupancy_date.valueOf()) : null;
                    data.ofci_delivery = data.ofci_delivery ? new Date(data.ofci_delivery.valueOf()) : null;

                }
                if (controllerParams.room_id) {

                    if (data.applied_id_template) {
                        WebApiService.genericController.get(
                                {
                                    controller: 'templateRoom', action: 'ItemById',
                                    domain_id: data.project_domain_id_template || data.domain_id,
                                    project_id: data.applied_id_template
                                }
                            , function (template) {
                                if (data.linked_template) {
                                    $scope.linked_template_info = 'Linked to ' + template.drawing_room_name;
                                } else {
                                    $scope.linked_template_info = template.drawing_room_name + ' has been applied to this room';
                                }
                            });
                    } else {
                        $scope.linked_template_info = 'No template has been applied to this room';
                    }
                }

                if (data.locked_date) {
                    data.locked_date = new Date(data.locked_date).toLocaleDateString();
                }

                $scope.data = data;
                $scope.room_count = data.room_quantity;
                calculateAssetBudget();
                setProjectStatus();

                //}

                $scope.isInFavorites = data.user_project_mine && data.user_project_mine.length > 0;
                ProgressService.unblockScreen();
                $scope.$emit('getBreadcrumb', data);
            }, function () {
                ProgressService.unblockScreen(); ProgressService.completeProgressBar();
                toastr.error('Error to retrieve data from server, please contact technical support');
            });
        }

        if (!$scope.params.add)
            load();


        $scope.toggleFavorite = function () {
            $scope.isInFavorites = !$scope.isInFavorites;
            if ($scope.isInFavorites) {
                $scope.addToMine($scope.data);
            } else {
                $scope.removeFromMine($scope.data);
            }
        };


        $scope.$on('AddRemoveFavorites', function (event, item, add) {
            if (item.domain_id == $scope.data.domain_id && (item.id || item.project_id) == $scope.data.project_id) {
                $scope.isInFavorites = add;
            }
        });


        function setProjectStatus() {
            var status = '';
            if ($scope.data.phase_id == undefined) {
                status = $scope.data.status;
            }
            else if ($scope.data.department_id == undefined) {
                status = $scope.data.project.status;
            }
            else if ($scope.data.room_id == undefined) {
                status = $scope.data.project_phase.project.status;
            }
            else {
                status = $scope.data.project_department.project_phase.project.status;
            }
        }

        function calculateAssetBudget() {
            $scope.data.asset_budget = isEmpty($scope.data.medial_budget) - (isEmpty($scope.data.warehouse_budget) + isEmpty($scope.data.freight_budget) + isEmpty($scope.data.tax_budget) + isEmpty($scope.data.warranty_budget) + isEmpty($scope.data.misc_budget) + isEmpty($scope.data.markup_budget) + isEmpty($scope.data.escalation_budget) + isEmpty($scope.data.install_budget));
        }

        function isEmpty(value) {
            var budget = 0;
            if (value != undefined && value != 'NaN' && value != '')
                budget = parseFloat(value);

            return budget;
        }

        function updateBudget() {
            return $q(function (resolve, reject) {
                if (controllerParams.phase_id || !$scope.project_default_changed) {
                    resolve();
                }
                else {
                    DialogService.Confirm('Project Defaults', 'Do you want to override asset amounts already in the project?').then(function () {
                        controllerParams.phase_id = 1;
                        resolve();
                    }, function () { resolve(); });
                }
            });

        }

        function isViewer() {

            if (AuthService.isViewer()) {
                DialogService.ViewersChangesModal();
                return true;
            }
            return false;
        }

        function validateAccess() {

            if (isViewer()) {
                return false;
            }
            
            if (!$scope.canChangeStatus() || (AuthService.isProjectLocked() && $scope.data.phase_id)
                    || (AuthService.isProjectLocked() && $scope.data.status === 'L')) {
                DialogService.LockedProjectModal($scope.data);
                return false;
            }

            return true;
        }

        function save() {
            if (!validateAccess()) {
                return;
            }

            $scope.detailsForm.$setSubmitted();

            // form validation
            if ($scope.detailsForm.$valid) {

                if ($scope.data.start_date) {
                    if ($scope.data.start_date > $scope.data.end_date) {
                        toastr.error('End date cannot be before start date');
                        return;
                    }
                }


                if ($scope.params.add) {
                    ProgressService.blockScreen();
                    var subState = $stateParams.serverController;
                    subState = subState.slice(0, subState.length - 1);
                    subState = subState.toLowerCase();

                    controller.save(controllerParams, $scope.data, function (data) {
                        ProgressService.unblockScreen();
                        var params = cloneParams(data);
                        $scope.room_count = data.room_quantity;

                        // Update the treeview
                        $scope.addItemTreeView(data);
                        $scope.$emit('updateProgressBar', $scope.params);

                        $state.go('index.' + subState,
                            params);


                        toastr.success("Add completed");
                    },
                    function () {
                        ProgressService.unblockScreen();
                        toastr.error("Error to create " + subState + ", please contact the technical support");
                    }
                    );
                } else {
                    updateBudget().then(function () {
                        ProgressService.blockScreen();
                        controller.update(controllerParams, $scope.data,
                            function (data) {
                                //update progress bar
                                $scope.$emit('updateProgressBar', $scope.params);
                                $scope.room_count = data.room_quantity;
                                $scope.updateItemTreeView($scope.data); // Update the item in the treeview
                                ProgressService.unblockScreen();
                                toastr.success("Save completed");
                            },
                            function () {
                                ProgressService.unblockScreen();
                                toastr.error("Error to save");
                            });
                    });
                }

                $scope.$emit('dataSaved');
                return true;
            } else {
                toastr.error("Please make sure you enter all the required fields");
                return false;
            }
        }


        function _getGenericParams(controller, action, id1, id2, id3, id4, id5) {
            return {
                controller: controller,
                action: action,
                domain_id: id1 || AuthService.getLoggedDomain(),
                project_id: id2,
                phase_id: id3,
                department_id: id4,
                room_id: id5
            };
        };

        function updateFinancials() {

            if ($scope.params.project_id && $scope.params.project_id != -1) {
                $scope.financialCosts = {};

                WebApiService.genericController.get(_getGenericParams('financials', 'All', AuthService.getLoggedDomain(), $scope.params.project_id, $scope.params.phase_id, $scope.params.department_id, $scope.params.room_id), function (financial) {
                    $scope.financialCosts = financial;

                }, function (error) {
                    toastr.error("Error to retrieve financial data from server, please contact technical support");
                });
            }

        }


        $scope.$on('saveData', function (event, params) {
            if (save()) {
                $scope.$emit('dataSaved', params);
            }
        });

        function cancel() {
            $scope.goToUpLevel(controllerParams);
        };

        function copyFrom() {
            if (!validateAccess()) {
                return;
            }

            $mdDialog.show({
                controller: 'CopyFromCtrl',
                templateUrl: 'app/Projects/Details/Modals/CopyFrom.html',
                fullscreen: true,
                clickOutsideToClose: true,
                locals: { local: { params: $stateParams } }
            }).then(function () {
                $scope.reloadTreeview();
            });
        };

        function copyProject() {

            if (isViewer()) {
                return;
            }

            $mdDialog.show({
                controller: 'CopyProjectCtrl',
                templateUrl: 'app/Projects/Details/Modals/CopyProject.html',
                fullscreen: true,
                clickOutsideToClose: true,
                locals: { local: { params: $stateParams, project_description: $scope.data.project_description } }
            }).then(function () {
                //$scope.reloadTreeview();
            });
        };


        function copyMoveTo() {
            if (!validateAccess()) {
                return;
            }

            $mdDialog.show({
                controller: 'CopyRoomToCtrl',
                templateUrl: 'app/Projects/Details/Modals/CopyRoomTo.html',
                fullscreen: true,
                clickOutsideToClose: true,
                locals: { local: { params: $scope.data, tree: $scope.tree, copy: true, move: true  } }
            }).then(function (room) {
                var stateParams = { project_id: room.project_id.toString(), phase_id: room.phase_id.toString(), department_id: room.department_id.toString() };
                $scope.reloadTreeview();
                $state.go('index.department', stateParams, { reload: true });
            });
        };

        function saveAsTemplateFn() {
            if (!validateAccess()) {
                return;
            }

            $mdDialog.show({
                controller: 'SaveAsTemplateCtrl',
                templateUrl: 'app/Projects/Details/Modals/SaveAsTemplate.html',
                fullscreen: true,
                clickOutsideToClose: true,
                locals: { local: { params: $stateParams } }
            });
        };

        $scope.splitRooms = function () {
            if (!validateAccess()) {
                return;
            }

            if (parseInt($scope.data.room_quantity) > 1) {
                $stateParams.room_quantity = $scope.data.room_quantity;
                $stateParams.room_name = $scope.data.drawing_room_name;
                $stateParams.room_number = $scope.data.drawing_room_number;

                $mdDialog.show({
                    controller: 'SplitRoomCtrl',
                    templateUrl: 'app/Projects/Details/Modals/SplitRoom.html',
                    fullscreen: true,
                    clickOutsideToClose: true,
                    locals: { local: { params: $stateParams } }
                }).then(function () {
                    ignoreRoomQuantity = true;
                    $scope.reloadTreeview();
                    load();
                });
            }
            else {
                toastr.info('In order to split Room Count, the count must be higher than 1');
            }
        };

        $scope.applyTemplate = function () {
            if (!validateAccess()) {
                return;
            }

            $mdDialog.show({
                controller: 'ApplyTemplateCtrl',
                templateUrl: 'app/Projects/Details/Modals/ApplyTemplate.html',
                fullscreen: true,
                clickOutsideToClose: true,
                locals: { local: { params: $stateParams, department_type_id: $scope.data.room_id && $scope.data.project_department ? $scope.data.project_department.department_type_id : null, room: $scope.data } }
            }).then(function () {
                load();
            });
        };


        //unlink template
        $scope.unlinkTemplate = function () {
            if (!validateAccess()) {
                return;
            }

            if ($stateParams.room_id && $scope.data.linked_template) {
                var dataSend = angular.copy($scope.data);
                dataSend.template_id = $scope.data.applied_id_template;

                WebApiService.template_room.save(getParams2('Unlink', $stateParams), dataSend, function (data) {

                    load();
                    toastr.success('Template Unlinked');
                }, function (error) {
                    if (error.status == 409) {
                        toastr.info('Error trying to unlink template');
                    }
                });
            }
        }

        function removeItem(level) {
            if (!validateAccess()) {
                return;
            }

            DialogService.Confirm('Are you sure?', 'The ' + level + ' will be deleted permanently!').then(function () {
                WebApiService.genericController.remove(angular.extend({
                    controller: level + 's',
                    action: 'Item',
                    domain_id: AuthService.getLoggedDomain()
                },
                        $scope.params),
                    function () {
                        toastr.success(level + ' deleted');
                        $scope.$emit('removeFromTreeView', $scope.params);
                    });
            });
        }

        /* float buttons */
        var saveButton = {
            label: 'Save',
            icon: 'save',
            click: save
        };
        
        var copyMoveToButton = {
            label: 'Copy/Move to',
            icon: 'move',
            click: copyMoveTo
        };        

        var cancelButton = {
            label: 'Cancel',
            icon: 'cancel',
            click: cancel
        };

        var copyFromButton = {
            label: 'Copy From',
            icon: 'copy_from',
            click: copyFrom
        };

        var copyProjectButton = {
            label: 'Copy Project',
            icon: 'content_copy',
            click: copyProject
        };

        var saveAsTemplate = {
            label: 'Save as Template',
            icon: 'note_add',
            click: saveAsTemplateFn
        };

        var remove = {
            label: 'Delete',
            icon: 'delete',
            click: removeItem
        }

        //var applyTemplate = {
        //    label: 'Apply Template',
        //    icon: "note_add",
        //    click: applyTemplate
        //};

        $scope.buttons = {
            //state: $scope.params.add,
            state: true,
            project: { edit: [saveButton, copyFromButton, copyProjectButton, remove], add: [saveButton, cancelButton] },
            phase: { edit: [saveButton, copyFromButton, remove], add: [saveButton, cancelButton] },
            department: { edit: [saveButton, copyFromButton, remove], add: [saveButton, cancelButton] },
            room: { edit: [saveButton, copyFromButton, copyMoveToButton, saveAsTemplate, remove/*, applyTemplate*/], add: [saveButton, cancelButton] }
        };


        $scope.$watch('data', function (newValue, oldValue) {
            if ($scope.detailsForm.$dirty && !angular.equals(newValue, oldValue)) {
                //compareObjects(newValue, oldValue);
                if (!ignoreRoomQuantity)
                    $scope.$emit('itemHasChanges');
                else
                    ignoreRoomQuantity = false;

                calculateAssetBudget();

            }
        }, true);

        $scope.$watch('data.markup', function (newValue, oldValue) {
            if ($scope.detailsForm.$dirty && newValue != oldValue) {
                $scope.project_default_changed = true;
            }
        }, true);

        $scope.$watch('data.escalation', function (newValue, oldValue) {
            if ($scope.detailsForm.$dirty && newValue != oldValue) {
                $scope.project_default_changed = true;
            }
        }, true);

        $scope.$watch('data.tax', function (newValue, oldValue) {
            if ($scope.detailsForm.$dirty && newValue != oldValue) {
                $scope.project_default_changed = true;
            }
        }, true);

        $scope.$watch('data.freight_markup', function (newValue, oldValue) {
            if ($scope.detailsForm.$dirty && newValue != oldValue) {
                $scope.project_default_changed = true;
            }
        }, true);

        $scope.$watch('data.install_markup', function (newValue, oldValue) {
            if ($scope.detailsForm.$dirty && newValue != oldValue) {
                $scope.project_default_changed = true;
            }
        }, true);

        $scope.changeProjectStatus = function () {
            if ($scope.data.status === 'L') {
                DialogService.openModal('app/Projects/Details/Modals/LockProjectModal.html', 'LockProjectModalCtrl', { data: $scope.data })
                    .then(function (data) {
                        $scope.data.locked_comment = data.locked_comment;
                        $scope.data.locked_date = data.locked_date;
                    });
            } else {
                $scope.data.locked_comment = null;
                $scope.data.locked_date = null;
            }
        }


        function compareObjects(s, t) {
            if (typeof s !== typeof t) {
                console.log('two objects not the same type');
                return;
            }
            if (typeof s !== 'object') {
                console.log('arguments are not typeof === "object"');
                return;
            }
            for (var prop in s) {
                if (s.hasOwnProperty(prop)) {
                    if (t.hasOwnProperty(prop)) {
                        if (!angular.equals(s[prop], t[prop])) {
                            console.log('property ' + prop + ' does not match');
                        }
                    } else {
                        console.log('second object does not have property ' + prop);
                    }
                }
            }
            // now verify that t doesn't have any properties 
            // that are missing from s
            for (var prop1 in t) {
                if (t.hasOwnProperty(prop1)) {
                    if (!s.hasOwnProperty(prop1)) {
                        console.log('first object does not have property ' + prop1);
                    }
                }
            }
        }

    }]);