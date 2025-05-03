xPlanner.controller('UsersListCtrl', ['$scope', 'GridService', 'HttpService', 'AuthService', 'DialogService', 'ProgressService',
        'WebApiService', 'toastr', '$q', '$state',
    function ($scope, GridService, HttpService, AuthService, DialogService, ProgressService, WebApiService, toastr, $q, $state) {

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
                            "<md-tooltip md-direction=\"bottom\">Add New User</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" style=\"color: black;\" ng-click=\"openAddEditModal(true)\"><i class=\"material-icons\">edit</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Edit User</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" style=\"color: black;\" ng-click=\"toggleLock('lock')\"><i class=\"material-icons\">lock</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Lock User(s)</md-tooltip>" +
                        "</button>" +
                         "<button class=\"md-icon-button md-button\" style=\"color: black;\" ng-click=\"toggleLock('unlock')\"><i class=\"material-icons\">lock_open</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Unlock User(s)</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" style=\"color: black;\" ng-click=\"delete()\"><i class=\"material-icons\">delete</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Delete User</md-tooltip>" +
                        "</button>" +
                    "</section>" +
                "</section>"
        };
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("Users", "AllWithRole", AuthService.getLoggedDomain()),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            schema: {
                model: {
                    id: "aspNetUser.id",
                    fields: {
                        creationDate: { type: 'date' },
                        lastLoginDate: { type: 'date' },
                        lastActivityDate: { type: 'date' }
                    }
                }
            },
            error: function () {
                toastr.error('Error to retrieve data from server, please contact technical support');
            }
        };
        var gridOptions = { groupable: true, height: window.innerHeight - 100 };

        var columns = [
                { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(usersGrid)\" ng-checked=\"allPagesSelected(usersGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, usersGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, usersGrid)\" ng-checked=\"isSelected(usersGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                { field: "aspNetUser.email", title: "Email", width: "17em" },
                { field: "role", title: "Role", width: "10em" },
                { field: "aspNetUser.comment", title: "Comments", width: "10em" },
                {
                    field: "aspNetUser.creationDate", title: "Creation Date", format: "{0:MM/dd/yyyy}", width: "11em", template: "#: aspNetUser.creationDate ? kendo.toString(kendo.parseDate(aspNetUser.creationDate), \"MM/dd/yyyy\") : '' #"
        },
                { field: "aspNetUser.lastLoginDate", title: "Last Login Date", format: "{0:MM/dd/yyyy}", width: "12em", template: "#: aspNetUser.lastLoginDate ? kendo.toString(kendo.parseDate(aspNetUser.lastLoginDate), \"MM/dd/yyyy\") : '' #" },
                //{ field: "aspNetUser.lastActivityDate", title: "Last Activity Date", format: "{0:MM/dd/yyyy}", width: "13em", template: "#: aspNetUser.lastActivityDate ? kendo.toString(kendo.parseDate(aspNetUser.lastActivityDate), \"MM/dd/yyyy\") : '' #" },
                //{ field: "aspNetUser.isOnLine", title: "Is Online", width: "8em", template: "<div align=center><i class=\"material-icons no-button\" ng-style=\"{color: #= aspNetUser.isOnLine # ? 'green' : 'red'}\">#= aspNetUser.isOnLine ? 'check' : ''#</i></div>" },
                { field: "lockoutEnabled", title: "Is Locked Out", width: "10em", template: "<div align=center><i class=\"material-icons no-button\" ng-style=\"{color: #= lockoutEnabled # ? 'green' : 'red'}\">#= lockoutEnabled ? 'check' : ''#</i></div>" }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions);

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var user = grid.dataItem(this);
                    $scope.openAddEditModal(true, user);
                });
            }
        };

        $scope.dataBound = function () {
            setDbClick($scope.usersGrid);
            GridService.dataBound($scope.usersGrid);
        };

        $scope.$on("kendoWidgetCreated", function (event, widget) {
            if (widget === $scope.usersGrid) {
                setDbClick($scope.usersGrid);
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

            if (!edit || item || GridService.verifySelected('edit', 'user', $scope.usersGrid, true)) {
                item = item || GridService.getSelecteds($scope.usersGrid)[0];
                DialogService.openModal('app/Users/Modals/EditUser.html', 'EditUserCtrl', edit ? { user: item || GridService.getSelecteds()[0], edit: true } : {})
                    .then(function () {
                        $scope.usersGrid.dataSource.read();
                    });
            }

        };

        /* Delete selected users */
        $scope.delete = function () {
            if (GridService.verifySelected('delete', 'user', $scope.usersGrid)) {
                DialogService.Confirm('Are you sure?', 'The users will be deleted permanently!').then(function () {
                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.genericController,
                        function (item) { return { controller: 'Users', action: 'Item', domain_id: AuthService.getLoggedDomain(), project_id: item.id }; },
                        $scope.usersGrid).then(function () {
                            ProgressService.unblockScreen();
                            toastr.success('Users Deleted!');
                        }, function () {
                            ProgressService.unblockScreen();
                        });
                    GridService.unselectAll($scope.usersGrid);
                });
            }
        };
        /* END - Delete selected users */

        /* Unlock or Lock the selected users */
        $scope.toggleLock = function (action) {
            if (GridService.verifySelected(action, 'user', $scope.usersGrid, false)) {

                ProgressService.blockScreen();
                switch (action) {
                    case 'lock':
                        var items = GridService.getSelecteds($scope.usersGrid).filter(function (i) { if (!i.lockoutEnabled) { i.lockoutEnabled = true; return true; } return false; });
                        break;
                    case 'unlock':
                        var items = GridService.getSelecteds($scope.usersGrid).filter(function (i) { if (i.lockoutEnabled) { i.lockoutEnabled = false; return true; } return false; });
                        break;
                    default:
                        return;
                }
                
                var promisseArray = [];
                angular.forEach(items, function (item) {
                    promisseArray.push(WebApiService.genericController.update({ controller: "users", action: "LockUnlock", domain_id: AuthService.getLoggedDomain(), project_id: item.id, phase_id: item.lockoutEnabled }, null, function () {
                        GridService.updateItems($scope.usersGrid, item);
                    }).$promise)
                });

                $q.all(promisseArray).then(function () {
                    GridService.unselectAll($scope.usersGrid);
                    ProgressService.unblockScreen();
                }, function () {
                    toastr.error("Was not possible " + action + " all the selected users, please contact technical support");
                    GridService.unselectAll($scope.usersGrid);
                    ProgressService.unblockScreen();
                });
            }
        };
        /* End - Unlock or Lock the selected users */
    }]);