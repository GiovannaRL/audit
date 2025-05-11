xPlanner.controller('UploadPOFilesCtrl', ['$scope', '$mdDialog', 'toastr', '$q', 'AuthService', 'HttpService', 'Upload', 'ProgressService', 'local', 'WebApiService',
    function ($scope, $mdDialog, toastr, $q, AuthService, HttpService, Upload, ProgressService, local, WebApiService) {

        $scope.files = {};

        $scope.show_firefox = false;

        if (navigator.userAgent.indexOf('Firefox') != -1) {
            $scope.show_firefox = true;
        }

        WebApiService.genericController.get({ controller: "filestream", action: "MaxFileSize", domain_id: 1 }, function (data) {
            $scope.fileSizeLimit = data.text;
            },
            function () {
                $scope.fileSizeLimit = 4;
            }
        );

        $scope.save = function () {
            
            $scope.uploadForm.$setSubmitted();

            if ($scope.uploadForm.$valid) {

                var validated = true;
                var quote = document.getElementById('quoteF').files;
                var po = document.getElementById('poF').files;

                if ($scope.show_firefox) {
                    if (quote.length == 0 && po.length == 0) {
                        validated = false;
                        toastr.error('Choose at least one file to upload');
                    }
                    else if (!local.params.po_number && po.length > 0) {
                        validated = false;
                        toastr.error('In order to upload a PO file you need to Receive PO first.');
                    }
                    else if (!local.params.quote_number && quote.length > 0) {
                        validated = false;
                        toastr.error('In order to upload a Quote file you need to Receive Quote first.');
                    }
                }
                else if (!$scope.files.quote && !$scope.files.po) {
                    validated = false;
                    toastr.error('Choose at least one file to upload');
                }
                else if (!local.params.po_number && $scope.files.po) {
                    validated = false;
                    toastr.error('In order to upload a PO file you need to Receive PO first.');
                }
                else if (!local.params.quote_number && $scope.files.quote) {
                    validated = false;
                    toastr.error('In order to upload a Quote file you need to Receive Quote first.');
                }

                if (validated) {
                    $scope.uploadForm.$setSubmitted();

                    var file_names = [];
                    var file_names_msg = [];
                    var database_names = [];
                    var extensions = [];

                    var file_names_full = ['quote', 'po'];
                    var database_names_full = ['quote', 'po'];
                    var extensions_full = ['pdf', 'pdf'];

                    for (var i = 0; i < file_names_full.length; i++) {
                        if ($scope.show_firefox) {
                            if (eval(file_names_full[i] + '.length') > 0) {
                                var arr_extension = eval(file_names_full[i] + '[0].name.split(".")');
                                var extension = arr_extension[arr_extension.length - 1].toLowerCase();

                                if (extensions_full[i].indexOf(extension, 0) >= 0) {
                                    file_names.push(file_names_full[i]);
                                    file_names_msg.push(file_names_full[i]);
                                    database_names.push(database_names_full[i]);
                                    extensions.push(extension);
                                }
                            }
                        }
                        else {
                            if (eval('$scope.files.' + file_names_full[i]) != undefined) {
                                var arr_extension = eval('$scope.files.' + file_names_full[i] + '.name.split(".")');
                                var extension = arr_extension[arr_extension.length - 1].toLowerCase();

                                if (extensions_full[i].indexOf(extension, 0) >= 0) {
                                    file_names.push('$scope.files.' + file_names_full[i]);
                                    file_names_msg.push(file_names_full[i]);
                                    database_names.push(database_names_full[i]);
                                    extensions.push(extension);
                                }
                            }
                        }
                    }

                    if (file_names.length > 0) {
                        ProgressService.blockScreen();
                        Upload.upload({
                            url: HttpService.file_stream("upload", local.params.domain_id, local.params.project_id, local.params.po_id + '.' + extensions[0], database_names[0], file_names_msg[0]),
                            data: { file: eval(file_names[0]) }
                        }).then(function (data) {
                            toastr.success(file_names_msg[0] + ' file Added');

                            if (file_names.length > 1) {
                                Upload.upload({
                                    url: HttpService.file_stream("upload", local.params.domain_id, local.params.project_id, local.params.po_id + '.' + extensions[1], database_names[1], file_names_msg[1]),
                                    data: { file: eval(file_names[1]) }
                                }).then(function (data) {
                                    toastr.success(file_names_msg[1] + ' file Added');
                                    ProgressService.unblockScreen();
                                    $mdDialog.hide($scope.files);
                                }, function (error) {
                                    var errorMessage = error && error.data && error.data.errorMessage;
                                    ProgressService.unblockScreen();
                                    toastr.error('Error trying to add ' + file_names_msg[1] + ' file, '
                                        + (errorMessage || 'please contact technical support'));
                                });
                            }
                            else {
                                $mdDialog.hide($scope.files);
                                ProgressService.unblockScreen();
                            }

                        }, function (error) {
                            var errorMessage = error && error.data && error.data.errorMessage;
                            toastr.error('Error trying to add ' + file_names_msg[0] + ' file, '
                                + (errorMessage || 'please contact technical support'));
                        });
                    }
                }
            } else {
                toastr.error('Please make sure no file exceeds the limit size');
            }


            //if ($scope.files.quote == undefined && $scope.files.po == undefined) {
            //    toastr.error('Choose at least one file to upload');
            //}
            //else {
            //    $scope.uploadForm.$setSubmitted();

            //    var file_names = ['quote', 'po'];
            //    var database_names = ['quote', 'po'];
            //    var extensions = ['pdf', 'pdf'];

            //    for (var i = 0; i < file_names.length; i++) {
            //        if (eval('$scope.files.' + file_names[i]) != undefined) {

            //            var arr_extension = eval('$scope.files.' + file_names[i] + '.name.split(".")');
            //            var extension = arr_extension[arr_extension.length - 1];

            //            if (extensions[i].indexOf(extension.toLowerCase(), 0) >= 0) {

            //                //if ($scope.uploadForm.$valid) {
            //                //this is necessary so the next call waits for the last one to complete
            //                var deferred = $q.defer();

            //                ProgressService.blockScreen();
            //                Upload.upload({
            //                    url: HttpService.file_stream("upload", local.params.domain_id, local.params.project_id, local.params.po_id + '.' + extension, database_names[i], file_names[i]),
            //                    data: { file: eval('$scope.files.' + file_names[i]) }
            //                }).then(function (data) {
            //                    //this is necessary so the next call waits for the last one to complete
            //                    deferred.resolve();
            //                    ProgressService.unblockScreen();
            //                    toastr.success('File(s) Added');
            //                    $mdDialog.hide($scope.files);
            //                }, function (error) {
            //                    deferred.reject(error);
            //                    ProgressService.unblockScreen();
            //                    toastr.error('Error to try add file(s), please contact technical support');
            //                });
            //                //}
            //            }
            //            else {
            //                toastr.error('Invalid extension for ' + file_names[i]);
            //            }
            //        }
            //    }
            //}
        };

        $scope.close = function () {
            $mdDialog.cancel();
        };
    }]);

