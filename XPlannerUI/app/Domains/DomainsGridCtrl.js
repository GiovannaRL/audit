xPlanner.controller('DomainsGridCtrl', ['$scope', 'HttpService', 'AuthService', 'GridService', '$mdDialog', 'toastr', 'WebApiService',
                    'ProgressService', 'DialogService', '$state', '$q',
    function ($scope, HttpService, AuthService, GridService, $mdDialog, toastr, WebApiService, ProgressService, DialogService, $state, $q) {

        if (!AuthService.isAuthenticated()) {

            AuthService.clearLocalStorage();
            $state.go('login');
            return;
        }

        /* kendo ui grid configurations*/
        var toolbar = {
                template:
                    "<section layout=\"row\" ng-cloack>" +
                    "<section layout=\"row\" layout-align=\"start center\">" +
                    "<button class=\"md-icon-button md-button md-accent\" style=\"color: black;\" ng-click=\"openAddEditModal(false)\"><i class=\"material-icons\">add</i><div class=\"md-ripple-container\"></div>" +
                    "<md-tooltip md-direction=\"bottom\">Add New Enterprise</md-tooltip>" +
                    "</button>" +
                    "<button class=\"md-icon-button md-button\" style=\"color: black;\" ng-click=\"openAddEditModal(true)\"><i class=\"material-icons\">edit</i><div class=\"md-ripple-container\"></div>" +
                    "<md-tooltip md-direction=\"bottom\">Edit Enterprise</md-tooltip>" +
                    "</button>" +
                    "</section>" +
                    "</section>"
            
        };
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("domains", "All"),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            schema: {
                model: {
                    id: "domain_id",
                    fields: {
                        created_at: { type: 'date' }
                    }
                }
            },
            error: function () {
                toastr.error("Error to retrieve data from server, please contact technical support");
            }
        };
        var gridOptions = { selectable: "row", height: window.innerHeight - 110 };
        var columns = [
            { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(domainsGrid)\" ng-checked=\"allPagesSelected(domainsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, domainsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, domainsGrid)\" ng-checked=\"isSelected(domainsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
            { headerTemplate: "<div align=center>ID</iv>", field: "domain_id", title: "ID", width: "5em", template: "<div align=center>#= domain_id #</div>" },
            { field: "name", title: "Enterprise", width: "14em" },
            { headerTemplate: "<div align=center>Show AW Info</iv>", field: "show_audax_info", title: "Show Audaxware Data", template: "<div align=center>#= show_audax_info ? 'Yes' : 'No' #</div>", width: "10em" },
            { headerTemplate: "<div align=center>Enabled</iv>", field: "enabled", title: "Enabled", template: "<div align=center>#= enabled ? 'Yes' : 'No' #</div>", width: "8em" },
            { field: "created_at", title: "Created At", width: "10em", format: "{0:MM/dd/yyyy}" },
            { field: "user_count", title: "User Count", width: "10em", template: "<div align=center>#= user_count #</div>" },
            { field: "user_last_month", title: "Acess Last Month", width: "13em", template: "<div align=center>#= user_last_month #</div>" },
            { field: "user_last_year", title: "Acess Last Year", width: "12em", template: "<div align=center>#= user_last_year #</div>" },
            { field: "manufacturers", title: "Manufacturer", width: "13em", template: "<div align=center>#= manufacturers && manufacturers.length ? manufacturers.map(m => m.manufacturer_description).join(', ') : '' #</div>" }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions);
        /* END - kendo ui grid configurations*/
        //$scope.dataBound = GridService.dataBound;

    /* END - Select the grid's rows */

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var user = grid.dataItem(this);
                    $scope.openAddEditModal(true, user);
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.domainsGrid);
            GridService.dataBound($scope.domainsGrid);
        };

        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (widget === $scope.domainsGrid) {
                setDbClick($scope.domainsGrid);
            }
        });
        /* END - kendo ui grid configurations*/

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        $scope.openAddEditModal = function (edit, item) {

            if (!edit || item || GridService.verifySelected('edit', 'enterprise', $scope.domainsGrid, true)) {
                item = item || GridService.getSelecteds($scope.domainsGrid)[0];
                DialogService.openModal('app/Domains/Modals/EditDomain.html', 'EditDomainCtrl', edit ? { domain: item.toJSON() || GridService.getSelecteds()[0], edit: true } : { existingDomains: $scope.domainsGrid.dataSource.data() })
                    .then(function () {
                        $scope.domainsGrid.dataSource.read();
                    });
            }

        };


        /* Enable or Disable the selected Enterprises */
        $scope.toggleLock = function (action) {
            if (GridService.verifySelected(action, 'enterprise', $scope.domainsGrid, false)) {

                ProgressService.blockScreen();
                switch (action) {
                    case 'disable':
                        var items = GridService.getSelecteds($scope.domainsGrid).filter(function (i) { if (i.enabled) { i.enabled = false; return true; } return false; });
                        break;
                    case 'enable':
                        var items = GridService.getSelecteds($scope.domainsGrid).filter(function (i) { if (!i.enabled) { i.enabled = true; return true; } return false; });
                        break;
                    default:
                        return;
                }

                var promisseArray = [];
                angular.forEach(items, function (item) {
                    promisseArray.push(WebApiService.genericController.update({ controller: "domains", action: "EnableDisable", domain_id: item.id, project_id: item.enabled }, null, function () {
                        GridService.updateItems($scope.domainsGrid, item);
                    }).$promise)
                });

                $q.all(promisseArray).then(function () {
                    GridService.unselectAll($scope.domainsGrid);
                    ProgressService.unblockScreen();
                }, function () {
                    toastr.error("Was not possible " + action + " all the selected enterprises, please contact technical support");
                    GridService.unselectAll($scope.domainsGrid);
                    ProgressService.unblockScreen();
                });
            }
        };
        /* End - Enable or Disable the selected Enterprises */

        /* Delete selected users */
        $scope.delete = function () {
            if (GridService.verifySelected('delete', 'enterprise', $scope.domainsGrid)) {
                DialogService.Confirm('Are you sure?', 'The users will be deleted permanently!').then(function () {
                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.genericController,
                        function (item) { return { controller: 'domains', action: 'Item', domain_id: item.domain_id }; },
                        $scope.domainsGrid).then(function () {
                            ProgressService.unblockScreen();
                            toastr.success('Enterprise(s) Deleted!');
                        }, function () {
                            ProgressService.unblockScreen();
                        });
                    GridService.unselectAll($scope.domainsGrid);
                });
            }
        };
        /* END - Delete selected users */

        //$scope.openAddModal = function () {
        //    $mdDialog.show({
        //        controller: 'AddDomainCtrl',
        //        templateUrl: 'app/Domains/Modals/AddDomain.html',
        //        fullscreen: true,
        //        clickOutsideToClose: true,
        //        locals: { local: {existingDomains: $scope.domainsGrid.dataSource.data()}}
        //    }).then(function (domainsAdded) {
        //        ProgressService.blockScreen();
        //        angular.forEach(domainsAdded, function (domain) {
        //            var data = domain.created_at.substring(0, 10);
        //            data = data.split('-');
        //            $scope.domainsGrid.dataSource.add({ domain_id: domain.domain_id, name: domain.name, created_at: data[1] + '/' + data[2] + '/' + data[0], show_audax_info: domain.show_audax_info, rls_user: domain.rls_user, rls_pwd: domain.rls_pwd, pb_workspace_id: domain.pb_workspace_id, pb_dataset_name: domain.pb_dataset_name });
        //        });
        //        ProgressService.unblockScreen();
        //    });
        //};

        //$scope.delete = function (item) {
        //    DialogService.Confirm('Are you sure?', 'The domain will be deleted permanently!').then(function () {
        //        ProgressService.blockScreen();
        //        WebApiService.genericController.remove({controller: "domains", action: "Item", domain_id: item.domain_id},
        //            function () {
        //                $scope.domainsGrid.dataSource.remove(item);
        //                var oldDomains = AuthService.getAvailableDomains();
        //                AuthService.setAvailableDomains(oldDomains.filter(function (i) { return i.domain_id !== item.domain_id }));
        //                ProgressService.unblockScreen();
        //                toastr.success("Domain Deleted");
        //            }, function () {
        //                ProgressService.unblockScreen("Error to try delete domain, please contact technical support");
        //                toastr.error();
        //            });
        //    });
        //};
    }]);