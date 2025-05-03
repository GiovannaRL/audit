// We define this based on jQuery as it is required by the module and we could not find a built version

if ( typeof(queryString) === 'undefined' )
    queryString = {};

if ( typeof(queryString.stringify) === 'undefined')
    queryString.stringify = $.param;

