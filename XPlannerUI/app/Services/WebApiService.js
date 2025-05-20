xPlanner.factory('WebApiService', ['$resource', '$location',
    function ($resource, $location) {

        var host = $location.protocol() + '://' + $location.host() + ($location.port() ? ':' + $location.port() : '') + '/xPlannerAPI/api/';

        return {
            treeview_projects: $resource(host + 'treeviews/:action/:domain_id'),
            client: $resource(host + 'clients/:client_id'),
            facility: $resource(host + 'facilities/:facility_id'),
            genericController: $resource(host + ':controller/:action/:domain_id/:project_id/:phase_id/:department_id/:room_id',
                null, {
                    'update': { method: 'PUT', timeout: 12345678 },
                    'remove_with_data': { method: 'POST' },
                    'patch': { method: 'PATCH', hasBody: true }
                }),
            detailsController: $resource(host + ':controller/Item/:domain_id/:project_id/:phase_id/:department_id/:room_id',
                null, {
                    'update': { method: 'PUT' }
                }),
            project_values: $resource(host + 'projectvalues/:domain_id/:project_id/:phase_id/:department_id/:room_id'),
            department_type: $resource(host + '/xPlannerAPI/api/departmentType/all/:domain_id/:department_type_id'),
            phase: $resource(host + 'phases/:phase_id/project/:project_id', null, {
                'update': { method: 'PUT' }
            }),
            project: $resource(host + 'projects/:project_id', null, {
                'update': { method: 'PUT' }
            }),
            department: $resource(host + 'departments/:department_id/project/:project_id/phase/:phase_id', null, {
                'update': { method: 'PUT' }
            }),
            room: $resource(host + 'rooms/:room_id/project/:project_id/phase/:phase_id/department/:department_id', null, {
                'update': { method: 'PUT' }
            }),
            options: $resource(host + 'options/equipment/:equipment_id/equipment_domain/:domain_id/project/:project_id/phase/:phase_id/department/:department_id/room/:room_id'),
            options_save: $resource(host + 'options/equipment/:equipment_id/equipment_domain/:domain_id/project/:project_id', null, {
                'update': { method: 'PUT' }
            }),
            colors: $resource(host + 'options/equipment/:equipment_id/equipment_domain/:domain_id/project/:project_id/phase/:phase_id/department/:department_id/room/:room_id'),
            domains: $resource(host + 'domains/:action/:domain_id'),
            asset_inventory_consolidated: $resource(host + 'assetsInventoryConsolidated/:action/:domain_id/:project_id/:phase_id/:department_id/:room_id/:asset_id'),
            asset_inventory_consolidated_save: $resource(host + 'assetsInventoryConsolidated/Item/:domain_id/:project_id/:cost_field'),
            replace_inventory: $resource(host + 'replaceInventory/Items/:domain_id/:project_id', null, {
                'update': { method: 'PUT' }
            }),
            project_mine: $resource(host + 'projectsMine/Item/:domain_id/:project_id'),
            project_address: $resource(host + 'address/:action/:domain_id/:project_id/:id', null, {
                'update': { method: 'PUT' }
            }),
            phase_documents: $resource(host + 'phasedocuments/:action/:domain_id/:project_id/:phase_id/:drawing_id', null, {
                'update': { method: 'PUT' }
            }),
            cost_center: $resource(host + 'costCenters/:action/:domain_id/:project_id/:id', null, {
                'update': { method: 'PUT' }
            }),
            asset_option: $resource(host + 'assetoptions/:action/:domain_id/:asset_id/:asset_option_id',
                null, {
                    'update': { method: 'PUT' },
                    'remove_with_data': { method: 'POST' }
                }),
            option: $resource(host + 'assetoptions/:action/:domain_id/:asset_option_id',
               null, {
                   'update': { method: 'PUT' }
            }),
            copy_from: $resource(host + 'Rooms/CopyRoom/:domain_id/:project_id/:copy/:options',
                {
                    'save': { method: 'POST' }
                }),
            asset_inventory: $resource(host + 'assetsInventory/:action/:domain_id/:project_id/:phase_id/:department_id/:room_id', null, {
                'update': { method: 'PUT' }
            }),
            template_room: $resource(host + 'templateroom/:action/:domain_id/:project_id/:phase_id/:department_id/:room_id', null, {
                'update': { method: 'PUT' }
            }),
            template_room_filtered: $resource(host + 'templateroom/:action/:domain_id/:project_id/:template_type'),
            file_stream: $resource(host + 'filestream/file/:domain_id/:filename/:container/:drawing_id'),
            dashboard: $resource(host + 'dashboard/:action/:id1'),
            split_rooms: $resource(host + 'rooms/splitRoom/:domain_id/:project_id/:phase_id/:department_id/:room_id/:is_linked_template/:template_name'),
            add_multi_rooms: $resource(host + 'rooms/AddMultiRoom/:domain_id/:project_id/:phase_id/:department_id/:is_linked_template/:template_id'),
            user_data: $resource(host + 'aspnetuserroles/:action/:domain_id/:user_name'),
            add_asset_from_project: $resource(host + 'AssetsInventory/FromProject/:domain_id/:project_id/:phase_id/:department_id/:room_id/:action', null, {
                'save': { method: 'POST', isArray: true }
            }),
            add_asset_from_project_multiple_rooms: $resource(host + 'AssetsInventory/FromProjectToMultipleLocations/:domain_id/:project_id/:action/:with_budgets', null, {
                'save': { method: 'POST', isArray: true }
            }),
            add_asset_from_project_with_budgets: $resource(host + 'AssetsInventory/FromProjectWithBudgets/:domain_id/:project_id/:phase_id/:department_id/:room_id/:action', null, {
                'save': { method: 'POST', isArray: true }
            }),
            link_asset_from_project: $resource(host + 'AssetsInventory/LinkInventory/:domain_id/:project_id/:target_inventory_id', null, { 'delete': { method: 'POST' } }), // The version we are using of angular does not support hasBody for the actions, so I had to switch to use post
            remove_room_picture: $resource(host + 'rooms/picture/:domain_id/:project_id/:phase_id/:department_id/:room_id/:filename')
        };
    }]);
