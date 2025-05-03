xPlanner.config(['$stateProvider', '$urlRouterProvider', '$mdIconProvider', '$mdThemingProvider', 'OAuthProvider',
    'OAuthTokenProvider', 'localStorageServiceProvider',
    function ($stateProvider, $urlRouterProvider, $mdIconProvider, $mdThemingProvider, OAuthProvider, OAuthTokenProvider,
        localStorageServiceProvider) {

        // localStorage configurations
        localStorageServiceProvider.setPrefix('xPlannerUI');
        localStorageServiceProvider.setStorageType('localStorage');

        // icons
        $mdIconProvider
            .defaultIconSet("images/icons/svg/menu/menu.svg", 24)
            .icon("menu", "images/icons/svg/menu/menu.svg", 36)
            .icon("expand", "images/icons/svg/expand_more.svg", 24)
            .icon("more", "images/icons/svg/add.svg", 24)
            .icon("close", "images/icons/svg/close.svg", 24)
            .icon("delete", "images/icons/svg/delete.svg", 24)
            .icon("save", "images/icons/svg/save.svg", 24)
            .icon("save_black", "images/icons/svg/save_black.svg", 24)
            .icon("logoff", "images/icons/svg/logoff.svg", 24)
            .icon("move", "images/icons/svg/swap.svg", 24)
            .icon("cancel", "images/icons/svg/cancel.svg", 24)
            .icon("searched_black", "images/icons/svg/searched_black.svg", 24)
            .icon("download", "images/icons/svg/download.svg", 24)
            .icon("file", "images/icons/svg/file.svg", 24)
            .icon("file_simple", "images/icons/svg/file_simple.svg", 24)
            .icon("build_black", "images/icons/svg/build_black.svg", 24)
            .icon("content_copy", "images/icons/svg/content_copy.svg", 24)
            .icon("note_add", "images/icons/svg/note_add.svg", 24)
            .icon("assignment", "images/icons/svg/assignment.svg", 24)
            .icon("file_upload", "images/icons/svg/file_upload.svg", 24)
            .icon("local_parking", "images/icons/svg/local_parking.svg", 24)
            .icon("delete_forever", "images/icons/svg/delete_forever.svg", 24)
            .icon("reply", "images/icons/svg/reply.svg", 24)
            .icon("add_circle", "images/icons/svg/add_circle.svg", 24)
            .icon("add_circle_outline", "images/icons/svg/add_circle_outline.svg", 24)
            .icon("receive", "images/icons/svg/receive.svg", 24)
            .icon("request", "images/icons/svg/request.svg", 24)
            .icon("collapse_expand", "images/icons/svg/collapse_expand.svg", 24)
            .icon("excel", "images/icons/svg/excel.svg", 24)
            .icon("pdf", "images/icons/svg/pdf.svg", 24)
            .icon("html", "images/icons/svg/html.svg", 24)
            .icon("lock", "images/icons/svg/lock.svg", 24)
            .icon("link", "images/icons/svg/link.svg", 24)
            .icon("split", "images/icons/svg/split.svg", 24)
            .icon("forward", "images/icons/svg/forward.svg", 24)
            .icon("flag", "images/icons/svg/flag.svg", 24)
            .icon("input_white", "images/icons/svg/input_white.svg", 24)
            .icon("flip_to_front", "images/icons/svg/flip_to_front.svg", 24)
            .icon("copy_from", "images/icons/svg/copy_from.svg", 24)
            .icon("regenerate", "images/icons/svg/regenerate.svg", 24)
            .icon("rotate_right", "images/icons/svg/rotate_right.svg", 24)
            /* Side menu icons*/
            .icon("logout", "images/icons/svg/menu/logout.svg", 24)
            .icon("home", "images/icons/svg/menu/home.svg", 24)
            .icon("projects", "images/icons/svg/menu/projects.svg", 24)
            .icon("assets", "images/icons/svg/menu/assets.svg", 24)
            .icon("room_templates", "images/icons/svg/menu/room_templates.svg", 24)
            .icon("department_types", "images/icons/svg/menu/department_types.svg", 24)
            .icon("admin", "images/icons/svg/menu/admin.svg", 24)
            .icon("contact_us", "images/icons/svg/menu/contact_us.svg", 24)
            .icon("user_guide", "images/icons/svg/menu/user_guide.svg", 24)
            .icon("themes", "images/icons/svg/menu/themes.svg", 24)
            .icon("dashboard", "images/icons/svg/menu/dashboard.svg", 24)
            .icon("user_management", "images/icons/svg/face.svg", 24)
            .icon("enterprise", "images/icons/svg/account_balance.svg", 24)
            .icon("file_upload_gray", "images/icons/svg/file_upload_gray.svg", 24)


        /* END - Side menu icons*/

        // Palette colors
        $mdThemingProvider.theme('default')
            .primaryPalette('red')
            .accentPalette('deep-orange');

        //$mdThemingProvider.theme('default')
        //     .primaryPalette('grey')
        //     .accentPalette('red');

        $mdThemingProvider.theme('indigo')
            .primaryPalette('indigo')
            .accentPalette('deep-orange');

        $mdThemingProvider.theme('green')
            .primaryPalette('green')
            .accentPalette('pink');

        $mdThemingProvider.theme('cyan')
            .primaryPalette('cyan')
            .accentPalette('orange');

        $mdThemingProvider.theme('grey')
            .primaryPalette('grey')
            .accentPalette('red');

        $mdThemingProvider.theme('red')
            .primaryPalette('red')
            .accentPalette('deep-orange');


        $mdThemingProvider.alwaysWatchTheme(true);
        $mdThemingProvider.registerStyles(require('raw-loader!../Content/Themes.css'));


        // Routes configuration
        // TODO: Use the domain_id here instead of 1
        $urlRouterProvider.when('', '/projects');
        $urlRouterProvider.when('/', '/projects');

        $stateProvider
            .state('index', {
                url: "/projects",
                templateUrl: "app/Index/Index.html",
                controller: 'IndexCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.project_add', {
                url: "/add",
                templateUrl: "app/Projects/Details/Project.html",
                //controller: 'DetailsCtrl',
                params: {
                    serverController: 'Projects',
                    add: true
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.project', {
                url: "/:project_id",
                templateUrl: "app/Projects/Details/Project.html",
                //controller: 'DetailsCtrl',
                params: {
                    serverController: 'Projects',
                    tab: 'details'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.project_documents', {
                url: "/:project_id/documents",
                templateUrl: "app/Projects/Documents/ProjectDocumentsTab.html",
                controller: 'ProjectDocumentsTabCtrl',
                params: {
                    serverController: 'Projects',
                    tab: 'documents'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.room_documents', {
                url: "/:project_id/phase/:phase_id/department/:department_id/room/:room_id/documents",
                templateUrl: "app/Projects/Documents/RoomDocumentsTab.html",
                controller: 'RoomDocumentsTabCtrl',
                params: {
                    serverController: 'rooms',
                    tab: 'roomDocuments'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.project_assets', {
                url: "/:project_id/assets",
                templateUrl: "app/Projects/Assets/Main.html",
                controller: 'AssetsMainCtrl',
                params: {
                    serverController: 'Projects',
                    tab: 'assets'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.project_pos', {
                url: "/:project_id/pos",
                templateUrl: "app/POs/PO.html",
                controller: 'POCtrl',
                params: {
                    serverController: 'Projects',
                    tab: 'purchaseOrders'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.project_pos_details', {
                url: "/:project_id/pos/:po_id",
                templateUrl: "app/POs/PODetails.html",
                controller: 'PODetailsCtrl',
                params: {
                    serverController: 'Projects',
                    tab: 'purchaseOrders'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.project_reports', {
                url: "/:project_id/reports",
                templateUrl: "app/Reports/ReportList.html",
                controller: 'ReportListCtrl',
                params: {
                    serverController: 'Projects',
                    tab: 'reports'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.project_dashboard', {
                url: "/:project_id/dashboard",
                templateUrl: "app/Dashboard/Dashboard.html",
                controller: 'DashboardCtrl',
                params: {
                    serverController: 'Projects',
                    tab: 'dashboard'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.project_connectivity', {
                url: "/:project_id/connectivity",
                templateUrl: "app/Projects/Connectivity/ConnectivityTab.html",
                controller: 'ConnectivityTabCtrl',
                params: {
                    serverController: 'Projects',
                    tab: 'connectivity'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.phase_add', {
                url: "/:project_id/phase/add",
                templateUrl: "app/Projects/Details/Phase.html",
                //controller: 'DetailsCtrl',
                params: {
                    serverController: 'Phases',
                    add: true
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.phase', {
                url: "/:project_id/phase/:phase_id",
                templateUrl: "app/Projects/Details/Phase.html",
                //controller: 'DetailsCtrl',
                params: {
                    serverController: 'Phases',
                    tab: 'details'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.phase_dashboard', {
                url: "/:project_id/phase/:phase_id/dashboard",
                templateUrl: "app/Dashboard/Dashboard.html",
                controller: 'DashboardCtrl',
                params: {
                    serverController: 'Phases',
                    tab: 'dashboard'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.phase_assets', {
                url: "/:project_id/phase/:phase_id/assets",
                templateUrl: "app/Projects/Assets/Main.html",
                controller: 'AssetsMainCtrl',
                params: {
                    serverController: 'Phases',
                    tab: 'assets'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.phase_pos', {
                url: "/:project_id/phase/:phase_id/pos",
                templateUrl: "app/POs/PO.html",
                controller: 'POCtrl',
                params: {
                    serverController: 'Phases',
                    tab: 'purchaseOrders'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.phase_pos_details', {
                url: "/:project_id/phase/:phase_id/pos/:po_id",
                templateUrl: "app/POs/PODetails.html",
                controller: 'PODetailsCtrl',
                params: {
                    serverController: 'Phases',
                    tab: 'purchaseOrders'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.phase_reports', {
                url: "/:project_id/phase/:phase_id/reports",
                templateUrl: "app/Reports/ReportList.html",
                controller: 'ReportListCtrl',
                params: {
                    serverController: 'Phases',
                    tab: 'reports'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.phase_connectivity', {
                url: "/:project_id/phase/:phase_id/connectivity",
                templateUrl: "app/Projects/Connectivity/ConnectivityTab.html",
                controller: 'ConnectivityTabCtrl',
                params: {
                    serverController: 'Phases',
                    tab: 'connectivity'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.department_add', {
                url: "/:project_id/phase/:phase_id/department/add",
                templateUrl: "app/Projects/Details/Department.html",
                //controller: 'DetailsCtrl',
                params: {
                    serverController: 'Departments',
                    add: true
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.department', {
                url: "/:project_id/phase/:phase_id/department/:department_id",
                templateUrl: "app/Projects/Details/Department.html",
                //controller: 'DetailsCtrl',
                params: {
                    serverController: 'Departments',
                    tab: 'details'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.department_dashboard', {
                url: "/:project_id/phase/:phase_id/department/:department_id/dashboard",
                templateUrl: "app/Dashboard/Dashboard.html",
                controller: 'DashboardCtrl',
                params: {
                    serverController: 'Departments',
                    tab: 'dashboard'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.department_assets', {
                url: "/:project_id/phase/:phase_id/department/:department_id/assets",
                templateUrl: "app/Projects/Assets/Main.html",
                controller: 'AssetsMainCtrl',
                params: {
                    serverController: 'Departments',
                    tab: 'assets'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.department_pos', {
                url: "/:project_id/phase/:phase_id/department/:department_id/pos",
                templateUrl: "app/POs/PO.html",
                controller: 'POCtrl',
                params: {
                    serverController: 'Departments',
                    tab: 'purchaseOrders'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.department_pos_details', {
                url: "/:project_id/phase/:phase_id/department/:department_id/pos/:po_id",
                templateUrl: "app/POs/PODetails.html",
                controller: 'PODetailsCtrl',
                params: {
                    serverController: 'Departments',
                    tab: 'purchaseOrders'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.department_reports', {
                url: "/:project_id/phase/:phase_id/department/:department_id/reports",
                templateUrl: "app/Reports/ReportList.html",
                controller: 'ReportListCtrl',
                params: {
                    serverController: 'Departments',
                    tab: 'reports'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.department_connectivity', {
                url: "/:project_id/phase/:phase_id/department/:department_id/connectivity",
                templateUrl: "app/Projects/Connectivity/ConnectivityTab.html",
                controller: 'ConnectivityTabCtrl',
                params: {
                    serverController: 'Departments',
                    tab: 'connectivity'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.room_add', {
                url: "/:project_id/phase/:phase_id/department/:department_id/room/add",
                templateUrl: "app/Projects/Details/Room.html",
                //controller: 'DetailsCtrl',
                params: {
                    serverController: 'Rooms',
                    add: true
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.room', {
                url: "/:project_id/phase/:phase_id/department/:department_id/room/:room_id",
                templateUrl: "app/Projects/Details/Room.html",
                //controller: 'DetailsCtrl',
                params: {
                    serverController: 'Rooms',
                    tab: 'details'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.room_dashboard', {
                url: "/:project_id/phase/:phase_id/department/:department_id/room/:room_id/dashboard",
                templateUrl: "app/Dashboard/Dashboard.html",
                controller: 'DashboardCtrl',
                params: {
                    serverController: 'Rooms',
                    tab: 'dashboard'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.room_assets', {
                url: "/:project_id/phase/:phase_id/department/:department_id/room/:room_id/assets",
                templateUrl: "app/Projects/Assets/Main.html",
                controller: 'AssetsMainCtrl',
                params: {
                    serverController: 'Rooms',
                    tab: 'assets'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.room_pos', {
                url: "/:project_id/phase/:phase_id/department/:department_id/room/:room_id/pos",
                templateUrl: "app/POs/PO.html",
                controller: 'POCtrl',
                params: {
                    serverController: 'Rooms',
                    tab: 'purchaseOrders'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.room_pos_details', {
                url: "/:project_id/phase/:phase_id/department/:department_id/room/:room_id/pos/:po_id",
                templateUrl: "app/POs/PODetails.html",
                controller: 'PODetailsCtrl',
                params: {
                    serverController: 'Rooms',
                    tab: 'purchaseOrders'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.room_reports', {
                url: "/:project_id/phase/:phase_id/department/:department_id/room/:room_id/reports",
                templateUrl: "app/Reports/ReportList.html",
                controller: 'ReportListCtrl',
                params: {
                    serverController: 'Rooms',
                    tab: 'reports'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('index.room_connectivity', {
                url: "/:project_id/phase/:phase_id/department/:department_id/room/:room_id/connectivity",
                templateUrl: "app/Projects/Connectivity/ConnectivityTab.html",
                controller: 'ConnectivityTabCtrl',
                params: {
                    serverController: 'Rooms',
                    tab: 'connectivity'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('login', {
                url: "/login",
                templateUrl: "app/Security/Login/Login.html",
                controller: 'LoginCtrl',
            }).state('reset_password', {
                url: "/resetPassword/:email/:token",
                templateUrl: "app/Security/Reset Password/ResetPassword.html",
                controller: 'ResetPasswordCtrl'
            }).state('subscription', {
                url: "/subscription",
                templateUrl: "app/Subscription/Subscription.html",
                controller: 'SubscriptionCtrl'
            }).state('notice', {
                url: "/notice",
                templateUrl: "app/Subscription/Notice.html",
                controller:''
            }).state('domains', {
                url: "/domains",
                templateUrl: "app/Domains/DomainsGrid.html",
                controller: 'DomainsGridCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isAdminUser();
                    }]
                }
            }).state('users', {
                url: "/users",
                templateUrl: "app/Users/UsersList.html",
                controller: 'UsersListCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isAdminUser();
                    }]
                }
            }).state('audit', {
                url: "/audit",
                templateUrl: "app/Audit/AuditedList.html",
                controller: 'AuditedListCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isAdminUser();
                    }]
                }
            }).state('assetCode', {
                url: "/assetcodes",
                templateUrl: "app/AssetCodes/AssetCodeList.html",
                controller: 'AssetCodeListCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isAdminUser();
                    }]
                }
            }).state('assetsWorkspace', {
                url: "/workspace",
                templateUrl: "app/Assets/Workspace.html",
                controller: 'AssetsWorkspaceCtrl',
                params: {
                    tab: 'assets'
                },
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isAuthenticated();
                    }]
                }
            }).state('assetsWorkspace.assets', {
                url: "/assets",
                templateUrl: "app/Assets/AssetsList.html",
                controller: 'AssetsListCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isAuthenticated();
                    }]
                }
            }).state('assetsWorkspace.manufacturers', {
                url: "/manufacturers",
                templateUrl: "app/Manufacturer/ManufacturersList.html",
                controller: 'ManufacturersListCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('assetsWorkspace.vendors', {
                url: "/vendors",
                templateUrl: "app/Vendor/VendorsList.html",
                controller: 'VendorsListCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('assetsWorkspace.categories', {
                url: "/categories",
                templateUrl: "app/Category/CategoriesList.html",
                controller: 'CategoriesListCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isAuthenticated();
                    }]
                }
            }).state('assetsWorkspace.bundles', {
                url: "/bundles",
                templateUrl: "app/Bundle/BundleList.html",
                controller: 'BundleListCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('assetsWorkspace.assetsDetails', {
                url: "/assets/{domain_id:int}/{asset_id:int}",
                templateUrl: "app/Assets/AssetDetails.html",
                controller: 'AssetDetailsCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isAuthenticated();
                    }]
                }
            }).state('assetsWorkspace.manufacturersDetails', {
                url: "/manufacturers/{domain_id:int}/{manufacturer_id:int}",
                templateUrl: "app/Manufacturer/ManufacturerDetails.html",
                controller: 'ManufacturerDetailsCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('assetsWorkspace.vendorsDetails', {
                url: "/vendors/{domain_id:int}/{vendor_id:int}",
                templateUrl: "app/Vendor/VendorDetails.html",
                controller: 'VendorDetailsCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('assetsWorkspace.bundlesDetails', {
                url: "/bundles/{domain_id:int}/{bundle_id:int}",
                templateUrl: "app/Bundle/BundleDetails.html",
                controller: 'BundleDetailsCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('room-templates', {
                url: "/room-templates",
                templateUrl: "app/Room Templates/RoomTemplatesList.html",
                controller: 'RoomTemplatesListCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('room-templates-details', {
                url: "/room-templates/{domain_id:int}/{project_id:int}/{phase_id:int}/{department_id:int}/{room_id:int}",
                templateUrl: "app/Room Templates/RoomTemplateDetails.html",
                controller: 'RoomTemplateDetailsCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('dashboard', {
                url: "/dashboard",
                templateUrl: "app/Dashboard/Dashboard.html",
                controller: 'DashboardCtrl',
                resolve: {
                    currentAuth: ['RouteService', function (RouteService) {
                        return RouteService.isNotManufaturerDomainType();
                    }]
                }
            }).state('no-domain', {
                url: "/no-domain",
                templateUrl: "app/Security/Login/NoDomain.html"
            });

        OAuthProvider.configure({
            baseUrl: '/xPlannerAPI',
            clientId: 'AudaxWare',
            grantPath: 'Token'
            //clientSecret: 'CLIENT_SECRET' // optional
        });

        // TODO(MUST): Remove this, we should only get the cookie if
        OAuthTokenProvider.configure({
            options: { secure: false }
        });

        // Interceptor
        //$httpProvider.interceptors.push('AuthInterceptorService');

    }]);