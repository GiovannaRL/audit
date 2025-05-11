xPlanner.controller('VendorDetailsCtrl', ['$scope', 'WebApiService', 'ProgressService', '$stateParams', 'toastr',
        'DialogService', '$state', 'AudaxwareDataService', 'AuthService',
    function ($scope, WebApiService, ProgressService, $stateParams, toastr, DialogService, $state, AudaxwareDataService, AuthService) {

        $scope.$emit('initialTab', 'vendors');

        $scope.contactType = 'vendor';
        $scope.params = $stateParams;

        function _getGenericParams(save) {
            return {
                controller: 'vendor',
                action: 'Item',
                domain_id: save ? AuthService.getLoggedDomain() : $scope.params.domain_id,
                project_id: $scope.params.vendor_id
            };
        };

        ProgressService.blockScreen();
        WebApiService.genericController.get(_getGenericParams(), function (data) {
            $scope.vendor = data;
            $scope.canModify = AuthService.getLoggedDomain() == 1 || AuthService.getLoggedDomain() === data.domain_id;
            ProgressService.unblockScreen();
        }, function () {
            ProgressService.unblockScreen();
            toastr.error('Error to retrieve vendor, please contact the technical support');
        });

        /* BEGIN - save vendor */
        function _save() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            $scope.detailsForm.$setSubmitted();

            if ($scope.detailsForm.$valid) {
                AudaxwareDataService.CheckHasToDuplicate($scope.vendor, 'vedor').then(function () {
                    WebApiService.genericController.update(_getGenericParams(true), $scope.vendor, function (data) {
                        if (data.vendor_id !== $scope.vendor_id || data.domain_id !== $scope.domain_id)
                            $state.go('assetsWorkspace.vendorsDetails', data);
                        toastr.success('Vendor saved');
                    });
                });
            } else {
                toastr.error('Please make sure you enter all the required fields');
            }
        };
        /* END - save vendor */


        function _back() {
            $state.go('assetsWorkspace.vendors');
        }

        /* BEGIN - delete vendor */
        function _del() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            DialogService.Confirm('Are you sure?', 'The vendor will be deleted permanently').then(function () {
                ProgressService.blockScreen();
                WebApiService.genericController.remove(_getGenericParams(), function (data) {
                    ProgressService.unblockScreen();
                    toastr.success("Vendor deleted");
                    _back();
                }, function (error) {
                    ProgressService.unblockScreen();
                    if (error.status === 409) toastr.info(error.data);
                    else toastr.success("Error to try delete vendor, please contact technical support");
                });
            });
        };
        /* END - delete vendor */

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