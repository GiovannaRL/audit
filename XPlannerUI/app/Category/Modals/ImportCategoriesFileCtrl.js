xPlanner.controller('ImportCategoriesFileCtrl', ['$scope', '$mdDialog', 'toastr', 'AuthService', 'Upload', 'HttpService',
        'ProgressService',
    function ($scope, $mdDialog, toastr, AuthService, Upload, HttpService, ProgressService) {

        $scope.import = function () {

            $scope.importForm.$setSubmitted();

            if ($scope.importForm.$valid) {
                ProgressService.blockScreen();
                Upload.upload({
                    url: HttpService.generic('categories', 'import', AuthService.getLoggedDomain()),
                    data: { file: $scope.file }
                }).then(function () {
                    toastr.success('The import is being generated. You\'ll receive a notification when it\'s ready.');
                    ProgressService.unblockScreen();
                    $mdDialog.hide();
                }, function (error) {
                    var errorMessage = error && error.data && error.data.errorMessage;
                    toastr.error(errorMessage || 'Error trying to import data, please contact technical support.');
                    ProgressService.unblockScreen();
                });
            } else {
                toastr.error('Please make sure you entered all the required fields.');
            }
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    }]);