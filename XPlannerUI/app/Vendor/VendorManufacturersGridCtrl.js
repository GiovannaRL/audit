xPlanner.controller('VendorManufacturersGridCtrl', ['$scope', 'HttpService', 'GridService', '$state', 'AuthService',
    function ($scope, HttpService, GridService, $state, AuthService) {

        /* kendo ui grid configurations*/
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("vendor", "Manufacturers", $scope.params.domain_id, $scope.params.vendor_id),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            schema: {
                model: {
                    id: "manufacturer_id"
                }
            }
        };
        var gridOptions = {
            noRecords: "No manufacturers available",
            height: 300
        };
        var columns = [
                { field: "manufacturer_description", title: "Name" },
                { field: "added_by", title: "Added By" },
                {
                    headerTemplate:
                    "<div align=center><button style=\"padding: 0px; bottom: -4px;\" class=\"md-icon-button md-button no-button\"><i class=\"material-icons no-button\">comment</i><div class=\"md-ripple-container\"></div>" +
                        "<md-tooltip md-direction=\"bottom\">Comment</md-tooltip>" +
                    "</button></div>", template: "<div ng-if=\" #: comment != null # \" align=center><md-icon class=\"no-button grid-item-color\">comment</md-icon><md-tooltip md-direction=\"bottom\">#: comment #</md-tooltip></div>", width: 70
                }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, null, gridOptions);

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var manufacturer = grid.dataItem(this);
                    $state.go('assetsWorkspace.manufacturers.details', { domain_id: manufacturer.domain_id, manufacturer_id: manufacturer.manufacturer_id });
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.manufacturersGrid);
        };

        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (widget === $scope.manufacturersGrid) {
                setDbClick($scope.manufacturersGrid);
            }
        });
        /* END - kendo ui grid configurations*/
    }]);