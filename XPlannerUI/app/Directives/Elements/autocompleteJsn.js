xPlanner.directive('awAutocompleteJsn', ['WebApiService', '$mdUtil', '$log', 'AuthService', 'DialogService', function (WebApiService, $mdUtil, $log, AuthService, DialogService) {
    return {
        restrict: 'E',
        scope: {
            endpoint: '@',
            controllerName: '@',
            queryControllerAction: '@',
            saveControllerAction: '@',
            params: '=',
            selectedText: '=?',
            selectedId: '=',
            selectedItem: '=?',
            selectedDomain: '=?',
            textProperty: '@', //Item property that displayes the text (default is name)
            textProperty2: '@',
            textPropertySeparator: '@',
            idProperty: '@', //Item property that has the selected item id (default is id)
            domainProperty: '@',
            label: '@',
            max: '@',
            min: '@',
            placeholder: '@',
            flex: '@',
            addOption: '=',
            watchObject: '=',
            watchedObjectId: '='
        },
        require: ['^?mdInputContainer', '?ngModel'],
        link: function (scope, elem, attrs, ctrl) {

            if (scope.watchObject && attrs.watchedObjectId === undefined) {
                $log.error('[audaxware autocomplete] watch-object property requires a parent-object-id property');
                return;
            }

            var controller = WebApiService[scope.endpoint || 'genericController'];
            if (typeof (scope.domainProperty) == "undefined")
                scope.domainProperty = "domain_id";
            if (typeof (scope.idProperty) == "undefined")
                scope.idProperty = "id";
            if (typeof (scope.textProperty) == "undefined")
                scope.textProperty = "name";

            scope.data = [];
            var params = angular.extend({}, scope.params);
            params.controller = scope.controllerName;

            function createFilterFor(query) {
                var lowercaseQuery = query.toLowerCase();
                return function filterFn(item) {
                    return (item[scope.textProperty].toLowerCase().indexOf(lowercaseQuery) !== -1) || (scope.textProperty2 && item[scope.textProperty2].toLowerCase().indexOf(lowercaseQuery) !== -1);
                };
            }

            function selectItem() {
                var found = scope.data.find(
                    function (item) {                        
                        return item[scope.idProperty] == ((scope.selectedItem && scope.selectedItem[scope.idProperty]) || scope.selectedId)
                            && item[scope.domainProperty] == ((scope.selectedItem && scope.selectedItem[scope.domainProperty]) || scope.selectedDomain);
                    }
                );
                if (typeof (found) != "undefined") {
                    var ok = true;
                    if (scope.selectedItem != null && scope.selectedItem.Id == found.Id) {
                        if (scope.selectedItem.utility1 != found.utility1) {
                            ok = false;
                        }
                        else if (scope.selectedItem.utility2 != found.utility2) {
                            ok = false;
                        }
                        else if (scope.selectedItem.utility3 != found.utility3) {
                            ok = false;
                        }
                        else if (scope.selectedItem.utility4 != found.utility4) {
                            ok = false;
                        }
                        else if (scope.selectedItem.utility5 != found.utility5) {
                            ok = false;
                        }
                        else if (scope.selectedItem.utility6 != found.utility6) {
                            ok = false;
                        }
                    }
                    if (ok) {
                        _setProperties(found);
                    }
                    
                }
            }

            var getItems = function () {
                params.action = scope.queryControllerAction || 'All/' + AuthService.getLoggedDomain();
                controller.query(params, function (data) {
                    scope.data = data;
                    selectItem();
                    
                }, function (error) {
                    console.log('an error occurred on "autocomplete" directive');
                });
            }
            //getItems();

            scope.searchText = null;
            scope.querySearch = function (query) {
                var results = query ? scope.data.filter(createFilterFor(query)) : [];
                if (!scope.addOption) {
                    if (scope.selectedItem && scope.textProperty) {
                        scope.selectedItem = {};
                        scope.selectedItem[scope.textProperty] = query;
                    }
                    else if (scope.selectedItem) {
                        scope.selectedItem = query;
                    }
                    else {
                        scope.selectedText = query;
                    }
                }
                return results;
            }

            scope.createNew = function (text) {
                var data = {};
                data[scope.textProperty] = text;

                if (AuthService.getLoggedUserType() == "3") {
                    DialogService.ViewersChangesModal();
                    return;
                }

                DialogService.openModal('app/Assets/Modals/AddJSN.html', 'AddJSNCtrl', data).then(function (data) {
                    _setProperties(data);
                    getItems();
                }, function (error) {
                });
            }


            function _setProperties(item) {
                scope.selectedDomain = item[scope.domainProperty];
                scope.selectedId = item[scope.idProperty];
                scope.selectedText = item[scope.textProperty];
                scope.selectedItem = item;
            }

            scope.checkAdd = function (text) {
                if (text) {
                    var found = scope.data.find(
                        function (item) {
                            return item[scope.textProperty].toLowerCase() === text.toLowerCase();
                        }
                        );

                    if (typeof (found) != "undefined") {
                        // No need to add
                        return false;
                    }

                    return true;
                }

                return false;
            }

            var ngModelCtrl = ctrl[1];
            var containerCtrl = ctrl[0];
            if (!containerCtrl || !ngModelCtrl) return;

            containerCtrl.element.addClass('aw-autocomplete');

            /* Validation messages */
            var isErrorGetter = containerCtrl.isErrorGetter || function () {
                return ngModelCtrl.$invalid && (
                  ngModelCtrl.$touched ||
                  (ngModelCtrl.$$parentForm && ngModelCtrl.$$parentForm.$submitted)
                );
            };

            scope.$watch(function () { return !scope.watchObject || scope.watchedObjectId; }, function (newValue) {
                if (newValue)
                    getItems();
            });

            scope.$watch(isErrorGetter, function (newValue) {
                containerCtrl.setInvalid(newValue);
            });

            scope.$watch(function () { return scope.selectedItem || scope.selectedText; }, function (newValue, oldValue) {
                if (!newValue != !oldValue) containerCtrl.element.toggleClass('md-input-has-value');
                //ngModelCtrl.$setViewValue(newValue)
                $element = angular.element(elem);
                $element.val(newValue);
            });

            elem.focusout(function () {
                if (containerCtrl && ngModelCtrl.$untouched) {
                    ngModelCtrl.$setTouched();
            //        ngModelCtrl.$setDirty();
                }
            });

            scope.$on('$destroy', function () {
                containerCtrl.element.removeClass('aw-autocomplete');
                containerCtrl.element.removeClass('md-input-has-value');
            });
        },
        templateUrl: 'app/Directives/Elements/autocomplete.html'
    }
}]);