xPlanner.service('ProgressService', ['ngProgressFactory', function (ngProgressFactory) {

    _progressBar = ngProgressFactory.createInstance();
    _progressBar.setHeight("2px");
    _progressBar.setColor("#FF6E40");
    var contexts = {};

    function _blockScreen(context) {
        if (context != null)
            contexts[context] = 0;
        kendo.ui.progress(angular.element("#spiner"), true);
    };

    function _unblockScreen(context) {
        if (context != null)
            delete contexts[context];
        if (Object.keys(contexts).length == 0)
            kendo.ui.progress(angular.element("#spiner"), false);
    };

    function _startProgressBar() {
        _progressBar.start();
    }

    function _stopProgressBar() {
        _progressBar.stop();
    }

    function _completeProgressBar() {
        _progressBar.complete();
    }

    return {
        blockScreen: _blockScreen,
        unblockScreen: _unblockScreen,
        startProgressBar: _startProgressBar,
        stopProgressBar: _stopProgressBar,
        completeProgressBar: _completeProgressBar
    }

}]);
