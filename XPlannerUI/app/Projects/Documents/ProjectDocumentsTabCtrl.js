xPlanner.controller('ProjectDocumentsTabCtrl', ['$scope', 'AuthService', 'HttpService', 'WebApiService', 'GridService',
        'DialogService', 'ProgressService', 'toastr', '$mdDialog', '$stateParams', 'FileService',
    function ($scope, AuthService, HttpService, WebApiService, GridService, DialogService, ProgressService, toastr,
        $mdDialog, $stateParams, FileService) {

        $scope.params = angular.copy($stateParams);
        $scope.$emit('detailsParams', $scope.params);
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
                        '<button class="md-icon-button md-button" ng-click="openAssociation()"><md-icon md-svg-icon="link"></md-icon><div class="md-ripple-container"></div>' +
                            '<md-tooltip md-direction="bottom">Establish association(s) to the document(s)</md-tooltip>' +
                        '</button>' +
                    '</section>' +
                '</section>'
        };
        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic('projectDocuments', 'All', AuthService.getLoggedDomain(), $scope.params.project_id),
                    headers: {
                        Authorization: 'Bearer ' + AuthService.getAccessToken()
                    }
                }
            },
            schema: {
                model: { id: 'id', fields: { date_added: { type: 'date' } } }
            }
        };

        $scope.gridHeight = window.innerHeight - 200;
        var gridOptions = { noRecords: 'No documents available', height: $scope.gridHeight };
        var columns = [
                { headerTemplate: '<md-checkbox class="checkbox" md-indeterminate="allSelected(documentsGrid)" ng-checked="allPagesSelected(documentsGrid)" aria-label="checkbox" ng-click="select($event, documentsGrid, true)"></md-checkbox>', template: '<md-checkbox class="checkbox" ng-click="select($event, documentsGrid)" ng-checked="isSelected(documentsGrid, dataItem)" aria-label="checkbox"></md-checkbox>', width: '3em' },
                {
                    field: 'filename', title: 'File Name', template:
                    '<section><a href="" ng-click="downloadFile(dataItem)">#: filename #</a></section>'
                },
                {
                    field: 'document_types.name', title: 'Type'
                },
                { field: 'status', title: 'Status' },
                { field: 'version', title: 'Version' },
                {
                    field: 'date_added', title: 'Date Added', format: '{0:MM-dd-yyyy}',
                    filterable: {
                        ui: 'datepicker'
                    }
                }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions);
        /* END - kendo ui grid configurations*/

        function editDocument(doc) {
            if (!validateAccess()) {
                return;
            }

            var params = { project_id: $scope.params.project_id, domain_id: AuthService.getLoggedDomain() };

            DialogService.openModal('app/Projects/Documents/Modals/AddProjectDocument.html', 'AddProjectDocumentCtrl',
                { params: params, document: doc, edit: true }).then(function (document) {
                    $scope.documentsGrid.dataSource.read();
                });

        }

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var document = grid.dataItem(this);
                    editDocument(document);
                });
            }
        };

        $scope.dataBound = function (grid) {
            setDbClick(grid);
            GridService.dataBound(grid);
        };

        $scope.anySelected = GridService.anySelected;

        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */

        $scope.downloadFile = function (document) {
            FileService.downloadFile(HttpService.generic('projectDocuments', 'Download', document.project_domain_id, document.project_id, document.id), document.filename + (document.file_extension ? ("." + document.file_extension) : ''));
        };

        $scope.openAssociation = function () {
            if (!validateAccess()) {
                return;
            }

            if (GridService.verifySelected('do the association', 'document', $scope.documentsGrid, true)) {
                DialogService.openModal('app/Projects/Documents/Modals/AddDocAssociation.html', 'AddDocAssociationCtrl',
                    {
                        params: { project_id: $scope.params.project_id, domain_id: AuthService.getLoggedDomain() },
                        documents: GridService.getSelecteds($scope.documentsGrid)
                    })
                    .then(function () {
                        $scope.documentsGrid.dataSource.read();
                    });
            }
        };

        $scope.openAddModal = function () {
            if (!validateAccess()) {
                return;
            }

            var params = { project_id: $scope.params.project_id, domain_id: AuthService.getLoggedDomain() };

            DialogService.openModal('app/Projects/Documents/Modals/AddProjectDocument.html', 'AddProjectDocumentCtrl', { params: params }).then(function (document) {
                $scope.documentsGrid.dataSource.read();
            });
        };

        /* Delete selected documents */
        $scope.delete = function () {
            if (!validateAccess()) {
                return;
            }

            if (GridService.verifySelected('delete', 'document', $scope.documentsGrid)) {
                DialogService.Confirm('Are you sure?', 'The document(s) will be deleted permanently!').then(function () {
                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.genericController,
                        function (item) {
                            return {
                                controller: 'projectDocuments', action: "Item",
                                domain_id: item.project_domain_id, project_id: item.project_id, phase_id: item.id
                            };
                        },
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

        /* BEGIN - Sub Grid configuration */
        $scope.nestedGridHeight = 400;

        $scope.deleteAssociation = function (dataItem) {
            if (!validateAccess()) {
                return;
            }

            DialogService.Confirm('Are you sure?', 'If you remove the association the asset will be removed from the association list. <br/>You will have to use the association button in the top should you need to reestablish the association.').then(function () {
                ProgressService.blockScreen();
                WebApiService.genericController.remove({ controller: 'assetsInventory', action: "LinkedToDoc", domain_id: dataItem.domain_id, project_id: dataItem.inventory_id },
                            function () {
                                toastr.success('Asset association has been removed!');
                                $scope.documentsGrid.dataSource.read();
                                ProgressService.unblockScreen();
                                
                            }, function () {
                                toastr.error('Error to remove association with asset, please contact the technical support.');
                                ProgressService.unblockScreen();
                            });
            });
        }

        $scope.getDocAssociations = function (doc) {

            var dataSource = {
                transport: {
                    read: {
                        url: HttpService.generic('assetsInventory', 'linkedToDoc', doc.project_domain_id, doc.project_id, doc.id),
                        headers: {
                            Authorization: 'Bearer ' + AuthService.getAccessToken()
                        }
                    }
                }
            };

            var columns = [
                {
                    width: 60, template: '<button class="md-icon-button md-button" ng-click="deleteAssociation(this.dataItem)" ng-if="' + $scope.isNotViewer + '"><i class="material-icons">delete</i><div class="md-ripple-container"></div>' +
                            '<md-tooltip md-direction="bottom">Delete Association</md-tooltip>' +
                        '</button>'
                },
                    { field: 'phase_description', title: 'Phase' },
                    { field: 'department_description', title: 'Department' },
                    { field: 'room_number', title: 'Room Number' },
                    { field: 'room_name', title: 'Room Name' },
                    { field: 'asset_code', title: 'Asset' }
            ];

            var options = { noRecords: 'No associations established', height: $scope.nestedGridHeight };

            var pageable = { pageSizes: [10, 20, 30], pageSizeDefault: 10 };

            return GridService.getStructure(dataSource, columns, null, options, pageable);
        };
        /* END - Sub Grid configuration */

        function validateAccess() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return false;
            }
            if (AuthService.getProjectStatus($scope.params.project_id) == "L") {
                DialogService.LockedProjectModal();
                return false;
            }

            return true;
        }
    }]);