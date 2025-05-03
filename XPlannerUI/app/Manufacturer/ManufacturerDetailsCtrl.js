xPlanner.controller('ManufacturerDetailsCtrl', ['$scope', 'WebApiService', '$stateParams', 'toastr', 'ProgressService',
        'DialogService', '$rootScope', '$state', 'AudaxwareDataService', 'AuthService',
    function ($scope, WebApiService, $stateParams, toastr, ProgressService, DialogService, $rootScope, $state,
        AudaxwareDataService, AuthService) {

        $scope.$emit('initialTab', 'manufacturers');

        $scope.params = $stateParams;
        $scope.contactType = 'manufacturer';

        function _getGenericParams(save) {
            return {
                controller: 'manufacturer',
                action: 'Item',
                domain_id: save ? AuthService.getLoggedDomain() : $scope.params.domain_id,
                project_id: $scope.params.manufacturer_id
            };
        };

        ProgressService.blockScreen();
        WebApiService.genericController.get(_getGenericParams(), function (data) {
            $scope.manufacturer = data;
            $scope.canModify = AuthService.getLoggedDomain() == 1 || AuthService.getLoggedDomain() === data.domain_id;
            ProgressService.unblockScreen();
        });

        function _save() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            $scope.detailsForm.$setSubmitted();

            if ($scope.detailsForm.$valid) {
                AudaxwareDataService.CheckHasToDuplicate($scope.manufacturer, 'manufacturer').then(function () {
                    WebApiService.genericController.update(_getGenericParams(true), $scope.manufacturer, function (data) {
                        if (data.manufacturer_id !== $scope.manufacturer_id || data.domain_id !== $scope.domain_id)
                            $state.go('assetsWorkspace.manufacturersDetails', data);
                        //$scope.$parent.$broadcast('ManufacturerUpdate', data);
                        toastr.success('Manufacturer saved');
                    });
                });
            } else {
                toastr.error('Please make sure you enter all the required fields');
            }
        };

        function _back() {
            $state.go('assetsWorkspace.manufacturers')
        };

        function _del() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            DialogService.Confirm('Are you sure?', 'The manufacturer will be deleted permanently').then(function () {
                ProgressService.blockScreen();
                WebApiService.genericController.remove(_getGenericParams(), function (data) {
                    ProgressService.unblockScreen();
                    toastr.success("Manufacturer deleted");
                    _back();
                }, function (error) {
                    ProgressService.unblockScreen();
                    if (error.status === 409) toastr.info(error.data);
                    else toastr.success("Error to try delete manufacturer, please contact technical support");
                });
            });
        };

        /* float buttons */
        var saveButton = {
            label: 'Save',
            icon: 'save',
            click: _save,
            showContidion: true
        };

        var deleteButton = {
            label: 'Delete',
            icon: 'delete_forever',
            click: _del,
            showContidion: true
        };

        var backButton = {
            label: 'Back to List',
            icon: 'reply',
            click: _back
        };

        $scope.buttons = [saveButton, deleteButton, backButton];
        $scope.buttons.state = true;
        /* end float button */
    }]);