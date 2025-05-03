xPlanner.controller('CustomizedDifferencesCtrl', ['$scope', 'local', 'AssetColumnsName', 'AssetColumnsType', 'WebApiService', 'toastr', 'GridService',
        'ProgressService', '$mdDialog',
    function ($scope, local, AssetColumnsName, AssetColumnsType, WebApiService, toastr, GridService, ProgressService,
        $mdDialog) {

        WebApiService.genericController.query({
            controller: 'assets', action: 'CustomizedDifferences', domain_id: local.customizedAsset.domain_id, project_id: local.customizedAsset.asset_id
        },
            function (data) {
                //columns
                var columns = [{ field: 'group', title: 'Group', hidden: true },
                    { field: 'column', title: 'Field' },
                    { field: 'customized', title: 'Customized Asset' },
                    { field: 'original', title: 'Original Asset' }
                ];

                // datasource
                var dataSource = {
                    data: data.map(function (item) {
                        return {
                            column: AssetColumnsName[item.field] || item.field, group: AssetColumnsType.fieldsMap[item.field],
                            customized: item.customized, original: item.original
                        };
                    })
                        .filter(function (item) { return item.column; }),
                    sort: { field: "column", dir: "asc" },
                    group: [{ field: "group" }]
                };

                $scope.gridHeight = window.innerHeight - 170;

                $scope.options = GridService.getStructure(dataSource, columns, null,
                    { height: $scope.gridHeight, groupable: true, editable: false }, { pageSizeDefault: 100 });
                /* end - grid configuration */

                $scope.collapseExpand = GridService.collapseExpand;

                $scope.dataBound = function (grid) {
                    ProgressService.unblockScreen();
                    GridService.dataBound($scope.differencesGrid);
                }

            }, function (error) {
                toastr.error('Error to try get the differences, please contact the technical support');
            });

        $scope.close = function () {
            $mdDialog.hide();
        }
    }]);