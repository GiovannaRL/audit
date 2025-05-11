xPlanner.controller('RoomDocumentsTabCtrl', ['$scope', 'AuthService', 'HttpService', 'WebApiService', 'ProgressService', 'toastr', 'FileService',
        '$stateParams', 'DialogService',
    function ($scope, AuthService, HttpService, WebApiService, ProgressService, toastr,
        FileService, $stateParams, DialogService) {

        $scope.$emit('detailsParams', angular.copy($stateParams));

        $scope.photosFileMatrix = [];

        function loadPictures() {

            WebApiService.genericController.query({
                controller: 'rooms', action: 'pictures', domain_id: AuthService.getLoggedDomain(),
                project_id: $stateParams.project_id, phase_id: $stateParams.phase_id,
                department_id: $stateParams.department_id, room_id: $stateParams.room_id
            }, function (data) {

                $scope.photosFileMatrix = [];
                if (data && data.length > 0) {
                    var row = -1;
                    data.forEach(function (p, idx) {
                        if (idx % 4 === 0) {
                            $scope.photosFileMatrix.push([]);
                            row++;
                        }
                        $scope.photosFileMatrix[row].push({ id: p.id, file: HttpService.generic('filestream', 'file', AuthService.getLoggedDomain(), p.blobFilename, 'photo') });
                    });
                }
            }, function () {
                toastr.error('Error to try get images files from room');
            });
        }

        $scope.addPicture = function () {
            DialogService.openModal('app/Projects/Documents/Modals/UploadRoomPicture.html',
                'UploadRoomPictureCtrl', { params: $stateParams }).then(function () {
                    loadPictures();
                });
        }

        loadPictures();

        $scope.deletePicture = function (photo) {

            DialogService.Confirm('Delete Picture', 'The picture will be deleted permanently. Are you sure?')
                .then(function () {
                    
                    WebApiService.remove_room_picture.remove({
                        domain_id: AuthService.getLoggedDomain(),
                        project_id: $stateParams.project_id, phase_id: $stateParams.phase_id,
                        department_id: $stateParams.department_id, room_id: $stateParams.room_id,
                        filename: photo.id
                    }, function () {
                        loadPictures();
                        toastr.success('Photo Deleted!');
                        ProgressService.unblockScreen();
                    }, function (error) {
                        console.error(error);
                        toastr.error('Error to try delete picture');
                        ProgressService.unblockScreen();
                    });
                });
        }
    }]);