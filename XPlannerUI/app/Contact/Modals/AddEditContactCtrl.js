xPlanner.controller('AddEditContactCtrl', ['$scope', 'local', '$mdDialog', 'StatesList', 'toastr', 'WebApiService',
    'ProgressService', '$stateParams', 'AuthService', 'GridService',
    function ($scope, local, $mdDialog, StatesList, toastr, WebApiService, ProgressService, $stateParams, AuthService, GridService) {

            var params = local.item || local.params || $stateParams;
            
            $scope.statesList = StatesList;
            $scope.newContact = local.contact ? false : true;
            $scope.contact = angular.copy(local.contact) || {
                contact_type: local.type,
                purchase_order: local.purchaseOrder ? [local.purchaseOrder] : []
            };
            var oldName = $scope.contact.name;
            $scope.selectedTabIndex = 0;
            $scope.hideExistingContacts = !$scope.newContact || !local.purchaseOrder;
            $scope.showExistingContacts = !$scope.hideExistingContacts;
            

            $scope.$on("kendoWidgetCreated", function (event, widget) {
                // This was needed since I couldn't get grid binding value cause of ng-if clause
                $scope.existingContactsGrid = widget;
            });

            function _getGenericParams(action, name) {
                return {
                    controller: local.type + 'Contacts', action: action,
                    domain_id: params.domain_id, project_id: params[local.type + '_id'],
                    phase_id: name
                };
            };
           
            WebApiService.genericController.query(_getGenericParams('All'), function (data) {
                if (!$scope.newContact) {
                    $scope.anotherContacts = data.filter(function (i) {
                        return i.name != local.contact.name;
                    });
                } else {
                    $scope.anotherContacts = data;

                    if ($scope.showExistingContacts && !$scope.anotherContacts.length) {
                        $scope.selectedTabIndex = 1;
                    }

                    var gridOptions = {
                        name: 'AddEditContactGrid',
                        noRecords: "No contacts available",
                        height: 500
                    };
                    var columns = [
                        { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(existingContactsGrid)\" ng-checked=\"allPagesSelected(existingContactsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, existingContactsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, existingContactsGrid)\" ng-checked=\"isSelected(existingContactsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                        { field: "name", title: "Name", width: 150 },
                        { field: "title", title: "Title", width: 150 },
                        { field: "email", title: "Email", width: 150 },
                        { field: "phone", title: "Phone", width: 130 },
                        { field: "mobile", title: "Mobile", width: 130 },
                        { field: "fax", title: "Fax", width: 130 }
                    ];

                    
                    var POContact = local.POContact;
                    var anotherContact = $scope.anotherContacts;

                    for (var i = 0; i < POContact.length; i++) {
                        for (var j = 0; j < anotherContact.length; j++) {
                            if (POContact[i].name == anotherContact[j].name) {
                                anotherContact.splice([j], 1);
                            }
                        }
                    };                   
                    

                    var dataSource = { data: $scope.anotherContacts, schema: { model: { id: "vendor_contact_id" } } }
                    $scope.isSelected = GridService.isSelected;
                    $scope.allSelected = GridService.allSelected;
                    $scope.select = GridService.select;
                    $scope.allPagesSelected = GridService.allPagesSelected;
                    $scope.options = GridService.getStructure(dataSource, columns, null, gridOptions);
                    $scope.dataBound = function (grid) {
                        GridService.dataBound(grid);
                    };
                }
            });

            $scope.save = function () {
                var isPurchaseOrderAssociation = $scope.showExistingContacts && $scope.selectedTabIndex === 0;

                if (isPurchaseOrderAssociation) {
                    var selecteds = GridService.getSelecteds($scope.existingContactsGrid);
                    if (!selecteds || !selecteds.length) {
                        toastr.error('Select at least one vendor contact');
                        return;
                    }

                    $scope.addEditContactForm.$setSubmitted();

                    WebApiService.genericController['save']({
                        controller: 'PurchaseOrders',
                        action: 'AddVendorContacts',
                        domain_id: local.purchaseOrder.domain_id,
                        project_id: local.purchaseOrder.project_id,
                        phase_id: local.purchaseOrder.po_id
                    }, selecteds, function () {
                        toastr.success(local.type + ' contact(s) added to Purchase Order');
                        ProgressService.unblockScreen();
                        $mdDialog.hide(selecteds);
                    }, function () {
                        ProgressService.unblockScreen();
                        toastr.error('Error trying to add contacts to Purchase Order. Please contact the technical support!');
                    });
                    return;
                }

                if ($scope.addEditContactForm.$valid) {
                    $scope.addEditContactForm.$setSubmitted();
                    var method = $scope.newContact ? 'save' : 'update';

                    ProgressService.blockScreen();
                    WebApiService.genericController[method]({
                        controller: local.type + 'Contacts',
                        action: 'Item',
                        domain_id: params.domain_id,
                        project_id: params[local.type + '_id'],
                        phase_id: AuthService.getLoggedDomain(),
                        department_id: oldName
                    }, $scope.contact, function (data) {
                        toastr.success(local.type + ' contact ' + ($scope.newContact ? 'saved' : 'updated'));
                        ProgressService.unblockScreen();
                        $mdDialog.hide([data.toJSON()]);
                    }, function () {
                        ProgressService.unblockScreen();
                        toastr.error('Error to try ' + ($scope.newContact ? 'save' : 'update') + ' the ' + local.type + ' contact. Please contact the technical support!');
                    });

                } else {
                    toastr.error('Please make sure you enter correctly all fields');
                }
            };

            $scope.close = function () {
                $mdDialog.cancel();
            };
}]);