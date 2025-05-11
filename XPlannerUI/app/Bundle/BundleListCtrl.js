xPlanner.controller('BundleListCtrl', ['$scope', 'HttpService', 'GridService', 'ProgressService', 'toastr', 'AuthService',
        'DialogService', '$state',
    function ($scope, HttpService, GridService, ProgressService, toastr, AuthService, DialogService, $state) {

    $scope.$emit('initialTab', 'bundles');

    /* grid configuration */
    var columns = [
                { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(bundlesListGrid)\" ng-checked=\"allPagesSelected(bundlesListGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, bundlesListGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, bundlesListGrid)\" ng-checked=\"isSelected(bundlesListGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                { field: "name", title: "Bundle", width: 300 },
                { field: "project", title: "Related Project", width: 300 },
                { field: "date_added", title: "Date Added", width: 200, template: "#: date_added ? kendo.toString(kendo.parseDate(date_added), \"MM-dd-yyyy\") : '' #" },
                { field: "added_by", title: "Added By", width: 230 },
                { field: "domain.name", title: "Owner", template: "{{ dataItem.domain.name | capitalize }}", width: 150 },
                {
                    headerTemplate:
                    "<section layout=\"row\" layout-align=\"center end\"><button style=\"margin-left: -0.25em; padding-bottom: 0.2em;\" class=\"md-icon-button md-button no-button\"><i class=\"material-icons no-button\">comment</i><div class=\"md-ripple-container\"></div>" +
                        "<md-tooltip md-direction=\"bottom\">Comment</md-tooltip>" +
                    "</button></section>", template: "<div ng-if=\" #: comment != null && comment != '' # \" align=center><md-icon class=\"no-button grid-item-color\">comment</md-icon><md-tooltip md-direction=\"bottom\">#: comment #</md-tooltip></div>", width: 70
                }
    ];

    /*Get the data to dataSource*/
    var dataSource = {
        transport: {
            read: {
                url: HttpService.generic("bundle", "All", AuthService.getLoggedDomain()),
                headers: {
                    Authorization: "Bearer " + AuthService.getAccessToken()
                }
            },
            error: function () {
                toastr.error("Error to retrieve data from server, please contact technical support");
            }
        },
        schema: {
            model: {
                id: "bundle_domain_id"
            },
            parse: function (data) {
                return data.map(function (i) {
                    i.bundle_domain_id = i.domain_id.toString() + i.bundle_id.toString();
                    if (i.project)
                        i.project = i.project.project_description
                    return i;
                });
            }
        }
    };
    $scope.options = GridService.getStructure(dataSource, columns, null, { noRecords: "No bundles available", height: window.innerHeight - 170, groupable: true });

    function setDbClick(grid) {
        if (grid) {
            grid.tbody.find("tr").dblclick(function () {
                var bundle = grid.dataItem(this);
                $scope.showDetails(bundle);
            });
        }
    };

    $scope.dataBound = function () {
        setDbClick($scope.bundlesListGrid);
        GridService.dataBound($scope.bundlesListGrid);
    }

    /* Select the grid's rows */
    $scope.isSelected = GridService.isSelected;
    $scope.allSelected = GridService.allSelected;
    $scope.select = GridService.select;
    $scope.allPagesSelected = GridService.allPagesSelected;
    /* END - Select the grid's rows */

    $scope.collapseExpand = GridService.collapseExpand;
    /* END - grid configuration */

    /* Show bundle details*/
    $scope.showDetails = function (bundle) {
        if (!bundle) {
            if (!GridService.verifySelected('view details', 'bundle', $scope.bundlesListGrid, true)) return;
            bundle = GridService.getSelecteds($scope.bundlesListGrid)[0];
        }

        GridService.unselectAll($scope.bundlesListGrid);
        $state.go('assetsWorkspace.bundlesDetails', { domain_id: bundle.domain_id, bundle_id: bundle.bundle_id });
    };

    // Add a new bundle
    $scope.openAddModal = function () {
    };

    // delete bundles
    $scope.delete = function () {
    };

}]);