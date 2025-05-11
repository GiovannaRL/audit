xPlanner.controller('UploadDashboardFilesCtrl', ['$scope', '$mdDialog', 'toastr', 'AuthService', 'HttpService', 'Upload', 'ProgressService', 'local',
    function ($scope, $mdDialog, toastr, AuthService, HttpService, Upload, ProgressService, local) {

        $scope.files = {};

        $scope.save = function () {

            if ($scope.files.powerbi == undefined) {
                toastr.error('Choose a Power BI file to upload');
            }
            else {
                $scope.uploadForm.$setSubmitted();

                var file_names = ['powerbi'];
                var database_names = ['powerbi'];
                var extensions = ['pbix'];
                
                ProgressService.blockScreen();
                Upload.upload({
                    //TODO: TROCAR PROJECT_ID
                    url: HttpService.file_stream_powerbi("upload", AuthService.getLoggedDomain(), 115, $scope.files.saveAs),
                    data: { file: $scope.files.powerbi, filename: $scope.files.saveAs }
                }).then(function (data) {
                    ProgressService.unblockScreen();
                    toastr.success('Dashboard added successfully');
                    $mdDialog.hide($scope.files);
                }, function (error) {
                    var errorMessage = error && error.data && error.data.errorMessage;
                    ProgressService.unblockScreen();
                    toastr.error(errorMessage || 'Error to add Dashboard. Please notice only DirectQuery dashboards are supported (Import option is not supported). If you are using DirectQuery, please contact Audaxware Technical Support');
                });
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);

