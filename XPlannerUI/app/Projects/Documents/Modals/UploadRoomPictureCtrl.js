xPlanner.controller('UploadRoomPictureCtrl', ['$scope', '$mdDialog', 'toastr', 'AuthService', 'WebApiService',
    'ProgressService', 'local', 'UtilsService',
    function ($scope, $mdDialog, toastr, AuthService, WebApiService, ProgressService, local, UtilsService) {

        $scope.files = [];
        $scope.filesF = [];

        $scope.show_firefox = navigator.userAgent.indexOf('Firefox') != -1;
        if ($scope.show_firefox) {
            $scope.filesF.push({ id: 'photoFx0' });
        } else {
            $scope.files.push({ id: 'photo0' });
        }

        $scope.fileSizeLimit = 7; // megabytes

        var pictures = [];

        function getExtensionAndName(fullName) {

            var splited = fullName.split('.');

            return {
                extension: splited[splited.length - 1],
                name: splited.slice(0, splited.length - 1).join()
            };
        }

        function getBase64(file) {
            return new Promise(function (resolve, reject) {
                const reader = new FileReader();
                reader.readAsDataURL(file);
                reader.onload = function () {
                    var idx = reader.result.indexOf('base64,');
                    if (idx >= 0) {
                        resolve(reader.result.substring(idx + 7));
                    } else {
                        resolve(reader.result);
                    }
                };
                reader.onerror = function (error) { reject(error); };
            });
        }

        function uploadFiles() {

            WebApiService.genericController.save(angular.extend({
                controller: 'rooms', action: 'pictures',
                domain_id: AuthService.getLoggedDomain()
            }, local.params),
                { pictures: pictures }, function () {
                    toastr.success('Pictures successfully uploaded');
                    ProgressService.unblockScreen();
                    $mdDialog.hide();
                }, function (error) {
                    console.error(error);
                    toastr.error('Error to upload one or more pictures');
                    ProgressService.unblockScreen();
                });
        }

        function uploadNoFirefox(photosAdded) {

            var finalIdx = photosAdded.length - 1;

            pictures = [];
            photosAdded.forEach(function(file, idx) {
                getBase64(file.picture).then(function(base64File) {

                    var extensionName = getExtensionAndName(file.picture.name);

                    pictures.push({
                        fileExtension: extensionName.extension,
                        fileName: extensionName.name,
                        base64File: base64File
                    });

                    if (idx === finalIdx) {
                        uploadFiles();
                    }
                });
            });
        }

        function uploadFirefox(photosAdded) {

            var finalIdx = photosAdded.length - 1;

            pictures = [];

            photosAdded.forEach(function(_, idx) {
                var files = document.getElementById('photoFx' + idx).files;
                if (files && files.length > 0) {

                    getBase64(files[0]).then(function(base64File) {
                        var extensionName = getExtensionAndName(files[0].name);

                        pictures.push({
                            fileExtension: extensionName.extension,
                            fileName: extensionName.name,
                            base64File: base64File
                        });

                        if (idx === finalIdx) {
                            uploadFiles();
                        }
                    });
                }
            });
        }

        function getAddedFilesNoFirefox() {
            return $scope.files.filter(function (item) { return item.picture; });
        }

        function getAddedFilesFirefox() {
            return $scope.filesF.filter(function (item, idx) {
                var elem = document.getElementById('photoFx' + idx);
                return elem && elem.files && elem.files.length > 0;
            });
        }

        $scope.upload = function () {

            $scope.uploadForm.$setSubmitted();

            if ($scope.uploadForm.$valid) {
                var photosAdded;
                if ($scope.show_firefox) {
                    photosAdded = getAddedFilesFirefox();
                } else {
                    photosAdded = getAddedFilesNoFirefox();
                }

                if (!photosAdded || photosAdded.length <= 0) {
                    validated = false;
                    toastr.error('Choose at least one file to upload');
                    return;
                }

                ProgressService.blockScreen();
                if ($scope.show_firefox) {
                    uploadFirefox(photosAdded);
                } else {
                    uploadNoFirefox(photosAdded);
                }
            } else {
                toastr.error('Please make sure no file exceeds the size limit');
            }
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.addMore = function () {

            var empty = null;
            if (!$scope.show_firefox) {
                empty = UtilsService.GetEmptyFileNoFirefox($scope.files);
            } else {
                empty = UtilsService.GetEmptyFileFirefox($scope.filesF);
            }

            if (!empty) {
                if ($scope.show_firefox) {
                    $scope.filesF.push({ id: 'photoFx' + $scope.filesF.length });
                }
                else {
                    $scope.files.push({ id: 'photo' + $scope.files.length });
                }
            } else {
                toastr.info('You must fill all the current pictures in order to add a new one');
            }
        }
    }]);