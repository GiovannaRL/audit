xPlanner.factory('AssetOptionService', ['toastr', 'WebApiService', 'ProgressService', 'FileService',
        'UtilsService', 'HttpService',
    function (toastr, WebApiService, ProgressService, FileService, UtilsService, HttpService) {

        var _validatedForm = function (form, option) {

            form.$setSubmitted();

            if (form.$valid) {
                if (option.min_cost && option.max_cost && Number(option.min_cost) > Number(option.max_cost)) {
                    toastr.error('The Max value must be grather than Min value');
                    return false;
                }

                return true;
            }

            toastr.error("Please make sure you entered correctly all the fields");
            return false;

        }

        var _saveOption = function (form, option, assetInfo, isEdit, isFirefox) {
            return new Promise(function (resolve, reject) {

                if (!_validatedForm(form, option)) reject();

                ProgressService.blockScreen('addEditOption');

                var func = isFirefox ? 'GetBase64FileFirefox' : 'GetBase64NoFileFirefox';
                var param = isFirefox ? 'imageF' : option.image;

                FileService[func](param).then(function (file) {
                    delete option.image;
                    option = angular.extend(option, { picture: file });

                    var method = isEdit ? 'update' : 'save';
                    var params = { action: "Item", domain_id: assetInfo.domain_id, asset_id: assetInfo.asset_id };

                    if (isEdit) {
                        params.asset_option_id = option.asset_option_id
                    }

                    WebApiService.asset_option[method](params, option, function (data) {
                        toastr.success('Option ' + (isEdit ? 'Updated' : 'Saved'));
                        ProgressService.unblockScreen('addEditOption');
                        resolve();
                    }, function () {
                        ProgressService.unblockScreen('addEditOption');
                        toastr.error("Erro to save option, please contact technical support");
                        reject();
                    });

                }, function (error) { reject(error); });
            });
        };

        var _uploadImageClick = function () {
            document.getElementById('image').click();
        };

        var _fillSettings = function (option, settings) {

            if (!settings || (option.data_type !== 'FR' && option.data_type !== 'FI'))
                return angular.extend(option, { settings: null });

            var settingsJson = '{'
            for (var key in settings) {
                settingsJson += ' "' + UtilsService.MapAssetOptionSetting(key) + '": "' + settings[key] + '",';
            }

            if (settingsJson.length > 1) {
                settingsJson = settingsJson.slice(0, -1); // remove last ,
            }
            settingsJson += ' }';

            return angular.extend(option, { settings: settingsJson });
        }

        var _getSettings = function (settingsJsonStr) {

            if (!settingsJsonStr || settingsJsonStr == '{}') return null;

            var tmpSettings = JSON.parse(settingsJsonStr);
            var settings = {};
            for (var key in tmpSettings) {
                settings[UtilsService.MapAssetOptionSettingReverse(key)] = tmpSettings[key];
            }
            return settings;
        }

        function _removeSavedPìcture(option) {
            option.domain_document = null;
            option.document_domain_id = null;
            option.document_id = null;
            return option;
        }

        function _getPicturePath(optionDomainId, picture) {
            return HttpService.generic('filestream', 'file', optionDomainId, picture.blob_file_name, 'photo');
        }

        return {
            SaveOption: _saveOption,
            ValidateForm: _validatedForm,
            UploadImageClick: _uploadImageClick,
            FillSettings: _fillSettings,
            GetSettings: _getSettings,
            RemoveSavedPìcture: _removeSavedPìcture,
            GetPicturePath: _getPicturePath
        }

    }]);
