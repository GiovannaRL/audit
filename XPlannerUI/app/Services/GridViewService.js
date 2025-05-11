xPlanner.factory('GridViewService', ['ProgressService', 'WebApiService', 'toastr', '$q', 'DialogService', 'GridService', 'AuthService',
    function (ProgressService, WebApiService, toastr, $q, DialogService, GridService, AuthService) {

        function _isSystemView(name, showMessage, isSave) {
            if (name && ((name.charAt(0) === '(' && name.charAt(name.length - 1) === ')'))) {
                if (showMessage)
                    toastr.error("The views inside () are reserved views of the system and cannot be saved or deleted." + (isSave ? ' Please choose a name without the () to your customized view' : ''));
                return true;
            }
            return false;
        };

        function saveViewAux(grid, type, name, update, consolidatedColumns, is_private) {
            return $q(function (resolve, reject) {

                var options = grid.getOptions();
                if (options && options.dataSource && options.dataSource.data)
                    delete options.dataSource.data;

                var state = {
                    name: name,
                    type: type,
                    is_private: is_private,
                    gridview_id: update ? update.gridview_id : null,
                    domain_id: AuthService.getLoggedDomain(),
                    added_by: AuthService.getLoggedUserEmail(),
                    consolidated_columns: consolidatedColumns.length > 0 ? JSON.stringify(consolidatedColumns) : null,
                    grid_state: JSON.stringify(angular.extend(options, { isConsolidated: false }))
                };

                var params = update ? { controller: "GridView", action: "Item", domain_id: AuthService.getLoggedDomain(), project_id: type } : { controller: "GridView", action: "Item", domain_id: AuthService.getLoggedDomain(), project_id: type };

                ProgressService.blockScreen();
                WebApiService.genericController[update ? 'update' : 'save'](params, state, function (data) {
                    
                    update ? toastr.success("Grid's view updated") : toastr.success("Grid's view saved");
                    resolve(data);
                    ProgressService.unblockScreen();
                }, function (data) {
                    var errorMsg = "Error to " + (update ? 'update' : 'save') + (" grid's view, please contact the technical support");
                    if (data.data) {
                        errorMsg = data.data;
                    }
                    ProgressService.unblockScreen();
                    toastr.error(errorMsg);
                    reject();
                });
            });
        };

        function _saveView(grid, type, name, current_views, consolidatedColumns, is_private) {

            return $q(function (resolve, reject) {
                if (name && name.trim() !== '') {

                    if (_isSystemView(name, true, true)) return;


                    name = name.toLowerCase();
                    var viewExists = current_views.find(function (i) { return i.name.toLowerCase() === name });
                    var sharedViewExist = current_views.find(function (i) { return i.name.toLowerCase() === name + ' (shared)' });
                    var isAdmin = AuthService.getLoggedUserType() == 1;

                    if (sharedViewExist && !is_private && !isAdmin) {
                        toastr.error("There is another Shared View created with the same name. Please choose another name.")
                        return;
                    };

                    if (viewExists || sharedViewExist) {
                        var auxDialog = sharedViewExist ? "Shared " : "";
                        var auxView = sharedViewExist ? sharedViewExist : viewExists;
                        DialogService.Confirm(auxDialog + "View's name already exists", "Save the view with this name will replace the existing " + auxDialog + "view. Do you want to continue?")
                            .then(function () {
                                saveViewAux(grid, type, auxView.name, auxView, consolidatedColumns, is_private).then(function (data) { resolve({ newView: data, updatedView: auxView }) }, function () { reject(); });
                            }, function () { reject(); });
                        return;
                    }

                    saveViewAux(grid, type, name, null, consolidatedColumns, is_private).then(function (data) { resolve({ newView: data }) }, function () { reject(); });
                    
                } else {
                    toastr.error('Please make sure you enter a view name');
                    reject();
                }
            });
        };

        function _loadView(grid, view, height, data) {

            return $q(function (resolve, reject) {
                if (!view) {
                    toastr.error("You need to select a view to load");
                    reject();
                    return;
                }

                GridService.SetSavedState(grid, data, null, null, height, view);
                resolve();
            });
        };

        function _deleteView(view) {
            return $q(function (resolve, reject) {

                var userName = AuthService.getLoggedUserEmail();

                if (!view) {
                    toastr.error("You need to select a view to delete");
                    reject();
                    return;
                }

                var isAdmin = AuthService.getLoggedUserType() == 1;

                if (view.added_by != userName && !isAdmin) {
                    toastr.error('You cannot delete shared view created by other users, this view was created by ' + view.added_by  + '.');
                    reject();
                    return;
                }

                if (!_isSystemView(view.name, true)) {
                    DialogService.Confirm('Are you sure?', 'The view will be deleted permanently').then(function () {
                        ProgressService.blockScreen();
                        WebApiService.genericController.remove_with_data({
                            controller: "GridView", action: "Item", domain_id: view.type
                        }, view,
                        function () {
                            ProgressService.unblockScreen();
                            toastr.success('View Deleted');
                            resolve();
                            }, function (data) {
                            ProgressService.unblockScreen();
                            toastr.error(data.data);
                            reject();
                        });
                    }, function () { reject(); });
                }
            });
        };

        return {
            deleteView: _deleteView,
            loadView: _loadView,
            saveView: _saveView,
            isSystemView: _isSystemView
        };

    }]);