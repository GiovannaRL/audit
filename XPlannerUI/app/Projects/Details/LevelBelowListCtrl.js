xPlanner.controller('LevelBelowListCtrl', ['$scope', 'GridService', 'HttpService', 'AuthService', '$state', 'DialogService',
            'ProgressService', 'WebApiService', 'toastr',
    function ($scope, GridService, HttpService, AuthService, $state, DialogService, ProgressService, WebApiService, toastr) {

        $scope.type = $scope.params.department_id ? 'room' : $scope.params.phase_id ? 'department' : 'phase';
        $scope.isNotViewer = (AuthService.getLoggedUserType() != "3");

        var typePlural = $scope.type + 's';
        var typeCapitalize = $scope.type.charAt(0).toUpperCase() + $scope.type.slice(1);

        /* kendo ui grid configurations*/
        var toolbar = {
            template: "<section layout=\"row\" ng-cloak ng-if=\"" + $scope.isNotViewer + "\">" +
                        "<section layout=\"row\" layout-align=\"start center\" class=\"gray-color\">" +
                            "<button class=\"md-icon-button md-button\" ng-click=\"openAddModal()\">" +
                                "<i class=\"material-icons\">add</i>" +
                                "<md-tooltip md-direction=\"bottom\">Add {{type | capitalize}}</md-tooltip>" +
                            "</button>" +
                            "<button class=\"md-icon-button md-button\" ng-click=\"edit()\">" +
                                "<i class=\"material-icons\">edit</i>" +
                                "<md-tooltip md-direction=\"bottom\">Edit {{type | capitalize}}</md-tooltip>" +
                            "</button>" +
                            "<button class=\"md-icon-button md-button\" ng-click=\"delete()\">" +
                                "<i class=\"material-icons\">delete</i>" +
                                "<md-tooltip md-direction=\"bottom\">Delete {{type | capitalize }}(s)</md-tooltip>" +
                            "</button>" +

                            "<button class=\"md-icon-button md-button\" ng-click=\"reload()\">" +
                                "<i class=\"material-icons\">refresh</i>" +
                                "<md-tooltip md-direction=\"bottom\">Reload {{type | capitalize}}s</md-tooltip>" +
                            "</button>" +
                        "</section>" +
                        //"<section layout=\"row\" layout-align=\"end center\" flex=\"100\">" +
                        //    "<button class=\"md-icon-button md-button\" ng-click=\"openAddModal()\">" +
                        //        "<md-icon class=\"md-accent\">add_circle</md-icon>" +
                        //        "<md-tooltip md-direction=\"bottom\">Add {{type | capitalize}}</md-tooltip>" +
                        //    "</button>" +
                        //"</section>" +
                    "</section>"
        };

        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic(typePlural, $scope.type === 'phase' ? 'All' : 'AllWithFinancials',
                        AuthService.getLoggedDomain(), $scope.params.project_id,
                        $scope.params.phase_id, $scope.params.department_id, $scope.params.room_id),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            schema: {
                model: {
                    id: $scope.type + "_id",
                    fields: {
                        equip_move_in_date: {
                            type: "date"
                        },
                        ofci_delivery: {
                            type: "date"
                        },
                        occupancy_date: {
                            type: "date"
                        }
                    }
                }
            }
        };
        var gridOptions = { noRecords: "No " + typePlural + " available", height: 370 };

        function _getColumns() {
            switch ($scope.type) {
                case 'room':
                    columns = [{ headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(itemsGrid)\" ng-checked=\"allPagesSelected(itemsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, itemsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, itemsGrid)\" ng-checked=\"isSelected(itemsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                        { field: "drawing_room_name", title: "Room Name", width: 130 },
                        { field: "drawing_room_number", title: "Room No", width: 120 },
                        { field: "final_room_name", title: "Wayfinding Name", width: 170 },
                        { field: "final_room_number", title: "Wayfinding No", width: 150 },
                        { field: "room_quantity", title: "Room Count", width: 100 },
                        { field: "total_budget_amt", title: "Tot Bgt", width: 120, template: "<aw-currency value=\"#: total_budget_amt # \"></aw-currency>" },
                        { field: "total_po_amt", title: "PO Amt", width: 120, template: "<aw-currency value=\"#: total_po_amt # \"></aw-currency>" },
                        { field: "buyout_delta", title: "+/-", width: 120, template: "<aw-currency value=\"#: buyout_delta # \"></aw-currency>" },
                         {
                             headerTemplate:
                             "<button style=\"margin-left: -0.85em; padding-bottom: 0.2em;\" class=\"md-icon-button md-button no-button\"><i class=\"material-icons no-button\">comment</i>" +
                                 "<md-tooltip md-direction=\"bottom\">Comment</md-tooltip>" +
                             "</button>", template: "<div ng-if=\" #: comment != null # \" align=center><md-icon class=\"no-button grid-item-color\">comment</md-icon><md-tooltip md-direction=\"bottom\">#: comment #</md-tooltip></div>", width: 70
                         },
                    ];
                    break;
                case 'department':
                    columns = [{ headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(itemsGrid)\" ng-checked=\"allPagesSelected(itemsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, itemsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, itemsGrid)\" ng-checked=\"isSelected(itemsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                        { field: "description", title: "Name", width: 170 },
                        { field: "type", title: "Type", width: 150 },
                        { field: "area", title: "Area", width: 100 },
                        { field: "num_rooms", title: "# Rooms", width: 120 },
                        { field: "total_budget_amt", title: "Tot Bgt", width: 120, template: "<aw-currency value=\"#: total_budget_amt # \"></aw-currency>" },
                        { field: "total_po_amt", title: "PO Amt", width: 120, template: "<aw-currency value=\"#: total_po_amt # \"></aw-currency>" },
                        { field: "buyout_delta", title: "+/-", width: 120, template: "<aw-currency value=\"#: buyout_delta # \"></aw-currency>" },
                         {
                             headerTemplate:
                             "<button style=\"margin-left: -0.85em; padding-bottom: 0.2em;\" class=\"md-icon-button md-button no-button\"><i class=\"material-icons no-button\">comment</i>" +
                                 "<md-tooltip md-direction=\"bottom\">Comment</md-tooltip>" +
                             "</button>", template: "<div ng-if=\" #: comment != null # \" align=center><md-icon class=\"no-button grid-item-color\">comment</md-icon><md-tooltip md-direction=\"bottom\">#: comment #</md-tooltip></div>", width: 70
                         },
                    ];
                    break;
                case 'phase':
                    columns = [{ headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(itemsGrid)\" ng-checked=\"allPagesSelected(itemsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, itemsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, itemsGrid)\" ng-checked=\"isSelected(itemsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                        { field: "description", title: "Name", width: 270 },
                        { field: "equip_move_in_date", title: "Asset Date", width: 130, template: "#: equip_move_in_date ? kendo.toString(kendo.parseDate(equip_move_in_date), \"MM-dd-yyyy\") : '' #" },
                        { field: "ofci_delivery", title: "OFCI Delivery", width: 150, template: "#: ofci_delivery ? kendo.toString(kendo.parseDate(ofci_delivery), \"MM-dd-yyyy\") : '' #" },
                        { field: "occupancy_date", title: "Go Live", width: 130, template: "#: occupancy_date ? kendo.toString(kendo.parseDate(occupancy_date), \"MM-dd-yyyy\") : '' #" },
                         {
                             headerTemplate:
                             "<div align=center><button  style=\"padding-bottom: 0.2em;\" class=\"md-icon-button md-button no-button\"><i class=\"material-icons no-button\">comment</i>" +
                                 "<md-tooltip md-direction=\"bottom\">Comment</md-tooltip>" +
                             "</button></div>", template: "<div ng-if=\" #: comment != null # \" align=center><md-icon class=\"no-button grid-item-color\">comment</md-icon><md-tooltip md-direction=\"bottom\">#: comment #</md-tooltip></div>", width: 100
                         },
                    ];
                    break;
            }

            return columns;
        };

        $scope.options = GridService.getStructure(dataSource, _getColumns(), toolbar, gridOptions);

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var data = grid.dataItem(this);
                    $state.go('index.' + $scope.type, data);
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.itemsGrid);
            GridService.dataBound($scope.itemsGrid);
        }

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        /* END - kendo ui grid configurations*/

        $scope.edit = function () {
            if (!validateAccess()) {
                return;
            }

            if (GridService.verifySelected('edit', $scope.type, $scope.itemsGrid, true)) {
                $state.go('index.' + $scope.type, GridService.getSelecteds($scope.itemsGrid)[0]);
            }
        };

        /* Open modal to add room */
        $scope.openAddModal = function () {
            if (!validateAccess()) {
                return;
            }

            var template;
            var controller = 'LevelBelowAddCtrl';

            switch ($scope.type) {
                case 'room':
                    template = 'app/Projects/Details/Modals/AddRoom.html';
                    controller = 'AddRoomCtrl';
                    $scope.params.department_type_id = $scope.data.department_type.department_type_id;
                    break;
                case 'department':
                    template = 'app/Projects/Details/Modals/AddDepartment.html';
                    break;
                case 'phase':
                    template = 'app/Projects/Details/Modals/AddPhase.html';
                    break;
            };


            DialogService.openModal(template, controller, { params: $scope.params, type: $scope.type }, true).then(
                function (item) {
                    if ($scope.type == 'room') {
                        var rooms = item.text.split('||');
                        for (var i = 0; i < rooms.length; i++) {
                            var data = JSON.parse(rooms[i]);
                            $scope.addItemTreeView(data);
                        }

                    }
                    else {
                        $scope.addItemTreeView(item);

                        if ($scope.type !== 'phase')
                            $scope.itemsGrid.dataSource.read();
                        else
                            $scope.itemsGrid.dataSource.pushCreate({
                                domain_id: item.domain_id, project_id: item.project_id, phase_id: item.phase_id,
                                description: item.description, equip_move_in_date: item.equip_move_in_date, ofci_delivery: item.ofci_delivery,
                                occupancy_date: item.occupancy_date, comment: item.comment
                            });
                    }
                });
        };
        /* END - open modal to add room */

        /* Delete selected rooms */
        $scope.delete = function () {
            if (!validateAccess()) {
                return;
            }

            if (GridService.verifySelected('delete', $scope.type, $scope.itemsGrid)) {
                DialogService.Confirm('Are you sure?', 'The ' + $scope.type + '(s) will be deleted permanently!').then(function () {

                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.genericController,
                        function (item) {
                            return {
                                domain_id: item.domain_id, project_id: item.project_id, phase_id: item.phase_id,
                                department_id: item.department_id, room_id: item.room_id, controller: typePlural, action: 'Item'
                            };
                        },
                        $scope.itemsGrid).then(function () {
                            ProgressService.unblockScreen();
                            toastr.success(typeCapitalize + '(s) Deleted');
                            $scope.reloadTreeview();
                        }, function (error) {
                            if (error.status == 409)
                                toastr.info('Some, or all, of the ' + typePlural + ' could not be deleted. ' + error.data);
                            else
                                toastr.error('Error to delete ' + $scope.type + '(s), please contact the technical support');
                            $scope.reloadTreeview();
                            ProgressService.unblockScreen();
                        });
                    GridService.unselectAll($scope.itemsGrid);
                });
            }
        };
        /* END - Delete selected rooms */

        /* Refresh grid items */
        $scope.reload = function () {
            $scope.itemsGrid.dataSource.read();
        };
        /* END - Refresh grid items */

        function validateAccess() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return false;
            }
            if (AuthService.getProjectStatus($scope.params.project_id) == "L") {
                DialogService.LockedProjectModal();
                return false;
            }

            return true;
        }
    }]);