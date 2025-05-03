xPlanner.controller('UploadAssetFilesCtrl', ['$window', '$scope', '$mdDialog', 'toastr', '$q', 'AuthService', 
    'HttpService', 'Upload', 'ProgressService', 'local', 'WebApiService',
    function ($window, $scope, $mdDialog, toastr, $q, AuthService, HttpService, Upload, ProgressService, local, WebApiService) {

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


        $scope.upload = function () {
            
            $scope.uploadForm.$setSubmitted();

            if ($scope.uploadForm.$valid) {

                var validated = true;
                var cutsheet = document.getElementById('cutsheetF').files;
                var cadblock = document.getElementById('cadblockF').files;
                var photo = document.getElementById('photoF').files;
                var revit = document.getElementById('revitF').files;

                if ($scope.show_firefox) {
                    if (cutsheet.length == 0 && cadblock.length == 0 && photo.length == 0 && revit.length == 0) {
                        validated = false;
                        toastr.error('Choose at least one file to upload');
                    }
                }
                else if (!$scope.files.cutsheet && !$scope.files.cadblock && !$scope.files.photo && !$scope.files.revit) {
                    validated = false;
                    toastr.error('Choose at least one file to upload');
                }
                if (!validated) {
                    return;
                }

                $scope.uploadForm.$setSubmitted();

                var file_names = [];
                var file_names_msg = [];
                var database_names = [];
                var extensions = [];

                var file_names_full = ['cutsheet', 'cadblock', 'photo', 'revit'];
                var database_names_full = ['cut_sheet', 'cad_block', 'photo', 'revit'];
                var extensions_full = ['pdf', 'dwg', 'jpg', 'rvt,rft,rfa'];

                for (var i = 0; i < file_names_full.length; i++) {
                    if ($scope.show_firefox) {
                        if (eval(file_names_full[i] + '.length') > 0) {
                            var arr_extension = eval(file_names_full[i] + '[0].name.split(".")');
                            var extension = arr_extension[arr_extension.length - 1];

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

                var finishUpload = function () {
                    $mdDialog.hide($scope.files);
                    ProgressService.unblockScreen();
                }
                ProgressService.blockScreen();
                var uploadFile = function (index) {
                    if (file_names.length <= index) {
                        finishUpload();
                        return;
                    }
                    Upload.upload({
                        url: HttpService.file_stream("upload", local.asset.domain_id, local.asset.asset_id, local.asset.asset_id + '.' + extensions[index], database_names[index], file_names_msg[index]),
                        data: { file: eval(file_names[index]) }
                    }).then(function (data) {
                        toastr.success(file_names_msg[index] + ' uploaded sucessfully');
                        uploadFile(index + 1);
                    },
                   function (error) {
                       var errorMessage = error && error.data && error.data.errorMessage;
                       toastr.error('Error trying to upload ' + file_names_msg[index] + ' file, '
                           + (errorMessage || 'please contact technical support'));
                       $scope.cancel();

                   })
                }
            } else {
                ProgressService.unblockScreen();
                toastr.error('Please make sure no file exceeds the size limit');
            }
            uploadFile(0);
        };

        $scope.cancel = function () {
            ProgressService.unblockScreen();
            $mdDialog.cancel();
            
        };
    }]);

