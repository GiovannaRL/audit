xPlanner.controller('UploadAssetsCtrl', ['$scope', '$mdDialog', 'toastr', '$q', 'AuthService', 'HttpService', 'Upload', 'ProgressService', 'local',
    function ($scope, $mdDialog, toastr, $q, AuthService, HttpService, Upload, ProgressService, local) {

        $scope.files = {};

        $scope.save = function () {

                $scope.uploadForm.$setSubmitted();

                    ProgressService.blockScreen();

                    Upload.upload({
                        url: HttpService.generic("Assets", "Import", AuthService.getLoggedDomain()),
                        data: { file: $scope.files.assets }
                    }).then(function (data) {
                        ProgressService.unblockScreen();
                        $mdDialog.hide();
                        toastr.success('The import is being generated. You\'ll receive a notification when it\'s ready.');
                    }, function (error) {
                        var errorMessage = error && error.data && error.data.errorMessage;
                        ProgressService.unblockScreen();
                        toastr.error(errorMessage || 'Error trying to upload file, please contact technical support');
                    });

            }

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);

