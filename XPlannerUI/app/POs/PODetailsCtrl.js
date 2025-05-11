xPlanner.controller('PODetailsCtrl', ['$scope', 'AuthService', '$stateParams', 'WebApiService', 'toastr', 'ProgressService',
    'DialogService', 'HttpService', 'GridService', '$q', '$state', 'StatusListeditMulti',
    function ($scope, AuthService, $stateParams, WebApiService, toastr, ProgressService, DialogService, HttpService, GridService,
        $q, $state, StatusListeditMulti) {
        $scope.vendorCtrlParams = { domain_id: AuthService.getLoggedDomain() };
        $scope.$emit('detailsParams', $stateParams);

        $scope.detailsParams = angular.copy($stateParams);
        $scope.statusList = StatusListeditMulti.filter(function (item) { return item.value != "Plan"});
        $scope.isNotViewer = (AuthService.getLoggedUserType() != "3");
        //$scope.addressCtrlParams = { domain_id: AuthService.getLoggedDomain(), project_id: $stateParams.project_id };

        function _getGenericParams() {
            return {
                controller: 'PurchaseOrders', action: 'Item', domain_id: AuthService.getLoggedDomain()/*$scope.detailsParams.domain_id*/,
                project_id: $scope.detailsParams.project_id, phase_id: $scope.detailsParams.po_id
            };
        };

        function _getGenericParamsAddress() {
            return {
                controller: 'Address', action: 'All', domain_id: AuthService.getLoggedDomain()/*$scope.detailsParams.domain_id*/,
                project_id: $scope.detailsParams.project_id
            };
        };

        
        function _updatePOView(newPO) {
            oldVendor = newPO;
            if (newPO) {
                if (newPO.toJSON) {
                    newPO = newPO.toJSON();
                }
                newPO.vendor = $scope.data ? $scope.data.vendor : newPO.vendor;
                $scope.data = newPO;
                if (newPO.quote_expiration_date) {
                    $scope.data.quote_expiration_date = new Date(newPO.quote_expiration_date);
                    $scope.minQuoteExDate = new Date(newPO.quote_requested_date);
                }
                $scope.$emit('updateProgressBar', $scope.detailsParams);
                var total_assets_po_amt = 0;
                if ($scope.assignedAssetsGrid) {
                    $q.all($scope.assignedAssetsGrid.dataSource.data().map(function (item) {
                        total_assets_po_amt += (Number(item.total_po_amt || 0));
                    })).then(function (data) {
                        $scope.$emit('updateProgressBar', $scope.detailsParams);
                        calculateDelta(total_assets_po_amt);
                    });
                }
            }

        };

        function _reloadAssets() {
            $scope.assignedAssetsGrid.dataSource.read();
        };

        /* Open request PO/Quote modal */
        $scope.requestPOQuote = function (type) {
            if (!validateAccess()) {
                return;
            }

            DialogService.openModal('app/POs/Modals/RequestPOQuote.html', 'RequestPOQuoteCtrl', { po: $scope.data, type: type }, true)
                .then(function (newPO) {
                    _updatePOView(newPO);
                    _reloadAssets();
                });
        };

        /* Open Receive Quote Modal */
        $scope.receiveQuote = function () {
            if (!validateAccess()) {
                return;
            }

            DialogService.openModal('app/POs/Modals/ReceiveQuote.html', 'ReceiveQuoteCtrl', { po: $scope.data }, true)
                .then(function (newPO) {
                    _updatePOView(newPO);
                    _reloadAssets();
                });
        };

        /* Open Receive PO Modal */
        $scope.receivePO = function () {
            if (!validateAccess()) {
                return;
            }

            DialogService.openModal('app/POs/Modals/ReceivePO.html', 'ReceivePOCtrl', { po: $scope.data }, true)
                .then(function (newPO) {
                    _updatePOView(newPO);
                    _reloadAssets();
                });
        };

        function calculateDelta(total_assets_po_amt) {

            var delta = Math.abs((Math.round((total_assets_po_amt + (Number($scope.data.install) || 0) + (Number($scope.data.freight) || 0) + (Number($scope.data.warehouse) || 0) + (Number($scope.data.tax) || 0) + (Number($scope.data.warranty) || 0) + (Number($scope.data.misc) || 0) - (Number($scope.data.quote_discount) || 0)) * 100) / 100) - $scope.data.quote_amount);
            $scope.data.invalid_po = (delta !== 0);

            if (delta !== 0) {
                toastr.info('Data entered for this purchase does not match the quote amount. Delta amount: $' + delta.toFixed(2) + '. This PO will not be used for procurement calculations until this is addressed.');
            }
        }

     
        function saveValidation(oldVendor, newVendor, hasPO) {
            return $q(function (resolve, reject) {
                if ((oldVendor != newVendor) && hasPO) {
                    DialogService.Confirm('Are you sure ?', 'Changing the vendor will delete contact(s) related to purchase order permanently.').then(function () {
                        resolve();
                    }, function () {
                        reject();
                    })
                } else {
                    resolve();
                }

            })
        }   


        /* Update PO */
        $scope.save = function () {
            if (!validateAccess()) {
                return;
            }       

            $scope.poForm.$setSubmitted();

            if ($scope.poForm.$valid) {

                saveValidation(oldVendor.vendor_id, $scope.data.vendor.vendor_id, $scope.data.vendor_contact && $scope.data.vendor_contact.length > 0).then(function () {

                    ProgressService.blockScreen();

                    $scope.data.project_addresses = null;

                    var items = $scope.assignedAssetsGrid.dataSource.data();

                    // Get only modified assets
                    var modifiedItems = items.filter(function (item) { return item.dirty });
                    
                    for (var i = 0; i < modifiedItems.length; i++) {
                        var status = modifiedItems[i].current_location;
                        if (status == "Delivered" && modifiedItems[i].received_date != null) {
                            toastr.error('Assigned Assets: To set Received Date the status must be Received or Completed for asset code: ' + modifiedItems[i].asset_code);
                            ProgressService.unblockScreen();
                            return false;
                        }
                        else if (status != "Received" && status != "Delivered" && status != "Completed" && (modifiedItems[i].delivered_date != null || modifiedItems[i].received_date != null)) {
                            toastr.error('Assigned Assets: To set Delivered or Received Dates the status must be Received, Delivered or Completed for asset code: ' + modifiedItems[i].asset_code);
                            ProgressService.unblockScreen();
                            return false;
                        }
                        else if (modifiedItems[i].delivered_date != null && modifiedItems[i].received_date != null) {
                            var delivered_date = new Date(modifiedItems[i].delivered_date);
                            var received_date = new Date(modifiedItems[i].received_date)
                            if (delivered_date >= received_date) {
                                toastr.error('Assigned Assets: Received Date must be higher than Delivered Date for asset code: ' + modifiedItems[i].asset_code);
                                ProgressService.unblockScreen();
                                return false;
                            }
                        }
                    }

                    WebApiService.genericController.update({
                        controller: 'InventoryPurchaseOrder', action: 'Item', domain_id: $scope.detailsParams.domain_id, project_id: $scope.detailsParams.project_id, phase_id: $scope.detailsParams.po_id
                    }, modifiedItems, function () {

                        var total_assets_po_amt = items.reduce(function (a, b) {
                            return (Number(b['total_po_amt'] || 0)) + a;
                        }, 0);

                        console.log(total_assets_po_amt);
                        $scope.$emit('updateProgressBar', $scope.detailsParams);
                        calculateDelta(total_assets_po_amt);

                        WebApiService.genericController.update({
                            controller: 'PurchaseOrders', action: 'Item',
                            domain_id: $scope.data.domain_id, project_id: $scope.data.project_id, phase_id: $scope.data.po_id
                        }, $scope.data, function () {
                            toastr.success('PO updated');
                            $scope.assignedAssetsGrid.dataSource.read();                            
                            ProgressService.unblockScreen();
                        }, function () {
                            toastr.error('Error to update PO, please contact the technical support');
                            ProgressService.blockScreen();
                        });
                    }, function () {
                        toastr.error('Error to update PO, please contact the technical support');
                        ProgressService.unblockScreen();
                        location.reload(true);
                    });                    
                })




            } else {

                toastr.error('Please make sure you entered correctly all the fields');

            }

            
        };
        /* END - Update PO */

        function temporary() { };

        $scope.buttonsOpened = true;

        /*GET ADDRESS*/
        WebApiService.genericController.query(_getGenericParamsAddress(), function (data) {
            $scope.addressList = data;
        }, function () {
            toastr.error('Error to retrieve addresses from server, please contact the technical support');
        });

        /* Get PO */
        ProgressService.blockScreen();
        WebApiService.genericController.get(_getGenericParams(), function (data) {
            _updatePOView(data);
            //_emitButtons(data);
            $scope.options = GridService.getStructure(dataSource, columns, toolbar, { noRecords: "There is no assigned assets", editable: true, groupable: true, height: 400 });
            ProgressService.unblockScreen();
        }, function () {
            toastr.error('Error to retrieve data from server, please contact the technical support');
            ProgressService.unblockScreen();
        });

        /* Back to PO list*/
        $scope.back = function () {
            $state.go('index.' + $scope.getLevel($scope.detailsParams) + '_pos', $scope.detailsParams);
        };

        /* Delete the current PO */
        $scope.del = function () {
            if (!validateAccess()) {
                return;
            }

            DialogService.Confirm('Are you sure?', 'The purchase order will be deleted permanently').then(function () {
                ProgressService.blockScreen();
                WebApiService.genericController.remove(_getGenericParams(), function (data) {
                    ProgressService.unblockScreen();
                    toastr.success("Purchase order deleted");
                    $scope.back();
                }, function (error) {
                        ProgressService.unblockScreen();
                        toastr.error("Error to try delete vendor, please contact technical support");
                });
            });
        };


        /* upload files */
        $scope.filesUpload = function () {
            if (!validateAccess()) {
                return;
            }

            DialogService.openModal('app/POs/Modals/UploadPOFiles.html', 'UploadPOFilesCtrl', { params: $scope.data }, true);
        };
        /* END - upload files */

        /* BEGIN - Assigned Assets */

        /* grid configuration */
        var columns = [
            { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(assignedAssetsGrid)\" ng-checked=\"allPagesSelected(assignedAssetsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, assignedAssetsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, assignedAssetsGrid)\" ng-checked=\"isSelected(assignedAssetsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
            { field: "asset_code", title: "Code", width: 110 },
            { field: "manufacturer_description", title: "Manufacturer", width: 170, template: '<span>#: manufacturer_description #<md-tooltip>#: manufacturer_description #</md-tooltip></span>' },
            { field: "serial_number", title: "Model Number", width: 150 },
            { field: "serial_name", title: "Model Name", width: 150 },
            { field: "asset_description", title: "Name", width: 170, template: '<span>#: asset_description #<md-tooltip>#: asset_description #</md-tooltip></span>' },
            { field: "jsn_code", title: "JSN", width: 100 },
            { field: "po_qty", title: "Net New", width: 110, template: "<center>#: po_qty#</center>" },
            {
                field: "total_po_amt", title: "Total Amt", width: 150, editor: dateEditorCurrency, template: "<aw-currency value=\"#: total_po_amt # \"></aw-currency>", attributes: {
                    "id": "total_po_amt"
                }, format: "n2"
            },
            { field: "po_unit_amt", title: "Unit Amt", width: 110, template: "<aw-currency value=\"#: po_unit_amt  # \"></aw-currency>" },
            //{ field: "budget_qty", title: "Planned Qty", width: 130, template: "<center>#: budget_qty#</center>" },
            { field: "budget_amt", title: "Planned Amt", width: 130, template: "<aw-currency value=\"#: budget_amt # \"></aw-currency>" },
            { field: "option_codes", title: "Options Code", width: "200px", attributes: { "class": "no-multilines" }, template: "#: option_codes || '' #" },
            { field: "option_descriptions", title: "Options Description", width: "200px", attributes: { "class": "no-multilines" }, template: "#: option_descriptions || '' #" },
            { field: "option_prices", title: "Options Budget", width: "180px", template: "#= option_prices != null && option_prices != undefined ? option_prices !== 'Pending' ? '<aw-currency value=\"' + option_prices +  '\"></aw-currency>' : option_prices : '' #" },
            {
                field: "current_location", title: "Status", width: 150, editor: statusDropDown,
            },
            {
                field: "delivered_date", title: "Delivered Date", width: 150, editor: dateEditor, format: "{0:MM/dd/yyyy}", template: "#: delivered_date ? kendo.toString(kendo.parseDate(delivered_date), \"MM/dd/yyyy\") : '' #",
            },
            {
                field: "received_date", title: "Received Date", width: 150, editor: dateEditor, format: "{0:MM/dd/yyyy}", template: "#: received_date ? kendo.toString(kendo.parseDate(received_date), \"MM/dd/yyyy\") : '' #",
            },
        ];


        function statusDropDown(container, options) {
            $('<input data-text-field="name" data-value-field="value" data-bind="value:' + options.field + '"/>')
                .appendTo(container)
                .kendoDropDownList({
                    dataSource: $scope.statusList
                });
        };

        function dateEditorCurrency(container, options) {
            $('<input data-text-field="' + options.field + '" class="k-input k-textbox" data-value-field="' + options.field + '" data-bind="value:' + options.field + '" ng-ignore-enter />')
                .appendTo(container);
        }

        function dateEditor(container, options) {
            $('<input data-text-field="' + options.field + '" data-value-field="' + options.field + '" data-bind="value:' + options.field + '" data-format="' + options.format + '"/>')
                .appendTo(container)
                .kendoDatePicker({});
        }

        var toolbar = {
            template: '<section layout="row" ng-cloak ng-if=\"' + $scope.isNotViewer + '\">' +
                '<section layout="row" layout-align="start center" flex="85" class="gray-color">' +
                //"<button class=\"md-icon-button md-button md-accent\" ng-click=\"openAddModal()\">" +
                //    "<i class=\"material-icons\">add</i>" +
                //    "<md-tooltip md-direction=\"bottom\">Add Asset</md-tooltip>" +
                //"</button>" +
                '<button class="md-icon-button md-button" ng-click="deleteAsset()">' +
                '<i class="material-icons">delete</i>' +
                '<md-tooltip md-direction="bottom">Delete Asset</md-tooltip>' +
                '</button>' +
                '</section>' +
                '<section layout="row" layout-align="end center" flex="15">' +
                '<button class="md-icon-button md-button md-accent" ng-click="openAddModal()">' +
                '<i class="material-icons">add_circle</i>' +
                '<md-tooltip md-direction="bottom">Add Asset</md-tooltip>' +
                '</button>' +
                '</section>' +
                '</section>'
        };

        /*Get the data to dataSource*/
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic('InventoryPurchaseOrder', 'All', AuthService.getLoggedDomain()/*$scope.detailsParams.domain_id*/,
                        $scope.detailsParams.project_id, $scope.detailsParams.po_id),
                    headers: {
                        Authorization: 'Bearer ' + AuthService.getAccessToken()
                    }
                }
            },
            error: function () {
                toastr.error('Error to retrieve data from server, please contact technical support');
            },
            schema: {
                model: {
                    fields: {
                        asset_code: { editable: false }, manufacturer_description: { editable: false },
                        serial_number: { editable: false }, serial_name: { editable: false }, asset_description: { editable: false },
                        po_qty: { editable: false }, po_unit_amt: { editable: false }, budget_qty: { editable: false },
                        //budget_amt: { editable: false }, total_po_amt: { type: "number", validation: { min: 0 }},
                        option_codes: { editable: false }, option_descriptions: { editable: false }, option_prices: { editable: false }
                    }
                }
            }
        };
        //$scope.options = GridService.getStructure(dataSource, columns, toolbar, { noRecords: "There is no assigned assets", editable: true, groupable: true, height: 400 });

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;

        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (event.targetScope.$parent.label === 'Assigned Assets') { $scope.assignedAssetsGrid = widget; }
        });

        $scope.dataBound = function () {

            //if ($scope.data.status !== 'Quote Received') {
            //    $scope.assignedAssetsGrid.tbody.find('#total_po_amt').click(false);
            //} else {
            //    $scope.assignedAssetsGrid.tbody.find('#total_po_amt').addClass('editable-cell');
            //}

            //GridService.dataBound($scope.assignedAssetsGrid);
        };
        /* END - grid configuration */

        /* delete Assigned Asset */
        $scope.deleteAsset = function () {
            if (!validateAccess()) {
                return;
            }

            if (GridService.verifySelected('delete', 'asset', $scope.assignedAssetsGrid)) {
                DialogService.Confirm('Are you sure?', 'The asset(s) will be deleted permanently').then(function () {

                    ProgressService.blockScreen();
                    $q.all(GridService.getSelecteds($scope.assignedAssetsGrid).map(function (i) {
                        var ids = i.inventory_ids.split(';');
                        ids.map(function (inventoryId) {
                            return WebApiService.genericController.remove({
                                controller: 'InventoryPurchaseOrder', action: 'Item', domain_id: i.po_domain_id, project_id:
                                    i.project_id, phase_id: i.po_id, department_id: inventoryId
                            }, function () {
                                GridService.RemoveItem($scope.assignedAssetsGrid, i);
                            }).$promise;
                        });
                    })).then(function () {
                        toastr.success('Asset(s) Deleted');
                        ProgressService.unblockScreen();
                    }, function () {
                        toastr.error('Error to delete one o more assetss, please contact the technical support');
                        ProgressService.unblockScreen();
                    });
                });
            }
        };
        
        function validateAccess() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return false;
            }
            if (AuthService.getProjectStatus($scope.detailsParams.project_id) == "L") {
                DialogService.LockedProjectModal();
                return false;
            }

            return true;
        }


        /* Add new asset */
        $scope.openAddModal = function () {
            if (!validateAccess()) {
                return;
            }

            DialogService.openModal('app/POs/Modals/AddPOAsset.html', 'AddPOAsset', { params: $scope.data }, true).then(function () {
                ProgressService.blockScreen();
                $scope.assignedAssetsGrid.dataSource.read().then(function () {
                    ProgressService.unblockScreen();
                });
            });
        }

        /* END - Assigned Assets */
    }]);