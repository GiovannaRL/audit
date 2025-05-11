xPlanner.controller('ManufacturerVendorsGridCtrl', ['$scope', 'HttpService', 'GridService', '$state', 'AuthService',
    function ($scope, HttpService, GridService, $state, AuthService) {

        /* kendo ui grid configurations*/
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("manufacturer", "Vendors", $scope.params.domain_id, $scope.params.manufacturer_id),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            schema: {
                model: {
                    id: "vendor_id"
                }
            }
        };
        var gridOptions = {
            noRecords: "No vendors available",
            height: 300
        };
        var columns = [
                { field: "name", title: "Name" },
                { field: "territory", title: "Territory" },
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
                    var vendor = grid.dataItem(this);
                    $state.go('assetsWorkspace.vendors.details', { domain_id: vendor.domain_id, vendor_id: vendor.vendor_id });
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.vendorsGrid);
        };

        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (widget === $scope.vendorsGrid) {
                setDbClick($scope.vendorsGrid);
            }
        });
        /* END - kendo ui grid configurations*/
    }]);