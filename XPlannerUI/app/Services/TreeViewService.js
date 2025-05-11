xPlanner.factory('TreeViewService', ['localStorageService', function (localStorageService) {

    function _removeItem(tree, item) {
        tree.remove(node);
    };

    function _search(tree, value) {

        if (value !== "") {
            tree.dataSource.filter({
                field: "text",
                operator: "contains",
                value: value
            });
        } else {
            tree.dataSource.filter({});
        }
    };

    function _setTreeViewDataSource(treeData, data, model) {

        treeData = new kendo.data.HierarchicalDataSource({
            data: data,
            schema: {
                model: model
            }
        });
    };

    function _getLocalStorage(name) {
        if (localStorageService.isSupported) {
            return localStorageService.get(name);
        } else if (localStorageService.cookie.isSupported) {
            return localStorageService.cookie.get(name);
        }
    };

    function _setLocalStorage(name, items) {
        if (localStorageService.isSupported) {
            localStorageService.set(name, items);
        } else if (localStorageService.cookie.isSupported) {
            localStorageService.cookie.set(name, items);
        }
    };

    function _addItemTreeview(tree, item, node, localStorage) {
        tree.append({ id: item.id, domain_id: item.domain_id, text: item.text }, node);

        // Update at local storage
        var items = _getLocalStorage(localStorage);
        items.push(item);
        _setLocalStorage(items);
    };

    function _updateItemTreeView(tree, item, node) {
        tree.text(node, item.description);
    };

    function _getTreeData(tree) {
        return tree.dataSource.data();
    };

    /*function _getTableData(tree) {
        var items = tree.dataSource.data();

        var departmentData = [];
        var phaseData = [];

        var phases, departments;

        angular.forEach(items, function (project) {
            project.load();
            phases = project.children.data();
            angular.forEach(phases, function (phase) {
                phaseData.push({ phase_desc: phase.text, phase_id: phase.id, project_desc: project.text, project_id: project.id});
                phase.load();
                departments = phase.children.data();
                angular.forEach(departments, function (department) {
                    departmentData.push({
                        department_desc: department.text, department_id: department.id, phase_desc: phase.text, phase_id: phase.id,
                        project_desc: project.text, project_id: project.id
                    });
                });
            });
        });

        localStorageService.set("AllDepartmentsGrid", departmentData);
        localStorageService.set("AllPhasesGrid", phaseData);
        return {departments: departmentData, phases: phaseData};
    };*/

    return {
        RemoveItem: _removeItem,
        Search: _search,
        SetTreeViewDataSource: _setTreeViewDataSource,
        AddItemTreeview: _addItemTreeview,
        UpdateItemTreeView: _updateItemTreeView,
        GetTreeData: _getTreeData
    }
}]);