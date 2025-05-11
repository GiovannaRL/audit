// Functions to kendo grid without angular
xPlanner.factory('KendoGridService', ['DialogService', '$timeout', 'localStorageService', 'toastr', '$q', 'WebApiService',
        'ProgressService', 'AudaxwareDataService', 'AuthService', '$location',
    function (DialogService, $timeout, localStorageService, toastr, $q, WebApiService, ProgressService, AudaxwareDataService,
        AuthService, $location) {

        /* private functions */
        function getAllData(grid) {
            if (grid) {
                var returnArray = [];

                angular.forEach(new kendo.data.Query(grid.dataSource.data()).filter(grid.dataSource.filter()).data, function (item) {
                    returnArray.push(item);
                });

                return returnArray;
            }
        };
        /* END - private functions */

        function _getAllPageData(grid) {

            var items = grid.items();

            data = [];

            items.each(function (index, item) {
                data.push(grid.dataItem(item));
            });

            return data;
        }

        // Select one row
        function _selectRow(event, elem) {

            var grid = event.data.grid;
            var grid_name = event.data.grid_name;

            if (grid) {

                elem = elem || this;

                var checked = elem.checked,
                            row = $(elem).closest("tr"),
                            dataItem = grid.dataItem(row);

                grid.selecteds = grid.selecteds || [];
                if (checked) {
                    //-select the row
                    row.addClass("k-state-selected");
                    grid.selecteds.push(dataItem);

                    if (grid.selecteds.length == grid.items().length) {
                        $("#select-all-" + grid_name).prop('checked', true);
                    }
                } else {
                    //-remove selection
                    row.removeClass("k-state-selected");
                    $("#select-all-" + grid_name).prop('checked', false);

                    // remove item from selecteds array
                    var index = grid.selecteds.indexOf(dataItem);
                    grid.selecteds.splice(index, 1);
                }
            }
        };

        // Get the selected items from grid
        function _getSelecteds(grid) {
            return grid ? grid.selecteds || [] : [];
        };

        // Get template to select all checkbox using the kendo ui checkbox
        function _getSelectAllTemplate(grid_name) {
            return '<input type="checkbox" id="select-all-' + grid_name + '" class="k-checkbox"/><label class ="k-checkbox-label" for="select-all-' + grid_name + '"></label>';
        };

        // Get template to select a row in the grid using the kendo ui checkbox
        function _getSelectRowTemplate(id) {
            return '<input type="checkbox" id="' + id + '" class="k-checkbox"><label class="k-checkbox-label" for="' + id + '"></label>';
        };

        // Get template to asset_code as a link
        function _getAssetCodeLinkTemplate(asset) {
            if (asset) {
                var href = $location.absUrl().replace($location.path(), '/workspace/assets/' + (asset.asset_domain_id ? asset.asset_domain_id : asset.domain_id) + '/' + asset.asset_id);
                return '<a href="' + href + '" target="_blank">' + asset.asset_code + '</a>';
            }
        }

        function _getLocationLink(title, data) {
            if (data && data.project_id && data.phase_id && data.department_id && data.room_id) {
                var href = $location.absUrl().replace($location.path(), '/projects/' + data.project_id + '/phase/' + data.phase_id + '/department/' + data.department_id + '/room/' + data.room_id + '/assets');
                return '<span title="' + title +'"><a href="' + href + '">' + title + '</a></span>';
            }
        }

        // To be apply when the status of the select all checkbox changes
        // TODO - Configure select all to indeterminate status
        function _selectAllChange(ev) {

            if (ev.data.grid) {
                var checked = ev.target.checked;

                if (checked) {
                    _selectAll(ev.data.grid, null, !ev.data.grid.options.pageable);
                } else {
                    _unselectAll(ev.data.grid);
                }
            }
        }

        // Select all the items in the grid
        function _selectAll(grid, itemsGrid, allGrid, allData) {

            if (!itemsGrid)
                itemsGrid = grid.items();

            itemsGrid.addClass('k-state-selected');
            grid.tbody.find('.k-checkbox')
                .prop('checked', true);
            grid.thead.find('.k-checkbox')
               .prop('checked', true);

            if (allGrid)
                grid.selecteds = allData || getAllData(grid);
            else {
                if (!grid.selecteds)
                    grid.selecteds = [];
                itemsGrid.each(function (index, item) {
                    grid.selecteds.push(grid.dataItem(item));
                });
            }
        }

        function _addSelectClassAllCheckbox(grid) {

            grid.thead.find('.k-checkbox')
               .prop('checked', true);
        };

        function _removeSelectClassAllCheckbox(grid) {

            grid.thead.find('.k-checkbox')
               .prop('checked', false);
        };

        // Unselect all the items in the grid
        function _unselectAll(grid) {
            var items = grid.items();
            grid.selecteds = [];

            items.removeClass('k-state-selected');
            grid.tbody.find('.k-checkbox')
                .prop('checked', false);
            grid.thead.find('.k-checkbox')
                .prop('checked', false);
        }

        // function which collapse all the groups in the grid
        function _collapseAll(grid) {
            if (grid) {
                grid.tbody.find(".k-i-collapse").click();
                grid.expand = true;
            }
        };

        // function which expands all the groups in the grid
        function _expandAll(grid) {
            if (grid) {
                grid.tbody.find(".k-i-expand").click();
                grid.expand = false;
            }
        };

        // function that if the groups are collapsed expand all otherwise collpse all
        function _collapseExpand(grid) {
            if (grid) {
                grid.expand !== false ? _expandAll(grid) : _collapseAll(grid);
            }
        };

        // Verify if all the items in the grid are selected
        function _allPagesSelected(grid) {
            if (grid && grid.selecteds)
                return grid.selecteds.length > 0 && getAllData(grid).length === grid.selecteds.length;

            return false;
        };

        // Add the class k-state-selected to the items on the grid which should be selected
        function _addSelectedClass(grid, items, all) {

            if (grid) {

                if (all) {
                    grid.items().addClass('k-state-selected')
                    .find(".k-checkbox")
                    .attr("checked", "checked");

                    _addSelectClassAllCheckbox(grid);
                    return;
                }

                if (!items)
                    items = grid.selecteds;

                var cont = 0;

                angular.forEach(items, function (item) {
                    var elem = grid.tbody.find("tr[data-uid='" + item.uid + "']");
                    if (elem.length > 0) {
                        cont++;
                        elem.addClass('k-state-selected')
                            .find(".k-checkbox")
                            .attr("checked", "checked");
                    }
                });

                if (grid.selecteds && grid.dataSource.pageSize() === cont) {
                    _addSelectClassAllCheckbox(grid);
                } else {
                    _removeSelectClassAllCheckbox(grid);
                }
            }
        };

        /* Put in members parameter the columns which should be filtered
            and it fiter */
        function _setFilteredMembers(filter, members) {
            if (filter.filters) {
                for (var i = 0; i < filter.filters.length; i++) {
                    _setFilteredMembers(filter.filters[i], members);
                }
            }
            else {
                members[filter.field] = true;
            }
        };

        // Save grid state on localStorage
        function _saveState(grid, saveOn, timeout) {
            $timeout(function () {
                var options = grid.getOptions();
                options.dataSource.data = [];
                if (localStorageService.isSupported) {
                    localStorageService.set(saveOn, options);
                } else if (localStorageService.cookie.isSupported) {
                    localStorageService.cookie.set(saveOn, options);
                }
            }, timeout || 0);
        };

        // To be apply when the data bound event is fired
        function _dataBound(grid, saveStateOn) {
            if (grid) {
                if (grid.dataSource.group().length) {
                    _collapseAll(grid);
                }

                /* appends an onclick event to the header columns to show a alert message when the user tries to 
                    sort by a grouped column */
                if (!grid.notFirstBound) {
                    grid.thead.find("th[data-role='columnsorter']").on('click', function (e) {
                        if (grid.dataSource.group().find(function (i) { return i.field == e.currentTarget.attributes["data-field"].nodeValue; })) {
                            DialogService.Alert('Grouped column', 'When the data is grouped, it is always sorted by the grouped columns in ascending order, if you want to sort the data by this column you need to ungroup.');
                        }
                    });
                    grid.notFirstBound = true;
                }

                if (!_allPagesSelected(grid)) {
                    //grid.selecteds = [];
                    _addSelectedClass(grid);
                } else {
                    _addSelectedClass(grid, null, true);
                }

                var filter = grid.dataSource.filter();
                grid.thead.find(".k-link.k-state-active").removeClass("k-state-active");
                if (filter) {
                    var filteredMembers = {};
                    _setFilteredMembers(filter, filteredMembers);
                    grid.thead.find("th[data-field]").each(function () {
                        var cell = $(this);
                        var filtered = filteredMembers[cell.data("field")];
                        if (filtered) {
                            cell.find(".k-link").addClass("k-state-active");
                        }
                    });
                }
                if (saveStateOn) {
                    _saveState(grid, saveStateOn);
                }

                filter = JSON.stringify(filter);
                if (grid.appliedFilters != filter) {
                    grid.appliedFilters = filter;
                    grid.dataSource.page(1);
                }
            }
        };

        function _initiateGridEvents(grid, grid_name) {

            //bind click event to the checkboxes
            grid.table.on("click", ".k-checkbox", { grid: grid, grid_name: grid_name }, _selectRow);

            // bind change event to the select all checkbox
            $('#select-all-' + grid_name).on('change', { grid: grid }, _selectAllChange);
        }

        // Clear all filters from grid
        function _clearFilters(grid) {
            if (grid.dataSource.filter()) {
                grid.dataSource.filter(null);
                _unselectAll(grid);
            }
        };

        function _anySelected(grid) {
            return grid && grid.selecteds && grid.selecteds.length > 0;
        }

        function _verifySelected(action, type, grid, onlyOne) {

            if (!_anySelected(grid)) {
                onlyOne ? toastr.error('You need to select ' + ("aeiou".indexOf(type.charAt(0).toLowerCase()) > -1 ? 'an ' : 'a ') + type + ' to ' + action) : toastr.error('You need to select at least one ' + type + ' to ' + action);
                return false;
            }

            if (onlyOne && _getSelecteds(grid).length > 1) {
                toastr.error('You can ' + action + ' only one ' + type + ' at a time');
                return false;
            }

            return true;

        };

        // set the grid header authorization
        function _addHeader(options) {
            if (options.dataSource.transport) {
                options.dataSource.transport.read ?
                options.dataSource.transport.read.headers = {
                    Authorization: "Bearer " + AuthService.getAccessToken()
                } : options.dataSource.transport.options.read.headers = {
                    Authorization: "Bearer " + AuthService.getAccessToken()
                };
            }
            return options;
        }

        // set the grid height
        function _addHeightToOptions(options, height) {
            options.height = height || (window.innerHeight - 170);
            return options;
        }

        // check the saved columns with the default columns
        function _checkcolumns(options, defaultColumns) {

            if (options && defaultColumns) {
                var currentColumn;
                var removedColumns = [];
                // Remove duplicates (duplicates were inserted due a bug in the past)
                var columnsMap = {};
                var defaultColumnsMap = {};

                options.columns.removeIf(
                        function(item, index)
                        {
                            if (columnsMap.hasOwnProperty(item.field))
                            {
                                return true;
                            }
                            else
                            {
                                columnsMap[item.field] = item;
                            }
                        }
                    );

                // Fills out map with default columns
                angular.forEach(defaultColumns, function (item, index) {defaultColumnsMap[item.field] = item});


                // Removes columns that no longer exist
                for (var i = 0; i < options.columns.length; i++) {
                    if (options.columns[i].field) {
                        if (defaultColumnsMap.hasOwnProperty(options.columns[i].field)) {
                            currentColumn = defaultColumnsMap[options.columns[i].field];
                            currentColumn.hidden = options.columns[i].hidden;
                            currentColumn.locked = options.columns[i].locked;
                            options.columns[i] = currentColumn;
                        }
                        else
                        {
                            options.columns.splice(i, 1);
                            --i;
                        }
                    } else if (i === 0 && options.columns[i].headerTemplate) {
                        options.columns[i] = defaultColumns[0];
                    }
                };

                // Adds new columns
                defaultColumns.map(function (column) {
                    if (column.field) {
                        if (!columnsMap.hasOwnProperty(column.field))
                        {
                            column.hidden = true;
                            options.columns.push(column);
                            columnsMap[column.field] = column;
                        }
                    }
                });

            }
            return options;
        };

        // save grid state on the database
        function _saveGridState(options, type, name, notify) {
            return $q(function (resolve, reject) {

                if (options && options.dataSource) {
                    if (options.dataSource.data) {
                        delete options.dataSource.data;
                    }

                    if (options.dataSource.transport) {
                        if (options.dataSource.transport.read)
                            delete options.dataSource.transport.read.headers;
                        else
                            delete options.dataSource.transport.options.read.headers;
                    }
                }

                var state = {
                    name: name,
                    type: type,
                    grid_state: JSON.stringify(options)
                };

                WebApiService.genericController.save({
                    controller: "GridView", action: "Item"//, domain_id: type, project_id: name
                }, state, function () {
                    if (notify) {
                        toastr.success('Grid state saved');
                    }
                    resolve();
                }, function (error) {
                    toastr.error('Error to save grid state, please contact the technical support');
                    reject();
                });
            })
        };

        function _setSchema(options, schema) {
            if (schema)
                options.dataSource.schema ?
                    options.dataSource.schema = schema :
                    options.dataSource.options.schema = schema;
            return options;
        }

        // Get the saved status from database or localstorage
        function _getSavedState(view, getFrom, params, height, defaultColumns, defaultSchema) {
            return $q(function (resolve, reject) {
                var myJson = superJson.create();
                var options;

                if (view) {
                    resolve(_setSchema(_addHeader(_checkcolumns(_addHeightToOptions(myJson.parse(view.grid_state), height), defaultColumns)), defaultSchema));
                    return;
                }

                if (getFrom) {
                    options = localStorageService.isSupported ? localStorageService.get(getFrom) : localStorageService.cookie.isSupported ? localStorageService.cookie.get(getFrom) : null;
                }

                if (!options) {
                    if (params) {
                        params.domain_id = params.domain_id || params.type;
                        params.project_id = params.project_id || params.name;
                        WebApiService.genericController.get(params, function (data) {
                            resolve(_setSchema(_addHeader(_checkcolumns(_addHeightToOptions(myJson.parse(data.grid_state), height), defaultColumns)), defaultSchema));
                        }, function (error) {
                            error.status === 404 ? resolve(null) : reject();
                        });
                    } else {
                        resolve(null);
                    }

                } else {
                    resolve(_setSchema(_addHeader(_checkcolumns(_addHeightToOptions(options, height), defaultColumns)), defaultSchema));
                }
            });
        };

        function _setSavedState(grid, data, getFrom, params, height, view, url, defaultView, columnsEditor, dataBound, pdfExportFn, saveGridStateFn) {

            return $q(function (resolve, reject) {
                var saveInDB = false;

                _getSavedState(view, getFrom, params, height, defaultView ? defaultView.columns : null, defaultView ? defaultView.dataSource.options ? defaultView.dataSource.options.schema : defaultView.dataSource.schema : null)
                    .then(function (options) {

                        if (!options) {
                            if (!defaultView)
                                return;
                            options = _addHeightToOptions(defaultView, height);
                            saveInDB = params ? true : false;
                        }
                        options.pageable.pageSizes = defaultView.pageable.pageSizes;
                        if (options.pageable.pageSizes.indexOf(options.pageable.pageSize) < 0) {
                            options.pageable.pageSize = defaultView.pageable.pageSize;
                        }

                        if (url) {
                            options.dataSource.transport.read ?
                            options.dataSource.transport.read.url = url : options.dataSource.transport.options.read.url = url;
                        }

                        if (!options.dataSource.filter)
                            options.dataSource.filter = null;

                        if (columnsEditor) {
                            angular.forEach(columnsEditor, function (ed) {
                                options.columns.map(function (c) { if (c.field === ed.field) c.editor = ed.editor; return c; });
                            });
                        }

                        /* Add functions to options */
                        options.dataBound = dataBound;
                        options.pdfExport = pdfExportFn;
                        if (saveGridStateFn) {
                            options.columnHide = saveGridStateFn;
                            options.columnShow = saveGridStateFn;
                            options.columnReorder = saveGridStateFn;
                        }
                        options.filterMenuInit = function (e) {

                            if (e.field === "asset_category" || e.field === "asset_subcategory") {
                                var filterMultiCheck = this.thead.find("[data-field=" + e.field + "]").data("kendoFilterMultiCheck")
                                filterMultiCheck.container.empty();
                                filterMultiCheck.checkSource.sort({ field: e.field, dir: "asc" });

                                filterMultiCheck.checkSource.data(filterMultiCheck.checkSource.view().toJSON());
                                filterMultiCheck.createCheckBoxes();
                            }
                        };
                        //options.columnMenuInit = function (e) {

                        //    console.log(e);

                        //    if (e.field === "asset_category" || e.field === "asset_subcategory") {
                        //        var filterMultiCheck = e.container.find(".k-filterable").data("kendoFilterMultiCheck")
                        //        filterMultiCheck.container.empty();
                        //        filterMultiCheck.checkSource.sort({field: e.field, dir: "asc"});


                        //        filterMultiCheck.checkSource.data(filterMultiCheck.checkSource.view().toJSON());
                        //        filterMultiCheck.createCheckBoxes();
                        //    }
                        //}
                        /* END - Add functions to options */

                        grid.setOptions(options);
                        //grid.dataSource.page(1);// always start on page 1

                        _saveState(grid, getFrom);

                        if (data) {
                            grid.dataSource.data(data);
                        }

                        if (saveInDB) {
                            options = grid.getOptions();
                            _saveGridState(options, params.type, params.name).then(function () { resolve(); }, function () { reject(); });
                        } else {
                            resolve();
                        }


                    }, function () { reject(); });
            });
        };

        function _getCurrencyTemplate(value) {

            valueView = kendo.toString(Number(value), 'n2');

            template = "<section class=\"currency\">";
            if (valueView) {
                template += "<section class=\"dollar-sign\">$</section>";
            }

            template += "<section class=\"dollar-value\">" + valueView + "</section></section>";

            return template;
        };

        function _getCheckIconTemplate() {
            return "<center><i class=\"material-icons no-button\" style=\"color: green\">check</i></center>";
        };

        function _getDownloadFileWithDomainTemplate(filename, domain, container, img) {
            return "<section align=center onclick=\"downloadFile('" + filename + "', " + domain + ", '" + container + "')\">" + (img ? "<img class=\"icon\" src=\"" + img + "\">" : "") + "</section>";
        };

        function _exportGrid(to, grid) {
            switch (to) {
                case 'pdf':
                    DialogService.Confirm('Export to PDF', 'This operation will lock the browser for a few seconds. Do you want to continue?')
                        .then(function () {
                            ProgressService.blockScreen();
                            grid.saveAsPDF().then(function () {
                                grid.showColumn(0);
                                $(".k-pager-wrap.k-grid-pager.k-widget.k-floatwrap").css("display", "block");
                                ProgressService.unblockScreen();
                            });
                        });
                    break;
                case 'excel':
                    grid.saveAsExcel();
            }
        };

        function _deleteItems(webApiController, getParams, grid, data, verifyAudaxwareData) {

            var items = verifyAudaxwareData ? AudaxwareDataService.RemoveAudaxwareItems(grid.selecteds) : grid.selecteds;

            return $q.all(items.map(function (item) {
                return webApiController[data ? 'remove_with_data' : 'remove'](getParams ? getParams(item) : item, data ? data : null, function () {
                    grid.dataSource.remove(item); // removes the item from the grid
                    grid.selecteds.splice(grid.selecteds.indexOf(item), 1);
                }).$promise;
            }));
        };

        function _exportToPDF_Event(grid) {
            grid.hideColumn(0);
            $(".k-pager-wrap.k-grid-pager.k-widget.k-floatwrap").css("display", "none");
            return grid;
        };

        function _getStructure(dataSource, columns, toolbar, gridOptions, pageable, exportConfig) {

            return _addHeightToOptions({
                toolbar: toolbar ? [toolbar] : null,
                excel: exportConfig ? exportConfig.excel : null,
                pdf: exportConfig ? exportConfig.pdf : null,
                dataSource: dataSource,
                pageable: pageable === false ? false : {
                    pageSizes: pageable && pageable.pageSizes ? pageable.pageSizes : [50, 100, 300, 500, 1000, 2000, 3000],
                    pageSize: pageable && pageable.pageSizeDefault ? pageable.pageSizeDefault : 50,
                    buttonCount: 5,
                    input: true
                },
                rowTemplate: gridOptions && gridOptions.rowTemplate ? gridOptions.rowTemplate : null,
                scrollable: true,
                sortable: true,
                filterable: gridOptions && gridOptions.filterable === false ? false : {
                    operators: {
                        string: {
                            contains: "Contains",
                            doesnotcontain: "Does not contain",
                            eq: "Is equal To",
                            neq: "Is not equal To",
                            startswith: "Starts With",
                            endswith: "Ends with",
                        }
                    }
                },
                resizable: true,
                reorderable: gridOptions && gridOptions.reorderable,
                groupable: gridOptions && gridOptions.groupable,
                editable: gridOptions && gridOptions.editable ? gridOptions.editable : false,
                mobile: true,
                noRecords: gridOptions && gridOptions.noRecords ? {
                    template: gridOptions.noRecords
                } : null,
                selectable: gridOptions && gridOptions.selectable ? gridOptions.selectable : null,
                columnMenu: gridOptions && gridOptions.columnMenu ? gridOptions.columnMenu : null,
                columns: columns
            }, gridOptions && gridOptions.height ? gridOptions.height : null);
        };

        function _removeSelectedItems(grid, indexes) {

            if (grid && indexes.length == grid.selecteds.length) {
                _unselectAll(grid);
                return;
            }

            var auxArray = [];

            angular.forEach(indexes, function (index) {
                grid.tbody.find("tr[data-uid='" + grid.selecteds[index].uid + "']").removeClass('k-state-selected');
                auxArray.push(grid.selecteds[index]);
            });

            grid.selecteds = grid.selecteds.filter(function (i) {
                return auxArray.indexOf(i) < 0
            });
        }

        return {
            SelectRow: _selectRow,
            GetSelecteds: _getSelecteds,
            GetSelectAllTemplate: _getSelectAllTemplate,
            GetSelectRowTemplate: _getSelectRowTemplate,
            SelectAllChange: _selectAllChange,
            CollapseAll: _collapseAll,
            CollapseExpand: _collapseExpand,
            ExpandAll: _expandAll,
            InitiateGridEvents: _initiateGridEvents,
            DataBound: _dataBound,
            AddSelectedClass: _addSelectedClass,
            SaveState: _saveState,
            SelectAll: _selectAll,
            UnselectAll: _unselectAll,
            ClearFilters: _clearFilters,
            VerifySelected: _verifySelected,
            SetSavedState: _setSavedState,
            GetSavedState: _getSavedState,
            SaveGridState: _saveGridState,
            GetCurrencyTemplate: _getCurrencyTemplate,
            GetCheckIconTemplate: _getCheckIconTemplate,
            GetDownloadFileWithDomainTemplate: _getDownloadFileWithDomainTemplate,
            ExportGrid: _exportGrid,
            DeleteItems: _deleteItems,
            ExportToPDF_Event: _exportToPDF_Event,
            GetStructure: _getStructure,
            RemoveSelectedItems: _removeSelectedItems,
            AnySelected: _anySelected,
            GetAllPageData: _getAllPageData,
            GetAssetCodeLinkTemplate: _getAssetCodeLinkTemplate,
            GetLocationLink: _getLocationLink
        };

    }]);