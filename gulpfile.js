var gulp = require('gulp');
var less = require('gulp-less');
var uglify = require('gulp-uglify');
var rev = require('gulp-rev');
var gzip = require('gulp-gzip');
var clone = require('gulp-clone');
var brotli = require('gulp-brotli');
var merge = require('merge-stream');

gulp.task('default', ['scripts', 'styles']);

gulp.task('styles', function () {
  var base = gulp.src('wwwroot/css/**/!(*.min).css')
    .pipe(less({ compress: true }))
    .on('error', function(e) {
        console.error('>>> ERROR IN STYLES', e);
        this.emit('end');
      })
    .pipe(rev())
    .pipe(gulp.dest('wwwroot/css'));

  return preEncodeResources(base, 'wwwroot/css');
});

gulp.task('scripts', function () {
  var base = gulp.src('wwwroot/js/**/!(*.min).js')
    .pipe(uglify({ compress: true }))
    .on('error', function(e) {
        console.error('>>> ERROR IN SCRIPTS', e);
        this.emit('end');
      })
    .pipe(rev())
    .pipe(gulp.dest('wwwroot/js'));

  return preEncodeResources(base, 'wwwroot/js');
});

var preEncodeResources = function(baseStream, outPath) {
  var streams = [];

  streams.push(baseStream
      .pipe(clone())
      .pipe(gzip({ append: true }))
      .pipe(gulp.dest(outPath)));

  var MODE_GENERIC = 0,
      MODE_TEXT = 1;

  streams.push(baseStream
      .pipe(clone())
      .pipe(brotli.compress({
        extension: 'br',
        skipLarger: true,
        mode: MODE_TEXT,
        quality: 11
      }))
      .pipe(gulp.dest(outPath)));

  streams.push(baseStream.pipe(rev.manifest('wwwroot/rev-manifest.json', {
      base: 'wwwroot/',
      merge: true
    }))
    .pipe(gulp.dest('wwwroot/')));

  return merge.apply(null, streams);
};
