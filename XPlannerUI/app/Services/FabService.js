xPlanner.factory('FabService', [function () {

    var _chosen = {
        effect: 'slidein',
        position: 'br',
        method: 'click',
        action: 'fire'
    };

    return {
        chosen: _chosen
    }

}]);