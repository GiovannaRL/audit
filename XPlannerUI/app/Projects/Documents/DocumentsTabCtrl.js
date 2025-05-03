xPlanner.controller('DocumentsTabCtrl', ['$scope', 'AuthService', 'HttpService', 'WebApiService', 'GridService', 'DialogService', 'ProgressService',
        'toastr', '$mdDialog',
    function ($scope, AuthService, HttpService, WebApiService, GridService, DialogService, ProgressService, toastr, $mdDialog) {

        $scope.isNotViewer = (AuthService.getLoggedUserType() != "3");

        /* kendo ui grid configurations*/
        var toolbar = {
            template:
                '<section layout="row" ng-cloak ng-if=\"' + $scope.isNotViewer + '\">' +
                    '<section layout="row" layout-align="start center"  class="gray-color">' +
                        '<button class="md-icon-button md-button" ng-click="openAddModal()"><i class="material-icons">add</i><div class="md-ripple-container"></div>' +
                            '<md-tooltip md-direction="bottom">Add Document(s)</md-tooltip>' +
                        '</button>' +
                         '<button class="md-icon-button md-button" ng-click="delete()"><i class="material-icons">delete</i><div class="md-ripple-container"></div>' +
                            '<md-tooltip md-direction="bottom">Delete Document(s)</md-tooltip>' +
                        '</button>' +
                    '</section>' +
                    //"<section layout=\"row\" layout-align=\"end center\" flex=\"100\">" +
                    //    "<button class=\"md-icon-button md-button\" ng-click=\"openAddModal()\">" +
                    //        "<md-icon class=\"md-accent\">add_circle</i><div class=\"md-ripple-container\"></div>" +
                    //        "<md-tooltip md-direction=\"bottom\">Add New Document</md-tooltip>" +
                    //    "</button>" +
                    //"</section>" +
                '</section>'
        };
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.phase_documents('All', AuthService.getLoggedDomain(), $scope.params.project_id, $scope.params.phase_id),
                    headers: {
                        Authorization: 'Bearer ' + AuthService.getAccessToken()
                    }
                }
            },
            schema: { model: { id: 'drawing_id', fields: { date_added: { type: 'date' } } } }
        };
        var gridOptions = { groupable: true, noRecords: 'No documents available', height: 300 };
        var columns = [
                { headerTemplate: '<md-checkbox class="checkbox" md-indeterminate="allSelected(documentsGrid)" ng-checked="allPagesSelected(documentsGrid)" aria-label="checkbox" ng-click="select($event, documentsGrid, true)"></md-checkbox>', template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, documentsGrid)\" ng-checked=\"isSelected(documentsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                { field: 'filename', title: 'File Name', width: '65%', template:
                '<section><a href="" ng-click="downloadFile(#: drawing_id #)">#: filename #</a></section>'
                },
                {
                    field: 'date_added', title: 'Date Added', width: '30%', format: '{0:MM-dd-yyyy}',
                    filterable: {
                        ui: 'datepicker'
                    }
                }
        ];

        $scope.downloadFile = function (drawingId) {
            window.open(HttpService.phase_documents('download', AuthService.getLoggedDomain(), $scope.params.project_id, $scope.params.phase_id, drawingId), '_self');
        };

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions);
        /* END - kendo ui grid configurations*/

        $scope.anySelected = GridService.anySelected;

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.dataBound = GridService.dataBound;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        $scope.openAddModal = function () {
            if (AuthService.getLoggedUserType() == '3') {
                DialogService.ViewersChangesModal();
                return;
            }
            var params = { action: 'Item', project_id: $scope.params.project_id, phase_id: $scope.params.phase_id, domain_id: AuthService.getLoggedDomain() };

                DialogService.openModal('app/Projects/Documents/Modals/AddDocument.html', 'AddDocumentCtrl', {params: params}, true).then(function () {
                $scope.documentsGrid.dataSource.read();
            });
        };

        /* Delete selected documents */
        $scope.delete = function () {
            if (AuthService.getLoggedUserType() == '3') {
                DialogService.ViewersChangesModal();
                return;
            }
            if (GridService.verifySelected('delete', 'document', $scope.documentsGrid)) {
                DialogService.Confirm('Are you sure?', 'The document(s) will be deleted permanently!').then(function () {
                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.phase_documents,
                        function (item) { return { action: 'Item', domain_id: item.domain_id, project_id: item.project_id, phase_id: item.phase_id, drawing_id: item.drawing_id }; },
                        $scope.documentsGrid).then(function () {
                            ProgressService.unblockScreen();
                            toastr.success('Document(s) Deleted!');
                        }, function () {
                            toastr.error('Error to try to delete document(s), please contact technical support');
                            ProgressService.unblockScreen();
                        });
                    GridService.unselectAll($scope.documentsGrid);

                });
            }
        };
        /* END - Delete selected documents */
    }]);