/// <binding ProjectOpened='default' />
const gulp = require('gulp');
const concat = require('gulp-concat');
const cleanCSS = require('gulp-clean-css');
const rename = require('gulp-rename');
const minify = require('gulp-minify');
const sass = require('gulp-sass')(require('sass'));
const autoprefixer = require('gulp-autoprefixer');
const purgecss = require('gulp-purgecss');

const importFiles = {
    cssBundle: [
        "wwwroot/scss/main.css"
    ],
    cssTemplate: [
        "wwwroot/scss/template.css"
    ],
    jsBundle: [
        "node_modules/split-grid/dist/split-grid.js",
        "wwwroot/lib/character-counter/character-counter.js",
        "node_modules/jquery/dist/jquery.js",
        "node_modules/jquery-validation/dist/jquery.validate.js",
        "node_modules/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.js",
        "node_modules/@yaireo/tagify/dist/tagify.min.js",
        "wwwroot/lib/slide-panel/slide-panel.js",
        "node_modules/egalink-toasty.js/dist/toasty.min.js",
        "node_modules/simplemde/dist/simplemde.min.js"
    ],
    jsDragsort: [
        "node_modules/@yaireo/dragsort/dist/dragsort.js"
    ],
    cssCodeMirror: [
        "node_modules/codemirror/lib/codemirror.css",
        "node_modules/codemirror/theme/eclipse.css",
        "node_modules/codemirror/addon/lint/lint.css",
        "node_modules/codemirror/addon/fold/foldgutter.css",
        "node_modules/codemirror/addon/hint/show-hint.css",
        "node_modules/codemirror/addon/display/fullscreen.css",
    ],
    jsCodeMirror: [
        "node_modules/codemirror/lib/codemirror.js",

        "node_modules/codemirror/addon/hint/anyword-hint.js",
        "node_modules/codemirror/addon/hint/css-hint.js",
        "node_modules/codemirror/addon/hint/html-hint.js",
        "node_modules/codemirror/addon/hint/javascript-hint.js",
        "node_modules/codemirror/addon/hint/show-hint.js",
        "node_modules/codemirror/addon/hint/xml-hint.js",

        "node_modules/codemirror/addon/lint/lint.js",
        //"node_modules/codemirror/addon/lint/json-lint.js",
        //"node_modules/codemirror/addon/lint/javascript-lint.js",
        "node_modules/codemirror/addon/lint/css-lint.js",
        //"node_modules/codemirror/addon/lint/html-lint.js",

        "node_modules/codemirror/mode/htmlmixed/htmlmixed.js",
        "node_modules/codemirror/mode/javascript/javascript.js",
        "node_modules/codemirror/mode/xml/xml.js",
        "node_modules/codemirror/mode/css/css.js",

        "node_modules/codemirror/addon/selection/active-line.js",

        "node_modules/codemirror/addon/edit/closebrackets.js",
        "node_modules/codemirror/addon/edit/closetag.js",
        "node_modules/codemirror/addon/edit/matchbrackets.js",
        "node_modules/codemirror/addon/edit/matchtags.js",

        "node_modules/codemirror/addon/fold/brace-fold.js",
        "node_modules/codemirror/addon/fold/comment-fold.js",
        "node_modules/codemirror/addon/fold/foldcode.js",
        "node_modules/codemirror/addon/fold/foldgutter.js",
        "node_modules/codemirror/addon/fold/indent-fold.js",
        "node_modules/codemirror/addon/fold/xml-fold.js",

        "node_modules/codemirror/addon/display/autorefresh.js",
        "node_modules/codemirror/addon/display/fullscreen.js"
    ]
};

// CSS
function compileScssMain() {
    return gulp.src('./scss/main.scss')
        .pipe(sass().on('error', sass.logError))
        .pipe(gulp.dest('./wwwroot/scss/'));
}

function compileScssTemplate() {
    return gulp.src('./scss/Pages/Template/template.scss')
        .pipe(sass().on('error', sass.logError))
        .pipe(gulp.dest('./wwwroot/scss/'));
}

function bundleCSS() {
    return gulp.src(importFiles.cssBundle)
        .pipe(concat('bundle.css'))
        .pipe(autoprefixer())
        .pipe(gulp.dest('./wwwroot/css/'));
}
function templateCSS() {
    return gulp.src(importFiles.cssTemplate)
        .pipe(concat('template.css'))
        .pipe(autoprefixer())
        .pipe(gulp.dest('./wwwroot/css/'));
}

function codeMirrorCSS() {
    return gulp.src(importFiles.cssCodeMirror)
        .pipe(concat('codemirror.css'))
        .pipe(autoprefixer())
        .pipe(gulp.dest('./wwwroot/css/'));
}

function minCSS() {
    return gulp.src(['./wwwroot/css/*.css', '!./wwwroot/css/*.min.css'])
        .pipe(cleanCSS())
        .pipe(rename({
            suffix: '.min',
        }))
        .pipe(gulp.dest('./wwwroot/css/'));
}

// JSamer_WI

function bundleJS() {
    return gulp.src(importFiles.jsBundle)
        .pipe(concat('bundle.js'))
        .pipe(gulp.dest('./wwwroot/js/'));
}

function dragsortJS() {
    return gulp.src(importFiles.jsDragsort)
        .pipe(concat('dragsort.js'))
        .pipe(gulp.dest('./wwwroot/js/'));
}

function codeMirrorJS() {
    return gulp.src(importFiles.jsCodeMirror)
        .pipe(concat('codemirror.js'))
        .pipe(gulp.dest('./wwwroot/js/'));
}

function minJS() {
    return gulp.src(['./wwwroot/js/*.js', '!./wwwroot/js/*.min.js'])
        .pipe(minify({
            ext: {
                min: '.min.js'
            },
            ignoreFiles: ['-min.js']
        }))
        .pipe(gulp.dest('./wwwroot/js/'))
}

// Run all pipelines
exports.default = gulp.series(compileScssMain, compileScssTemplate, bundleCSS, templateCSS, codeMirrorCSS, minCSS, bundleJS, dragsortJS, codeMirrorJS, minJS);