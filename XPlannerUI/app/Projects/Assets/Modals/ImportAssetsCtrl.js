xPlanner.controller('ImportAssetsCtrl', ['$scope', '$mdMedia', '$mdStepper', '$mdDialog', '$timeout', 
        'toastr', '$stateParams', 'AuthService', 'ProgressService', 'HttpService', 'GridService', 'Upload', 'WebApiService', '$sce', 'KendoGridService', 
    function ($scope, $mdMedia, $mdStepper, $mdDialog, $timeout, toastr, $stateParams, AuthService,
        ProgressService, HttpService, GridService, Upload, WebApiService, $sce, KendoGridService) {

        
        var steppers;

        $timeout(
            function () {
                steppers = $mdStepper('stepper-import');
            }, false);
        var fields = {
            step1: ['file'],
            step2: [],
            step2: []
        };

        $scope.wizardData = {};
        $scope.mobileStep = $mdMedia('xs');
        $scope.currentStep = 1;
        $scope.show_firefox = false;
        $scope.file = {};
        $scope.totalChanged = 0; 
        $scope.totalErrors = 0;
        $scope.totalNew = 0;
        $scope.totalNewCatalog = 0;
        

        if (navigator.userAgent.indexOf('Firefox') != -1) {
            $scope.show_firefox = true;
        }


        function getColumns(columns){
            var _columns = [
                   { headerTemplate: "<md-checkbox class=\"checkbox\" md-indeterminate=\"allSelected(wizardData.importGrid)\" ng-checked=\"allPagesSelected(wizardData.importGrid)\" aria-label=\"checkbox\" ng-click=\"select($event, wizardData.importGrid, true)\"></md-checkbox>", template: "<md-checkbox class=\"checkbox\" ng-click=\"select($event, wizardData.importGrid)\" ng-checked=\"isSelected(wizardData.importGrid, dataItem)\" aria-label=\"checkbox\"></md-checkbox>", width: "3em" },
                   { field: "id", title: "ID", width: 90 },
                   { field: "statusLabel", title: "Status", width: 120, filterable: {multi:true}},
                   { field: "statusComment", title: "Comment", width: 250, attributes: { "class": "no-multilines" },
                    template: function(item)
                       {
                           var comment = item.statusComment || "";
                           return '<span title="' + comment + '"> ' + comment + '</span>';
                       }
                   },
                   {
                       field: "Code", title: "Code", width: 120,
                       template: codeTemplate
                   },
                   { field: "jsn", title: "JSN", width: 100 },
                   { field: "resp", title: "Resp", width: 100 },
                   { field: "jsnNomenclature", title: "JSN Nomenclature", width: 140, hidden: true },
                   { field: "manufacturer", title: "Manufacturer", width: 200 },
                   { field: "modelNumber", title: "Model No.", width: 200 },
                   { field: "modelName", title: "Model Name", width: 200 },
                   { field: "phase", title: "Phase", width: 150 },
                   { field: "department", title: "Department", width: 240 },
                   { field: "roomNumber", title: "Room No.", width: 180 },
                   { field: "roomName", title: "Room Name", width: 200 },
                   { field: "worksheet", title: "Worksheet", width: 200},
                   { field: "u1", title: "U1", width: 80 },
                   { field: "u2", title: "U2", width: 80 },
                   { field: "u3", title: "U3", width: 80 },
                   { field: "u4", title: "U4", width: 80 },
                   { field: "u5", title: "U5", width: 80 },
                   { field: "u6", title: "U6", width: 80 },
                   { field: "plannedQty", title: "Budget Qty", width: 150 },
                   { field: "unitBudget", title: "Unit Budget (Net)", width: 150, template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unitBudget); } },
                   { field: "unitMarkup", title: "Unit Markup (%)", width: 150 , template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unitMarkup); }},
                   { field: "unitTax", title: "Unit Tax (%)", width: 150  , template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unitTax); }},
                   { field: "unitInstallNet", title: "Unit Install (Net)", width: 150  , template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unitInstallNet); }},
                   { field: "unitInstallMarkup", title: "Unit Install Markup (%)", width: 150  , template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unitInstallMarkup); }},
                   { field: "unitFreightNet", title: "Unit Freight (Net)", width: 150  , template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unitFreightNet); }},
                   { field: "unitFreightMarkup", title: "Unit Freight Markup (%)", width: 150  , template: function (dataItem) { return KendoGridService.GetCurrencyTemplate(dataItem.unitFreightMarkup); }}

            ];
            return _columns;
        }


        /* Select the grid's rows */
        $scope.isSelected = GridService.isSelected;
        $scope.allSelected = GridService.allSelected;
        $scope.select = GridService.select;
        $scope.allPagesSelected = GridService.allPagesSelected;
        /* END - Select the grid's rows */


        function codeTemplate(item)
        {
            var input = '<input class="codeClassForTemplate"/>'

            return input
        }

        function getDataSource(data) {
            var statusLabels = $scope.data[0].statusLabels;
            for (var i=0; i < data.length; ++i)
            {
                data[i].statusLabel = statusLabels[data[i].status];
            }
            return _dataSource = new kendo.data.DataSource({
                data: data,
                pageSize:20,
                schema: {
                    model: {
                        id: "id"
                    }
                },
                error: function () {
                    ProgressService.unblockScreen();
                    toastr.error("Error to retrieve data from server, please contact technical support");
                }
            });
        }

        $scope.gridHeight = window.innerHeight - 320;
        var _gridOptions = {
            reorderable: true,
            groupable: true,
            noRecords: "No import data available",
            height: $scope.gridHeight
        };


        $scope.fireFileUpload = function () {
            $('#file').trigger('click');
        };


        /* Go to the next step */
        $scope.next = function () {
            switch ($scope.currentStep) {
                case 1:
                    _analyze();
                    break;
                case 2:
                    _viewColumns();
                    break;
                case 3:
                    _import();
                    break;
            }
        };

        /* Back to previous step or cancel */
        $scope.back = function () {
            steppers.back();
            $scope.currentStep--;
            if ($scope.data.unusedColumns.length == 0 && $scope.currentStep == 2){
                steppers.back();
                $scope.currentStep--;
            }
        };

        $scope.cancel = function () {
                $mdDialog.cancel();
        };
              

        /* Go to next step */
        function _viewColumns() {
            $scope.currentStep++;
            steppers.clearError();
            steppers.next();
        };

        function onCodeChange(e) {
            var element = e.sender.element;
            var row = element.closest("tr");
            var grid = $scope.wizardData.importGrid;

            var dataItem = grid.dataItem(row);
            var value = e.sender.value();

            dataItem.set("code", value);
            dataItem.set("status", 1);
            if (typeof(dataItem.oldCode) === 'undefined')
                dataItem.oldCode = "";
            if (value == dataItem.oldCode)
                return;
            dataItem.oldCode = value;
            if (value != "")
            {
                dataItem.set("statusLabel", "New");
                ++$scope.data.totalNew;
                --$scope.data.totalErrors;
            }
            else
            {
                dataItem.set("statusLabel", "Error");
                --$scope.data.totalNew;
                ++$scope.data.totalErrors;
            }
            $scope.$apply();
        };

        $scope.dataBound = function(e) {
            GridService.dataBound($scope.wizardData.importGrid);
            var grid = e.sender;
            var items = e.sender.items();

            items.each(function(e) {
                var dataItem = grid.dataItem(this);
                var codeField = $(this).find('.codeClassForTemplate');
                $(codeField).kendoDropDownList({
                    value: dataItem.code,
                    dataSource: dataItem.matchingCodes,
                    change: onCodeChange
                });

/*


                $(ddt).kendoDropDownList({
                    value: dataItem.code,
                    dataSource: [{value:'AID0001', displayValue:'AID0001'},{value:'AID0002', displayValue:'AID0002'}],
                    dataTextField: "displayValue",
                    dataValueField: "value",
                    change: onCodeChange
                });*/
            });
        };
        
        function _analyze() {
            $scope.importAssetsForm.$setSubmitted();
            var totalItems = [];
            var unusedSheets = [];
            var unusedColumns = [];
            var usedColumns = [];
            var hasUnusedSheets;

            if ($scope.importAssetsForm.$valid) {
                ProgressService.blockScreen();
                if ($scope.file.arrangeColumnsType == null) 
                    $scope.file.arrangeColumnsType = 0;
                
                Upload.upload({
                    url: HttpService.generic("AssetsInventoryImporter", "Analyze", AuthService.getLoggedDomain(), $stateParams.project_id, $scope.file.arrangeColumnsType),
                    data: { file: $scope.file.asset }
                }).then(function (data) {
                    $scope.data = data.data;
                    var pageable = {
                        pageSizes: [5, 10, 20, 50],
                        pageSizeDeafult: 20,
                        buttonCount: 5
                    };

                    for (var i = 0; i < data.data.length; i++) {
                        var sheet = data.data[i];
                        if (sheet.status == 0 && sheet.errorMessage == null) {

                            for (var j = 0; j < sheet.items.length; j++) {
                                var item = sheet.items[j];
                                totalItems.push(item);                            
                            }

                            $scope.totalChanged += sheet.totalChanged;
                            $scope.totalErrors += sheet.totalErrors;
                            $scope.totalNew += sheet.totalNew;
                            $scope.totalNewCatalog += sheet.totalNewCatalog;

                            for (var j = 0; j < sheet.unusedColumns.length; j++) {
                                var column = sheet.unusedColumns[j];
                                if (!unusedColumns.includes(column)) 
                                    unusedColumns.push(column);                                
                            }       
                            
                        }
                        else
                        {
                            var error = sheet.errorMessage;
                            var sheetError = (sheet.workSheetName + ' : ' + error);
                            unusedSheets.push(sheetError);
                            
                        }                     


                    };

                    if (unusedSheets.length > 0) {
                        $scope.hasUnusedSheets = true;

                    }

                    $scope.wizardData.options = GridService.getStructure(getDataSource(totalItems), getColumns(usedColumns), null, _gridOptions, pageable);
                    $scope.wizardData.options.dataBound = $scope.dataBound;

                    for (var i = 0; i < totalItems.length; i++) {
                        if (totalItems[i].matchingCodes)
                        {
                            totalItems[i].matchingCodes.push("");
                        }
                    }
                    
                    ProgressService.unblockScreen();

                    var columns = 0;
                    var fields = '<div layout="row" layout-align-gt-sm="space-between start" class="md-block layout-align-gt-sm-space-between-end layout-row">';
                    // Add empty spaces to ensure the last line remains aligned
                    while ((unusedColumns.length % 5) != 0) {
                        unusedColumns.push(""); 
                    }

                    for (var i = 0; i < unusedColumns.length; i++) {
                        if ((i % 5) == 0) {
                            fields = fields + '</div>';
                            fields = fields + '<div layout="row" layout-align-gt-sm="space-between start" class="md-block layout-align-gt-sm-space-between-end layout-row">';
                        }
                        fields = fields + '<label flex="20" class="flex-20">' + unusedColumns[i] + '</label>'; 
                    }
                    fields = fields + '</div>';
                    $scope.fieldsList = $sce.trustAsHtml(fields);
                    _viewColumns();
                    if (unusedColumns.length == 0) 
                        _viewColumns();

                    if (unusedSheets.length > 0) {
                        var sheet = '<div class="list-group-item" layout=""';
                        for (var i = 0; i < unusedSheets.length; i++) {
                            sheet = sheet + '<label flex="20" class="flex-20">' + unusedSheets[i] + '</label></br>';

                        }
                        sheet = sheet + '</div>';

                    }
                    $scope.unusedSheetsList = $sce.trustAsHtml(sheet);
                },
                function (error) {
                    ProgressService.unblockScreen();
                    if (error.data != null)
                        {
                        for (var i = 0; i < error.data.length; i++) {
                            var sheet = error.data[i].workSheetName;
                            var errorMessage = error.data[i].errorMessage;
                            toastr.error(sheet + ' : ' + errorMessage);
                        }
                    }
                    else
                    {
                        toastr.error("Error to perform import, if the problem persists, please contact tecnical support");
                    }
                    return '';
                })
                
            } else {
                var error = false;
                for (var i = 0; i < fields.step1.length; i++) {
                    for (var prop in $scope.importAssetsForm[fields.step1[i]].$error) {
                        error = true;
                        break;
                    }

                    if (error) {
                        steppers.error('Missing required fields');
                        return;
                    }

                    error = false;
                }
            }
        }

        
        function _import() {
            var assets = GridService.getSelecteds($scope.wizardData.importGrid);
            var labels = $scope.data[0].statusLabels;
            if (assets.length == 0) {
                toastr.error("Select at least one item to import!");
                return;
            }
            
            for (var i = 0; i < assets.length; i++) {
                if (assets[i].assetCode == null && assets[i].assetCodesList != null && assets[i].assetCodesList.length > 1) {
                    toastr.error("Asset code is required for the selected items!");
                    return false;
                }

                if (labels[assets[i].status] == 'Error') {
                    toastr.error("Some of your selected items contain errors, please unselect all items with error");
                    return false;
                }
            }

            ProgressService.blockScreen();
            WebApiService.genericController.update({ controller: "AssetsInventoryImporter", action: "ImportAsync", domain_id: AuthService.getLoggedDomain(), project_id: $stateParams.project_id }, assets, function (data) {
                toastr.success("Import process has started, you will be notified when it finishes");
                ProgressService.unblockScreen();
                $mdDialog.hide(data);
            }, function (data) {
                ProgressService.unblockScreen();
                toastr.error("Error to intialize import. The server might be too busy. Please contact technical support or try again later.");
            });
        }
    }]);
