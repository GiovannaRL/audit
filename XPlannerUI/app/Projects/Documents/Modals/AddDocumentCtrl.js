xPlanner.controller('AddDocumentCtrl', ['$scope', '$mdDialog', 'toastr', 'AuthService', 'HttpService', 'Upload', 'ProgressService', 'local',
    function ($scope, $mdDialog, toastr, AuthService, HttpService, Upload, ProgressService, local) {

        $scope.document = {};

        $scope.add = function () {

            $scope.addDocumentForm.$setSubmitted();

            var arr_extension = $scope.document.doc.name.split('.');
            var extension = arr_extension[arr_extension.length-1];

            if ($scope.addDocumentForm.$valid) {
               ProgressService.blockScreen();
                Upload.upload({
                    url: HttpService.phase_documents('upload', AuthService.getLoggedDomain(), local.params.project_id, local.params.phase_id, $scope.document.saveAs, extension),
                    data: { file: $scope.document.doc, filename: $scope.document.saveAs }
                }).then(function () {
                    ProgressService.unblockScreen();
                    toastr.success('File Added');
                    //$mdDialog.close(data);
                    $mdDialog.hide($scope.document);
                }, function (error) {
                    ProgressService.unblockScreen();
                    toastr.error('Error to try add document, please contact technical support');
                });
            }
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
}]);