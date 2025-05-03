xPlanner.controller('AddDocAssociationCtrl', ['$scope', '$timeout', '$mdStepper', 'local', '$mdDialog', 'HttpService',
        'AuthService', 'GridService', 'WebApiService', 'toastr', 'ProgressService',
    function ($scope, $timeout, $mdStepper, local, $mdDialog, HttpService, AuthService, GridService, WebApiService, toastr, ProgressService) {

        $scope.params = local.params;
        $scope.alreadAssociated = local.documents[0].project_room_inventory;
        $scope.document = angular.copy(local.documents[0]);

        $scope.save = function () {

            ProgressService.blockScreen();

            angular.forEach(local.documents, function (doc) {
                doc.project_room_inventory = $scope.associations.map(function (item) {
                    //item.project_domain_id = item.domain_id;
                    item.linked_document = doc.id;
                    return item;
                });

                WebApiService.genericController.update({
                    controller: 'ProjectDocuments', action: 'Associations', domain_id: doc.project_domain_id, project_id:
                    doc.project_id, phase_id: doc.id
                }, doc, function () {
                    ProgressService.unblockScreen();
                    toastr.success('Document associations successfully established.');
                    $mdDialog.hide();
                }, function (error) {
                    ProgressService.unblockScreen();
                    toastr.error("Error to try establish the document associations, please contact the technical support.");
                });
            });
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    }]);