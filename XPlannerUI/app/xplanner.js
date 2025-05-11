var xPlanner = angular.module("xPlanner", ["ui.router", "kendo.directives", "ngResource",
    "LocalStorageModule", "ngMaterial", "ngMessages", "ng-mfb", "md.data.table", 'ngAnimate',
    "angular-oauth2", "toastr", "kendo.directives", "ngProgress", "ngFileUpload", "ngMenuSidenav"/*, "mdPickers"*/,
    "mdSteppers", "angular-click-outside", "powerbi", "ngSanitize"])
    .constant('_', window._) // Add lodash to DI;