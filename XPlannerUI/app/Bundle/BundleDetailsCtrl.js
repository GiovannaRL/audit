xPlanner.controller('BundleDetailsCtrl', ['$scope', 'AuthService', 'ProgressService', 'WebApiService', '$stateParams', 'HttpService',
        'GridService', '$state',
    function ($scope, AuthService, ProgressService, WebApiService, $stateParams, HttpService, GridService, $state) {

        $scope.$emit('initialTab', 'bundles');

        var loggedDomain = AuthService.getLoggedDomain();
        var params = $stateParams;
        $scope.canModify = true;//loggedDomain == 1 || loggedDomain === data.domain_id;

        $scope.projectParams = { domain_id: loggedDomain };
        $scope.gridHeight = 300;

        // Get the bundle information
        ProgressService.blockScreen();
        WebApiService.genericController.get({ controller: 'bundle', action: 'Item', domain_id: params.domain_id, project_id: params.bundle_id },
            function (data) {
                $scope.bundle = data;
                ProgressService.unblockScreen();

                /* Assets grid configuration */

                var columns = [
                        { field: "asset.asset_code", title: "Code", width: 300 },
                        { field: "asset.asset_description", title: "Description", width: 300 },
                        { field: "asset.model_number", title: "Model No.", width: 200 },
                        { field: "asset.model_name", title: "Model Name", width: 230 },
                        { field: "manufacturer", title: "Manufacturer", width: 150 }
                ];

                var toolbar = {
                    template:
                        "<section layout=\"row\" ng-cloack>" +
                            "<section layout=\"row\" layout-align=\"start center\" class=\"gray-color\">" +
                                "<button class=\"md-icon-button md-button\" ng-click=\"addAsset()\"><i class=\"material-icons\">add</i><div class=\"md-ripple-container\"></div>" +
                                    "<md-tooltip md-direction=\"bottom\">Add Asset</md-tooltip>" +
                                "</button>" +
                                "<button class=\"md-icon-button md-button\" ng-click=\"deleteAsset()\"><i class=\"material-icons\">delete</i><div class=\"md-ripple-container\"></div>" +
                                    "<md-tooltip md-direction=\"bottom\">Delete Asset</md-tooltip>" +
                                "</button>" +
                            "</section>" +
                            //"<section layout=\"row\" layout-align=\"end center\" flex=\"100\">" +
                            //    "<button class=\"md-icon-button md-button\" ng-click=\"addAsset()\">" +
                            //        "<md-icon class=\"material-icons md-accent\">add_circle</md-icon><div class=\"md-ripple-container\"></div>" +
                            //        "<md-tooltip md-direction=\"bottom\">Add New Asset</md-tooltip>" +
                            //    "</button>" +
                            //"</section>" +
                        "</section>"
                };

                if ($scope.canModify)
                    columns.splice(0, 0, { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(bundleAssetsGrid)\" ng-checked=\"allPagesSelected(bundleAssetsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, bundleAssetsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, bundleAssetsGrid)\" ng-checked=\"isSelected(bundleAssetsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" });

                var dataSource = {
                    transport: {
                        read: {
                            url: HttpService.generic("bundleAsset", "All", $scope.bundle.domain_id, $scope.bundle.bundle_id),
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
                            id: "domain_id_asset_id"
                        },
                        parse: function (data) {
                            return data.map(function (i) {
                                i.domain_id_asset_id = i.domain_id.toString() + i.asset_id.toString();
                                return i;
                            });
                        }
                    }
                };

                $scope.options = GridService.getStructure(dataSource, columns, toolbar, { noRecords: "No assets available", height: $scope.gridHeight, groupable: true });
                /* Assets grid configuration */
            });

        /* Select the grid's rows */
        if ($scope.canModify) {
            $scope.isSelected = GridService.isSelected;
            $scope.allSelected = GridService.allSelected;
            $scope.select = GridService.select;
            $scope.allPagesSelected = GridService.allPagesSelected;
        }
        /* END - Select the grid's rows */

        function _back() {
            $state.go('assetsWorkspace.bundles');
        }

        /* float buttons */
        var saveButton = {
            label: 'Save',
            icon: 'save',
            click: _back,
            displayConditionally: true
        };

        var deleteButton = {
            label: 'Delete',
            icon: 'delete_forever',
            click: _back,
            displayConditionally: true
        };

        var backButton = {
            label: 'Back to List',
            icon: 'reply',
            click: _back
        };

        $scope.buttons = [saveButton, deleteButton, backButton];
        $scope.buttons.state = true;
        /* end float button */

    }]);