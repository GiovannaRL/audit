xPlanner.controller('RoomTemplatesListCtrl', ['$scope', 'GridService', 'HttpService', 'DialogService', 'AuthService', '$state',
        'ProgressService', 'AudaxwareDataService', 'WebApiService', 'toastr',
    function ($scope, GridService, HttpService, DialogService, AuthService, $state, ProgressService, AudaxwareDataService,
        WebApiService, toastr) {

        $scope.gridHeight = window.innerHeight - 160;

        /* BEGIN - grid configuration */
        ProgressService.blockScreen();
        var columns = [
                   { headerTemplate: "<md-checkbox style=\"align: center\" class=\"checkbox\" md-indeterminate=\"allSelected(templatesListGrid)\" ng-checked=\"allPagesSelected(templatesListGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, templatesListGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, templatesListGrid)\" ng-checked=\"isSelected(templatesListGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                   { field: "project_name", title: "Scope", width: 180, filterable: { multi: true, search: true } },
                   { field: "department_type", title: "Department Type", width: 180 },
                   { field: "description", title: "Name", width: 250 },
                   { field: "date_added", title: "Date Added", width: 120, template: "#: date_added ? kendo.toString(kendo.parseDate(date_added), \"MM-dd-yyyy\") : '' #" },
                   { field: "owner", title: "Owner", width: 110 },
                   {
                       headerTemplate:
                       "<button style=\"padding-bottom: 0.2em;\" class=\"md-icon-button md-button no-button\"><i class=\"material-icons no-button\">comment</i><div class=\"md-ripple-container\"></div>" +
                           "<md-tooltip md-direction=\"bottom\">Comment</md-tooltip>" +
                       "</button>", template: "<div ng-if=\" #: comment != null # \" align=center><md-icon class=\"no-button grid-item-color\">comment</md-icon><md-tooltip md-direction=\"bottom\">#: comment #</md-tooltip></div>", width: 70, field: "comment", filterable: false, sortable: false,
                   }
        ];

        var dataSource = {
            transport: {
                read: {
                    url: HttpService.generic("templateRoom", "TemplateList", AuthService.getLoggedDomain()),
                    headers: { Authorization: "Bearer " + AuthService.getAccessToken() }
                },
                error: function () {
                    ProgressService.unblockScreen();
                    toastr.error("Error to retrieve data from server, please contact technical support");
                }
            },
            schema: {
                model: {
                    fields: { date_added: { type: 'date' } }
                }
            }//,
            //group: [{ field: 'project_name' }, { field: 'department_type' }]
        };

        var current_templates = null;

        
        $scope.options = GridService.getStructure(dataSource, columns, null, { groupable: true, noRecords: "No templates available", height: $scope.gridHeight });

        function setDbClick(grid) {
            if (grid) {
                grid.tbody.find("tr").dblclick(function () {
                    var template = grid.dataItem(this);
                    $scope.showDetails(template);
                });
            }
        };

        $scope.dataBound = function () {
            ProgressService.unblockScreen();
            setDbClick($scope.templatesListGrid);
            GridService.dataBound($scope.templatesListGrid);
            if (current_templates == null) {
                current_templates = $scope.templatesListGrid.dataSource.data();
            }
            
        };


        $scope.select = GridService.select;
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.allPagesSelected = GridService.allPagesSelected;

        $scope.collapseExpand = GridService.collapseExpand;
        /* END - grid configuration */

        /* Search box */
        $scope.search = function (value) {

            var items = current_templates || $scope.templatesListGrid.dataSource.data();

            if (!value) {
                $scope.templatesListGrid.dataSource.data(current_templates);
            } else {
                value = value.toLowerCase();
                var columns = $scope.templatesListGrid.columns;
                $scope.templatesListGrid.dataSource.data(items.filter(function (item) {
                    for (var i = 1; i < columns.length; i++) {
                        if (!columns[i].hidden && String(item[columns[i].field]).toLowerCase().indexOf(value) > -1) {
                            return true;
                        }
                    };
                }));
            }
            GridService.unselectAll($scope.templatesListGrid);
        };

        /* BEGIN - Clear all filter*/
        $scope.clearAllFilters = function (name) {
            $scope.searchBoxValue = null;
            GridService.ClearFilters($scope.templatesListGrid);
            $scope.search();
        };
        /* END - Clear all filters */

        /* Go to page that shows the template details */
        $scope.showDetails = function (template) {

            if (!template) {
                if (!GridService.verifySelected('view the details', 'template', $scope.templatesListGrid, true)) return;
            }

            $state.go('room-templates-details', template || GridService.getSelecteds($scope.templatesListGrid)[0]);
        };

        /* Open the dialog to add a new template */
        $scope.addTemplate = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            DialogService.openModal('app/Room Templates/Modals/AddTemplate.html', 'AddTemplateCtrl', null).then(function (template) {
                $scope.showDetails(template);
            });
        };

        /* Deletes one or more templates */
        $scope.delete = function () {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            if (!GridService.verifySelected('delete', 'template', $scope.templatesListGrid)) return;

            AudaxwareDataService.ShowDeleteDialog('templates', GridService.getSelecteds($scope.templatesListGrid)).then(function () {
                ProgressService.blockScreen();
                GridService.deleteItems(WebApiService.genericController, function (i) {
                    return {
                        controller: 'templateRoom', action: 'Item', domain_id: i.domain_id, project_id: i.project_id,
                        phase_id: i.phase_id, department_id: i.department_id, room_id: i.room_id
                    }
                },
                    $scope.templatesListGrid, null, true).then(function (data) {
                        ProgressService.unblockScreen();
                        if (data.lenght > 0) {
                            toastr.success('Template(s) Deleted');
                            GridService.unselectAll($scope.templatesListGrid);
                        }
                    }, function (error) {
                        ProgressService.unblockScreen();
                        if (error.status === 409) toastr.info("Some, or all, of the templates could not be deleted -- there is linked room(s)");
                        else toastr.error('Error to delete one o more templates, please contact the technical support');
                        GridService.unselectAll($scope.templatesListGrid);
                    });
            });
        }
    }]);