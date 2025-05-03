xPlanner.directive('awContactsGrid', ['GridService', 'AuthService', 'HttpService', 'ProgressService', 'toastr', 'DialogService', 'WebApiService',
    function (GridService, AuthService, HttpService, ProgressService, toastr, DialogService, WebApiService) {
        return {
            restrict: 'E',
            scope: {
                data: '=',
                canEdit: '=',
                contactType: '@',
                params: '=',
                domainProperty: '@',
                idProperty: '@',
                gridHeight: '@',
                purchaseOrder: '='
            },
            link: function (scope, elem, attrs, ctrl) {

                scope.gridHeight = scope.gridHeight || 300;
                var isVendor = scope.contactType === "vendor";

                /* kendo ui grid configurations*/
                var dataSource = angular.extend({}, scope.data ? { data: scope.data } : {
                    transport: {
                        read: {
                            url: HttpService.generic(scope.contactType + "Contacts", "AllWithID", scope.params[scope.domainProperty || 'domain_id'], scope.params[scope.idProperty || (scope.contactType + '_id')], AuthService.getLoggedDomain()),
                            headers: {
                                Authorization: "Bearer " + AuthService.getAccessToken()
                            },
                            data: {
                                purchaseOrderId: scope.purchaseOrder && scope.purchaseOrder.po_id,
                                purchaseOrderDomainId: scope.purchaseOrder && scope.purchaseOrder.domain_id,
                                purchaseOrderProjectId: scope.purchaseOrder && scope.purchaseOrder.project_id
                            }
                        }
                    }
                }, { schema: { model: { id: isVendor ? "vendor_contact_id" : "id" } } });

                var gridOptions = {
                    noRecords: "No contacts available",
                    height: scope.gridHeight
                };
                var columns = [
                    { field: "name", title: "Name", width: 150 },
                    { field: "title", title: "Title", width: 150 },
                    { field: "email", title: "Email", width: 150 },
                    { field: "phone", title: "Phone", width: 130 },
                    { field: "mobile", title: "Mobile", width: 130 },
                    { field: "fax", title: "Fax", width: 130 },
                    {
                        headerTemplate:
                            "<div align=center><button ng-style=\"{padding: '0px', bottom: canEdit ? '4px' : '-4px', left: canEdit ? '-4px' : '0px'}\" class=\"md-icon-button md-button no-button\"><i class=\"material-icons no-button\">comment</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Comment</md-tooltip>" +
                            "</button></div>", template: "<div ng-if=\" #: comment != null # \" align=center><md-icon class=\"no-button grid-item-color\">comment</md-icon><md-tooltip md-direction=\"bottom\">#: comment #</md-tooltip></div>", width: 70
                    }
                ];

                if (scope.canEdit) {
                    columns.splice(0, 0,
                        { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(contactsGrid)\" ng-checked=\"allPagesSelected(contactsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, contactsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, contactsGrid)\" ng-checked=\"isSelected(contactsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" });
                }

                scope.options = GridService.getStructure(dataSource, columns, null, gridOptions);

                function setDbClick(grid) {
                    if (grid) {
                        grid.tbody.find("tr").dblclick(function () {
                            var contact = grid.dataItem(this);
                            scope.openAddEditModal(true, contact);
                        });
                    }
                };

                scope.dataBound = function () {
                    if (scope.canEdit)
                        setDbClick(scope.contactsGrid);
                    GridService.dataBound(scope.contactsGrid);
                };
                /* END - kendo ui grid configurations*/

                /* Select the grid's rows */
                if (scope.canEdit) {
                    scope.isSelected = GridService.isSelected;
                    scope.allSelected = GridService.allSelected;
                    scope.select = GridService.select;
                    scope.allPagesSelected = GridService.allPagesSelected;
                }
                /* END - Select the grid's rows */

                /* Open the add/edit contact modal*/
                scope.openAddEditModal = function (edit, contact) {
                    if (edit && !contact) {
                        if (!GridService.verifySelected('edit', 'contact', scope.contactsGrid, true)) return;

                        contact = GridService.getSelecteds(scope.contactsGrid)[0];
                    }

                    // Check if it is not updating contact from a different domain
                    var contact_domain_id = contact && (contact.domain_id || contact.contact_domain_id); // Different field name for vendor and manufacturer entities
                    if (edit && contact_domain_id !== AuthService.getLoggedDomain()) {
                        toastr.error('It is not possible to edit this contact. It does not belong to your organization');
                        return;
                    }

                    DialogService.openModal('app/Contact/Modals/AddEditContact.html', 'AddEditContactCtrl',
                        { type: scope.contactType, contact: contact, purchaseOrder: scope.purchaseOrder, params: scope.params, POContact: scope.contactsGrid._data },
                        true
                    ).then(function (data) {
                        edit ? GridService.updateItems(scope.contactsGrid, data) : scope.contactsGrid.dataSource.pushCreate(data);
                    });
                };
                /* END - Open the add/edit contact modal*/

                /* Delete selected contacts*/
                scope.delete = function () {
                    if (GridService.verifySelected('delete', 'contact', scope.contactsGrid)) {

                        if (scope.purchaseOrder) {
                            var selecteds = GridService.getSelecteds(scope.contactsGrid);

                            WebApiService.genericController['save']({
                                controller: 'PurchaseOrders',
                                action: 'RemoveVendorContacts',
                                domain_id: scope.purchaseOrder.domain_id,
                                project_id: scope.purchaseOrder.project_id,
                                phase_id: scope.purchaseOrder.po_id
                            }, selecteds, function () {
                                angular.forEach(selecteds, function (item) {
                                    GridService.RemoveItem(scope.contactsGrid, item)
                                });
                                ProgressService.unblockScreen();
                                toastr.success('Contact(s) removed');
                            }, function () {
                                ProgressService.unblockScreen();
                                toastr.error('Error to remove contact(s), please contact the technical support');
                            });

                            return;
                        }

                        DialogService.Confirm('Are you sure?', 'The contact(s) will be deleted permanently').then(function () {
                            ProgressService.blockScreen();
                            GridService.deleteItems(WebApiService.genericController,
                                function (item) { return { controller: scope.contactType + 'Contacts', action: "Item", domain_id: item[scope.contactType + '_domain_id'], project_id: item[scope.contactType + '_id'], phase_id: AuthService.getLoggedDomain(), department_id: item.name }; },
                                scope.contactsGrid, null, true).then(function (items) {
                                    ProgressService.unblockScreen();
                                    toastr.success('Contact(s) Deleted');
                                }, function () {
                                    toastr.error('Error to delete contact(s), please contact the technical support');
                                    ProgressService.unblockScreen();
                                });
                            GridService.unselectAll(scope.contactsGrid);
                        });
                    }
                };
                /* END - Delete selected cost centers */

            },
            templateUrl: 'app/Directives/Elements/ContactsGrid.html'
        }
    }]);