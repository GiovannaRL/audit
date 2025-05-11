xPlanner.controller('CompareAssetsCtrl', ['$scope', '$mdDialog', 'local', 'GridService', 'ProgressService', 'toastr', 'AssetInventoryColumns', 
    function ($scope, $mdDialog, local, GridService, ProgressService, toastr, AssetInventoryColumns) {

        var content = compareAssets(local.items[0], local.items[1], local.consolidatedColumns, local.isConsolidated);

        $scope.asset1Content = local.items[0].asset_code + ' (IDs: ' + reduceInventoryIdColumn(local.items[0].inventory_id, local.items[0].inventory_ids) + ')';
        $scope.asset2Content = local.items[1].asset_code + ' (IDs: ' + reduceInventoryIdColumn(local.items[1].inventory_id, local.items[1].inventory_ids) + ')';

        //columns
        var columns = [{ field: 'column', title: 'Column Name', width: '100px' },
            { field: 'asset1', title: 'Asset 1', width: '110px' },
            { field: 'asset2', title: 'Asset 2', width: '110px' },
            {
                field: 'group', title: 'Group', width: '80px', hidden: !local.isConsolidated,
                template: function (dataItem) {
                    return dataItem.group == 'consolidated' ? "<section align=center ><i class=\"material-icons no-button\" style=\"color: green\">check</i></section>" : ""
                }
            }
        ];

        

        // datasource
        var dataSource = {
            data: content,
            group: local.isConsolidated ? [{ field: "group" }] : []
            }

        $scope.gridHeight = window.innerHeight - 210;

        $scope.options = GridService.getStructure(dataSource, columns, null,
            { height: $scope.gridHeight, groupable: true, editable: false, noRecords: "The assets selected are equal", }, { pageSizeDefault: 100 });
        /* end - grid configuration */

        $scope.collapseExpand = GridService.collapseExpand;

        $scope.dataBound = function () {
            ProgressService.unblockScreen();
            GridService.dataBound($scope.compareAssetsGrid);
        }

        function reduceInventoryIdColumn(inventoryId, inventoryIds) {
            var correctId = inventoryId == 0 ? inventoryIds : inventoryId;
            return correctId.length > 25 ? correctId.substring(1, 25) + '...' : correctId;
        }

        function compareAssets(asset1, asset2, consolidatedColumns, isConsolidated) {

            var columnsToIgnore = ["uid", "inventory_ids", "date_modified", "unit_budget", "id", "total_assets_options", "departments_qty", "phases_qty", "rooms_qty", "pdoc_filename", "mounting_height", "po_status", "quantity" ];
            var comparedColumns = new Array();
            var columnName = '';
            for (var prop in asset1) {

                if (asset1[prop] != asset2[prop] && typeof asset1[prop] != 'object' && columnsToIgnore.indexOf(prop) < 0 && prop.slice(-3) != '_ow' && prop.slice(-3) != '_id') {

                    columnName = AssetInventoryColumns.hasOwnProperty(prop) ? AssetInventoryColumns[prop] : prop;
                    comparedColumns.push({ column: columnName, asset1: asset1[prop], asset2: asset2[prop], group: (consolidatedColumns.indexOf(prop) < 0 || !isConsolidated ? 'unconsolidated' : 'consolidated') });
                }
                    
            }

            return comparedColumns;
        }


        $scope.close = function () {
            $mdDialog.cancel();
        }
    }]);