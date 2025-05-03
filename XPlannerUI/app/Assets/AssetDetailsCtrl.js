xPlanner.controller('AssetDetailsCtrl', ['$scope', 'WebApiService', 'AuthService', 'toastr', 'PlacementList',
        'AssetPropertyList', 'ProgressService', 'DialogService', 'AssetsService', '$q', '$stateParams', '$state', 'HttpService',
    'AudaxwareDataService', '$filter', 'AssetClassList', 'GridService', '$mdDialog',
    function ($scope, WebApiService, AuthService, toastr, PlacementList, AssetPropertyList, ProgressService,
        DialogService, AssetsService, $q, $stateParams, $state, HttpService, AudaxwareDataService, $filter, AssetClassList, GridService, $mdDialog) {

        $scope.selectedTab = 0;

        $scope.$on('$viewContentLoaded', function (event) {
            
            $scope.$emit('initialTab', 'assets');
            $scope.requiredSettings = [];
            $scope.classes = AssetClassList;
            $scope.loggedDomain = AuthService.getLoggedDomain();
            $scope.height = window.innerHeight;
            $scope.isManufacturerDomain = AuthService.isManufacturerDomain();
            var linkDuplicatedAsset = false;

            $scope.asset = {};

            function _getAsset(noReloadCategories) {
                ProgressService.blockScreen();
                WebApiService.genericController.get(_getGenericParams("assets", "Item", $stateParams.domain_id, $stateParams.asset_id), function (asset) {

                    _updateAssetView(asset);

                    if (!noReloadCategories) {
                        WebApiService.genericController.query({ controller: 'categories', action: 'all', domain_id: asset.domain_id },
                            function (cats) {
                                $scope.categories = cats;
                                $scope.asset.categorySelected = cats.find(function (cat) {
                                    return cat.domain_id == asset.assets_subcategory.category_domain_id && cat.category_id == asset.assets_subcategory.category_id;
                                });
                                $scope.reloadSubategories(true);
                            });
                    }

                    if (asset.discontinued) {
                        _getAlternatAssets();
                    }

                    _defineFloatButtons();
                    ProgressService.unblockScreen();
                }, function () {
                    ProgressService.unblockScreen();
                    toastr.error('Error to retrieve data from server, please contact technical support');
                });
            }

            _getAsset();

            function _getGenericParams(controller, action, id1, id2, id3, id4, id5) {
                return {
                    controller: controller,
                    action: action,
                    domain_id: id1 || AuthService.getLoggedDomain(),
                    project_id: id2,
                    phase_id: id3,
                    department_id: id4,
                    room_id: id5
                };
            };

            $scope.controllerParams = { domain_id: AuthService.getLoggedDomain() };

            $scope.reloadSubategories = function (verifySubcategory) {

                if ($scope.asset.categorySelected) {

                    return WebApiService.genericController.query(_getGenericParams("Subcategories", "All", AuthService.getLoggedDomain(), $scope.asset.categorySelected.domain_id, $scope.asset.categorySelected.category_id), function (subs) {
                        $scope.subcategories = subs;
                        $scope.asset.assets_subcategory = verifySubcategory ? subs.find(function (s) { return s.domain_id == $scope.asset.assets_subcategory.domain_id && s.subcategory_id == $scope.asset.assets_subcategory.subcategory_id }) : null;
                    }, function () {
                        toastr.error("Error to retrieve data from server, please contact technical support");
                    });
                }
            };

            function _updateAssetView(asset) {

                if ($scope.subcategories) {
                    var asset_sub = asset.assets_subcategory || $scope.asset.assets_subcategory;
                    asset.assets_subcategory = $scope.subcategories.find(function (item) {
                        return item.domain_id == asset_sub.domain_id && item.category_id == asset_sub.category_id
                            && item.subcategory_id == asset_sub.subcategory_id;
                    });
                } else {
                    asset.assets_subcategory = asset.assets_subcategory || $scope.asset.assets_subcategory;
                }
                asset.manufacturer = asset.manufacturer || $scope.asset.manufacturer;
                asset.domain = asset.domain || $scope.asset.domain;

                asset.categorySelected = $scope.asset.categorySelected || asset.assets_subcategory.assets_category;

                asset.isCustom = asset.asset_code.slice(-1) === 'C';
                asset.avg_last_cost = asset.avg_cost ? $filter('currency')(asset.avg_cost) + (asset.last_cost ? ' / ' + $filter('currency')(asset.last_cost) : '') : '';
                asset.related_asset = asset.assets1.find(function (item) {
                    return item.domain_id === AuthService.getLoggedDomain();
                });

                $scope.asset = asset;               
            };

            function _getAlternatAssets() {
                ProgressService.blockScreen();
                WebApiService.genericController.query(_getGenericParams("alternateAsset", "All", $scope.asset.domain_id,
                    $scope.asset.subcategory_domain_id, $scope.asset.subcategory_id, $stateParams.asset_id), function (assets) {
                        ProgressService.unblockScreen();
                        $scope.alternate_assets = assets;
                    }, function () {
                        ProgressService.unblockScreen();
                        toastr.error('Error to retrieve data from server, please contact technical support');
                    });
            };

            $scope.changeAsset = function (newAsset, mustReload) {
                $state.go('assetsWorkspace.assetsDetails', { domain_id: newAsset.domain_id, asset_id: newAsset.asset_id }, { reload: mustReload });
            };

            $scope.assetProperties = AssetPropertyList;

            $scope.placementList = PlacementList;

            $scope.querySearch = function (query, array, property, searchProperty) {

                if (query) {
                    $scope.asset[property] = query;
                    var lowerQuery = angular.lowercase(query);

                    return array.filter(function (i) { return angular.lowercase(i).indexOf(lowerQuery) > -1 });
                }

                return array;
            };

            $scope.toggleDiscontinued = function () {
                if ($scope.asset.discontinued && !$scope.alternate_assets) {
                    _getAlternatAssets();
                }
            };

            $scope.saveAux = function (asset, duplicate) {
                return $q(function (resolve, reject) {

                    if ($scope.asset.default_resp.name != undefined) {
                        var respObject = $scope.asset.default_resp;
                        $scope.asset.default_resp = respObject.name;
                    }
                    AssetsService.UpdateAsset(_getGenericParams("assets", "Item", null, asset.asset_id), asset, duplicate ? 'Asset Duplicated' : 'Asset Updated').then(function (data) {
                        if (!duplicate) {
                            _updateAssetView(data);
                        }
                        resolve(data);
                    }, function () {
                        reject();
                    });
                });
            };

            $scope.editAssetCode = function () {
                if (AuthService.getLoggedUserType() == "3") {
                    DialogService.ViewersChangesModal();
                    return;
                }

                DialogService.openModal('app/Assets/Modals/EditAssetCode.html', 'EditAssetCodeCtrl', null, true).then(function (asset_code) {
                    $scope.asset.asset_code = asset_code;
                });
            };

            function _CheckRequiredSettings(asset) {

                for (var i = 0; i < $scope.requiredSettings.length; i++) {
                    if (asset[$scope.requiredSettings[i]] === null || asset[$scope.requiredSettings[i]] === undefined
                        || asset[$scope.requiredSettings[i]] === '')
                        return false;
                }
                return true;
            }

            $scope.showDifferences = function () {
                DialogService.openModal('app/Assets/Modals/CustomizedDifferences.html', 'CustomizedDifferencesCtrl', { customizedAsset: $scope.asset }, true);
            }

            function save() {

                return $q(function (resolve, reject) {
                    if (AuthService.getLoggedUserType() == "3") {
                        DialogService.ViewersChangesModal();
                        return;
                    }

                    if (parseInt($scope.asset.min_cost) > parseInt($scope.asset.max_cost)) {
                        toastr.error('The Max value must be grather than Min value');
                        return;
                    }

                    ProgressService.blockScreen();
                    $scope.detailsForm.$setSubmitted();
                    $scope.detailsForm.subcategory.$setTouched();

                    if ($scope.detailsForm.$valid) {
                        $scope.asset.jsn_id = $scope.asset.jsn != null ? $scope.asset.jsn.id : null;
                        $scope.asset.jsn_domain_id = $scope.asset.jsn != null ? $scope.asset.jsn.domain_id : null;
                        
                        if($scope.asset.jsn != null){
                            $scope.asset.jsn_utility1_ow = $scope.asset.jsn_utility1 != $scope.asset.jsn.utility1 ? true : $scope.asset.jsn_utility1_ow;
                            $scope.asset.jsn_utility1 = $scope.asset.jsn.utility1;
                            $scope.asset.jsn_utility2_ow = $scope.asset.jsn_utility2 != $scope.asset.jsn.utility2 ? true : $scope.asset.jsn_utility2_ow;
                            $scope.asset.jsn_utility2 = $scope.asset.jsn.utility2;
                            $scope.asset.jsn_utility3_ow = $scope.asset.jsn_utility3 != $scope.asset.jsn.utility3 ? true : $scope.asset.jsn_utility3_ow;
                            $scope.asset.jsn_utility3 = $scope.asset.jsn.utility3;
                            $scope.asset.jsn_utility4_ow = $scope.asset.jsn_utility4 != $scope.asset.jsn.utility4 ? true : $scope.asset.jsn_utility4_ow;
                            $scope.asset.jsn_utility4 = $scope.asset.jsn.utility4;
                            $scope.asset.jsn_utility5_ow = $scope.asset.jsn_utility5 != $scope.asset.jsn.utility5 ? true : $scope.asset.jsn_utility5_ow;
                            $scope.asset.jsn_utility5 = $scope.asset.jsn.utility5;
                            $scope.asset.jsn_utility6_ow = $scope.asset.jsn_utility6 != $scope.asset.jsn.utility6 ? true : $scope.asset.jsn_utility6_ow;
                            $scope.asset.jsn_utility6 = $scope.asset.jsn.utility6;
                        }


                        if (_CheckRequiredSettings($scope.asset)) {
                            ProgressService.unblockScreen();
                            AudaxwareDataService.CheckDuplicateAsset($scope.asset).then(function (data) {
                                ProgressService.blockScreen();
                                $scope.saveAux($scope.asset, data).then(function (newAsset) {
                                    if (data) {
                                        $scope.asset.asset_code = newAsset.asset_code;
                                    }
                                    $scope.changeAsset(newAsset);
                                    $scope.$emit('dataSaved');
                                    ProgressService.unblockScreen();
                                    resolve(true);
                                }, function () { ProgressService.unblockScreen(); });
                            });

                        } else {
                            toastr.error('Please make sure you enter all the required settings on Attributes section');
                            reject();
                        }
                    } else {
                        ProgressService.unblockScreen();
                        toastr.error("Please make sure you enter correctly all the fields");
                        reject();
                    }
                });
            };

            /* Assign to project */
            function assigToProject() {
                if (AuthService.getLoggedUserType() == "3") {
                    DialogService.ViewersChangesModal();
                    return;
                }

                AudaxwareDataService.CheckDuplicateAsset($scope.asset).then(function (data) {
                    if (data) {
                        $scope.saveAux($scope.asset, true).then(function (newAsset) {
                            DialogService.openModal('app/Assets/Modals/AssignProject.html', 'AssignProjectCtrl', { asset: newAsset }, true);
                            $scope.changeAsset(newAsset);
                        });
                    } else {
                        DialogService.openModal('app/Assets/Modals/AssignProject.html', 'AssignProjectCtrl', { asset: $scope.asset }, true);
                    }
                });
            };
            /* END - Assign to project */

            /* upload files */
            function filesUpload() {
                if (AuthService.getLoggedUserType() == "3") {
                    DialogService.ViewersChangesModal();
                    return;
                }

                AudaxwareDataService.CheckDuplicateAsset($scope.asset).then(function (data) {
                    if (data) {
                        $scope.saveAux($scope.asset, true).then(function (newAsset) {
                            DialogService.openModal('app/Assets/Modals/UploadAssetFiles.html', 'UploadAssetFilesCtrl', { asset: newAsset }, true).then(function () {
                                //$scope.changeAsset(newAsset);
                                _getAsset();
                            });

                        });
                    } else {
                        DialogService.openModal('app/Assets/Modals/UploadAssetFiles.html', 'UploadAssetFilesCtrl', { asset: $scope.asset }, true).then(function (files) {
                            _getAsset(true);
                        });
                    }
                });
            };
            /* END - upload files */

            function _back() {
                $state.go('assetsWorkspace.assets');
            };

            /* delete asset */
            function deleteAsset() {
                if (AuthService.getLoggedUserType() == "3") {
                    DialogService.ViewersChangesModal();
                    return;
                }

                AssetsService.DeleteAsset($scope.asset).then(function () {
                    _back();
                });
            };
            /* END - delete asset */

            /* duplicate asset */
            function _duplicateAsset() {
                if (AuthService.getLoggedUserType() == "3") {
                    DialogService.ViewersChangesModal();
                    return;
                }

                if (hasChanges) {
                    DialogService.Confirm('Are you sure?', 'You have made changes to the asset, do you want to save and duplicate?').then(function () {
                        save().then(function () {
                            _duplicateAsset2();
                        })

                    }, function () { return });

                }
                else {
                    _duplicateAsset2();
                }

            }

            function linkAssets() {
                return $q(function (resolve, reject) {
                    DialogService.Confirm('Duplicate Asset', 'Do you want to link and restrict editing of the original asset?').then(function () {
                        linkDuplicatedAsset = true;
                        resolve();
                    }, function () { resolve(); });
                });

            }

            function _duplicateAsset2() {


                linkAssets().then(function () {
                    ProgressService.blockScreen();
                    $scope.asset.default_resp = $scope.asset.default_resp.name;
                    AssetsService.DuplicateAsset($scope.asset, linkDuplicatedAsset).then(function (data) {
                        //_updateAssetView(data);
                        $scope.changeAsset(data);
                        ProgressService.unblockScreen();
                    }, function () { ProgressService.unblockScreen(); });
                });
            }
            /* END - duplicate asset */

            function _defineFloatButtons() {
                /* float buttons */
                var saveButton = {
                    label: 'Save',
                    icon: 'save',
                    click: save,
                    conditionShow: true,
                    condition: true
                };

                var assigProjectButton = {
                    label: 'Make available to project',
                    icon: 'local_parking',
                    click: assigToProject,
                    conditionShow: true,
                    condition: true
                };

                var uploadFilesButton = {
                    label: 'Upload Files',
                    icon: 'file_upload',
                    click: filesUpload,
                    conditionShow: true,
                    condition: true
                };

                var deleteButton = {
                    label: 'Delete',
                    icon: 'delete_forever',
                    click: deleteAsset,
                    conditionShow: true,
                    justAudaxware: true,
                    condition: true
                };

                var backButton = {
                    label: 'Back to List',
                    icon: 'reply',
                    click: _back,
                    condition: true
                };

                var duplicateAsset = {
                    label: 'Duplicate Asset',
                    icon: 'content_copy',
                    click: _duplicateAsset,
                    condition: true
                };

                var addAssetRequest = {
                    label: 'Request to add asset to Audaxware',
                    icon: 'input_white',
                    click: _addAssetRequest,
                    condition: ($stateParams.domain_id != 1 && $scope.asset.domain_id != 1),
                };

                var addAssetToAudaxware = {
                    label: 'Add asset to Audaxware',
                    icon: 'flip_to_front',
                    click: _addAssetToAudaxware,
                    // Besides been in the audaxware domain, it needs to have 'audaxware' email
                    condition: ($scope.asset.approval_pending_domain != null && AuthService.isAudaxwareDomainAndEmail()),
                    justAudaxware: true
                };

                //$scope.buttons = [saveButton, /*assigProjectButton,*/ uploadFilesButton, duplicateAsset, addAssetRequest, deleteButton, backButton];
                $scope.buttons = [saveButton, uploadFilesButton, duplicateAsset, addAssetRequest, addAssetToAudaxware, deleteButton, backButton];

                $scope.buttons.state = true;
                /* end float button */
            }

            $scope.downloadFile = function (filename, asset_domain_id, container, asset_id) {
                window.open(HttpService.generic('filestream', 'file', asset_domain_id, filename, container, asset_id), '_self');
            };

            $scope.showPopover = function () {
                $scope.popoverIsVisible = true;
            };

            $scope.hidePopover = function () {
                $scope.popoverIsVisible = false;
            };

            $scope.downloadPhoto = function (filename, asset_domain_id) {
                return HttpService.generic('filestream', 'file', asset_domain_id, filename, 'photo');
            };

            function _objetctIsModified(newOne, oldOne, idField) {
                if (newOne || oldOne) {
                    if ((newOne && !oldOne) || (!newOne && oldOne))
                        return true;

                    return newOne[idField] != oldOne[idField] || newOne.domain_id != oldOne.domain_id;

                } else {
                    return false;
                }
            }


            //BEGIN USED IN PROJECT
            function userInProjects() {
                ProgressService.blockScreen();
                var columns = [
                           {
                               field: "projectDescription", title: "Project", width: 250,
                               template: function (dataItem) {
                                   return GridService.GetProjectLinkTemplate(dataItem);
                               }, lockable: false
                           },
                           { field: "quantity", title: "Quantity", width: 150 }
                ];

                var dataSource = {
                    transport: {
                        read: {
                            url: HttpService.generic("assets", "Project", $scope.loggedDomain, $stateParams.domain_id, $stateParams.asset_id),
                            headers: { Authorization: "Bearer " + AuthService.getAccessToken() }
                        },
                        error: function () {
                            ProgressService.unblockScreen();
                            toastr.error("Error to retrieve used in projects from server, please contact technical support");
                        }
                    }
                };


                $scope.options = GridService.getStructure(dataSource, columns, null, { groupable: false, noRecords: "This asset is not being used in any projects", height: window.innerHeight - 220 });
                ProgressService.unblockScreen();
                $scope.dataBound = function () {
                    GridService.dataBound($scope.projectsGrid);
                };
            }

            //END USED IN PROJECTS
            $scope.$watch('selectedTab', function (newValue, oldValue) {
                if (newValue == 1 && oldValue == 0)
                    userInProjects();
            });

            $scope.$watch('asset.jsn', function (newValue, oldValue) {
                var new_v = newValue == null ? null : newValue.id;
                var old_v = oldValue == null ? null : oldValue.id;

                if (new_v != old_v && $scope.asset.jsn != null && new_v != $scope.asset.jsn_id) {
                    DialogService.Confirm('Asset Update', 'Do you want to replace the asset utilities?').then(function () {
                        $scope.updatejsn = $scope.asset.jsn;
                    });
                }
            });

            $scope.$watch('asset', function (newValue, oldValue) {

                if ($scope.detailsForm && $scope.detailsForm.$dirty) {
                    //JULIANA: DO NOT ERASE THIS PLEASE. IT'S USED TO COMPARE OBJECTS WHEN IT'S NECESSARY
                    //compareObjects(newValue, oldValue);

                    var updated = false;
                    var newValue1 = JSON.parse(JSON.stringify(newValue));
                    var oldValue1 = JSON.parse(JSON.stringify(oldValue));
                    newValue1.default_resp = oldValue1.default_resp = null;
                    newValue1.manufacturer = oldValue1.manufacturer = null;
                    newValue1.assets_subcategory = oldValue1.assets_subcategory = null;
                    newValue1.categorySelected = oldValue1.categorySelected = null;
                    newValue1.asset_description = oldValue1.asset_description = null;
                    newValue1.updated_at = oldValue1.updated_at = null;

                    var old_resp = oldValue.default_resp.name == undefined ? oldValue.default_resp : oldValue.default_resp.name;
                    var new_resp = newValue.default_resp.name == undefined ? newValue.default_resp : newValue.default_resp.name;
                    var old_alternate_asset = oldValue.alternate_asset != null ? (oldValue.alternate_asset.asset_id == undefined || oldValue.alternate_asset.asset_id == null ? oldValue.alternate_asset : oldValue.alternate_asset.asset_id) : null;
                    var new_alternate_asset = newValue.alternate_asset != null ? (newValue.alternate_asset.asset_id == undefined || newValue.alternate_asset.asset_id == null ? newValue.alternate_asset : newValue.alternate_asset.asset_id) : null;
                    //there is problem with angular.equals and object inside object, so we need to compare manually
                    if ((old_resp != undefined && old_resp != new_resp) ||
                        (old_alternate_asset != null && old_alternate_asset != new_alternate_asset) ||
                        _objetctIsModified(newValue.manufacturer, oldValue.manufacturer, 'manufacturer_id') ||
                        _objetctIsModified(newValue.assets_subcategory, oldValue.assets_subcategory, 'subcategory_id') ||
                        _objetctIsModified(newValue.categorySelected, oldValue.categorySelected, 'category_id')) {
                        updated = true;
                    }

                    if (!angular.equals(newValue1, oldValue1) || updated) {
                        //compareObjects(newValue1, oldValue1);
                        $scope.$emit('itemHasChanges');
                    }

                }
            }, true);

            $scope.$on('saveData', function (event, params) {
                save().then(function () {
                    $scope.$emit('dataSaved', params);
                });
            });


            function _addAssetToAudaxware() {
                if (AuthService.getLoggedUserType() == "3") {
                    DialogService.ViewersChangesModal();
                    return;
                }

                var confirmMsg = 'This asset will be added to Audaxware enterprise. Are you sure?'
                var sucessMsg = 'Asset was duplicated to Audaxware. An email was sent to this enterprise users.';
                if ($scope.asset.approval_modify_aw_asset == true) {
                    confirmMsg = 'By accepting this request the original asset on Audaxware database will be replaced. Do you wish to continue?';
                    sucessMsg = 'Audaxware asset was updated. An email was sent to this enterprise users.';
                }

                $mdDialog.show({
                    templateUrl: 'app/Assets/Modals/AddAssetToAWConfirmation.html',
                    controller: ['$mdDialog', 'toastr', function ($mdDialog, toastr) {
                        this.confirmationMsg = confirmMsg;
                        this.accept = function () {
                            $mdDialog.hide('accept');
                        }
                        this.reject = function () {
                            $mdDialog.hide(this.comment);
                        }
                        this.close = function () {
                            $mdDialog.hide('close');
                        }
                    }],
                    controllerAs: 'ctrl',
                    fullscreen: true,
                }).then(function (pressedButton) {
                    if (pressedButton == 'accept') {

                        ProgressService.blockScreen();
                        WebApiService.genericController.update({ controller: 'Assets', action: 'ApproveRequest', domain_id: AuthService.getLoggedDomain(), project_id: $scope.asset.asset_id, phase_id: $scope.asset.approval_modify_aw_asset }, null, function (data) {
                            ProgressService.unblockScreen();
                            toastr.success(sucessMsg);

                            $stateParams.domain_id = data.domain_id;
                            $stateParams.asset_id = data.asset_id;
                            _getAsset(true);
                        }, function (error) {
                            toastr.error('Error trying to request update, please contact the technical support');
                            ProgressService.unblockScreen();
                        });

                    }
                    else if (pressedButton != 'close') {
                        ProgressService.blockScreen();
                        WebApiService.genericController.update({ controller: 'Assets', action: 'DenyRequest', domain_id: AuthService.getLoggedDomain(), project_id: $scope.asset.asset_id, phase_id: ($scope.asset.approval_modify_aw_asset == null ? false : $scope.asset.approval_modify_aw_asset), department_id: pressedButton}, null, function (data) {
                            ProgressService.unblockScreen();
                            toastr.success('The request was denied sucessfully and the asset was removed. An email was sent to this enterprise users.');

                            _back();
                        }, function (error) {
                            toastr.error('Error trying to request update, please contact the technical support');
                            ProgressService.unblockScreen();
                        });
                    }
                });

            }


            function _addAssetRequest() {
                if (AuthService.getLoggedUserType() == "3") {
                    DialogService.ViewersChangesModal();
                    return;
                }

                if ($scope.asset.domain_id == 1) {
                    toastr.error("Only assets from your Enterprise are allowed. Please do another selection.");
                    return;
                }


                $mdDialog.show({
                    templateUrl: 'app/Assets/Modals/RequestToAddToAudaxware.html',
                    controller: ['$mdDialog', 'toastr', function ($mdDialog, toastr) {
                        this.custom = $scope.asset.isCustom;
                        this.asset_code = $scope.asset.asset_code.substring(0, $scope.asset.asset_code.length - 1);
                        this.save = function () {
                            $mdDialog.hide(this.data);
                        }
                        this.close = function () {
                            $mdDialog.hide();
                        }
                    }],
                    controllerAs: 'ctrl',
                    fullscreen: true,
                }).then(function (data) {
                    if (data != undefined) {

                        ProgressService.blockScreen();

                        WebApiService.genericController.update({
                            controller: 'Assets', action: 'AddAssetRequest',
                            domain_id: AuthService.getLoggedDomain(),
                            project_id: $scope.asset.asset_id
                        }, data, function (data) {
                            ProgressService.unblockScreen();
                            toastr.success('An email was sent to Audaxware requesting the update.');
                        }, function (error) {
                            toastr.error('Error trying to request update, please contact the technical support');
                            ProgressService.unblockScreen();
                            });
                    }
                });

                

            }


            function compareObjects(s, t) {
                if (typeof s !== typeof t) {
                    console.log("two objects not the same type");
                    return;
                }
                if (typeof s !== "object") {
                    console.log('arguments are not typeof === "object"');
                    return;
                }
                for (var prop in s) {
                    if (s.hasOwnProperty(prop)) {
                        if (t.hasOwnProperty(prop)) {
                            if (!angular.equals(s[prop], t[prop])) {
                                console.log("property " + prop + " does not match");
                            }
                        } else {
                            console.log("second object does not have property " + prop);
                        }
                    }
                }
                // now verify that t doesn't have any properties 
                // that are missing from s
                for (prop in t) {
                    if (t.hasOwnProperty(prop)) {
                        if (!s.hasOwnProperty(prop)) {
                            console.log("first object does not have property " + prop);
                        }
                    }
                }
            }
        });
    }]);
