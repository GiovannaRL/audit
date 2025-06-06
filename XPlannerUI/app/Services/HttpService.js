xPlanner.service('HttpService', ['$location', function ($location) {

    var host = $location.protocol() + '://' + $location.host() + ($location.port() ? ':' + $location.port() : '') + "/xPlannerAPI/api/";

    // treeview_projects
    this.treeview_projects = function (action, domain_id, project_id, phase_id, department_id, room_id) {
        return host + 'treeviews/' + action + '/' + domain_id + (project_id ? '/' + project_id +
            (phase_id ? '/' + phase_id +
                (department_id ? '/' + department_id +
                    (room_id ? '/' + room_id : '')
                : '')
            : '')
        : '');
    };

    // project_locations
    this.project_locations = function (controller, action, domain_id, project_id, phase_id, department_id, room_id) {
        return host + controller + '/' + action + '/' + domain_id +
            (project_id ? '/' + project_id +
                (phase_id ? '/' + phase_id +
                    (department_id ? '/' + department_id +
                        (room_id ? '/' + room_id : '')
                        : '')
                    : '')
                : '');
    };

    //equipment_inventory
    this.asset_inventory_consolidated = function (domain_id, project_id, phase_id, department_id, room_id, filterPoQty, showOnlyApprovedAssets, consolidated) {
        return host + 'assetsInventoryConsolidated/All/' + domain_id + '/' + project_id + '/' + phase_id +
            '/' + department_id + '/' + room_id + '?filterPoQty=' + filterPoQty + '&showOnlyApprovedAssets=' + showOnlyApprovedAssets + '&groupBy=' + consolidated;
    };

    this.asset_inventory = function (domain_id, project_id, phase_id, department_id, room_id) {
        return host + 'assetsInventory/AllInventories/' + domain_id + '/' + project_id + '/' + phase_id +
            '/' + department_id + '/' + room_id;
    };

    this.asset_inventory_filter_relocate = function (domain_id, project_id, budget_qty) {
        return host + 'assetsInventory/AllFilterRelocate/' + domain_id + '/' + project_id + '/' + budget_qty;
    };

    this.asset_inventory_doc_link = function (domain_id, project_id, phase_id, department_id, room_id) {
        return host + 'assetsInventory/DocToLink/' + domain_id + '/' + project_id + '/' + phase_id +
            '/' + department_id + '/' + room_id;
    };

    this.asset_inventory_available_for_po = function (domain_id, project_id, phase_id, department_id, room_id, show_approved) {
        return host + 'assetsInventory/AllInventoriesAvailableForPO/' + domain_id + '/' + project_id + '/' + phase_id +
            '/' + department_id + '/' + room_id + '?showOnlyApprovedAssets=' + show_approved;
    };

    // assets
    this.assets = function (action, domain_id) {
        return host + 'assets/' + action + '/' + domain_id;
    };

    // search_room_asset
    this.search_room_asset = function (domain_id, project_id, equipment_ids, phase_id, department_id, room_id) {
        return host + 'assetrooms/get/' + domain_id + '/' + project_id + '/' +
            phase_id + '/' + department_id + '/' + room_id + '/' + equipment_ids;
    };

    // assign_assets_to_po
    this.available_po_assets = function (domain_id, project_id, phase_id, department_id, room_id, show_unapproved) {
        return host + 'InventoryPurchaseOrder/ToAssign/' + domain_id + '/' + project_id + '/' +
            phase_id + '/' + department_id + '/' + room_id + '/' + show_unapproved;
    };

    //pos
    /*this.pos = function (domain_id, project_id, phase_id, department_id, room_id) {
        return host + 'purchaseorders/domain/' + domain_id + '/project/' + project_id + '/phase/' + phase_id +
            '/department/' + department_id + '/room/' + room_id;
    };*/

    // project addresses
    this.project_addresses = function (action, domain_id, project_id, id) {
        return host + 'address/' + action + '/' + domain_id + '/' + project_id + '/' + id;
    };

    // phase documents
    this.phase_documents = function (action, id1, id2, id3, id4, id5) {
        return host + 'phasedocuments/' + action + (id1 ? '/' + id1 + (id2 ?
            ('/' + id2 + (id3 ?
                ('/' + id3 +
                    (id4 ? '/' + id4 +
                        (id5 ? '/' + id5 : '')
                    : '')
                ) : '')
            ) : '')
            : '');
    };

    // file stream - upload
    this.file_stream = function (action, domain_id, asset_id, filename, column_name, container) {
        return host + 'filestream/' + action + '/' + domain_id + '/' + asset_id + '/' + filename + '/' + column_name + '/' + container;
    };

    // file stream - upload - power bi
    this.file_stream_powerbi = function (action, domain_id, project_id, filename) {
        return host + 'dashboard/' + action + '/' + domain_id + '/' + project_id + '/' + filename;
    };

    // it connectivity
    this.it_connectivity = function (action, domain_id, project_id, phase_id, department_id, room_id) {
        return host + 'itconnectivity/' + action + '/' + domain_id + '/' + project_id + '/' + phase_id + '/' + department_id + '/' + room_id;
    };

    //// it connectivity - connected assets
    //this.connected_assets = function (action, domain_id, project_id, phase_id, department_id, room_id) {
    //    return host + 'connectedassetsitconnectivity/' + action + '/' + domain_id + '/' + project_id + '/' + phase_id + '/' + department_id + '/' + room_id;
    //};

    // cost center
    this.cost_center = function (action, domain_id, project_id, id) {
        return host + 'costCenters/' + action + '/' + domain_id + '/' + project_id + '/' + id;
    };

    // asset options
    this.asset_options = function (action, domain_id, asset_id) {
        return host + 'assetoptions/' + action + '/' + domain_id + '/' + asset_id;
    };

    

    // Generic
    this.generic = function (controller, action, id1, id2, id3, id4, id5) {
        var url = host + controller + '/' + action;
        var ids = [id1, id2, id3, id4, id5];
        ids.forEach(function (id)
        {
            if (typeof id !== "undefined")
            {
                url += "/" + id;
            }
        });
        return url;
    };

    this.reportDownload = function (project_domain_id, project_id, id, type, timestamp) {
        return host + 'Report/download/' + project_domain_id + '/' + project_id + '/' + id + '?extension=' + type + '&timestamp=' + timestamp;
    }

}]);
