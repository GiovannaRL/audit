xPlanner.directive('awAddOption', ['OptionTypes', 'WebApiService', 'toastr', 'ProgressService',
        '$filter', 'AssetOptionService', 'FileService', 'UtilsService', 'AuthService', 'AssetOptionScope',
function (OptionTypes, WebApiService, toastr, ProgressService, $filter, AssetOptionService, FileService,
        UtilsService, AuthService, AssetOptionScope) {
    return {
        restrict: 'E',
        transclude: true,
        scope: {
            ctrl: '=',
            assetInfo: '=',
            option: '=?',
            isInventory: '=?',
            addedOptions: '=?'
        },
        link: function (scope, elem, attrs) {

            _initializeVariables();
            _checkIfIsEdit();
            _initializeData();

            scope.uploadImageClick = AssetOptionService.UploadImageClick;

            scope.deletePicture = function () {
                scope.data = AssetOptionService.RemoveSavedPìcture(scope.data);
            };

            scope.changeScope = function (selectedScope) {
                _initializeDataNoEdit();
                scope.selected_scope = selectedScope;
            };

            scope.changeOption = function (selectedOption) {
                scope.data = _getDataFromExistingOption(selectedOption);
                _setPicturePath(selectedOption);
            };

            function _initializeData() {
                scope.isEdit ? _initializeDataEdit() : _initializeDataNoEdit();
            };

            function _setPicturePath(option) {
                if (option.domain_document) {
                    scope.picturePath = AssetOptionService.GetPicturePath(option.domain_id, option.domain_document);
                }
            }

            function _getDataFromExistingOption(option) {

                var newOption = angular.extend({},
                        angular.copy(option),
                        { unit_price: option.unit_price || option.unit_budget || 0, quantity: option.quantity || 1, avg_last_cost: option.avg_cost ? $filter('currency')(option.avg_cost) + (option.last_cost ? '/' + $filter('currency')(option.last_cost) : '') : null }
                    );
                delete newOption.unit_budget;

                return newOption;
            }

            function _setSettings(option) {
                if (option.settings) {
                    scope.settings = AssetOptionService.GetSettings(option.settings);
                }
            }

            function _initializeDataEdit() {

                scope.data = _getDataFromExistingOption(scope.option);

                _setSettings(scope.data);
                _setPicturePath(scope.data);
            };

            function _initializeDataNoEdit() {
                scope.data = {
                    quantity: 1,
                    unit_price: 0,
                    asset_domain_id: scope.assetInfo.asset_domain_id,
                    asset_id: scope.assetInfo.asset_id,
                    domain_id: AuthService.getLoggedDomain()
                };
            };

            function _initializeVariables() {
                if (!scope.isInventory) {
                    _initializeCatalogVariables();
                } else {
                    _initializeInventoryVariables();
                }
                _initializeGlobalVariables();
            };

            function _initializeCatalogVariables() {
                scope.isEdit = false;
            };

            function _initializeInventoryVariables() {
                if (!scope.option) {
                    scope.selected_scope = AssetOptionScope['Catalog'];
                    _getAvailableOptions();
                }
            };

            function _initializeGlobalVariables() {
                scope.fileSizeLimit = FileService.ImageFileSizeLimit;
                scope.show_firefox = UtilsService.IsFirefox();
                scope.optionTypes = OptionTypes;
                scope.assetInfo = scope.assetInfo || {};
            };

            function _checkIfIsEdit() {
                scope.isEdit = scope.option && scope.option != {};
            };

            function _getAvailableOptions() {
                ProgressService.blockScreen();
                WebApiService.genericController.query({
                    controller: 'assetOptions', action: 'All',
                    domain_id: scope.assetInfo.asset_domain_id,
                    project_id: scope.assetInfo.asset_id
                },
                    function (data) {

                        scope.availableOptions = angular.copy(data.filter(function (op) {
                            return !scope.addedOptions.find(function (op1) {
                                return op.asset_option_id == op1.option_id;
                            });
                        }).map(function (op) {
                            op.assets_options = null;
                            op.quantity = 1;
                            return op;
                        }));

                        ProgressService.unblockScreen();
                    }, function () {
                        toastr.error('Error to try get the available options');
                        ProgressService.unblockScreen();
                    });
            };

            function _validateOption() {
                return AssetOptionService.ValidateForm(scope.addOptionForm, scope.data);
            }

            function _buildOption() {
                return angular.extend(AssetOptionService.FillSettings(scope.data, scope.settings),
                            { scope: scope.selected_scope || scope.data.scope });
            }

            function _saveOption() {
                return new Promise(function (resolve, reject) {
                    AssetOptionService.SaveOption(scope.addOptionForm,
                        _buildOption(), scope.assetInfo, scope.isEdit, scope.show_firefox)
                        .then(function (data) {
                            resolve(data);
                        }, function (err) {
                            reject(err);
                        });
                });
            };

            function _getOption() {
                return new Promise(function (resolve, reject) {

                    var func = scope.show_firefox ? 'GetBase64FileFirefox' : 'GetBase64NoFileFirefox';
                    var param = scope.show_firefox ? 'imageF' : scope.data.image;

                    FileService[func](param).then(function (file) {
                        scope.data.image = null;
                        resolve(angular.extend(_buildOption(), { picture: file }));
                    }, function (error) { reject(error); });
                });
            };

            scope.ctrl = angular.extend(scope.ctrl, { saveOption: _saveOption, validateOption: _validateOption, getOption: _getOption });
        },
        templateUrl: 'app/Directives/Elements/AddOption.html'
    }
}]);
