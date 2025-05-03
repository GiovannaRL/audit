xPlanner.controller('ReportListCtrl', ['$scope', 'HttpService', 'AuthService', 'ProgressService', 'GridService', '$stateParams',
        'toastr', 'DialogService', 'WebApiService', '$timeout',
    function ($scope, HttpService, AuthService, ProgressService, GridService, $stateParams, toastr, DialogService,
        WebApiService, $timeout) {

        var params = angular.copy($stateParams);

        $scope.$emit('detailsParams', params);
        $scope.gridHeight = window.innerHeight - 200;
        $scope.isNotViewer = (AuthService.getLoggedUserType() != "3");

        /* DataSource */
        var dataSource = {
            pageSize: 50,
            transport: {
                read: {
                    url: HttpService.generic('Report', 'All', AuthService.getLoggedDomain(), params.project_id, params.phase_id,
                        params.department_id, params.room_id),
                    headers: {
                        Authorization: 'Bearer ' + AuthService.getAccessToken()
                    }
                }
            },
            error: function () {
                ProgressService.unblockScreen();
                toastr.error('Error to retrieve reports from server, please contact technical support');
            },
            schema: { model: { id: 'id', fields: { last_run: { type: 'date' }, 'isPrivate': { type: 'boolean' } } } }
        };
        

        /* Grid options */
        var gridOptions = {
            filterable: true,
            reorderable: true,
            groupable: true,
            height: $scope.gridHeight,
            noRecords: {
                template: "No report was generated to this project"
            }
        };

        /* Toolbar */
        var toolbar = {
            template:
                "<section layout=\"row\" ng-if=\"" + $scope.isNotViewer + "\">" +
                    "<section layout=\"row\" layout-align=\"start center\" flex=\"90\">" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"generateReport()\">" +
                            "<i class=\"material-icons\">add</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Generate new report</md-tooltip>" +
                        "</button>" +
                         "<button class=\"md-icon-button md-button\" ng-click=\"deleteReports()\">" +
                            "<i class=\"material-icons\">delete</i><div class=\"md-ripple-container\"></div>" +
                            "<md-tooltip md-direction=\"bottom\">Delete Report(s)</md-tooltip>" +
                        "</button>" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"reloadReports()\">" +
                            "<i class=\"material-icons\">refresh</i>" +
                             "<md-tooltip md-direction=\"bottom\">Reload reports</md-tooltip>" +
                        "</button>" +
                    "</section>" +
                    "<section layout=\"row\" flex=\"10\" layout-align=\"end start\">" +
                        "<button class=\"md-icon-button md-button\" ng-click=\"collapseExpand(reportsGrid)\">" +
                            "<md-icon md-svg-icon=\"collapse_expand\"></md-icon>" +
                            "<md-tooltip md-direction=\"bottom\">Collapse/Expand All</md-tooltip>" +
                        "</button>" +
                    "</section>" +
                "</section>"
        };

        /* Grid columns */
        var columns = [
                { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(reportsGrid)\" ng-checked=\"allPagesSelected(reportsGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, reportsGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, reportsGrid)\" ng-checked=\"isSelected(reportsGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em", lockable: false },
                { field: "name", title: "Name", width: 180 },
                { field: "report_type.name", title: "Type", width: 180 },
                { field: "description", title: "Description", width: 230, template: "<span>#: description ? description : '' #<md-tooltip ng-if=\"#: description != null && description != '' #\">{{ dataItem.description }}</md-tooltip></span>" },
                {
                    field: "status_category", title: "Status", width: 150, template: "# if(status_category != 'Generating'){ #" +
                        "<span>#: status_category #</span>" +
                      "# } else { #" +
                        "<div layout=\"row\" layout-sm=\"column\" layout-align=\"space-around\">" +
                            "<md-progress-circular md-mode=\"determinate\" value=\"#: status #\" class=\"md-accent\" md-diameter=\"30px\"></md-progress-circular><\div>" +
                      "# } #"
                },
                {
                    field: "last_run", title: "Last Run", width: 200,
                    format: "{0:MM-dd-yyyy HH:mm tt}",
                    filterable: {
                        ui: "datetimepicker"
                    }
                },
                { field: "isPrivate", title: "Private", width: 110, template: "<div align=center ng-if=\"#: isPrivate #\"><i class=\"material-icons no-button\" style=\"color: green\">check</i></div>" },
                {
                    field: "however", title: "Actions", width: 150, filterable: false,
                    template: "<section layout=\"row\" layout-align=\"center center\" class=\"grid-buttons\">"
                        + "<button class=\"md-icon-button md-button\" ng-click=\"regenerateReport(dataItem)\" ng-disabled=\"dataItem.status_category !== 'Completed' && dataItem.status_category !== 'Error'\">" +
                            "<i class=\"material-icons\">refresh</i></div>" +
                            "<md-tooltip md-direction=\"bottom\">Regenerate report</md-tooltip>" +
                        "</button>" + "<button class=\"md-icon-button md-button\" ng-click=\"shareReport(dataItem)\" ng-disabled=\"dataItem.status_category !== 'Completed'\">" +
                            "<i class=\"material-icons\">share</i></div>" +
                            "<md-tooltip md-direction=\"bottom\">Share report</md-tooltip>" +
                        "</button>" + "<button class=\"md-icon-button md-button\" ng-click=\"downloadReport(dataItem, 'pdf')\"  ng-disabled=\"dataItem.status_category !== 'Completed'\" ng-if=\"dataItem.report_type.name.toLowerCase() != 'asset by room' && dataItem.report_type.name.toLowerCase() != 'room by room' && dataItem.report_type.name.toLowerCase() != 'equipment with costs' && dataItem.report_type.name.toLowerCase() != 'budget summary' && dataItem.report_type.name.toLowerCase() != 'room by room (government)' && dataItem.report_type.name.toLowerCase() != 'copied project inventory comparison' && dataItem.report_type.name.toLowerCase() != 'jsn rollup' && dataItem.report_type.name.toLowerCase() != 'government inventory' && dataItem.report_type.name.toLowerCase() != 'room equipment list' && dataItem.report_type.name.toLowerCase() != 'equipment dimensional and utilities' && dataItem.report_type.name.toLowerCase() != 'door list'\">" +
                            "<md-icon md-svg-icon=\"pdf\"></md-icon>" +
                            "<md-tooltip md-direction=\"bottom\">Download as pdf</md-tooltip>" +
                        "</button>" + "<button class=\"md-icon-button md-button\" ng-click=\"downloadReport(dataItem, 'html')\"  ng-disabled=\"dataItem.status_category !== 'Completed'\" ng-if=\"dataItem.report_type.name.toLowerCase() == 'budget summary'\">" +
                            "<md-icon md-svg-icon=\"html\"></md-icon>" +
                            "<md-tooltip md-direction=\"bottom\">Download as html</md-tooltip>" +
                        "</button>" + "<button ng-if=\"dataItem.report_type.name.toLowerCase() != 'asset book' && dataItem.report_type.name.toLowerCase() != 'procurement' && dataItem.report_type.name.toLowerCase() != 'shop drawing' && dataItem.report_type.name.toLowerCase() != 'illustration sheet' && dataItem.report_type.name.toLowerCase() != 'comprehensive interior design'\" class=\"md-icon-button md-button\" ng-click=\"downloadReport(dataItem, 'excel')\"  ng-disabled=\"dataItem.status_category !== 'Completed'\">" +
                            "<md-icon md-svg-icon=\"excel\"></md-icon>" +
                            "<md-tooltip md-direction=\"bottom\">Download as excel</md-tooltip>" +
                        "</button></section>", groupable: false
                }
        ];

        $scope.options = GridService.getStructure(dataSource, columns, toolbar, gridOptions);

        /* Select the grid's rows */
        $scope.dataBound = GridService.dataBound;
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;

        $scope.collapseExpand = GridService.collapseExpand;

        /* Call the server to get the current state of reports */
        $scope.reloadReports = function () {
            $scope.reportsGrid.dataSource.read();
        };

        /* Function to delete one oe more reports */
        $scope.deleteReports = function () {
            if (!validateAccess()) {
                return;
            }

            if (GridService.verifySelected('delete', 'report', $scope.reportsGrid)) {
                DialogService.Confirm('Are you sure?', 'The reports(s) will be deleted permanently').then(function () {
                    ProgressService.blockScreen();
                    GridService.deleteItems(WebApiService.genericController, function (i) { return { controller: 'Report', action: 'Item', domain_id: i.project_domain_id, project_id: i.project_id, phase_id: i.id } },
                        $scope.reportsGrid).then(function (data) {
                            ProgressService.unblockScreen();
                            toastr.success('Reports(s) Deleted');
                        }, function (error) {
                            ProgressService.unblockScreen();
                            toastr.error('Error to delete one o more reports, please contact the technical support');
                            GridService.unselectAll($scope.reportsGrid);
                        });
                });
            }
        };

        /* Open the modal to generate a new report */
        $scope.generateReport = function () {
            if (!validateAccess()) {
                return;
            }

            DialogService.openModal('app/Reports/Modals/GenerateReport.html', 'GenerateReportCtrl').then(function () {
                $scope.reportsGrid.dataSource.read();
            });
        };

        /* Download a report */
        var fileDownloadCheckTimer;

        function finishDownload(cookie_name) {
            window.clearInterval(fileDownloadCheckTimer);
            $.removeCookie(cookie_name);
            $('body').removeClass('wait');
        };

        $scope.downloadReport = function (report, type) {

            $('body').addClass('wait');
            var token = new Date().getTime();

            var xhr = new XMLHttpRequest();
            xhr.open('GET', HttpService.reportDownload(report.project_domain_id, report.project_id, report.id, type == 'excel' ? 'xlsx' : type, token), true);
            xhr.setRequestHeader('Authorization', 'Bearer ' + AuthService.getAccessToken());
            xhr.responseType = 'blob';
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4) {
                    console.log(xhr.response);
                    var a = document.createElement('a');
                    a.href = window.URL.createObjectURL(xhr.response); // xhr.response is a blob
                    a.download = report.name.replace(' ', '_').toLowerCase() + '_' + report.report_type.name.replace(' ', '_').toLowerCase() + '_' + report.id + '.' + (type == 'excel' ? 'xlsx' : type); // Set the file name.
                    a.style.display = 'none';
                    document.body.appendChild(a);
                    a.click();
                    //delete a; 
                }
            };
            xhr.send(null);

            fileDownloadCheckTimer = window.setInterval(function () {
                var cookieValue = $.cookie(report.id + 'Token');
                if (cookieValue == token)
                    finishDownload();
            }, 1000)
        };

        /* Regenerate a report */
        $scope.regenerateReport = function (report) {
            if (!validateAccess()) {
                return;
            }

            ProgressService.blockScreen();
            WebApiService.genericController.get({
                controller: 'Report', action: 'Regenerate', domain_id: report.project_domain_id, project_id: report.project_id, phase_id: report.id
            }, function () {
                $scope.reportsGrid.dataSource.read();
                toastr.success('The report is being regenerated. You\'ll receive a notification when it\'s ready.');
                ProgressService.unblockScreen();
            }, function () {
                toastr.error('Error to try regenerate the report, please contact the technical support');
                ProgressService.unblockScreen();
            });
        };

        var timeout = [];

        (function updateStatus() {
            WebApiService.genericController.query({
                controller: 'report', action: 'OnlyStatuses', domain_id: AuthService.getLoggedDomain(),
                project_id: params.project_id, phase_id: params.phase_id, department_id: params.department_id,
                room_id: params.room_id
            }, function (statuses) {
                angular.forEach($scope.reportsGrid.dataSource.data(), function (dataItem) {
                    if (dataItem) {
                        var foundItem = statuses.find(function (i) { return i.id == dataItem.id });
                        if (foundItem) {
                            dataItem.set('status', foundItem['statusPercentage']);
                            dataItem.set('status_category', foundItem['statusCategory']);
                        }
                    }
                });
                timeout.push($timeout(updateStatus, 1000));
            }, function () {
                console.log('Error to try update reports status, please contact the technical support');
            });
        })();

        $scope.$on('$destroy', function () {
            angular.forEach(timeout, function (promise) {
                $timeout.cancel(promise);
            });
        });

        $scope.shareReport = function (report) {

            report.isPrivate = !report.isPrivate;

            WebApiService.genericController.update({
                controller: 'Report', action: 'Item',
                domain_id: AuthService.getLoggedDomain(),
                project_id: params.project_id,
                phase_id: report.id
            }, report, function () {
                if (!report.isPrivate)
                    toastr.success('The report is now visible to anyone in the current domain');
                else
                    toastr.success('The report now is visible only for you');
                $scope.reportsGrid.dataSource.read();
            }, function () {
                toastr.error('Error to share report');
            });
        }

        function validateAccess() {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return false;
            }

            return true;
        }
    }]);
