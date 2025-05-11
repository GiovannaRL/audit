xPlanner.controller('DashboardCtrl', ['$scope', 'AuthService', 'ProgressService', 'toastr', 'DialogService', 'WebApiService', '$stateParams', '$state',
    function ($scope, AuthService, ProgressService, toastr, DialogService, WebApiService, $stateParams, $state) {

        var params = angular.copy($stateParams);

        $scope.$emit('detailsParams', params);
        $scope.isNotViewer = (AuthService.getLoggedUserType() != "3");
        //fill dashboard combo with all ids
        getAllReports();

        $scope.$watch('selectedDashboard', function (newValue) {
            if (newValue) {
                // Get the selected dashboard
                ProgressService.blockScreen();
                WebApiService.dashboard.get({ action: 'Get', id1: AuthService.getLoggedDomain(), id2: newValue.report_id, id3: newValue.dashboard_id, id4: newValue.linked_dashboard_id }, function (data) {

                    var embedUrl = data.report.embedUrl;
                    var config = {
                        type: 'report',
                        accessToken: data.accessToken,
                        embedUrl: embedUrl,
                        id: data.report.id,
                        settings: {
                            filterPaneEnabled: false,
                            navContentPaneEnabled: false
                        }
                    };

                    var report = powerbi.embed(document.getElementById('reportContainer'), config);
                    var domain_filter, project_filter, phase_filter, department_filter, room_filter;

                    domain_filter = {
                        $schema: "http://powerbi.com/product/schema#basic",
                        target: {
                            table: "AudaxWarePowerBI",
                            column: "Domain ID"
                        },
                        operator: "In",
                        values: [AuthService.getLoggedDomain()]
                    }

                    if (params.project_id != undefined) {

                        project_filter = {
                            $schema: "http://powerbi.com/product/schema#basic",
                            target: {
                                table: "AudaxWarePowerBI",
                                column: "Project ID"
                            },
                            operator: "In",
                            values: [params.project_id]
                        };

                        if (params.phase_id) {
                            phase_filter = {
                                $schema: "http://powerbi.com/product/schema#basic",
                                target: {
                                    table: "AudaxWarePowerBI",
                                    column: "Phase ID"
                                },
                                operator: "In",
                                values: [params.phase_id]
                            };

                            if (params.department_id) {
                                department_filter = {
                                    $schema: "http://powerbi.com/product/schema#basic",
                                    target: {
                                        table: "AudaxWarePowerBI",
                                        column: "Department ID"
                                    },
                                    operator: "In",
                                    values: [params.department_id]
                                };

                                if (params.room_id) {
                                    room_filter = {
                                        $schema: "http://powerbi.com/product/schema#basic",
                                        target: {
                                            table: "AudaxWarePowerBI",
                                            column: "Room ID"
                                        },
                                        operator: "In",
                                        values: [params.room_id]
                                    };
                                }
                            }
                        }
                    }

                    report.on('loaded', function (event) {
                        report.getFilters()
                          .then(function (filters) {
                              if (domain_filter)
                                  filters.push(domain_filter);
                              if (project_filter)
                                  filters.push(project_filter);
                              if (phase_filter)
                                  filters.push(phase_filter);
                              if (department_filter)
                                  filters.push(department_filter);
                              if (room_filter)
                                  filters.push(room_filter);
                              return report.setFilters(filters);
                          });
                    });

                    ProgressService.unblockScreen();
                }, function () {
                    ProgressService.unblockScreen();
                    toastr.error('Error to retrieve dashboard');
                });
            }
        });

        $scope.resolution = window.innerHeight - 310;


        function getAllReports(deleted) {
            WebApiService.genericController.query({
                controller: 'Dashboard', action: 'All',
                domain_id: AuthService.getLoggedDomain(), project_id: $stateParams.project_id
            }, function (dashboards) {
                $scope.availableDashboards = dashboards;
                if (dashboards.length > 0)
                    $scope.selectedDashboard = dashboards[0];
                else if(deleted == true) {
                    $state.go('dashboard', {}, { reload: true });
                }

            }, function () {
                toastr.error('Error to retrieve the available dashboards, please contact the technical support');
            });

        }


        /* upload files */
        $scope.filesUpload = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }
            DialogService.openModal('app/Dashboard/Modals/UploadDashboardFiles.html', 'UploadDashboardFilesCtrl', { project_id: 115 }, true).then(function () {
                getAllReports();
            });
        };
        /* END - upload files */

        $scope.deleteDashboard = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            if ($scope.selectedDashboard.linked_dashboard_id != '' && $scope.selectedDashboard.linked_dashboard_id != null) {
                toastr.error('This dashboad cannot be deleted because is linked to Audaxware enterprise.');
            }
            else{
                DialogService.Confirm('Are you sure?', 'The dashboard will be deleted permanently!').then(function () {
                    ProgressService.blockScreen();

                    WebApiService.genericController.remove({
                        controller: 'Dashboard', action: 'Delete',
                        domain_id: AuthService.getLoggedDomain(),
                        project_id: $scope.selectedDashboard.dashboard_id
                    }, function () {
                        toastr.success("Dashboard deleted successfully");
                        $scope.report = null;
                        ProgressService.unblockScreen();
                        getAllReports(true);
                    }, function () {
                        toastr.error('Error to retrieve the available dashboards, please contact the technical support');
                        ProgressService.unblockScreen();
                    });

                    
                
                });
            }
        };



        /* float buttons */
        //var uploadFilesButton = {
        //    label: 'Upload File',
        //    icon: 'file_upload',
        //    click: filesUpload,
        //    conditionShow: true
        //};

        //var deleteButton = {
        //    label: 'Delete',
        //    icon: 'delete_forever',
        //    click: deleteDashboard,
        //    conditionShow: true
        //};

        //$scope.buttons = [uploadFilesButton, deleteButton];
        //$scope.buttons.state = false;
        /* end float button */

    }]);