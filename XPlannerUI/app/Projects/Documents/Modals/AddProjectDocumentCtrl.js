xPlanner.controller('AddProjectDocumentCtrl', ['$scope', '$mdDialog', 'toastr', 'AuthService', 'HttpService', 'Upload',
        'ProgressService', 'local', 'WebApiService', 'DocumentStatusList',
    function ($scope, $mdDialog, toastr, AuthService, HttpService, Upload, ProgressService, local, WebApiService, DocumentStatusList) {

        $scope.document = angular.copy(local.document || {});
        $scope.edit = local.edit;

        WebApiService.genericController.query({ controller: 'documentTypes', action: 'All' }, function (types) {
            $scope.documentTypes = types;

            if (local.edit) {
                $scope.document.document_types = types.find(function (t) { return t.id === local.document.document_types.id });
            }
        });

        $scope.statusList = angular.copy(DocumentStatusList);

        function close(doc) {
            $mdDialog.hide(doc);
        };

        $scope.upload = function () {

            $scope.addDocumentForm.$setSubmitted();

            if ($scope.document.doc) {
                var arr_extension = $scope.document.doc.name.split(".");
                $scope.document.file_extension = arr_extension[arr_extension.length - 1]
            }

            if ($scope.addDocumentForm.$valid) {

                if ($scope.document.doc && $scope.document.document_types.name === 'Shop Drawing' && $scope.document.doc.type !== 'application/pdf') {
                    toastr.error("When selecting the type 'Shop Drawing' only pdf files are allowed.");
                    return;
                }

                ProgressService.blockScreen();

                WebApiService.genericController[local.edit ? 'update' : 'save']({
                    controller: 'ProjectDocuments', action: 'Item', domain_id:
                        AuthService.getLoggedDomain(), project_id: local.params.project_id
                }, $scope.document, function (document) {
                    if ($scope.document.doc) {
                        Upload.upload({
                            url: HttpService.generic("ProjectDocuments", "Upload", AuthService.getLoggedDomain(), local.params.project_id, document.id, $scope.document.file_extension),
                            data: { file: $scope.document.doc },
                            headers: {
                                Authorization: "Bearer " + AuthService.getAccessToken()
                            }
                        }).then(function (data) {
                            ProgressService.unblockScreen();
                            toastr.success('File Uploaded');
                            close($scope.document);
                        }, function (error) {
                            var errorMessage = error && error.data && (error.data.exceptionMessage || error.data.errorMessage);
                            ProgressService.unblockScreen();
                            toastr.error(errorMessage || 'Error to try upload document, please contact technical support.');
                        });
                    } else if (local.edit) {
                        ProgressService.unblockScreen();
                        toastr.success('Document successfully saved.');
                        close($scope.document);
                    } else {
                        ProgressService.unblockScreen();
                        toastr.error('You need to select an document to upload.');
                        close($scope.document);
                    }
                }, function () {
                    ProgressService.unblockScreen();
                    toastr.error('Error to try save document, please contact technical support');
                });
            }
        };

        $scope.fireFileUpload = function () {

            if (!$scope.document.document_types) {
                toastr.info('You need to select the document type to enable uploading document.');
                return;
            }

            $('#file').trigger('click');
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    }]);