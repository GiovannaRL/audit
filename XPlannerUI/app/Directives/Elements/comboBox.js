xPlanner.directive('awComboBox', ['WebApiService', '$filter', '$log', 'ProgressService', '$parse',
    function (WebApiService, $filter, $log, ProgressService, $parse) {
        return {
            restrict: 'A',
            scope: {
                endpoint: '@',
                textProperty: '@',
                textProperty2: '@',
                textPropertySeparator: '@',
                idProperty: '@',
                domainProperty: '@',
                selectedId: '=?',
                selectedDomain: '=?',
                selectedItem: '=?',
                label: '@',
                params: '=',
                controllerName: '@',
                queryControllerAction: '@',
                filterName: '@',
                orderBy: '@',
                watchObject: '=',
                watchedObjectId: '=',
                name: '@',
                form: '='
            },
            require: ['?ngModel'],
            link: function (scope, elem, attrs, ctrl) {

                scope.isRequired = attrs.required;

                if (scope.watchObject && attrs.watchedObjectId === undefined) {
                    $log.error('[audaxware combobox] watch-object property requires a parent-object-id property');
                    return;
                }

                if (!scope.name) {
                    $log.error('[audaxware combobox] name property is required');
                    return;
                }

                if (scope.isRequired && !scope.form) {
                    $log.error('[audaxware combobox] when using required property the form property must be informed');
                    return;
                }

                scope.domainProperty = attrs.noDomainProperty != undefined ? null : (scope.domainProperty || "domain_id");

                function _setProperties(item) {
                    scope.selectedDomain = item[scope.domainProperty];
                    scope.selectedId = item[scope.idProperty];
                    scope.selectedText = item[scope.textProperty];
                    scope.selectedItem = item;
                }

                function selectItem() {
                    var found = scope.data.find(
                        function (item) {
                            var val1 = item[scope.idProperty];
                            var val2 = (scope.selectedId || (scope.selectedItem && scope.selectedItem[scope.idProperty]));
                            if (typeof val2 == "string") {
                                var val1 = item[scope.idProperty].trim();
                                var val2 =(scope.selectedId || (scope.selectedItem && scope.selectedItem[scope.idProperty])).trim();
                            }

                            return val1 === val2
                                && (attrs.noDomainProperty != undefined || (item[scope.domainProperty] === (scope.selectedDomain || (scope.selectedItem && scope.selectedItem[scope.domainProperty]))));
                        }
                        );
                    if (typeof (found) != "undefined") {
                        _setProperties(found);
                    }
                }

                scope.data = [];
                var controller = WebApiService[scope.endpoint || 'genericController'];
                var params = angular.extend({}, scope.params);
                params.controller = scope.controllerName;

                params.action = scope.queryControllerAction || 'All';

                function getItems() {
                    ProgressService.blockScreen();
                    controller.query(params, function (data) {
                        if (scope.filterName)
                            data = data.map(function (i) { i[scope.textProperty] = $filter(scope.filterName)(i[scope.textProperty]); return i; });
                        if (scope.orderBy)
                            data = data.sort(function (a, b) { return a[scope.orderBy].toString().toLowerCase() <= b[scope.orderBy].toString().toLowerCase() ? -1 : 1 });

                        scope.data = data;
                        selectItem();
                        ProgressService.unblockScreen();
                    }, function (error) {
                        ProgressService.unblockScreen();
                        console.log('an error occurred on "combobox" directive');
                    });
                };

                scope.change = function () {
                    scope.selectedId = scope.selectedItem[scope.idProperty];
                    scope.selectedDomain = scope.selectedItem[scope.domainProperty];
                }

                scope.$watch(function () { return !scope.watchObject || scope.watchedObjectId; }, function (newValue) {
                    if (newValue) {
                        getItems();
                    }
                });

                /* Validation */
                var ngModelCtrl = ctrl[0];
                if (!ngModelCtrl) return;

                /* Validation messages */
                var isErrorGetter = function () {
                    return ngModelCtrl.$invalid && (
                      ngModelCtrl.$touched ||
                      (ngModelCtrl.$$parentForm && ngModelCtrl.$$parentForm.$submitted)
                    );
                };

                scope.$watch(isErrorGetter, function (newValue) {
                    if (scope.form)
                        scope.form[scope.name].$error = { required: newValue };
                });

                scope.$watch(function () { return scope.selectedItem; }, function (newValue, oldValue) {
                    if (newValue)
                        ngModelCtrl.$setViewValue(newValue);
                });

                elem.focusout(function () {
                    if (ngModelCtrl.$untouched) {
                        ngModelCtrl.$setTouched();
                        ngModelCtrl.$setDirty();
                    }
                });
            },
            templateUrl: 'app/Directives/Elements/comboBox.html'
        }
    }]);