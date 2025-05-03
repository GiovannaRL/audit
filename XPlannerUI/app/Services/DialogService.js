xPlanner.service('DialogService', ['$mdDialog', function ($mdDialog) {

    // Confirm Dialog
    this.Confirm = function (title, text, okText, cancelText, multiple) {

        var confirm = $mdDialog.confirm()
                .title(title)
                .htmlContent(text)
                .ok(okText || 'Yes')
                .cancel(cancelText || 'No');

        confirm._options.skipHide = !!multiple;
        // Sometimes we dont wanna hide a previously existent dialog to show a confirmation, as we can have a callback waiting for it

        return $mdDialog.show(confirm);
    };

    this.openModal = function (template, controller, variables, clickOutsideToClose, bindToController) {

        return $mdDialog.show({
            controller: controller,
            templateUrl: template,
            locals: { local: variables },
            fullscreen: true,
            clickOutsideToClose: clickOutsideToClose,
            bindToController: bindToController
        });
    };

    this.SaveChangesModal = function () {

        return this.openModal('app/Utils/Modals/SaveChanges.html', 'SaveChangesCtrl', null, false);

    };

    this.ViewersChangesModal = function () {

        return this.openModal('app/Utils/Modals/ViewersChanges.html', 'ViewersChangesCtrl', null, false);

    };

    this.LockedProjectModal = function (data) {

        var textContent = 'This project has status locked, it is not possible to update it.';
        if (data) {
            if (data.room_id) {
                textContent = 'This project has status locked, it is not possible to update its rooms.';
            } else if (data.department_id) {
                textContent = 'This project has status locked, it is not possible to update its departments.';
            } else if (data.phase_id) {
                textContent = 'This project has status locked, it is not possible to update its phases.';
            }
        }

        return $mdDialog.show(
            $mdDialog.alert()
            .clickOutsideToClose(true)
            .title('Locked Project')
            .textContent(textContent)
            .ariaLabel('Alert Dialog')
            .ok('OK')
        );

    };

    this.DuplicateAssetConfirm = function () {

        var confirm = $mdDialog.confirm()
                .title('Duplicate Asset')
                .textContent('The asset you are changing resides in the global Audaxware database. Changing this asset will create a custom version in your customized asset database. Are you sure you want to continue?')
                .ok('Yes')
                .cancel('No');

        return $mdDialog.show(confirm);
    };

    this.DuplicateItemsConfirm = function (itemName, multi, text) {

        if (itemName) {
            text = text || (multi ? 'One or more ' + itemName + ' you are changing reside in the global Audaxware database. Changing this ' + itemName + ' will create a custom version of them in your customized database. Are you sure you want to continue?' : 'The ' + itemName + ' you are changing resides in the global Audaxware database. Changing this ' + itemName + ' will create a custom version in your customized database. Are you sure you want to continue?');

            var confirm = $mdDialog.confirm()
                    .title('Duplicate ' + itemName.charAt(0).toUpperCase() + itemName.substring(1))
                    .textContent(text)
                    .ok('Yes')
                    .cancel('No');

            return $mdDialog.show(confirm);
        }
    };

    this.DuplicateItemAlert = function (text) {

        if (text) {
            var alert = $mdDialog.alert()
                .clickOutsideToClose(false)
                .title('Duplicate Item')
                .textContent(text)
                .ariaLabel('Alert Dialog')
                .ok('OK');

            return $mdDialog.show(alert);
        }
    };

    this.Alert = function (title, textContent, okMessage) {
        return $mdDialog.show(
            $mdDialog.alert()
            .clickOutsideToClose(true)
            .title(title)
            .textContent(textContent)
            .ariaLabel('Alert Dialog')
            .ok(okMessage || 'OK')
        );
    };

    this.NotImplementAlert = function () {
        return $mdDialog.show(
            $mdDialog.alert()
            .clickOutsideToClose(true)
            .title('Not yet implemented')
            .textContent('This functionality has not yet implemented')
            .ariaLabel('Alert Dialog')
            .ok('OK')
        );
    };

    this.Close = function (mdDialog) {
        mdDialog.cancel();
    };

    this.Hide = function (mdDialog, returnData) {
        mdDialog.hide(returnData);
    };

}]);