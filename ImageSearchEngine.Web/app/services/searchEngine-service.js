(function () {
    "use strict";

    angular
        .module("searchEngineApp")
        .factory("searchEngineService", searchEngineService);

 
    function searchEngineService($http) {
        var service = {
            getImages: getImages,
            getImagesFromLink: getImagesFromLink,
            getImagesRF: getImagesRF,
            getAllImages: getAllImages,
            getDescriptors: getDescriptors,
            getMetrics: getMetrics,
            getDatabases: getDatabases,
            searchImagesSameEvent: searchImagesSameEvent,
            getAllImages3D: getAllImages3D
        };

        return service;


        function getImages(imageName, dbName, metricCode, descriptorCode, providedResults, selectedProcessing) {
            var imageNameReplace = imageName.replace("\\", "!");
            imageNameReplace = imageNameReplace.replace("/", "!");
            imageNameReplace = imageNameReplace.replace("\\", "!");
            var url = "api/searchEngine/SearchImage/" + imageNameReplace + "/" + dbName + "/" + metricCode + "/" + descriptorCode + "/" + providedResults + "/" + selectedProcessing;
            return $http.get(url);
        }

        function getImagesFromLink(imageName, dbName, metricCode, descriptorCode, providedResults, selectedProcessing) {
            var url = "api/searchEngine/SearchImageLink";
            var rfQuery = {};
 
            rfQuery.ImageLink = imageName;
            rfQuery.DbName = dbName;
            rfQuery.Descriptor = descriptorCode;
            rfQuery.ProvidedResults = providedResults;
            rfQuery.Metric = metricCode;

            return $http.post(url, rfQuery);
        }
        

        function searchImagesSameEvent(imageName, dbName, metricCode, descriptorCode, providedResults, selectedProcessing) {
            var imageNameReplace = imageName.replace("\\", "!");
            imageNameReplace = imageNameReplace.replace("/", "!");
            imageNameReplace = imageNameReplace.replace("\\", "!");
            var url = "api/searchEngine/SearchImagesSameEvent/" + imageNameReplace + "/" + dbName + "/" + metricCode + "/" + descriptorCode + "/" + providedResults + "/" + selectedProcessing;
            return $http.get(url);
        }
         

        function getAllImages(dbName, numberOfImages) {
            var url = "api/searchEngine/AllImages/" + dbName + "/" + numberOfImages;
            return $http.get(url);
        }

        function getAllImages3D(dbName, numberOfImages) {
            var url = "api/searchEngine/AllImages3D/" + dbName + "/" + numberOfImages;
            return $http.get(url);
        }

        function getDescriptors(dbName) {
            var url = "api/searchEngine/GetDescriptors/" + dbName;
            return $http.get(url);
        }

        function getMetrics(descriptorName) {
            var url = "api/searchEngine/GetMetrics/" + descriptorName;
            return $http.get(url);
        }

        function getDatabases() {
            var url = "api/searchEngine/GetDatabases";
            return $http.get(url);
        }

        function getImagesRF(images, dbName, descriptor, providedResults, selectedRF) {
            var url = "api/searchEngine/SearchImageRF";
            var rfQuery = {};
            rfQuery.Images = images;
            rfQuery.DbName = dbName;
            rfQuery.Descriptor = descriptor;
            rfQuery.ProvidedResults = providedResults;
            rfQuery.RFAlgorithm = selectedRF;
            return $http.post(url, rfQuery);
        }

       
    }
})();
