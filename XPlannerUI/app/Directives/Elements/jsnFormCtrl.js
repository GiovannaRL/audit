xPlanner.directive('awJsnForm', ['AuthService', 'WebApiService', function (AuthService, WebApiService) {
    return {
        restrict: 'E',
        transclude: true,
        scope: {
            asset: '=',
            new: '@',
            name: '=',
            inventoryEditing: '@'
        },
        link: function (scope, elem, attrs, ctrl) {

            var originalJsn = angular.copy(scope.asset);

            scope.controllerParamsCode = {
                domain_id: AuthService.getLoggedDomain()
            }


            scope.utility1 = [{ code: 'N/A', description: 'N/A' }, { code: 'A', description: 'Hot and cold water' }, { code: 'B', description: 'Cold water and drain' }, { code: 'C', description: 'Hot water and drain' }, { code: 'D', description: 'Cold and hot water and drain' }, { code: 'E', description: 'Treated water and drain' }, { code: 'F', description: 'Cold, hot and treated water and drain' }, { code: 'G', description: 'Cold and treated water and drain' }, { code: 'H', description: 'Hot and treated water and drain' }, { code: 'I', description: 'Drain only' }, { code: 'J', description: 'Cold water only' }];
            scope.utility2 = [{ code: 'N/A', description: 'N/A' }, { code: 'A', description: '120 volt, conventional outlet' }, { code: 'B', description: '120 volt, special outlet' }, { code: 'C', description: '208/220 volt' }, { code: 'D', description: '120 and 208/220 volt' }, { code: 'E', description: '440 volt, 3 phase' }, { code: 'F', description: 'Special electrical requirements (includes, but is not limited to emergency power, multiple power connections, etc.)' }, { code: 'G', description: '208/220 volt, 3 phase' }];
            scope.utility3 = [{ code: 'N/A', description: 'N/A' }, { code: 'A', description: 'Oxygen' }, { code: 'B', description: 'Vacuum' }, { code: 'C', description: 'Air, low pressure (dental or non-medical)' }, { code: 'D', description: 'Air, high pressure (dental or non-medical)' }, { code: 'E', description: 'Oxygen and medical air' }, { code: 'H', description: 'Oxygen, vacuum and medical air' }, { code: 'J', description: 'Vacuum and HP air' }, { code: 'K', description: 'Medical air' }];
            scope.utility4 = [{ code: 'N/A', description: 'N/A' }, { code: 'A', description: 'Steam' }, { code: 'B', description: 'Nitrogen gas' }, { code: 'C', description: 'Nitrous oxide' }, { code: 'D', description: 'Nitrogen and nitrous oxide gas' }, { code: 'E', description: 'Carbon dioxide gas' }, { code: 'F', description: 'Liquid carbon dioxide' }, { code: 'G', description: 'Liquid nitrogen' }, { code: 'H', description: 'Instrument Air' }];
            scope.utility5 = [{ code: 'N/A', description: 'N/A' }, { code: 'A', description: 'Natural gas' }, { code: 'B', description: 'Liquid propane gas' }, { code: 'C', description: 'Methane' }, { code: 'D', description: 'Butane' }, { code: 'E', description: 'Propane' }, { code: 'F', description: 'Hydrogen gas' }, { code: 'G', description: 'Reserved' }, { code: 'H', description: 'Acetylene gas' }];
            scope.utility6 = [{ code: 'N/A', description: 'N/A' }, { code: 'A', description: 'Earth ground' }, { code: 'B', description: 'Lead lined walls' }, { code: 'C', description: 'Remote alarm ground' }, { code: 'D', description: 'Empty conduit with pull cord' }, { code: 'E', description: 'Vent to atmosphere' }, { code: 'F', description: 'Special gas requirements' }, { code: 'G', description: 'Lead lined walls; empty conduit with pull cord' }, { code: 'H', description: 'RF/Magnetic shielding' }, { code: 'J', description: 'Wall/ceiling support required' }, { code: 'K', description: 'Empty conduit/pull cord & wall/ceiling support required' }, { code: 'L', description: 'Wall/ceiling support required; CAT 6 wire to nearest Telecommunications room'}, { code: 'M', description: 'Earth ground and wall/ceiling support required' }, { code: 'P', description: 'Lead lined walls and wall/ceiling support required' }, { code: 'T', description: 'CAT 6 wire to nearest Telecommunications Room' }];

            WebApiService.genericController.query({ controller: 'jsn', action: 'All', domain_id: AuthService.getLoggedDomain() },
            function (jsn) {
                scope.anotherJSN = jsn.filter(function (item) { return item.domain_id == AuthService.getLoggedDomain() })
                    .map(function (item) { return item.jsn_code });

            });
            if (scope.inventoryEditing) {
                scope.new = true;   
            }

            scope.toggleLock = function () {
                scope.asset.jsn.jsn_ow = !scope.asset.jsn.jsn_ow;
                if (!scope.asset.jsn.jsn_ow) {
                    scope.jsnForm.$pristine = true;
                    scope.asset = angular.copy(originalJsn);
                    scope.asset.jsn.jsn_ow = false;
                }
            };
        },
        templateUrl: 'app/Directives/Elements/jsnForm.html'
    }
}]);
