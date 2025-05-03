xPlanner.directive('awAssetInventoryGrid', ['AuthService', 'HttpService', '$stateParams', 'ProgressService', 'toastr',
    'DialogService', 'WebApiService', 'GridViewService', 'KendoAssetInventoryService', 'KendoGridService', '$log',
    'localStorageAwService',
    function (AuthService, HttpService, $stateParams, ProgressService, toastr, DialogService, WebApiService, GridViewService,
        KendoAssetInventoryService, KendoGridService, $log, localStorageAwService) {
        return {
            restrict: 'E',
            scope: {
                isTemplate: '=?',
                isGlobalTemplate: '=?',
                params: '=',
                gridHeightSubtract: '@',
                emitButtons: '&',
                hideToolbar: '=?',
                linkedRoom: '=?',
                selectOnlyMode: '=?',
                selecteds: '=?',
                alreadyAssociated: '=?',
                documentLinkMode: '=?',
                documentLinkId: '=?',
                isAssetPoPage: '=?',
                showApprovedAssets: '=?',
                filterActionButton: '=?'
            },
            link: function (scope, elem, attrs) {
                scope.filterActionButton = scope.filterActionButton || false;

                if (scope.selectOnlyMode && attrs.selecteds === undefined) {
                    $log.error('[audaxware assets grid] select-only-mode property requires a selecteds property');
                    return;
                }

                if (scope.selectOnlyMode && attrs.alreadyAssociated === undefined) {
                    $log.error('[audaxware assets grid] select-only-mode property requires a already-associated property');
                    return;
                }

                if (scope.documentLinkMode && attrs.documentLinkId === undefined) {
                    $log.error('[audaxware assets grid] document-link-mode property requires a document-link-id property');
                    return;
                }

                scope.show_private_checkbox = true;
                if (AuthService.getLoggedUserType() == "3") {
                    scope.show_private_checkbox = false;
                }

                var allAssets;
                var gridName = 'assetsGrid';
                var gridLoaded = false;
                var localStorageViewName = scope.isGlobalTemplate ? 'asset-grid-view-gTemplate' : scope.isTemplate ? 'asset-grid-view-template' : 'asset-grid-view';
                var localStorageConsolidatedColumns = [];
                var consolidatedColumns = !localStorageAwService.get(localStorageConsolidatedColumns) ? [] : localStorageAwService.get(localStorageConsolidatedColumns);
                var isViewer = (AuthService.getLoggedUserType() == "3");
                var keepSearch = false;
                if (isViewer)
                    scope.hideToolbar2 = true;
                else
                    scope.hideToolbar2 = scope.hideToolbar || scope.selectOnlyMode;

                function openEditProfile(asset) {
                    editAssets([asset], true);
                };

                /* download files */
                window.downloadFile = function (filename, domainId, container) {
                    window.open(HttpService.generic('filestream', 'file', domainId, filename, container), '_self');
                };

                scope.gridHeight = window.innerHeight - (scope.gridHeightSubtract || 200);
                scope.viewName = null;
                scope.is_private = true;

                function setDbClick(grid) {
                    if (!scope.hideToolbar2 && grid) {
                        grid.tbody.find('tr').dblclick(function () {
                            var asset = grid.dataItem(this);
                            if (asset.linked_template == 0)
                                scope.edit(asset);
                            else
                                DialogService.Alert('Linked Template', 'This asset belongs to the inventory of a room that is linked to a template and cannot be modified. To modified it you need to unlink the template in the room details page before.');

                        });

                        grid.tbody.find('span.link').click(function (e) {
                            openEditProfile(grid.dataItem(this.closest('tr')));
                        });
                    } else if (scope.selectOnlyMode) {
                        grid.tbody.find('tr > td > input[type=checkbox].k-checkbox').click(function (ev) {
                            ev.data = { grid: grid };
                            KendoGridService.SelectRow(ev, this);
                            scope.selecteds = KendoGridService.GetSelecteds(grid);
                            scope.$apply();
                        });
                    }
                };

                function dataBound() {

                    setDbClick(scope.assetsGrid);
                    for (var i = 0; i < scope.assetsGrid._data.length; i++) {
                        scope.assetsGrid._data[i].resp = scope.assetsGrid._data[i].resp.trimEnd()
                    };
                    KendoAssetInventoryService.DataBound(scope.assetsGrid, scope.isTemplate, scope.isGlobalTemplate, scope.consolidated, scope.selectOnlyMode);
                    if (scope.selectOnlyMode) {
                        scope.assetsGrid.selecteds = scope.assetsGrid.dataSource.data().filter(function (item) {
                            return scope.alreadyAssociated.find(function (i) { return i.inventory_id === item.inventory_id });
                        });

                        KendoGridService.AddSelectedClass(scope.assetsGrid);
                    }
                    KendoGridService.UnselectAll(scope.assetsGrid);
                    gridLoaded = true;                    
                    ProgressService.unblockScreen('assetsGrid');
                };

                function _setFreezeColumnsQty() {
                    var qty = scope.assetsGrid.getOptions().columns.filter(function (column) { return column.locked; }).length;
                    scope.freeze_qty = qty > 0 ? qty : null;
                };

                /* Fires when Export PDF */
                function pdfExport() {
                    KendoGridService.ExportToPDF_Event(scope.assetsGrid);
                };
                /* END - kendo ui grid configurations*/

                function saveGridState() {
                    if (!scope.selectOnlyMode) {
                        KendoAssetInventoryService.SaveState(scope.assetsGrid, scope.isTemplate, scope.isGlobalTemplate, scope.consolidated);
                    }
                }

                scope.assetsGrid = $('#assetsGrid').kendoGrid(
                    {
                        dataBinding: function (e) {
                            ProgressService.blockScreen("assetsGrid");
                        },
                        dataBound: function(e) {
                            ProgressService.unblockScreen("assetsGrid");
                        }
                    }
                ).data("kendoGrid");
                
                KendoAssetInventoryService.SetSavedState(scope.assetsGrid, scope.isTemplate, scope.isGlobalTemplate, scope.gridHeight, null, gridName, dataBound, pdfExport, saveGridState, consolidatedColumns, scope.selectOnlyMode, scope.documentLinkMode ? scope.documentLinkId : null, scope.isAssetPoPage, scope.showApprovedAssets)
                    .then(function (consolidated) {

                        if (!scope.selectOnlyMode) {
                            KendoGridService.InitiateGridEvents(scope.assetsGrid, gridName);
                        } else {
                            scope.assetsGrid.thead.find("tr > th > input[type=checkbox].k-checkbox").click(function (ev) {
                                ev.data = { grid: scope.assetsGrid };
                                KendoGridService.SelectAllChange(ev);
                                scope.selecteds = KendoGridService.GetSelecteds(scope.assetsGrid);
                                scope.$apply();
                            });
                        }

                        _setFreezeColumnsQty();

                        /* Get Views */
                        WebApiService.genericController.query({
                            controller: "GridView", action: "All", domain_id: scope.isTemplate ? scope.isGlobalTemplate ? 'assets_inventory_global_template' : 'assets_inventory_template' : 'assets_inventory', project_id: AuthService.getLoggedDomain()
                        }, function (data) {
                            scope.views = data;
                            if (localStorageAwService.get(localStorageViewName) != null) {
                                scope.viewSelected = data.find(function (v) {
                                    return v.name === localStorageAwService.get(localStorageViewName).name;
                                });
                                if (scope.viewSelected) {
                                    scope.loadView(scope.viewSelected);
                                }
                            }
                        },
                            function()
                            {
                                toastr.error('Error to load assets, please contact technical support');
                                ProgressService.unblockScreen("assetsGrid");
                            }
                        );
                        /* End - Get Views */
                    });

                function _reloadGrid() {
                    scope.assetsGrid.dataSource.read().then(function () {

                        KendoGridService.UnselectAll(scope.assetsGrid);
                        //update progress bar
                        scope.$emit('updateProgressBar', scope.params);

                        allAssets = null;
                        scope.search(scope.searchBoxValue);
                    });
                };

                function validateAccess() {
                    if (AuthService.getLoggedUserType() == "3") {
                        DialogService.ViewersChangesModal();
                        return false;
                    }
                    if (AuthService.getProjectStatus(scope.params.project_id) == "L") {
                        DialogService.LockedProjectModal(scope.params);
                        return false;
                    }

                    return true;
                }

                function editAssets(assets, editProfile) {
                    if (!validateAccess()) {
                        return;
                    }

                    DialogService.openModal(editProfile ? 'app/Projects/Assets/Modals/EditProfile.html' : scope.isTemplate && scope.isGlobalTemplate ? 'app/Room Templates/Assets/Modals/EditAssetGlobal.html' : assets.length > 1 || assets[0].quantity > 1 ? 'app/Projects/Assets/Modals/EditMulti.html' : 'app/Projects/Assets/Modals/EditSingle.html',
                        !editProfile && scope.isTemplate && scope.isGlobalTemplate ? 'EditAssetGlobalCtrl' : 'EditAssetModalCtrl', {
                        params: scope.params,
                        assets: assets,
                        multiple: assets.length > 1 || assets[0].quantity > 1,
                        profile: editProfile
                    }, true).then(function (data) {
                        if (data && data.profileModified) {
                            DialogService.Confirm('Profile Modified', 'You have changed the options for this asset, do you want to update the options for all the assets of this type with the same profile in the project?')
                                .then(function () {
                                    ProgressService.blockScreen();
                                    WebApiService.genericController.update({
                                        controller: 'Profile', action: 'EditAllAssets',
                                        domain_id: AuthService.getLoggedDomain(), project_id: scope.params.project_id
                                    },
                                        data, function () {
                                            ProgressService.unblockScreen();
                                            toastr.success('Assets profile have been updated.');
                                            _reloadGrid();
                                        }, function (error) {
                                            ProgressService.unblockScreen();
                                            toastr.error('Error to try update assets profile, please contact the technical support');
                                        });
                                }, function () {
                                    _reloadGrid();
                                });
                        } else {
                            _reloadGrid();
                        }
                    });
                }

                /* Edit Assets */
                scope.edit = function (item) {

                    if (!item) {
                        if (!KendoGridService.VerifySelected('edit', 'asset', scope.assetsGrid)) return;
                    }

                    var assets = item ? [item] : KendoGridService.GetSelecteds(scope.assetsGrid);

                    var noLinked = assets.filter(function (i) { return i.linked_template == 0 });

                    if (noLinked.length != assets.length) {
                        if (noLinked.length == 0) {
                            DialogService.Alert('Linked Template', 'All the selected assets belong to the inventory of some room that is linked to a template and cannot be modified until these room(s) be unlinked from the template.');
                        } else {
                            DialogService.Confirm('Linked Template', 'Some of the selected assets belong to the inventory of a room that is linked to a template and it will not be modified. Do you want to continue?')
                                .then(function () {
                                    editAssets(noLinked);
                                });
                        }
                    }
                    else {
                        editAssets(assets);
                    }
                };
                /* END - Edit assets */

                /* Reloads data when the checkbox change */
                scope.$on("refreshGrid", function (event, data) {
                    scope.showApprovedAssets = data.showOnlyApproved;

                    scope.reloadData(); // Método da diretiva que recarrega os dados
                });

                scope.reloadData = function () {
                    
                    scope.assetsGrid.dataSource.transport.options.read.url = KendoAssetInventoryService.GetGridUrl(scope.isTemplate, scope.consolidated, consolidatedColumns, scope.selectOnlyMode, scope.documentLinkMode ? scope.documentLinkId : null, scope.isAssetPoPage, scope.showApprovedAssets);
                    scope.assetsGrid.dataSource.read().then(function () {
                        KendoGridService.UnselectAll(scope.assetsGrid);
                        allAssets = scope.assetsGrid.dataSource.data();
                        if (scope.searchBoxValue)
                            searchAssets(scope.searchBoxValue);
                    }, function () {
                        toastr.error("Error to retrieve data from server, please contact technical support");
                    });
                };

                function exportGrid(to) {
                    if (window.navigator.userAgent.indexOf("Edge") > -1) {
                        DialogService.Alert('Export Grid', 'Export to excel is not yet supported for Microsoft Edge.');
                        return;
                    }

                    var oldGrid = JSON.parse(JSON.stringify(scope.assetsGrid.dataSource.options.data));

                    scope.assetsGrid.dataSource.options.data = scope.assetsGrid.dataSource.options.data.map(function (i) {
                        i.cut_sheet = (i.cut_sheet ? 'TRUE' : 'FALSE');
                        i.cad_block = (i.cad_block ? 'TRUE' : 'FALSE');
                        i.revit = (i.revit ? 'TRUE' : 'FALSE');
                        i.photo = (i.asset_photo || i.photo ? 'TRUE' : 'FALSE');
                        i.tag_photo = (i.tag_photo ? 'TRUE' : 'FALSE');
                        if (i.source_location) {
                            var location = i.source_location.split('||');
                            i.source_location = location[location.length - 1];
                            location = i.source_room.split('||');
                            i.source_room = location[location.length - 1];
                        }
                        if (i.target_location) {
                            var location = i.target_location.split('||');
                            i.target_location = location[location.length - 1];
                            location = i.target_room.split('||');
                            i.target_room = location[location.length - 1];
                        }
                        return i;
                    });

                    KendoGridService.ExportGrid(to, scope.assetsGrid);
                    scope.assetsGrid.dataSource.options.data = oldGrid;
                    scope.assetsGrid.dataSource.read();
                };

                function importGrid() {
                    if (!validateAccess()) {
                        return;
                    }

                    DialogService.openModal('app/Projects/Assets/Modals/ImportAssets.html', 'ImportAssetsCtrl').then(function (data) {
                        if (data.text != null) {
                            var treeItems = data.text.split('||');
                            for (var i = 0; i < treeItems.length; i++) {
                                var item = JSON.parse(treeItems[i]);
                                scope.$emit('addItemTreeView', item);
                            }
                        }
                        _reloadGrid();
                    });
                }
                /* END - Export Grid */

                /*LOCK COST*/
                function lockCost() {
                    if (!validateAccess()) {
                        return;
                    }

                    WebApiService.asset_inventory.update(_getParamsProject($stateParams), null, function () {
                        toastr.success('Costs Locked!');
                        ProgressService.unblockScreen('assetsGrid');
                    }, function () {
                        ProgressService.unblockScreen('assetsGrid');
                        toastr.error('Error to Lock Cost, please contact technical support');
                    });
                }

                function _getParamsProject(params) {
                    return {
                        action: 'LockCost',
                        domain_id: AuthService.getLoggedDomain(),
                        project_id: params.project_id,
                        phase_id: params.phase_id ? params.phase_id : null,
                        department_id: params.department_id ? params.department_id : null,
                        room_id: params.room_id ? params.room_id : null
                    };
                }

                function deleteAssets(allSelected, noLinkedAssets) {

                    KendoAssetInventoryService.SetSelecteds(scope.assetsGrid, noLinkedAssets);

                    ProgressService.blockScreen('assetsGrid');
                    ProgressService.startProgressBar();
                    KendoAssetInventoryService.DeleteItems(KendoGridService.GetSelecteds(scope.assetsGrid))
                        .then(function () {
                            ProgressService.unblockScreen('assetsGrid');
                            toastr.success('Asset(s) Deleted!');
                            scope.assetsGrid.dataSource.read();
                        }, function (error) {
                            ProgressService.unblockScreen('assetsGrid');
                            if (error.status == 409) {
                                toastr.info('Some, or all, of the selected assets could not be deleted because purchase orders have been issued!');
                            } else {
                                toastr.error('Error to try delete one or more assets, please contact the technical support');
                            }
                            scope.assetsGrid.dataSource.read();
                        });

                    KendoGridService.UnselectAll(scope.assetsGrid);
                }

                scope.delete = function () {
                    if (!validateAccess()) {
                        return;
                    }

                    if (KendoGridService.VerifySelected('delete', 'asset', scope.assetsGrid)) {

                        var oldSelecteds = KendoGridService.GetSelecteds(scope.assetsGrid);
                        var noRelocated = oldSelecteds.filter(function (i) { return i.target_location == null });
                        var noLinkedAssets = noRelocated.filter(function (item) {
                            return item.linked_template == 0;
                        });


                        if (noRelocated.length == 0) {
                            DialogService.Alert('Relocated asset', 'The selected asset(s) is linked to a relocated asset. The link must be deleted in the destination.');
                            return;
                        }

                        if (noLinkedAssets.length == 0) {
                            DialogService.Alert('Linked Template', 'All the selected assets belong to the inventory of some room that is linked to a template and cannot be deleted until these room(s) be unlinked from the template.');
                            return;
                        }

                        var source = noLinkedAssets.filter(function (i) { return i.source_location != null });
                        var confirmationMsg = 'The assets will be deleted permanently!';
                        if (source.length > 0) {
                            confirmationMsg = 'One or more items are linked relocating asset. Deleting this asset(s) will delete the link as well. ' + confirmationMsg;
                        }

                        DialogService.Confirm('Are you sure?', confirmationMsg).then(function () {

                            if (noLinkedAssets.length != oldSelecteds.length) {
                                DialogService.Confirm('Linked Template', 'The selected assets that belong to the inventory of a room which is linked to a template will not be deleted. Do you want to continue?')
                                    .then(function () {
                                        deleteAssets(oldSelecteds, noLinkedAssets);
                                    });
                            } else {
                                deleteAssets(oldSelecteds, noLinkedAssets);
                            }
                        });
                    }
                };
                /* END - Delete assets */

                /* Search box */
                scope.search = function (value) {
                    searchAssets(value);
                };

                function searchAssets(value) {
                    if (gridLoaded) {
                        var items = allAssets || scope.assetsGrid.dataSource.data();
                        KendoGridService.UnselectAll(scope.assetsGrid);

                        if (!value) {
                            scope.assetsGrid.dataSource.data(allAssets || items);
                        } else {
                            value = value.toLowerCase();
                            allAssets = items;
                            var columns = scope.assetsGrid.columns;
                            var filteredItems = items.filter(function (item) {
                                for (var i = 1; i < columns.length; i++) {
                                    if (!columns[i].hidden && String(item[columns[i].field]).toLowerCase().indexOf(value) > -1) {  
                                        return true;
                                    }
                                };
                            });
                            scope.assetsGrid.dataSource.data(filteredItems);
                            if (filteredItems.length > 0) {
                                scope.assetsGrid.dataSource.page(1);
                            }
                        }
                    }
                }
                /* END - Search box */

                /* Open the check room modal*/
                scope.openCheckRoomModal = function () {

                    if (KendoGridService.VerifySelected('check room', 'asset', scope.assetsGrid)) {
                        DialogService.openModal('app/Projects/Assets/Modals/CheckRoomModal.html', 'CheckRoomModalCtrl', {
                            assets: KendoGridService.GetSelecteds(scope.assetsGrid),//.filter(function (i) { return i.rooms_qty > 1 }),
                            params: scope.params
                        }, true);
                    }
                };
                /* End - Open the check room modal*/

                scope.reloadGrid = function () {                    
                    scope.assetsGrid.dataSource.read().then(function () {
                        KendoGridService.UnselectAll(scope.assetsGrid);
                        //update progress bar
                        scope.$emit('updateProgressBar', scope.params);
                        allAssets = scope.assetsGrid.dataSource.data();

                        // Keep search after linking an asset
                        if (!keepSearch) {
                            scope.searchBoxValue = null;
                        }
                        keepSearch = false;
                        scope.search(scope.searchBoxValue);

                    });
                }

                scope.openLinkModal = function () {
                    if (!KendoGridService.VerifySelected('link', 'asset', scope.assetsGrid, false)) {
                        return;
                    }
                    var items = KendoGridService.GetSelecteds(scope.assetsGrid);
                    var linkedItems = items.filter(function (item) {
                        return item.source_location != null && item.source_location.length > 0;
                    });

                    if (linkedItems.length > 0) {
                        var ret = DialogService.Confirm('Unlink Assets', 'Are you sure you want to unlink the ' + linkedItems.length.toString() + ' selected asset(s)').then(
                            function () {
                                var linkedIds = [];
                                linkedItems.forEach(function (item, index) {
                                    linkedIds.push(item.inventory_id);
                                });

                                WebApiService.link_asset_from_project.delete(
                                    { domain_id: AuthService.getLoggedDomain(), project_id: scope.params.project_id },
                                    linkedIds,
                                    function () {
                                        toastr.success('Links have been removed successfully');
                                        scope.reloadGrid();
                                    },
                                    function () {
                                        toastr.error('Error to remove links, please contact technical support.');
                                    });
                            });
                    }
                    else {
                        if (items.length > 1) {
                            DialogService.Alert('Relocation link', 'You can only link one asset at a time');
                            return;
                        }
                        scope.openAddModal(true, items[0]);
                    }
                }


                /* Open the add asset modal*/
                scope.openAddModal = function (isLink, linkItem) {
                    if (!validateAccess()) {
                        return;
                    }
                    if (!scope.linkedRoom) {
                        DialogService.openModal('app/Projects/Assets/Modals/AddModal.html', 'AIAddModalCtrl', { params: scope.params, isToGlobalTemplate: scope.isGlobalTemplate, isLink: isLink, selected_asset: (isLink ? linkItem : null), isTemplate: scope.isTemplate }).then(function (data) {
                            if (data || isLink) {
                                keepSearch = isLink && scope.searchBoxValue ? true : false;
                                scope.reloadGrid();
                            }
                        });
                    } else {
                        DialogService.Alert('Linked Room', 'This room is linked to a template and the inventory cannot be modified. To add a new asset you need to unlink the room in the template section before.');
                    }
                };
                /* END - Open the add asset modal*/

                /* Open the replace asset modal*/
                scope.openReplaceModal = function () {
                    if (!validateAccess()) {
                        return;
                    }

                    if (KendoGridService.VerifySelected('replace', 'asset', scope.assetsGrid)) {
                        var items = KendoGridService.GetSelecteds(scope.assetsGrid);
                        var noLinkedTemplates = items.filter(function (i) { return i.linked_template != 1 });                        
                        for (var i = 0; i < items.length; i++) {
                            if (items[i].po_status.trimStart() === "PO Issued") {
                                toastr.error('The asset ' + items[i].asset_code + ' can not be replaced: purchase order have been issued!');
                                return;
                            }
                        }

                        if (!noLinkedTemplates || noLinkedTemplates.length < 1) {
                            DialogService.Alert('Linked Template', 'The selected asset(s) belong to the inventory of some room that is linked to a template and cannot be modified until the room be unlinked from the template.');
                            return;
                        } else if (noLinkedTemplates.length != items.length) {
                            DialogService.Alert('Linked Template', 'Some of the selected asset(s) belong to the inventory of some room that is linked to a template and will not be modified.');
                        }

                        DialogService.openModal('app/Projects/Assets/Modals/ReplaceAsset.html', 'ReplaceAssetModalCtrl',
                            { items: noLinkedTemplates, params: scope.params, isTemplate: scope.isTemplate }, true).then(function () {
                                scope.assetsGrid.dataSource.read().then(function () {
                                    allAssets = scope.assetsGrid.dataSource.data();
                                    KendoGridService.UnselectAll(scope.assetsGrid);
                                    //update progress bar
                                    scope.$emit('updateProgressBar', scope.params);
                                    scope.search(scope.searchBoxValue);
                                });
                            }, function () {
                                KendoGridService.UnselectAll(scope.assetsGrid);
                            });
                    }
                };
                /* END - Open the replace asset modal*/

                /* Open the replace asset modal*/
                scope.openMoveModal = function () {
                    if (!validateAccess()) {
                        return;
                    }

                    if (KendoGridService.VerifySelected('replace', 'asset', scope.assetsGrid)) {

                        var items = KendoGridService.GetSelecteds(scope.assetsGrid);
                        var noLinkedTemplates = items.filter(function (i) { return i.linked_template != 1 });

                        if (!noLinkedTemplates || noLinkedTemplates.length < 1) {
                            DialogService.Alert('Linked Template', 'The selected asset(s) belong to the inventory of some room that is linked to a template and cannot be modified until the room be unlinked from the template.');
                            return;
                        } else if (noLinkedTemplates.length != items.length) {
                            DialogService.Alert('Linked Template', 'Some of the selected asset(s) belong to the inventory of some room that is linked to a template and will not be modified.');
                        }

                        DialogService.openModal('app/Projects/Assets/Modals/MoveAsset.html', 'MoveAssetCtrl',
                            { items: noLinkedTemplates, params: scope.params }, true).then(function () {
                                scope.assetsGrid.dataSource.read().then(function () {
                                    allAssets = scope.assetsGrid.dataSource.data();
                                    KendoGridService.UnselectAll(scope.assetsGrid);
                                    //update progress bar
                                    scope.$emit('updateProgressBar', scope.params);
                                    scope.search(scope.searchBoxValue);
                                });
                            }, function () {
                                KendoGridService.UnselectAll(scope.assetsGrid);
                            });
                    }
                };

            /* Open the connect asset modal */
                scope.openConnectModal = function () {
                   
                    if (!validateAccess()) {
                        return;
                    }

                    if (KendoGridService.VerifySelected('connect', 'asset', scope.assetsGrid, true)) {

                        var item = KendoGridService.GetSelecteds(scope.assetsGrid);

                        if (item[0].ports && item[0].it_connections >= item[0].ports) {
                            toastr.error(item[0].asset_description + " does not have available ports to connect.");
                            KendoGridService.UnselectAll(scope.assetsGrid);
                            return;
                        }

                        DialogService.openModal('app/Projects/Assets/Modals/ConnectAsset.html', 'ConnectAssetModalCtrl',
                            { items: item, params: scope.params, isTemplate: scope.isTemplate }, true).then(function () {
                                scope.assetsGrid.dataSource.read().then(function () {
                                    allAssets = scope.assetsGrid.dataSource.data();
                                    KendoGridService.UnselectAll(scope.assetsGrid);
                                    //update progress bar
                                    scope.$emit('updateProgressBar', scope.params);
                                    scope.search(scope.searchBoxValue);
                                });
                            }, function () {
                                KendoGridService.UnselectAll(scope.assetsGrid);
                            });                     

                    }
                }





                /* END - Open the replace asset modal*/


                /* Open compare asset modal*/
                scope.openCompareAssetsModal = function () {
                    if (!validateAccess()) {
                        return;
                    }

                    var items = KendoGridService.GetSelecteds(scope.assetsGrid);

                    if (items.length < 2 || items.length > 2) {
                        toastr.error('You need to select two assets to compare');
                        return false;
                    }

                    DialogService.openModal('app/Projects/Assets/Modals/CompareAssets.html', 'CompareAssetsCtrl',
                        { items: items, params: scope.params, isConsolidated: scope.consolidated, consolidatedColumns: consolidatedColumns }, true).then(function () {
                            scope.assetsGrid.dataSource.read().then(function () {
                                KendoGridService.UnselectAll(scope.assetsGrid);
                                //update progress bar
                                scope.$emit('updateProgressBar', scope.params);
                                scope.search(scope.searchBoxValue);
                            });
                        }, function () {
                            KendoGridService.UnselectAll(scope.assetsGrid);
                        });
                };




                /* BEGIN - Save and set view */
                scope.closeToolbars = function () { scope.showViewsOptions = false; scope.showActionButtons = false; };

                function _isSystemView(name, showMessage, isSave) {
                    if ((name && name.charAt(0) === '(' && name.charAt(name.length - 1) === ')')) {
                        if (showMessage)
                            toastr.error('The views inside () are reserved views of the system and cannot be saved or deleted.' + (isSave ? 'Please choose a name without the () to your customized view' : ''));
                        return true;
                    }
                    return false;
                };


                function _getViewName(name) {
                    var newName = name;
                    if (_isSystemView(name)) 
                        return null;
                    
                    if (name.includes(' (shared)'))
                        newName = name.substring(0, name.length - 9);

                    return newName;
                };


                scope.loadView = function (view) {
                    if (!view) {
                        toastr.error('You need to select a view to load');
                        return;
                    }

                    scope.viewSelected = view;
                    KendoGridService.UnselectAll(scope.assetsGrid);

                    KendoAssetInventoryService.SetSavedState(scope.assetsGrid, scope.isTemplate, scope.isGlobalTemplate, scope.gridHeight, view, gridName, dataBound, pdfExport, saveGridState, consolidatedColumns, scope.selectOnlyMode, scope.documentLinkMode ? scope.documentLinkId : null, scope.isAssetPoPage, scope.showApprovedAssets)
                        .then(function (consolidated) {
                            scope.consolidated = consolidated;
                            _setFreezeColumnsQty();
                            scope.viewName = _getViewName(view.name);
                            scope.is_private = view.is_private;
                            KendoGridService.InitiateGridEvents(scope.assetsGrid, gridName);
                            consolidatedColumns = view.consolidated_columns ? JSON.parse(view.consolidated_columns) : [],
                            scope.searchBoxValue = null;
                            localStorageAwService.set(localStorageViewName, view);
                        });
                };

                scope.deleteView = function (view) {
                    GridViewService.deleteView(view).then(function () {
                        scope.views.splice(scope.views.indexOf(view), 1);
                        scope.viewName = null;
                        scope.is_private = true;
                    });
                };

                scope.saveView = function (name) {
                    GridViewService.saveView(scope.assetsGrid, scope.isTemplate ? scope.isGlobalTemplate ? 'assets_inventory_global_template' : 'assets_inventory_template' : 'assets_inventory', name, scope.views, consolidatedColumns, scope.is_private).then(function (data) {
                        if (data.updatedView) {
                            scope.views[scope.views.indexOf(data.updatedView)].grid_state = data.newView.grid_state;
                            scope.viewSelected = scope.views[scope.views.indexOf(data.updatedView)];
                        } else {
                            scope.views.push(data.newView);
                            scope.viewSelected = data.newView;
                        }
                        scope.viewName = data.newView.name;
                        scope.is_private = data.newView.is_private;
                        localStorageAwService.set(localStorageViewName, data.newView);
                    });
                };

                /* BEGIN - Clear all filter*/
                scope.clearAllFilters = function () {
                    scope.searchBoxValue = null;
                    KendoGridService.ClearFilters(scope.assetsGrid);
                    scope.search();
                };
                /* END - Clear all filters */

                scope.openPOModal = function () {
                    DialogService.NotImplementAlert();
                    //if (!validateAccess()) {
                    //    return;
                    //}
                };

                /*Fab button*/
                
                var lockButton = {
                    label: 'Lock Cost',
                    icon: 'lock',
                    click: { func: lockCost },
                    show: scope.isTemplate ? false : true
                }

                var exportButton = {
                    label: 'Export to Excel',
                    icon: 'file_simple',
                    click: { func: exportGrid, params: 'excel' },
                    show: true
                }

                var importButton = {
                    label: 'Import from Excel',
                    icon: 'file_upload',
                    click: { func: importGrid },
                    show: true
                }


                if (scope.emitButtons) {
                    if (isViewer)
                        scope.emitButtons({ value: [exportButton] })
                    else
                        scope.emitButtons({ value: [lockButton, exportButton, importButton] })
                }
                /*END - Fab button*/


                scope.collapseExpand = KendoGridService.CollapseExpand;

                scope.chooseDisplayedColumns = function () {                
                    var columns = scope.assetsGrid.columns;
                    DialogService.openModal('app/Utils/Modals/DisplayColumns.html', 'DisplayColumnsCtrl', { columns: columns, consolidated: consolidatedColumns })
                        .then(function (gridColumns) {                            
                            ProgressService.blockScreen('assetsGrid');

                            // Consolidated Columns
                            if (JSON.stringify(gridColumns.consolidated) != JSON.stringify(consolidatedColumns)) {
                                localStorageAwService.set(localStorageConsolidatedColumns, gridColumns.consolidated);
                                consolidatedColumns = gridColumns.consolidated;
                                if (scope.consolidated)
                                    scope.reloadData();
                            }  
                            
                            gridColumns.display.forEach(function (item, index) {
                                if (item.hidden != columns[index].hidden) {
                                    if (item.hidden) {
                                        scope.assetsGrid.hideColumn(index);
                                    } else {
                                        scope.assetsGrid.showColumn(index);
                                    }
                                }
                            });
                            ProgressService.unblockScreen("assetsGrid");                          
                        });
                };

                /* Freeze columns */
                scope.freezeColumns = function (qty) {
                    scope.freeze_qty = qty
                    if (qty >= 0) {
                        var oldOptions = scope.assetsGrid.getOptions();
                        var message = false;
                        var checkbox;

                        if (qty > 0) {
                            var cont = 0;
                            oldOptions.groupable = false;
                            for (var i = 0; i < oldOptions.columns.length; i++) {
                                if (!oldOptions.columns[i].headerTemplate && !oldOptions.columns[i].hidden && cont < qty) {
                                    if (!oldOptions.columns[i].template) {
                                        oldOptions.columns[i].locked = true;
                                        cont++;
                                    } else if (!message) {
                                        toastr.warning('One or more columns can\'t be frozen and were skipped');
                                        message = true;
                                    }
                                } else if (oldOptions.columns[i].headerTemplate) {
                                    checkbox = { column: oldOptions.columns[i], index: i };
                                } else {
                                    oldOptions.columns[i].locked = false;
                                }
                            }

                            oldOptions.columns.splice(checkbox.index, 1);
                            for (var i = 0; i < oldOptions.columns.length; i++) {
                                if (!oldOptions.columns[i].locked) {
                                    oldOptions.columns.splice(i - 1, 0, checkbox.column);
                                    break;
                                }
                            }
                        } else if (!oldOptions.groupable) {

                            oldOptions.groupable = true;
                            oldOptions.columns = oldOptions.columns.map(function (column, i) {
                                column.locked = column.lockable = false;
                                if (column.headerTemplate) {
                                    checkbox = { column: column, index: i };
                                }
                                return column;
                            });
                            oldOptions.columns.splice(checkbox.index, 1);
                            oldOptions.columns.unshift(checkbox.column);
                        }
                        scope.assetsGrid.setOptions(oldOptions);
                        // Bind the select functions again
                        KendoGridService.InitiateGridEvents(scope.assetsGrid, gridName);
                    }
                };

                scope.consolidate = function () {
                    if (consolidatedColumns.length == 0 && scope.consolidated) {
                        var mandatoryFields = KendoAssetInventoryService.GetMandatoryFields();
                        mandatoryFields.forEach(function (item) {
                            if (!consolidatedColumns.includes(item))
                                consolidatedColumns.push(item);
                            
                        });
                    }
                    
                    scope.reloadData();
                }


                window.showImage = function (elem, domain_id, rotate, photo) {
                    var rotation = rotate * 90;
                    if (!$(elem).closest('section').children('.image_popover').children('img').attr('src')) {
                        $(elem).closest('section').children('.image_popover').children('img').attr('src', HttpService.generic('filestream', 'file', domain_id, photo, 'photo'));

                    }
                    $(elem).closest('section').children('.image_popover').children('img')[0].style.transform = 'rotate(' + rotation + 'deg)';
                    $(elem).closest('section').children('.image_popover').children('img').show();
                };

                window.hideImage = function (elem) {
                    $(elem).closest('section').children('.image_popover').children('img').hide();
                };
            },
            templateUrl: 'app/Directives/Elements/AssetInventoryGrid.html'
        };
    }]);
