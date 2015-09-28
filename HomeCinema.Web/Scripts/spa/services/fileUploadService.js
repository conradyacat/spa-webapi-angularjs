﻿(function (app) {
    'use strict';

    app.factory('fileUploadService', fileUploadService);

    fileUploadService.$inject = ['$rootScope', '$http', '$timeout', '$upload', 'notificationService'];

    function fileUploadService($rootScope, $http, $timeout, $upload, notificationService) {
        $rootScope.upload = [];

        var service = {
            uploadImage: uploadImage
        };

        function uploadImage($files, movieId, callback) {
            for (var i = 0; i < $files.length; i++) {
                var $file = $files[i];
                (function (index) {
                    $rootScope.upload[index] = $upload.upload({
                        url: 'api/movie/images/upload/?movieId=' + movieId,
                        method: 'POST',
                        file: $file
                    }).progress(function (evt) {
                    }).success(function (data, status, headers, config) {
                        notificationService.displaySuccess(data.FileName + ' uploaded successfully');
                        callback();
                    }).error(function (data, status, headers, config) {
                        notificationService.displayError(data.Message);
                    });
                })(i);
            }
        }

        return service;
    }
})(angular.module('common.core'));