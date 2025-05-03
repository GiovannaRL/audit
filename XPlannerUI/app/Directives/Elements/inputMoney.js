xPlanner.directive('awInputMoney', [function () {
    return {
        restrict: 'E',
        scope: {
            ngModel: '='
        },
        link: function (scope, elem, attrs, ctrl) {

            scope.focus = function () {
                if (scope.ngModel && typeof (scope.ngModel) === "string") {
                    scope.ngModel = scope.ngModel.replace(/\$\s*/g, '');
                }
            }

            scope.blur = function () {
                if (scope.ngModel !== null && scope.ngModel !== undefined) {
                    scope.ngModel = '$' + scope.ngModel;
                    if (scope.ngModel.indexOf('.') == -1) {
                        scope.ngModel = scope.ngModel + '.00';
                    }
                }
            }
        },
        templateUrl: 'app/Directives/Elements/inputMoney.html'
    }
}]);