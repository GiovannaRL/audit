xPlanner.controller('ViewAuditedCtrl', ['$scope', 'GridService', 'HttpService', 'AuthService', 'ProgressService',
    'toastr', '$state', 'local', '$mdDialog',
    function ($scope, GridService, HttpService, AuthService, ProgressService, toastr, $state, local, $mdDialog) {

        if (!AuthService.isAuthenticated()) {

            AuthService.clearLocalStorage();
            $state.go('login');
            return;
        }

        $scope.auditedData = local.audit;
        $scope.modified_date = kendo.toString(kendo.parseDate($scope.auditedData.modified_date), "MM/dd/yyyy");

        /* kendo ui grid configurations*/
       
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("Audit", "GetItem", AuthService.getLoggedDomain(), $scope.auditedData.audit_log_id),
                    headers: {
                        Authorization: "Bearer " + AuthService.getAccessToken()
                    }
                }
            },
            schema: {
                model: {
                    id: "Id"
                }
            },
            error: function () {
                toastr.error('Error to retrieve data from server, please contact technical support');
            }
        };
        var gridOptions = { groupable: false, height: window.innerHeight - 250 };

        var columns = [
            { field: "column", title: "Column", width: "30%" },
            { field: "original", title: "Original Data", width: "30%" },
            { field: "modified", title: "Modified Data", width: "30%" }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, null, gridOptions);

   
        $scope.dataBound = function () {
            GridService.dataBound($scope.auditGrid);
        };
   
        /* END - kendo ui grid configurations*/
        $scope.close = function () {
            $mdDialog.cancel();
        };




    }]);