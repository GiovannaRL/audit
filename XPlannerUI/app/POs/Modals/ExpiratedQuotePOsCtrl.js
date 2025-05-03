xPlanner.controller('ExpiratedQuotePOsCtr', ['$scope', '$mdDialog', 'local', 'GridService', '$mdMedia',
    function ($scope, $mdDialog, local, GridService, $mdMedia) {

        $scope.largeMedia = $mdMedia('gt-sm');
        $scope.gridHeight = 440;

        /* kendo ui grid configurations*/
        var dataSource = {
            data: local.pos
        };
        var gridOptions = { reorderable: true, groupable: true, height: $scope.gridHeight };
        var columns = [
            { field: 'project_description', title: 'Project', width: '120px' },
            { field: 'phase_description', title: 'Phase', width: '120px' },
            { field: 'department_description', title: 'Department', width: '120px' },
            { field: 'room_description', title: 'Room', width: '120px' },
            { field: 'po_description', title: 'PO Description', width: '120px' },
            { field: 'expiration_date', title: 'Expiration Date', width: '120px', template: "#: expiration_date ? kendo.toString(kendo.parseDate(expiration_date), \"yyyy/MM/dd\") : '' #" },
            { field: 'vendor_name', title: 'Vendor', width: '120px' }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, null, gridOptions);
        /* END - kendo ui grid configurations*/

        $scope.dataBound = GridService.dataBound;

        // Function that closes the modal with a success promise
        $scope.close = function () {
            $mdDialog.hide();
        };
    }]);