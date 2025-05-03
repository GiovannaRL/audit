xPlanner.factory('GridService', ['toastr', 'DialogService', '$q', 'localStorageService', 'WebApiService', 'HttpService', '$stateParams',
        'AuthService', '$timeout', 'AudaxwareDataService', 'ProgressService', '$location',
    function (toastr, DialogService, $q, localStorageService, WebApiService, HttpService, $stateParams, AuthService, $timeout,
        AudaxwareDataService, ProgressService, $location) {

        function _exportToPDF_Event(grid) {
            grid.hideColumn(0);
            $(".k-pager-wrap.k-grid-pager.k-widget.k-floatwrap").css("display", "none");
            return grid;
        };

        function getAllData(grid) {
            if (grid) {
                var returnArray = [];

                angular.forEach(new kendo.data.Query(grid.dataSource.data()).filter(grid.dataSource.filter()).data, function (item) {
                    returnArray.push(item);
                });

                return returnArray;
            }
        };

        // Get template to project description as a link
        function _getProjectLinkTemplate(project) {
            if (project) {
                var href = $location.absUrl().replace($location.path(), '/projects/' + project.projectId);
                return '<a href="' + href + '">' + project.projectDescription + '</a>';
            }
        }


        function _select(e, grid, all) {
            if (!all) {
                var check = $(e.currentTarget);
                var row = check.closest("tr");
                var item = grid.dataItem(row);
                return _toggleItemSelected(item, grid);
            } else {

                var data = getAllData(grid);

                var items = grid.items();
                var num_selected = grid.selecteds ? grid.selecteds.length : 0;

                grid.selecteds = [];
                if (num_selected < items.length && items.length < data.length) {
                    _selectAll(grid, items);
                } else if (num_selected < data.length) {
                    _selectAll(grid, null, true, data);
                } else {
                    _unselectAll(grid);
                }
            }
        };

        function _getSelecteds(grid) {
            return grid ? grid.selecteds || [] : [];
        };

        function _isSelected(grid, item) {
            return grid && grid.selecteds && grid.selecteds.indexOf(item) > -1;
        };

        function _allSelected(grid) {
            if (grid && grid.selecteds)
                return grid && grid.selecteds && grid.selecteds.length > 0 && grid.selecteds.length < getAllData(grid).length && grid.selecteds.length == grid.items().length;

            return false;
        };

        function _unselectAll(grid) {
            grid.selecteds = [];
            grid.items().removeClass('k-state-selected');
        }

        function _selectAll(grid, itemsGrid, allGrid, allData) {
            if (!itemsGrid/* || allGrid*/)
                itemsGrid = grid.items();

            itemsGrid.addClass('k-state-selected');
            if (allGrid)
                grid.selecteds = allData || getAllData(grid);
            else {
                grid.selecteds = [];
                itemsGrid.each(function (index, item) {
                    grid.selecteds.push(grid.dataItem(item));
                });
            }
        }

        function _anySelected(grid) {
            return grid && grid.selecteds && grid.selecteds.length > 0;
        }

        function _RemoveSelectedItems(grid, indexes) {

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

        // Add the class k-state-active to the items on the grid which should be selected
        function _addSelectedClass(grid, items, all) {

            if (all) {
                grid.items().addClass('k-state-selected');
                return;
            }

            if (!items) {
                items = grid.selecteds;
            }

            angular.forEach(items, function (item) {
                grid.tbody.find("tr[data-uid='" + item.uid + "']").addClass('k-state-selected');
            });
        };

        /* Return a array with the new dataSource items selected equivalent to the old dataSource items selected, 
            assuming which the new dataSource have these items */
        function _copySelecteds(grid, newDataSource) {

            var oldSelecteds = angular.copy(grid.selecteds);

            if (newDataSource) {
                grid.selecteds = [];

                angular.forEach(oldSelecteds, function (item) {
                    grid.selecteds.push(newDataSource.getByUid(item.uid));
                })
            }
        };

        function _toggleItemSelected(item, grid) {

            if (!grid.selecteds)
                grid.selecteds = [];

            var index_of = grid.selecteds.indexOf(item);
            index_of !== -1 ? grid.selecteds.splice(index_of, 1) : grid.selecteds.push(item);
            grid.tbody.find("tr[data-uid='" + item.uid + "']").toggleClass('k-state-selected');
        };

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

        function _dataBound(grid, saveStateOn) {

            if (grid) {
                if (grid.dataSource.group().length) {
                    _collapseAll(grid);
                }

                /* appends an onclick event to the header columns to show a alert message when the user tries to 
                    sort by a grouped column */
                if (!grid.notFirstBound && grid.thead) {
                    grid.thead.find("th[data-role='columnsorter']").on('click', function (e) {
                        if (grid.dataSource.group().find(function (i) { return i.field == e.currentTarget.attributes["data-field"].nodeValue; })) {
                            DialogService.Alert('Grouped column', 'When the data is grouped, it is always sorted by the grouped columns in ascending order, if you want to sort the data by this column you need to ungroup.');
                        }
                    });
                    grid.notFirstBound = true;
                }

                if (!_allPagesSelected(grid))
                    grid.selecteds = [];
                else {
                    _addSelectedClass(grid, null, true);
                }

                var filter = grid.dataSource.filter();
                if (grid.thead) {
                    grid.thead.find(".k-link.k-state-active").removeClass("k-state-active");
                }
                if (filter) {
                    var filteredMembers = {
                    };
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

        function _deleteItems(webApiController, getParams, grid, data, verifyAudaxwareData) {

            var items = verifyAudaxwareData ? AudaxwareDataService.RemoveAudaxwareItems(grid.selecteds) : grid.selecteds;

            return $q.all(items.map(function (item) {
                return webApiController[data ? 'remove_with_data' : 'remove'](getParams ? getParams(item) : item, data ? data : null, function () {
                    grid.dataSource.remove(item); // removes the item from the grid
                    grid.selecteds.splice(grid.selecteds.indexOf(item), 1);
                }).$promise;
            }));
        };

        function _removeItem(grid, item) {
            if (grid && item) {
                grid.dataSource.remove(item);
                var index_of = grid.selecteds.indexOf(item);
                if (index_of > -1)
                    grid.selecteds.splice(index_of, 1);
            }
        };

        function _updateItems(grid, items) {

            if (Array.isArray(items)) {
                angular.forEach(items, function (item) {
                    grid.dataSource.pushUpdate(item);
                });
                return;
            }

            grid.dataSource.pushUpdate(items);
        };

        function _exportGrid(to, grid, excelFileName) {
            switch (to) {
                case 'pdf':
                    DialogService.Confirm('Export to PDF', 'This operation will lock the browser for a few seconds. Do you want to continue?')
                        .then(function () {
                            ProgressService.blockScreen();
                            grid.saveAsPDF().then(function () {
                                grid.showColumn(0);
                                $(".k-pager-wrap.k-grid-pager.k-widget.k-floatwrap").css("display", "block");
                                /*angular.forEach(grid.hideColumnsPDF, function (prop) {
                                    grid.showColumn(prop);
                                });*/
                                //grid.dataSource.pageSize(grid.current_page_size);
                                ProgressService.unblockScreen();
                            });
                        });
                    break;
                case 'excel':
                    //TODO JU: FIX THIS TO BE INSIDE THE CONNECTIVITYTABCTRL.JS AND REMOVE FROM HERE
                    if (excelFileName) {
                        grid.bind("excelExport", function (e) {
                            e.workbook.fileName = excelFileName;
                        });
                    }
                    grid.saveAsExcel();
            }
        }

        function _addHeader(options) {
            options.dataSource.transport.read ?
            options.dataSource.transport.read.headers = {
                Authorization: "Bearer " + AuthService.getAccessToken()
            } : options.dataSource.transport.options.read.headers = {
                Authorization: "Bearer " + AuthService.getAccessToken()
            };
            return options;
        }

        function _addHeightToOptions(options, height) {
            options.height = height || (window.innerHeight - 170);
            return options;
        }

        function _getStructure(dataSource, columns, toolbar, gridOptions, pageable, exportConfig) {

            return _addHeightToOptions({
                toolbar: toolbar ? [toolbar] : null,
                excel: exportConfig ? exportConfig.excel : null,
                pdf: exportConfig ? exportConfig.pdf : null,
                dataSource: dataSource,
                pageable: pageable === false ? false : {
                    pageSizes: pageable && pageable.pageSizes ? pageable.pageSizes : [50, 100, 200, 300],
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
                editable: gridOptions && gridOptions.editable,
                mobile: true,
                noRecords: gridOptions && gridOptions.noRecords ? {
                    template: gridOptions.noRecords
                } : null,
                selectable: gridOptions && gridOptions.selectable ? gridOptions.selectable : null,
                columnMenu: gridOptions && gridOptions.columnMenu ? gridOptions.columnMenu : null,
                columns: columns
            }, gridOptions && gridOptions.height ? gridOptions.height : null);
        };

        function _saveState(grid, saveOn, timeout) {
            $timeout(function () {
                var x = grid.getOptions();
                x.dataSource.data = [];
                if (localStorageService.isSupported) {
                    localStorageService.set(saveOn, x);
                } else if (localStorageService.cookie.isSupported) {
                    localStorageService.cookie.set(saveOn, x);
                }
            }, timeout || 0);
        };

        function _checkcolumns(options, defaultColumns) {

            if (options && defaultColumns) {
                var currentColumn;
                var removedColumns = [];

                for (var i = 0; i < options.columns.length; i++) {
                    if (options.columns[i].field) {
                        currentColumn = defaultColumns.find(function (item) { return item.field === options.columns[i].field });
                        if (currentColumn) {
                            currentColumn.hidden = options.columns[i].hidden;
                            currentColumn.locked = options.columns[i].locked;
                            options.columns[i] = currentColumn;
                        } else removedColumns.push(i);
                    } else if (i === 0 && options.columns[i].headerTemplate) {
                        options.columns[i] = defaultColumns[0];
                    }
                };

                angular.forEach(removedColumns, function (item, index) { options.columns.splice(item - index, 1); });

                defaultColumns.map(function (column) {
                    if (column.field) {
                        if (!options.columns.find(function (i) { return i.field === column.field })) {
                            column.hidden = true;
                            options.columns.push(column);
                        }
                    }
                });
            }
            return options;
        };

        function _getSavedState(view, getFrom, params, height, defaultColumns, defaultSchema) {
            return $q(function (resolve, reject) {

                var myJson = superJson.create();
                var options;

                if (view) {
                    resolve(_checkcolumns(_addHeightToOptions(myJson.parse(view.grid_state), height), defaultColumns));
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
                            resolve(_addHeader(_checkcolumns(_addHeightToOptions(myJson.parse(data.grid_state), height), defaultColumns)));
                        }, function (error) {
                            error.status === 404 ? resolve(null) : reject();
                        });
                    } else {
                        resolve(null);
                    }

                } else {
                    resolve(_addHeader(_checkcolumns(_addHeightToOptions(options, height), defaultColumns)));
                }
            });
        };

        function _saveGridState(options, type, name, notify) {
            return $q(function (resolve, reject) {

                if (options && options.dataSource)
                    if (options.dataSource.data)
                        delete options.dataSource.data;
                    if (options.dataSource.transport.read)
                        delete options.dataSource.transport.read.headers;
                    else
                        delete options.dataSource.transport.options.read.headers;

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

        function _SetSavedState(grid, data, getFrom, params, height, view, url, defaultView, columnsEditor) {

            return $q(function (resolve, reject) {
                var saveInDB = false;

                _getSavedState(view, getFrom, params, height, defaultView ? defaultView.columns : null, defaultView ? defaultView.dataSource.schema : null)
                    .then(function (options) {

                        if (!options) {
                            if (!defaultView)
                                return;
                            options = _addHeightToOptions(defaultView, height);
                            saveInDB = params ? true : false;
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

                        grid.setOptions(options);
                        grid.dataSource.page(1);// always start on page 1

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

        function _ClearFilters(grid) {
            if (grid.dataSource.filter())
                grid.dataSource.filter(null);
        };

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

        function _addItem(grid, items) {
            grid.dataSource.pushCreate(items);
        };

        function _allPagesSelected(grid) {
            if (grid && grid.selecteds)
                return grid && grid.selecteds && grid.selecteds.length > 0 && getAllData(grid).length === grid.selecteds.length;

            return false;
        };

        function _collapseAll(grid) {
            if (grid && grid.tbody) {
                grid.tbody.find(".k-i-collapse").click();
                grid.expand = true;
            }
        };

        function _expandAll(grid) {
            if (grid) {
                grid.tbody.find(".k-i-expand").click();
                grid.expand = false;
            }
        };

        function _collapseExpand(grid) {
            if (grid) {
                grid.expand !== false ? _expandAll(grid) : _collapseAll(grid);
            }
        };

        return {
            exportToPDF_Event: _exportToPDF_Event,
            select: _select,
            getSelecteds: _getSelecteds,
            isSelected: _isSelected,
            allSelected: _allSelected,
            unselectAll: _unselectAll,
            selectAll: _selectAll,
            anySelected: _anySelected,
            RemoveSelectedItems: _RemoveSelectedItems,
            addSelectedClass: _addSelectedClass,
            copySelecteds: _copySelecteds,
            toggleItemSelected: _toggleItemSelected,
            dataBound: _dataBound,
            deleteItems: _deleteItems,
            updateItems: _updateItems,
            exportGrid: _exportGrid,
            addHeightToOptions: _addHeightToOptions,
            getStructure: _getStructure,
            SetSavedState: _SetSavedState,
            GetSavedState: _getSavedState,
            SaveGridState: _saveGridState,
            ClearFilters: _ClearFilters,
            verifySelected: _verifySelected,
            addItem: _addItem,
            saveState: _saveState,
            allPagesSelected: _allPagesSelected,
            RemoveItem: _removeItem,
            collapseAll: _collapseAll,
            expandAll: _expandAll,
            collapseExpand: _collapseExpand,
            GetProjectLinkTemplate: _getProjectLinkTemplate
        };
    }]);