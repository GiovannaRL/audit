xPlanner.controller('IndexCtrl', ['$scope', '$state', 'WebApiService', 'AuthService',
    'toastr', 'DialogService', '$q', 'ProgressService', 'FabService', '$mdDialog', 'StatusListProject',
    function ($scope, $state, WebApiService, AuthService, toastr, DialogService,
        $q, ProgressService, FabService, $mdDialog, StatusListProject) {

        var params = {};
        var hasChanges = false;

        //var loadedData = false;
        $scope.add = false;
        $scope.treeviewAllData = [];
        $scope.treviewActiveData = [];
        $scope.treeviewFavoritiesData = [];
        $scope.searchTree = "";
        $scope.searchTreeLowerCase = "";

        //$scope.loggedUserType = AuthService.getLoggedUserType();
        $scope.isNotViewer = (AuthService.getLoggedUserType() != "3");


        $scope.$on('itemHasChanges', function () {
            hasChanges = true;
        });

        function _getGenericParams(controller, action, id1, id2, id3, id4, id5) {
            return {
                controller: controller,
                action: action,
                domain_id: id1 || AuthService.getLoggedDomain(),
                project_id: id2,
                phase_id: id3,
                department_id: id4,
                room_id: id5
            };
        };

        $scope.$on('updateProgressBar', function (event, value) {
            if (value != undefined) {
                if (value.domain_id == undefined)
                    value.domain_id = AuthService.getLoggedDomain();
                updateProgressBar(value);
            }
        });

        $scope.$on('getBreadcrumb', function (e, newValue) {
            $scope.path = {};
            updateProgressBar(newValue);

            if (newValue.room_id) {
                $scope.path = angular.extend($scope.path, { room_id: newValue.room_id, room_name: newValue.drawing_room_name });
                newValue = newValue.project_department;
            }

            if (newValue.department_id) {
                $scope.path = angular.extend($scope.path, { department_id: newValue.department_id, department_name: newValue.description });
                newValue = newValue.project_phase;
            }

            if (newValue.phase_id) {
                $scope.path = angular.extend($scope.path, { phase_id: newValue.phase_id, phase_name: newValue.description });
                newValue = newValue.project;
            }

            if (newValue.project_id) {
                $scope.path = angular.extend($scope.path, { project_id: newValue.project_id, project_name: newValue.project_description });
            }

        });

        function updateProgressBar(newValue) {
            if (newValue && newValue.project_id && newValue.project_id != -1) {
                $scope.costs = {};

                WebApiService.genericController.get(_getGenericParams('financials', 'All', newValue.domain_id, newValue.project_id, newValue.phase_id, newValue.department_id, newValue.room_id), function (financial) {
                    $scope.costs = financial;

                }, function (error) {
                    toastr.error("Error to retrieve financial data from server, please contact technical support");
                });
            }

        }


        $scope.getLevel = function (ids) {
            return ids.room_id ? 'room' : ids.department_id ? 'department' : ids.phase_id ? 'phase' : 'project';
        };

        $scope.goToProjectState = function (tab) {

            if (tab && (tab != $scope.activeTab || tab.name === 'pos')) {
                var level = $scope.getLevel(detailsParams);
                var stateData = {};

                stateData.state = 'index.' + level + (tab.router ? '_' + tab.router : '');
                stateData.params = angular.extend({}, detailsParams, { tab: tab.name });

                verifyChanges(stateData).then(function () {
                    $state.go(stateData.state, stateData.params);
                });
            }
        };

        function ChangeTreeviewNode(projectData) {
            var node = GetNode(projectData || detailsParams);
            if (node) {
                $scope.tree.expandTo($scope.tree.dataItem(node));
                $scope.tree.select(node);
            }
        }

        $scope.$watch(function () { return !$scope.add && $scope.treeData && $scope.treeData.data().length > 0 }, function (newValue) {
            if (newValue && detailsParams) {
                ChangeTreeviewNode();
            }
        });

        $scope.$watch(function () { return ($scope.add || (params.tab != 'details' && params.project_id)) && $scope.tree && $scope.tree.items().length > 0 ? params : null }, function (newValue) {
            if (newValue) {
                ProgressService.blockScreen();
                delete params.serverController;
                $scope.getAddBreadcrumb(params).then(function (path) { $scope.path = path; ProgressService.unblockScreen(); });
            }
        });

        $scope.tabs = [
            { label: "Details", name: "details" },
            { label: "Assets", router: "assets", name: "assets" },
            { label: "Purchase Orders", router: "pos", name: "purchaseOrders" },
            //{ label: "Dashboard", router: "dashboard", name: "dashboard" },
            { label: "Reports", router: "reports", name: "reports" },
            { label: "Documents", router: "documents", name: "documents" },
            { label: "Connectivity", router: "connectivity", name: "connectivity" },
            { label: "Documents", router: "documents", name: "roomDocuments" }];

        var detailsParams; // project_id of the current project
        $scope.path = {};

        /* Filter projects treeview options */
        $scope.mineButton = {
            icon: "edit",
            tooltip: "Edit mine"
        }

        $scope.filterOptions = {
            showProject: "a"
        };
        /* END - Filter projects treeview options */

        // Having the treeview item returns the state and parameters to get the object
        function GetStateParams(dataItem) {

            var state = "";
            var params = {
                tab: $scope.activeTab,
                domain_id: AuthService.getLoggedDomain()
            };

            switch (dataItem.level()) {
                case 0:
                    state = 'index.project';
                    params.project_id = dataItem.id;
                    params.controller = "Projects";
                    break;
                case 1:
                    state = 'index.phase';
                    params.project_id = dataItem.parentNode().id;
                    params.phase_id = dataItem.id;
                    params.controller = "Phases";
                    break;
                case 2:
                    state = 'index.department';
                    params.project_id = dataItem.parentNode().parentNode().id;
                    params.phase_id = dataItem.parentNode().id;
                    params.department_id = dataItem.id;
                    params.controller = "Departments";
                    break;
                case 3:
                    state = 'index.room';
                    params.project_id = dataItem.parentNode().parentNode().parentNode().id;
                    params.phase_id = dataItem.parentNode().parentNode().id;
                    params.department_id = dataItem.parentNode().id;
                    params.room_id = dataItem.id;
                    params.controller = "Rooms";
                    break;
            }

            return { state: state + ($scope.activeTab === 'assets' ? '_assets' : $scope.activeTab === 'purchaseOrders' ? '_pos' : $scope.activeTab === 'dashboard' ? '_dashboard' : $scope.activeTab === 'reports' ? '_reports' : $scope.activeTab === 'documents' && dataItem.level() == 0 ? '_documents' : $scope.activeTab === 'connectivity' ? '_connectivity' : $scope.activeTab === 'roomDocuments' && dataItem.level() == 3 ? '_documents' : ''), params: params };
        };

        // Verify if the current item has been modified and then ask user to save modifications before leaving the page
        function verifyChanges(params) {
            return $q(function (resolve, reject) {
                if (hasChanges) {
                    DialogService.SaveChangesModal().then(function (answer) {
                        hasChanges = false;
                        if (answer) {
                            $scope.$broadcast('saveData', params);
                            reject();
                        } else {
                            resolve();
                        }
                    }, function () {
                        reject();
                    });
                } else {
                    resolve();
                }
            });
        };

        $scope.$on('ChangeTreeviewNode', function (event, projectData) {
            if (projectData)
                ChangeTreeviewNode(projectData);
        });

        $scope.$on('dataSaved', function (event, stateData) {
            hasChanges = false;
            if (stateData)
                $state.go(stateData.state, stateData.params);
        });

        $scope.$on('removeFromTreeView',
            function (event, dataParams) {
                removeItemTreeview(dataParams);

                if (dataParams && detailsParams && dataParams.project_id === detailsParams.project_id) {
                    if (dataParams.phase_id) {
                        if (dataParams.phase_id === detailsParams.phase_id) {
                            if (dataParams.department_id) {
                                if (dataParams.department_id === detailsParams.department_id) {
                                    if ((dataParams.room_id && dataParams.room_id === detailsParams.room_id) || !dataParams.room_id) {
                                        $scope.goToUpLevel(dataParams, true);
                                    }
                                }
                            } else {
                                $scope.goToUpLevel(dataParams, true);
                            }
                        }
                    } else {
                        $scope.goToUpLevel(dataParams, true);
                    }
                }
            });

        // Change the details page to be shown
        $scope.changeDetailsItem = function (dataItem) {

            var stateData = GetStateParams(dataItem);

            // Verify if is not the same item
            if (!detailsParams || stateData.params.project_id != detailsParams.project_id || stateData.params.phase_id != detailsParams.phase_id
                || stateData.params.department_id != detailsParams.department_id || stateData.params.room_id != detailsParams.room_id) {

                if (detailsParams) {
                    verifyChanges(stateData).then(function () {
                        ProgressService.blockScreen();
                        $state.go(stateData.state, stateData.params);
                    });
                } else {
                    ProgressService.blockScreen();
                    $state.go(stateData.state, stateData.params);
                }
            }
        };

        $scope.changeDetailsItemBreadcrumb = function (nivel, path) {

            verifyChanges({ state: 'index.' + nivel, params: path }).then(function () {
                $state.go('index.' + nivel, path);
            });
        };

        // get the description of the object
        function GetDescription(item) {
            return item.project_description ? item.project_description : item.description ? item.description : (item.drawing_room_name + (item.drawing_room_number == null || item.drawing_room_number == '' ? '' : ' - ' + item.drawing_room_number) + (item.room_quantity == 1 ? '' : ' (' + item.room_quantity + ')'));
        };

        // Get a node in the treeview by its id
        function GetNode(item, parent) {
            var nodeData = { items: $scope.treeviewAllData };
            function setNodeData(id) {
                if (!nodeData || !nodeData.items)
                    return;
                var newNodeData = nodeData.items.find(function (dataItem) { return dataItem.id == id; });
                if (!nodeData) {
                    console.error('Error to find item by id ' + id);
                }
                else if (typeof (nodeData.items) === 'undefined') {
                    nodeData.items = [];
                }
                nodeData = newNodeData;
            }

            if ($scope.treeData && item.project_id) {
                var treeview = $scope.treeData;

                treeview = treeview.get(item.project_id);
                setNodeData(item.project_id);

                if (item.phase_id && (item.department_id || !parent)) { // is a phase

                    treeview.load();
                    treeview = treeview.children;
                    treeview = treeview.get(item.phase_id);
                    setNodeData(item.phase_id);

                    if (item.department_id && (item.room_id || !parent)) { // is a department

                        treeview.load();
                        treeview = treeview.children;
                        treeview = treeview.get(item.department_id);
                        setNodeData(item.department_id);

                        if (item.room_id && !parent) { // is a room

                            treeview.load();
                            treeview = treeview.children;
                            treeview = treeview.get(item.room_id);
                            setNodeData(item.room_id);
                        }
                    }
                }
                var ret = treeview ? $scope.tree.findByUid(treeview.uid) : treeview;
                if (ret)
                    ret.nodeData = nodeData;
                return ret;
            }
        }


        $scope.getAddBreadcrumb = function (parent_ids) {
            return $q(function (resolve, reject) {
                //update progressbar
                updateProgressBar(parent_ids);

                if (parent_ids && parent_ids.project_id) {
                    var node = GetNode(parent_ids);

                    if (node) {

                        if (parent_ids.department_id) {
                            parent_ids.department_name = $scope.tree.text(node);
                            parent_ids.room_id = -1;
                            node = node.parent().parent();
                        }

                        if (parent_ids.phase_id) {
                            parent_ids.phase_name = $scope.tree.text(node);
                            parent_ids.department_id = parent_ids.department_id || -1;
                            node = node.parent().parent();
                        }

                        parent_ids.project_name = $scope.tree.text(node);
                        parent_ids.phase_id = parent_ids.phase_id || -1;
                    }

                    resolve(parent_ids);
                } else {
                    parent_ids.project_id = -1;
                    resolve(parent_ids);
                }

            });
        }

        // Search the elements in the treeview
        $scope.search = function () {
            $scope.searchTreeLowerCase = $scope.searchTree.toLowerCase();
            SetTreeViewDataSource(true);
        }

        function SeparateProjectItems() {
            $scope.treeviewActiveData = $scope.treeviewAllData.filter(function (project) {
                return (project.status == 'A' || project.status == 'a');
            });
            $scope.treeviewFavoritiesData = $scope.treeviewAllData.filter(function (project) {
                return project.mine;
            });
        }

        // Function which gets the projects information to treeview from server
        function GetProjectsFromServer() {
            return $q(function (resolve, reject) {
                WebApiService.treeview_projects.query({ action: 'All', domain_id: AuthService.getLoggedDomain() }, function (data) {
                    $scope.filterOptions.show = true;
                    $scope.treeviewAllData = data;

                    AuthService.setProjectStatus(data);
                    
                    SeparateProjectItems();
                    resolve(data);
                }, function () {
                    reject();
                });
            })
        }

        function FilterData(data)
        {
            // Filter is empty
            if ($scope.searchTreeLowerCase.length < 1 || !data)
            {
                return data;
            }

            var filteredItems = [];
            for (var i = 0; i < data.length; i++) {
                var item = data[i];
                var visibleChildren = [];
                var text = item.text.toLowerCase();
                var itemVisible = text.indexOf($scope.searchTreeLowerCase) >= 0;
                // If I am not visible, then I check if any of my child is visible.
                // If none of my children are visible, then I am not visible
                if (!itemVisible)
                {
                    visibleChildren = FilterData(item.items);
                    if (visibleChildren && visibleChildren.length > 0)
                    {
                        var filteredData = Object.assign({}, data[i]);
                        filteredData.items = visibleChildren;
                        filteredItems.push(filteredData);
                    }
                }
                else
                {
                    filteredItems.push(data[i]);
                }
            }
            return filteredItems;
        }

        function GetSelectedSource() {
            var data = [];
            switch ($scope.filterOptions.showProject) {
                case 'a':
                    data = $scope.treeviewAllData;
                    break;
                case 'b':
                    data = $scope.treeviewActiveData;
                    break;
                case 'm':
                    data = $scope.treeviewFavoritiesData;
                    break;
                case 'c':
                    if (detailsParams) {
                        data = [GetFromProperty($scope.treeviewAllData, 'id', detailsParams.project_id)];
                    }
                    break;
                default:
                    break;
            }
            return  FilterData(data);
        }


        // Functions which sets the treeview's datasource
        function SetTreeViewDataSource(hasChildren) {
            // TODO(JLT): Ask why we need the hasChildren, no one calls it with value false
            var newData = GetSelectedSource();
            var model = hasChildren ? { id: "id", children: "items" } : { id: "id" };
            $scope.treeData = new kendo.data.HierarchicalDataSource({
                data: newData,
                schema: {
                    model: model
                }
            });
        };

        $scope.reloadTreeview = function () {
            ProgressService.blockScreen();
            GetProjectsFromServer().then(function (data) {
                SetTreeViewDataSource(true);
                ProgressService.unblockScreen();
            }, function () {
                toastr.error('Error to retrieve treeview data, please contact technical support');
                ProgressService.unblockScreen();
                AuthService.clearLocalStorage();
                $state.go('login');
            });
        }

        $scope.reloadTreeview();
        /* END - Sets the treeview configuration */

        // Remove the item of treeview
        function removeItemTreeview(item) {
            $scope.tree.remove(GetNode(item));
        }

        /* Configure the mine projects */
        function GetFromProperty(array, property, value) {
            return array.find(function (i) { return i[property] == value });
        }

        $scope.ChangeProjectsSource = function () {
            $scope.treeData.data(GetSelectedSource());
            if (detailsParams) {
                var node = GetNode(detailsParams);
                $scope.tree.expandTo($scope.tree.dataItem(node));
                $scope.tree.select(node);
            }
        };

        $scope.addToMine = function (item) {
            if (GetFromProperty($scope.treeviewFavoritiesData, 'id', item.id))
                return;
            WebApiService.project_mine.save({ project_id: item.id || item.project_id, domain_id: item.domain_id }, null,
                function () {
                    item.id = item.id || item.project_id;
                    item = $scope.treeviewAllData.find(function (i) { return i.id == item.id });
                    $scope.treeviewFavoritiesData.push(item);
                    toastr.success('Project added to favorites');
                    $scope.$broadcast('AddRemoveFavorites', item, true);
                    if ($scope.filterOptions.showProject == 'm') {
                        // Forces a refresh to include project into mine
                        ChangeProjectSource();
                    }
                }, function (data) {
                    toastr.error('Error to add project to mine, please contact technical support');
                });
        }

        $scope.removeFromMine = function (item) {
            if (!$scope.treeviewFavoritiesData || $scope.treeviewFavoritiesData.length <= 0)
                return;
            WebApiService.project_mine.remove({ project_id: item.id || item.project_id, domain_id: item.domain_id },
                function () {
                    var index = $scope.treeviewFavoritiesData.indexOf(GetFromProperty($scope.treeviewFavoritiesData, 'id', item.id));
                    $scope.treeviewFavoritiesData.splice(index, 1);
                    if ($scope.filterOptions.showProject == 'm') {
                        item.project_id = item.id;
                        ChangeProjectsSource();
                    }
                    $scope.$broadcast('AddRemoveFavorites', item, false);
                }, function () {
                    toastr.error('Error to remove the project from mine, please contact technical support');
                });
        }
        /* END - Configure the mine projects */

        // Add a new item in the tree
        $scope.$on('addItemTreeView', function (e, item) {
            if (item != undefined)
                $scope.addItemTreeView(item);
        });

        $scope.addItemTreeView = function (item) {
            console.log('treeview updating');
            var text = item.text ? item.text : GetDescription(item);
            var id = item.id ? item.id : (item.room_id ? item.room_id : item.department_id ? item.department_id : item.phase_id ? item.phase_id : item.project_id);
            //used for project level
            if (!item.phase_id) {
                text = getProjectDescription(item);
            }
            var itemAdd = item.phase_id ? { id: id, text: text } : { id: id, domain_id: item.domain_id, text: text, items: item.items };

            // Add item to treeview
            var parent = item.phase_id ? GetNode(item, true) : null;
            $scope.tree.append(itemAdd, parent);
            $scope.tree.select(GetNode(item));
            if (parent && parent.nodeData && parent.nodeData.items)
                parent.nodeData.items.push(itemAdd);
            else
                $scope.treeviewAllData.push(itemAdd);
        }

        // Update a existing item in the treeview
        $scope.updateItemTreeView = function (item) {
            var node = GetNode(item);
            var data_description = GetDescription(item);

            //used for project level
            if (!item.phase_id) {
                data_description = getProjectDescription(item);
            }
            $scope.tree.text(node, data_description);
            // If the project status has changed, we have to update the 
            // Active projects, so we reload the project items
            if (typeof (node.nodeData) != "undefined" && typeof (node.nodeData.status) != "undefined" &&
                node.nodeData.status != item.status) {
                node.nodeData.status = item.status;
                SeparateProjectItems();
            }
        };

        function getProjectDescription(item) {
            var description = "";
            var statusList = StatusListProject;
            var status = findStatus(item.status); //statusList.find(x => x.id == item.status);
            if (item.client_project_number) {
                description = item.client_project_number + " - ";
            }
            description = description + item.project_description + " (" + (status);
            if (item.locked_comment) {
                description = description + " - " + item.locked_comment;
            }
            description = description + ")";

            return description;
        }

        function findStatus(status) {
            var statusList = StatusListProject;
            for (var i = 0; i < statusList.length; i++) {
                if (statusList[i].id == status) {
                    return statusList[i].name;
                }
            }
            return "No Status";
        }

        /* float button */
        $scope.chosen = FabService.chosen;
        /* end float button */

        // Sets the state to add a new element one level below the item
        $scope.AddState = function (dataItem) {
            if (AuthService.getLoggedUserType() == "3") {
                //DialogService.ViewersChangesModal();
                return '';
            }
            return dataItem.level() == 0 ? 'index.phase_add({project_id: ' + dataItem.id + '})'
                : dataItem.level() == 1 ? 'index.department_add({project_id: ' + dataItem.parentNode().id + ', phase_id: ' + dataItem.id + '})'
                    : dataItem.level() == 2 ? 'index.department({project_id: ' + dataItem.parentNode().parentNode().id + ', phase_id: ' + dataItem.parentNode().id + ', department_id: ' + dataItem.id + '})'
                        : '';
        }

        $scope.AddStateProject = function () {
            if (AuthService.getLoggedUserType() == "3" || AuthService.getLoggedUserType() == "2") {
                return '';
            }
            return 'index.project_add';
        }



        $scope.checkIfRoom = function (dataItem) {
            if (AuthService.getLoggedUserType() == "3") {
                DialogService.ViewersChangesModal();
                return;
            }

            if (dataItem.level() == 2) {
                addRoom(dataItem.parentNode().parentNode().id, dataItem.parentNode().id, dataItem.id);
            }
        }

        $scope.checkIfViewer = function () {
            if (AuthService.getLoggedUserType() == "3" || AuthService.getLoggedUserType() == "2") {
                DialogService.ViewersChangesModal();
            }
        }

        function addRoom(project_id, phase_id, department_id) {
            var tree = { project_id: project_id.toString(), phase_id: phase_id.toString(), department_id: department_id.toString() };

            $mdDialog.show({
                controller: 'AddRoomCtrl',
                templateUrl: 'app/Projects/Details/Modals/AddRoom.html',
                fullscreen: true,
                clickOutsideToClose: true,
                locals: { local: { params: tree } }
            }).then(function (item) {
                //$scope.reloadTreeview();
                var rooms = item.text.split('||');
                for (var i = 0; i < rooms.length; i++) {
                    var data = JSON.parse(rooms[i]);
                    $scope.addItemTreeView(data);
                }
                $state.go('index.department', tree, { reload: true });
            });
        }


        /* Shows the details of the parent item, if there is a parent item*/
        $scope.goToUpLevel = function (params, notAdd) {

            var state;
            if (notAdd) {
                state = params.room_id ? 'index.department' : params.department_id ? 'index.phase' : params.phase_id ? 'index.project' : 'index';
            } else {
                state = params.department_id ? 'index.department' : params.phase_id ? 'index.phase' : params.project_id ? 'index.project' : 'index';
            }

            $state.go(state, { project_id: params.project_id, phase_id: params.phase_id, department_id: params.department_id }, { reload: state === 'index' });
        }

        // Listen when the project_id change
        $scope.$on('detailsParams', function (event, data) {
            detailsParams = data;
            params = angular.copy(data);
            $scope.add = data.add;
            $scope.activeTab = (data.tab === 'documents' && data.phase_id) || (data.tab === 'roomDocuments' && !data.room_id) ?
                'details' : (data.tab || 'details');
            $scope.showTabs = data.add || data.project_id
            ChangeTreeviewNode();
        });
    }]);
