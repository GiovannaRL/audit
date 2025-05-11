//xPlanner.directive('awItConnectivityGrid', ['GridService', 'AuthService', 'HttpService', 'ProgressService', 'toastr',
//    'DialogService', 'WebApiService',
//function (GridService, AuthService, HttpService, ProgressService, toastr, DialogService, WebApiService) {
//    return {
//        restrict: 'E',
//        scope: {
//            isTemplate: '=',
//            params: '=',
//            gridHeight: '@',
//            hideToolbar: '=',
//            emitButtons: '&',
//            inAccordion: '=',
//            linkedRoom: '@'
//        },
//        link: function (scope, elem, attrs, ctrl) {

//            /* Grid configuration */
//            var dataSource = {
//                transport: {
//                    read: {
//                        url: HttpService.it_connectivity('All', AuthService.getLoggedDomain(), scope.params.project_id, scope.params.phase_id, scope.params.department_id, scope.params.room_id),
//                        headers: {
//                            Authorization: 'Bearer ' + AuthService.getAccessToken()
//                        }
//                    }
//                },
//                error: function () {
//                    ProgressService.unblockScreen();
//                    toastr.error("Error to retrieve it connectvity information from server, please contact technical support");
//                },
//                schema: {
//                    model: {
//                        id: "box_id"
//                    }
//                }
//            };
//            var gridOptions = { groupable: true, noRecords: "No boxes available", height: scope.gridHeight || (window.innerHeight - 210) };
//            var exportConfig = {
//                excel: {
//                    fileName: 'IT Connectivity Report',
//                    allPages: true,
//                    filterable: true
//                }
//            };

//            var columns = [
//                { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(itConnectivityGrid)\" ng-checked=\"allPagesSelected(itConnectivityGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, itConnectivityGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, itConnectivityGrid)\" ng-checked=\"isSelected(itConnectivityGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
//                { field: "box_number", title: "Box no.", width: 100, template: "<center>#: box_number#</center>" },
//                { field: "name", title: "Description", width: 150 },
//                { field: "mis_room", title: "Default MIS Room", width: 150 },
//                { field: "max_jack_connections", title: "Allowed Jacks", width: 150, template: "<center>#: max_jack_connections#</center>" },
//                { field: "open_inserts", title: "Open inserts", width: 130, template: "<center>#: open_inserts#</center>" },
//                { field: "connected_assets", title: "Jack Connections(Connected Equipment)", width: 280, template: "#= connected_assets #" }
//            ];

//            scope.options = GridService.getStructure(dataSource, columns, null, gridOptions, null, exportConfig);

//            function linkedTemplateAlert() {
//                DialogService.Alert('Linked Room', 'This room is linked to a template and the connectivity information cannot be modified. To modified these informations you need to unlink the room in the template section before.');
//            }

//            function setDbClick(grid) {
//                if (!scope.hideToolbar && grid) {
//                    grid.tbody.find("tr").dblclick(function () {
//                        if (scope.linkedRoom == 'false') {
//                            var box = grid.dataItem(this);
//                            scope.openAddEditModal(true, box);
//                        } else {
//                            linkedTemplateAlert();
//                        }
//                    });
//                }
//            };

//            scope.dataBound = function () {
//                setDbClick(scope.itConnectivityGrid);
//                GridService.dataBound(scope.itConnectivityGrid);
//            };

//            /* Select the grid's rows */
//            scope.isSelected = GridService.isSelected;
//            scope.allSelected = GridService.allSelected;
//            scope.select = GridService.select;
//            scope.allPagesSelected = GridService.allPagesSelected;
//            /* END - Select the grid's rows */

//            /* END - Grid configuration*/

//            /* Open the add/edit box modal*/
//            scope.openAddEditModal = function (edit, box) {

//                if (!edit || (box || GridService.verifySelected('edit', 'box', scope.itConnectivityGrid))) {
//                    if (scope.linkedRoom == 'false') {
//                        var template = (edit && !box && GridService.getSelecteds(scope.itConnectivityGrid).length > 1) ?
//                            'app/Projects/Connectivity/Modals/EditMultiBox.html' : 'app/Projects/Connectivity/Modals/AddBoxConnectivity.html';

//                        DialogService.openModal(template, 'AddBoxConnectivityCtrl',
//                            { params: scope.params, edit: edit, boxes: box ? [box] : GridService.getSelecteds(scope.itConnectivityGrid), template: scope.isTemplate }, true)
//                        .then(function () {
//                            scope.itConnectivityGrid.dataSource.read();
//                        });
//                    } else {
//                        linkedTemplateAlert();
//                    }
//                }
//            };
//            /* END - Open the add/edit box modal*/

//            /* delete boxes */
//            scope.delete = function () {

//                if (GridService.verifySelected('delete', 'box', scope.itConnectivityGrid)) {
//                    if (scope.linkedRoom == 'false') {
//                        DialogService.Confirm('Are you sure?', 'The boxes will be deleted permanently.').then(function () {
//                            ProgressService.blockScreen();
//                            GridService.deleteItems(WebApiService.genericController,
//                                function (i) { return { controller: 'ITConnectivity', action: 'Item', domain_id: i.domain_id, project_id: i.box_id }; },
//                                scope.itConnectivityGrid).then(function () {
//                                    ProgressService.unblockScreen();
//                                    toastr.success('Boxes Deleted');
//                                }, function () {
//                                    ProgressService.unblockScreen();
//                                    toastr.error('Error to delete boxes, please contact the technical support');
//                                });
//                            GridService.unselectAll(scope.itConnectivityGrid);
//                        });
//                    } else {
//                        linkedTemplateAlert();
//                    }
//                }
//            };
//            /* END - delete boxes */

//            scope.exportGrid = function () {
//                GridService.exportGrid('excel', scope.itConnectivityGrid);
//            };
//        },
//        templateUrl: 'app/Directives/Elements/ITConnectivityGrid.html'
//    }
//}]);