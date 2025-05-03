xPlanner.service('AudaxwareDataService', ['DialogService', 'AuthService', '$q', function (DialogService, AuthService, $q) {

    /*
        verify if a item is from audaware domain and the user from another domain and thus should be duplicated
        If the item should be duplicated a dialog is show saying it
    */
    this.CheckHasToDuplicate = function (items, item_name, item_name_plural, opening) {

        if (items) {
            if (!items.length || items.length == 1) {
                var item = items[0] || items;
                if (AuthService.getLoggedDomain() !== 1 && item.domain_id === 1) {
                    return opening ? DialogService.DuplicateItemAlert('This ' + item_name + ' resides in the global Audaxware database. Changes on this ' + item_name + ' will create a custom version in your customized database.') : DialogService.DuplicateItemsConfirm(item_name);
                }
            } else {
                for (var i = 0; i < items.length; i++) {
                    if (AuthService.getLoggedDomain() !== 1 && items[i].domain_id === 1) {
                        return opening ? DialogService.DuplicateItemAlert('This ' + item_name_plural || item_name + ' ' + (type_plural ? 'reside' : 'resides') + 'in the global Audaxware database. Changes on this ' + item_name_plural || item_name + ' will create a custom version in your customized database.') : DialogService.DuplicateItemsConfirm(item_name_plural || item_name, true);
                    }
                };
            }
        }

        return $q(function (resolve) { resolve(false); });
    };

    /*
        Open a dialog to cofirm the asset duplication. The **justDuplicate** parameter indicates
            the asset you just be duplicate and no modification will be done in the new asset
    */
    this.CheckDuplicateAsset = function (asset, justDuplicate, action) { // action = 'edit' or 'delete'

        if (AuthService.getLoggedDomain() !== 1 && asset.domain_id === 1) {
            return justDuplicate ? DialogService.Confirm('Duplicate Asset', 'This asset resides in the global Audaxware database and cannot be modified. To execute this operation is needed create a custom version of it in your database so you can ' + (action || 'edit').toLowerCase() + ' the item. Do you want to continue?', 'Continue', 'Cancel')
                : DialogService.DuplicateAssetConfirm();
        }

        return $q(function (resolve) { resolve(false); });
    };

    /* 
     Given a set of items the function verifies if the user is from audaxware domain and if 
        not removes those items who are from audaxware domain 
    */
    this.RemoveAudaxwareItems = function (items) {
        if (AuthService.getLoggedDomain() === 1)
            return items;

        return items.filter(function (item) { return item.domain_id !== 1 });
    };

    /* 
    Given a set of items verifies if has any item the user cannot modified
   */
    this.HasAudaxwareItems = function (items) {

        if (AuthService.getLoggedDomain() === 1)
            return false;

        return items.find(function (item) { return item.domain_id === 1 }) != null;
    };

    /* 
    Given a set of items verifies if has any item the user can modified
   */
    this.HasNoAudaxwareItems = function (items) {

        if (AuthService.getLoggedDomain() === 1 && items.length > 0)
            return true;

        return items.find(function (item) { return item.domain_id !== 1 }) != null;
    };

    /* 
   Given a set of items verifies if the logged user can modify at least one of them
    returns: -1 = can't modify any / 0 = can modify any / 1 = can modify all
  */
    this.CanModifyAny = function (items) {

        if (AuthService.getLoggedDomain() === 1)
            return 1;

        return this.HasAudaxwareItems(items) ? this.HasNoAudaxwareItems(items) ? 0 : -1 : 1;
    };

    /* 
    Given a item verifies if the logged user can modify it
    */
    this.CanModify = function (item) {

        if (AuthService.getLoggedDomain() === 1)
            return true;

        return item.domain_id === AuthService.getLoggedDomain();
    };

    /* 
    Show the dialog that informs there is audaxware items which will be not modified
   */
    this.ShowCantModifyDialog = function (type_name, multiple, all) {

        return multiple ? DialogService.Alert('Audaxware ' + type_name, (all ? 'The ' : 'One or more ') + type_name + ' you are changing reside in the global Audaxware database and ' + (all ? 'cannot' : 'will not') + ' be modified.') :
            DialogService.Alert('Audaxware ' + type_name, 'This ' + type_name + ' resides in the global Audaxware database and cannot be modified.');
    };

    /* 
   Show the dialog that informs there is audaxware items which will be not deleted
  */
    this.ShowDeleteDialog = function (type_name, items) {

        if (items) {
            switch (this.CanModifyAny(items)) {
                case -1:
                    return DialogService.Alert('Audaxware ' + type_name, 'The ' + type_name + ' you are deleting reside in the global Audaxware database and cannot be deleted.')
                    break;
                case 0:
                    return DialogService.Confirm('Are you sure?', 'One or more ' + type_name + ' you are deleting reside in the global Audaxware database and will not be deleted. The others will be deleted permanently.')
                    break;
                case 1:
                    return DialogService.Confirm('Are you sure?', 'The ' + type_name + ' will be deleted permanently');
                    break;
            }
        }
    };

    this.GetDeleteMessage = function (type_name, some) {
        return some ? ('One or more ' + type_name + ' you are deleting reside in the global Audaxware database and will not be deleted. Do you want to continue?')
            : 'The ' + type_name + ' will be deleted permanently';
    };

}]);