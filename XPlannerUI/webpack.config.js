var path = require('path');

module.exports = {
    context: path.join(__dirname, 'app'),
    entry: './xPlannerConfig.js',
    output: {
        path: path.join(__dirname, 'build'),
        filename: 'xPlannerConfig.bundle.js'
    }
};