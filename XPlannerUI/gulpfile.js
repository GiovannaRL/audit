/// <binding BeforeBuild='build' Clean='clean' />
// include plug-ins
var gulp = require('gulp');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var del = require('del');
var gutil = require('gulp-util');
var templateCache = require('gulp-angular-templatecache');
var filesExist = require('files-exist');
var cleanCSS = require('gulp-clean-css');
var webpack = require('webpack-stream');

var config = {
    //Include all js files but exclude any min.js files
    src: ['app/**/*.js', '!app/**/*.min.js', '!app/xPlannerConfig.js', 'build/xPlannerConfig.bundle.js'],
    templates: ['app/**/*.html'],
    thirdParty: [
        //"node_modules/jquery/dist/jquery.min.js",
        //"node_modules/jqueryui/jquery-ui.min.js",
        //"node_modules/es6-promise/dist/es6-promise.auto.min.js",
            "ThirdParty/jQuery/jquery-2.1.4.min.js",
        "ThirdParty/JSZip/2014.3.1029/jszip.min.js",
        //"ThirdParty/Angular/1.4.8/angular.js",
        "ThirdParty/Angular/1.7.9/angular.js",
        //"ThirdParty/Angular/1.4.8/angular-resource.min.js",
        "ThirdParty/Angular/1.7.9/angular-resource.min.js",
        //"ThirdParty/Angular/1.4.8/angular-messages.min.js",
        "ThirdParty/Angular/1.7.9/angular-messages.min.js",
        "ThirdParty/Angular/ui-router/0.3.2/angular-ui-router.min.js",
        "ThirdParty/Angular/local-storage/0.1.5/angular-local-storage.js",
        "ThirdParty/Angular/toastr/1.6.0/js/angular-toastr.tpls.js",
        //"ThirdParty/Angular/1.4.8/angular-animate.min.js",
        "ThirdParty/Angular/1.7.9/angular-animate.min.js",
        //"ThirdParty/Angular/1.4.8/angular-aria.min.js",
        "ThirdParty/Angular/1.7.9/angular-aria.min.js",
        "ThirdParty/Angular/1.7.9/angular-sanitize.min.js",
        //"ThirdParty/Angular/material/1.0.9/js/angular-material.min.js",
        "ThirdParty/Angular/material/1.1.0/js/angular-material.min.js",
        "ThirdParty/Angular/material/mfb/0.6.2/js/mfb-directive.js",
        "ThirdParty/Angular/material/data-table/0.9.4/js/md-data-table.js",
        //"ThirdParty/Angular/cookies/1.5.0/angular-cookies.min.js",
        "ThirdParty/Angular/1.7.9/angular-cookies.min.js",
        "ThirdParty/Angular/oauth2/query-string.js",
        "ThirdParty/Angular/oauth2/3.0.1/dist/angular-oauth2.min.js",
        "ThirdParty/Angular/ng-progress/1.1.2/js/ngprogress.min.js",
        "ThirdParty/Angular/material/file-upload/12.2.13/js/ng-file-upload.min.js",
        "ThirdParty/Angular/material/sidenav-menu/0.0.2/js/material-menu-sidenav.min.js",
        "ThirdParty/KendoUI/R12020SP1/js/kendo.all.min.js",
        "ThirdParty/KendoUI/material/js/kendo.custom.js",
        "ThirdParty/Super-Json/0.0.2/super-json.js",
        "ThirdParty/moment/2.13.0/js/moment.min.js",
        //"ThirdParty/Angular/material/mdPickers/0.7.4/js/mdPickers.min.js",
        "ThirdParty/Angular/material/steppers/2.0.0/js/material-steppers.min.js",
        "ThirdParty/Angular/clickoutside/clickoutside.directive.js",
    
        "node_modules/powerbi-client/dist/powerbi.js",
        "node_modules/powerbi-client/dist/powerbi.min.js",
        "node_modules/angular-powerbi/dist/angular-powerbi.js",
        "ThirdParty/jQuery/cookie/1.4.1/js-cookie.js",
        "ThirdParty/lodash/lodash.min.js"
    ],
    cssFiles: [
        //"ThirdParty/Angular/material/1.0.9/css/angular-material.css",
        "ThirdParty/Angular/material/1.1.0/css/angular-material.css",
        //"ThirdParty/KendoUI/R12020/styles/kendo.common-material.core.min.css",
        "ThirdParty/KendoUI/R12020SP1/styles/kendo.common-material.min.css",
        "ThirdParty/KendoUI/R12020SP1/styles/kendo.material.min.css",
        "ThirdParty/KendoUI/material/css/kendo.custom.css",
        "ThirdParty/Angular/material/mfb/0.6.2/css/mfb.css",
        "ThirdParty/Angular/material/data-table/0.9.4/css/md-data-table.css",
        "ThirdParty/Angular/toastr/1.6.0/css/angular-toastr.css",
        "ThirdParty/Angular/ng-progress/1.1.2/css/ngProgress.css",
        "ThirdParty/Angular/material/sidenav-menu/0.0.2/css/material-menu-sidenav.min.css",
        //"ThirdParty/Angular/material/mdPickers/0.7.4/css/mdPickers.min.css",
        "ThirdParty/Angular/material/steppers/2.0.0/css/material-steppers.min.css",
        "Content/Site.css"    
    ],
    fontFolders: [
        'ThirdParty/KendoUI/R12020SP1/styles/fonts/*/*'
    ]
}

//delete the output file(s)
gulp.task('clean', function () {
    //del is an async function and not a gulp plugin (just standard nodejs)
    //It returns a promise, so make sure you return that from this task function
    //  so gulp knows when the delete is complete
    return del(['build/all.min.js', 'app/xPlannerTemplates.js', 'build/thirdParty.min.js', 'build/style.min.css', 'build/xPlannerConfig.bundle.js', 'build/fonts']);
});

gulp.task('createAngularTemplateCache', function () {
    //del is an async function and not a gulp plugin (just standard nodejs)
    //It returns a promise, so make sure you return that from this task function
    //  so gulp knows when the delete is complete
    return gulp.src(config.templates)
    .pipe(templateCache({ module: 'xPlanner', root: 'app/', filename: 'xPlannerTemplates.js' }))
    .pipe(gulp.dest('app'));
});

gulp.task('minifyThirdParty', function () {
    return gulp.src(filesExist(config.thirdParty))
      .pipe(concat('thirdParty.min.js'))
      .pipe(gulp.dest('build/'));
});

gulp.task('minifyCSS', function () {
    return gulp.src(filesExist(config.cssFiles))
      .pipe(cleanCSS())
      .pipe(concat('style.min.css'))
      .pipe(gulp.dest('build/'));
});

// Combine and minify all files from the app folder
// This tasks depends on the clean task which means gulp will ensure that the 
// Clean task is completed before running the scripts task.
gulp.task('minifyAppSources', gulp.series('createAngularTemplateCache', function () {

    return gulp.src(config.src)
      .pipe(uglify().on('error', gutil.log))
      .pipe(concat('all.min.js'))
      .pipe(gulp.dest('build/'));
}));

gulp.task('cleanThemes', function () {

    return del(['build/xPlannerConfig.bundle.js']);
});

gulp.task('buildThemes', function () {

    return gulp.src('app/xPlannerConfig.js')
        .pipe(webpack(require('./webpack.config.js')))
        .pipe(gulp.dest('build/'));
});

gulp.task('fonts', function () {
    return gulp.src(config.fontFolders)
        .pipe(gulp.dest('build/fonts'));
})


gulp.task('build', gulp.series('clean', 'buildThemes', 'minifyThirdParty', 'minifyAppSources', 'minifyCSS', 'fonts', function (done) { done(); }));
//gulp.task('themes', gulp.series('cleanBundle', 'buildBundle', function () { }));

//Set a default tasks
gulp.task('default', gulp.series('build', function () { }));
