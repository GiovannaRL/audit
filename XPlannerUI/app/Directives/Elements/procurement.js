xPlanner.directive('awProcurement', ['WebApiService', 'ProgressService', 'AuthService',
    function (WebApiService, ProgressService, AuthService) {
        return {
            restrict: 'E',
            transclude: true,
            scope: {
                data: '=',
                columnName: '@',
                projectLevel: '=',
                title: '@',
                size: '@',
                infoMessage: '@',
                readonly: '@',
                costs: '='
            },
            link: function (scope, elem, attrs, ctrl) {

                //scope.$on('updateProgressBar', function (event, value) {
                //    if (value != undefined) {
                //        if (value.domain_id == undefined)
                //            value.domain_id = AuthService.getLoggedDomain();
                //        updateProgressBar(value);
                //    }
                //});

                scope.$watch(function () {
                    return scope.data;
                }, function (newValue) {
                    scope.size = scope.size || 100;
                    scope.readonly = scope.readonly || false;
                    updateProgressBar(scope.data);
                });

                function updateProgressBar(newValue) {
                    if (newValue && newValue.project_id && newValue.project_id != -1) {
                        var cost = 0;
                        if (scope.costs != undefined) {
                            if (scope.costs.actual == null)
                                scope.costs.actual = '0';
                            if (scope.costs.projected == null)
                                scope.costs.projected = '0';
                            var po_total = parseInt(scope.costs.actual.replace("$", "").replace(new RegExp(',', 'g'), ''));
                            var projected_cost = parseInt(scope.costs.projected.replace("$", "").replace(new RegExp(',', 'g'), ''));
                            cost = (po_total * 100) / projected_cost;
                        }
                        else {
                            scope.costs = { actual: '$0.00', projected: '$0.00', planned: '$0.00', delta: '$0.00', budget: '$0.00' };
                        }
                        
                        scope.progress_cost = cost;
                    }

                }
                
            },
            templateUrl: 'app/Directives/Elements/procurement.html'
        }
    }]);