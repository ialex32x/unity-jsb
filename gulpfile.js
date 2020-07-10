const gulp = require('gulp')
const fs = require('fs')
const path = require('path')
const concat = require('gulp-concat');
const rename = require('gulp-rename');
const uglifyes = require('uglify-es');
const composer = require('gulp-uglify/composer');
const minify = composer(uglifyes, console);
const babel = require('gulp-babel');
const clean = require('gulp-clean');
const sequence = require('run-sequence');
// const oss = require('ali-oss')
const crypto = require('crypto')

const self_config = require('./gulpfile-config.json')

// 合并用户JS
gulp.task('compile', function () {
    let stream = gulp.src(self_config.source)
        // .pipe(babel())
        // .pipe(concat('main.js'))
        .pipe(minify())
        .pipe(gulp.dest(self_config.output));
    return stream;
});

gulp.task('copy-res', function () {
    let stream = gulp.src(self_config.output + '*')
        .pipe(rename({ 'extname': '.js.txt' }))
        .pipe(gulp.dest(self_config.resources + "dist"))
    return stream;
});

gulp.task('copy-config', function () {
    let stream = gulp.src(self_config.config + '*')
        .pipe(rename({ 'extname': '.json.txt' }))
        .pipe(gulp.dest(self_config.resources + "config"))
    return stream;
});

gulp.task('clean', function () {
    let stream = gulp.src([self_config.output + '*', self_config.resources + 'dist/*', self_config.resources + 'config/*'])
        .pipe(clean({ force: true }))
    return stream;
});

// 打包 
gulp.task('default', gulp.series('clean', 'compile', 'copy-res', 'copy-config', function (cb) {
    // target_platform = "release"
    cb();
}));
