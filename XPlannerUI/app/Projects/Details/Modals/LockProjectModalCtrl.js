xPlanner.controller('LockProjectModalCtrl', ['$scope', '$mdDialog', 'local',
    function ($scope, $mdDialog, local) {

        $scope.data = {
            locked_date: new Date().toLocaleDateString(),
            locked_comment: local.data.locked_comment
        };

        $scope.lock = function () {
            $mdDialog.hide($scope.data);
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        }

    }]);