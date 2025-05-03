xPlanner.controller('DisplayColumnsCtrl', ['$scope', '$mdDialog', 'local', 'GridService', 'CheckboxEditor', 'ProgressService', 'AssetColumnsType', 'KendoAssetInventoryService', 'toastr',
    function ($scope, $mdDialog, local, GridService, CheckboxEditor, ProgressService, AssetColumnsType, KendoAssetInventoryService, toastr) {

        $scope.groups = AssetColumnsType.distinctTypes;

        local.columns = angular.copy(local.columns);
        local.consolidated = angular.copy(local.consolidated);
        var column_type = AssetColumnsType.fieldsMap;        
        var defaultViewColumn = [
            "Phase", "Department", "Room No.", "Room Name", "Resp", "Type", "Code", "Description", "Status", "PO Status", "Lease qty", "DNP qty", "Net New",
            "PO qty", "PO amt", "PO delta", "Unit Budget", "Total Budget", "PO Number", "Delivered Date", "Photo", "CAD ID", "Model No.", "Model Name", "Manufacturer",
            "Room Count", "Planned qty", "Shop Drawing", "Flag", "Lead Time", "Install Date", "Profile", "Cost Center",
        ];

        var requiredColumns = ["Inventory ID", "JSN", "Code", "Source Location", "Target Location"];

        var gridColumn = {
            display: [],
            consolidated: []
        };

        var title_changes = {
            clearance_top: 'Clearance, Top(in)',
            clearance_bottom: 'Clearance, Bottom(in)',
            clearance_right: 'Clearance, Right(in)',
            clearance_left: 'Clearance, Left(in)',
            clearance_front: 'Clearance, Front(in)',
            clearance_back: 'Clearance, Back(in)',
            loaded_weight: 'Weight, Loaded(lb)',
            ship_weight: 'Weight, Shipping (lb)',
            depth: 'Dimension, Depth(in)',
            height: 'Dimension, Height(in)',
            width: 'Dimension, Width(in)',
            mounting_height: 'Dimension, Mounting Height(in)'
        };

        /* begin - grid configuration */

        //columns
        var columns = [{ field: 'column', title: 'Field' },
        { field: 'type', title: 'Group' },
        { field: 'display', title: 'Display', template: "<section layout=\"column\" layout-align=\"none center\"><md-checkbox style=\"margin-top: 0px; margin-bottom: 0px\" class=\"checkbox\" ng-checked=\"dataItem.display\" aria-label=\"checkbox\"></md-checkbox></section>", editor: CheckboxEditor },
            { field: 'consolidate', title: 'Consolidate', template: "<section layout=\"column\" layout-align=\"none center\"><md-checkbox style=\"margin-top: 0px; margin-bottom: 0px\" class=\"checkbox\" ng-checked=\"CheckRequiredConsolidatedColumns(dataItem)\" ng-disabled=lockConsolidatedCheckBox(dataItem) aria-label=\"checkbox\"></md-checkbox></section>", editor: CheckboxEditor }
        ];

        // datasource
        var dataSource = {
            data: local.columns.map(function (item) {
                return {
                    column: title_changes[item.field] || item.title, type: column_type[item.field], display: !item.hidden, consolidate: local.consolidated.includes(item.field)
                };
            })
                .filter(function (item) { return item.column; }),
            sort: { field: "column", dir: "asc" },
            group: [{ field: "type" }],
            schema: {
                model: {
                    fields: {
                        display: { type: 'boolean' },
                        type: { editable: false },
                        column: { editable: false }
                    }
                }
            },
        };

        $scope.gridHeight = window.innerHeight - 210;

        $scope.options = GridService.getStructure(dataSource, columns, null,
            { height: $scope.gridHeight, groupable: true, editable: true }, { pageSizeDefault: 100 });
        /* end - grid configuration */

        $scope.collapseExpand = GridService.collapseExpand;

        $scope.dataBound = function () {
            ProgressService.unblockScreen();
            GridService.dataBound($scope.displayColumnsGrid);
        }    
        
        $scope.lockConsolidatedCheckBox = function (dataItem) {
            var returnValue = requiredColumns.includes(dataItem.column) == true ? true : false;
            return returnValue;            
        }

        $scope.CheckRequiredConsolidatedColumns = function (dataItem) {
            var returnValue = requiredColumns.includes(dataItem.column) == true ? true : dataItem.consolidate;
            return returnValue;
        }

        $scope.save = function () {

            var result = $scope.displayColumnsGrid.dataSource.data();
            
            for (var i = 0; i < local.columns.length; i++) {

                var column = result.find(function (item) { return item.column == (title_changes[local.columns[i].field] || local.columns[i].title); });
                if (column) {
                    local.columns[i].hidden = !column.display;
                    if (column.consolidate) {
                        gridColumn.consolidated.push(local.columns[i].field);                        
                    }
                }
            }

            gridColumn.display = local.columns;
            ProgressService.blockScreen();
            $mdDialog.hide(gridColumn);
        }

        function _modififySelectForGroupColumns(group, newValue) {

            // A valid group must be informed and only boolean values are accepted by the field
            if (!group || typeof newValue !== "boolean" || $scope.groups.indexOf(group) === -1) {
                return;
            }


            $scope.displayColumnsGrid.dataSource.data(
                $scope.displayColumnsGrid.dataSource.data().map(function (item) {
                    if (item.type === group) { item.display = newValue;}
                    return item;
                })
            );
        }


        // Reset Default View Columns
        $scope.resetDefaultColumns = function () {
            var columns = $scope.displayColumnsGrid.dataSource.data();
            columns.forEach(function (item) {
                item.display = defaultViewColumn.includes(item.column) == true ? true : false;
            })
        }

        // Reset Default Consolidation
        $scope.resetDefaultConsolidation = function () {
            var notDefaultConsolidatedFields = KendoAssetInventoryService.GetNotConsolidatedFields();
            var columns = $scope.displayColumnsGrid.dataSource.data();
            columns.forEach(function (item) {
                var aux = local.columns.find((element) => element.title == item.column).field;
                item.consolidate = notDefaultConsolidatedFields.includes(aux) == true ? false : true;
            })

        }

        // Select all columns to consolidate
        $scope.selectAllToConsolidate = function (group) {
            if (!group)
                toastr.error('You must select a group to consolidate.');
            var columns = $scope.displayColumnsGrid.dataSource.data();
            columns.forEach(function (column) {
                if (column.type == group) {
                    column.consolidate = true;
                }
            });
        }

        // Unselect all columns to consolidate
        $scope.unselectAllToConsolidate = function (group) {
            if (!group)
                toastr.error('You must select a group to unconsolidate.');
            var columns = $scope.displayColumnsGrid.dataSource.data();
            columns.forEach(function (column) {
                if (column.type == group) {
                    column.consolidate = false;
                }
            });
        }   

        // Select all columns from group to display
        $scope.selectAllToDisplay = function (group) {
            if (!group)
                toastr.error('You must select a group to display.');
            _modififySelectForGroupColumns(group, true);
        }

        // Unselect all columns from group to hide
        $scope.unselectAllToDisplay = function (group) {
            if (!group) 
                toastr.error('You must select a group to hide.');
            
            _modififySelectForGroupColumns(group, false);
        }

        $scope.close = function () {
            $mdDialog.cancel();
        }
    }]);