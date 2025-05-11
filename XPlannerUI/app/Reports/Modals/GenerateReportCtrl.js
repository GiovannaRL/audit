xPlanner.controller('GenerateReportCtrl', ['$scope', '$mdMedia', '$mdStepper', '$mdDialog', '$timeout', 'WebApiService',
        'toastr', '$stateParams', 'AuthService', 'ProgressService', 'POStatusList',
    function ($scope, $mdMedia, $mdStepper, $mdDialog, $timeout, WebApiService, toastr, $stateParams, AuthService,
        ProgressService, POStatusList) {

        $scope.poStatuses = POStatusList;

        var steppers;
        $timeout(function () { steppers = $mdStepper('stepper-generating-report'); }, false);
        var fields = {
            step1: ['name', 'type', 'description', 'private'],
            step2: []
        };

        $scope.mobileStep = $mdMedia('xs');
        $scope.currentStep = 1;
        $scope.report = {po_status: 'All', isPrivate: true, include_budgets: true};
        $scope.locations = {
            params: angular.copy($stateParams),
            height: window.innerHeight - 320,
            selecteds: []
        };

        /* Get the report types */
        //WebApiService.genericController.query({ controller: 'ReportTypes', action: 'All' }, function (types) {
        //    $scope.reportTypes = types;
        //}, function () {
        //    toastr.error('Error to retrieve the report types, please contact the technical support');
        //});

        /* Get the cost centers */
        WebApiService.genericController.query({ controller: 'CostCenters', action: 'All', domain_id: AuthService.getLoggedDomain(), project_id: $stateParams.project_id },
            function (costCenters) {
                $scope.costCenters = costCenters;
                $scope.report.cost_center1 = -1;
            }, function () {
                toastr.error('Error to retrieve the cost centers, please contact the technical support');
            });
       

        /* Go to the next step */
        $scope.next = function () {
            switch ($scope.currentStep) {
                case 1:
                    _setReportSettings();
                    break;
                case 2:
                    _setReportLocation();
                    break;
                case 3:
                    _generateReport();
                    break;
            }
        };

        /* Back to previous step or cancel */
        $scope.back = function () {
            steppers.back();
            $scope.currentStep--;
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        /* Go to next step */
        function _nextStep() {
            $scope.currentStep++;
            steppers.clearError();
            steppers.next();

            if ($scope.currentStep === 3 && _isCopiedProjectsReport()) {
                /*GET COPIED PROJECTS*/
                WebApiService.genericController.query({ controller: 'Projects', action: 'CopiedProjects', domain_id: AuthService.getLoggedDomain(), project_id: $stateParams.project_id },
                    function (projects) {
                        $scope.projects = projects;
                    }, function () {
                        toastr.error('Error to retrieve the copied projects, please contact the technical support');
                    });
            }            
        };

        function _isCopiedProjectsReport() {
            return $scope.report.report_type.name.toLowerCase() == 'copied project inventory comparison';
        }

        /* Save information about settings */
        function _setReportSettings() {

            $scope.generateReportForm.$setSubmitted();

            if ($scope.generateReportForm.$valid) {
                _nextStep();
            } else {
                var error = false;
                for (var i = 0; i < fields.step1.length; i++) {
                    for (var prop in $scope.generateReportForm[fields.step1[i]].$error) {
                        error = true;
                        break;
                    }

                    if (error) {
                        steppers.error('Missing required fields');
                        return;
                    }

                    error = false;
                }
            }
        };

        /* Verifies if at least one location was selected */
        function _setReportLocation() {
            if ($scope.locations.selecteds.length > 0) {
                _nextStep();
            } else {
                steppers.error('At least one location must be selected');
            }
        };

        /* Generates the report */
        function _generateReport() {

            $scope.generateReportForm.$setSubmitted();

            if (!$scope.generateReportForm.$valid) {

                if (_isCopiedProjectsReport()) {
                    toastr.error('You must to select a project');
                }

                return;
            }

            ProgressService.blockScreen();
            WebApiService.genericController.save({ controller: 'Report', action: 'Item', domain_id: AuthService.getLoggedDomain(), project_id: $stateParams.project_id },
                angular.extend($scope.report, $stateParams, {
                    report_location: $scope.locations.selecteds,
                    project_domain_id: AuthService.getLoggedDomain()
                }), function () {
                    toastr.success('The report is being generated. You\'ll receive a notification when it\'s ready.');
                    ProgressService.unblockScreen();
                    $mdDialog.hide();
                }, function () {
                    toastr.error('Error to generate report, please contact the technical support');
                    ProgressService.unblockScreen();
                });
        }
    }]);