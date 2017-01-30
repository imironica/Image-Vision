(function () {
    "use strict";

    angular
        .module("searchEngineApp")
        .factory("textEngineService", textEngineService);


    function textEngineService($http) {
        var service = {
            getAllTexts: getAllTexts,
            getTopics: getTopics,
            clasifyText: clasifyText
        };

        return service;

        function getAllTexts(topic, orderOption) {

            var url = "api/searchEngine/AllTexts/" + topic + "/" + orderOption;
            return $http.get(url);
        }

        function getTopics() {
            var url = "api/searchEngine/GetTopics";
            return $http.get(url);
        }
        function clasifyText(value) {

            var url = "api/searchEngine/Text/";
            var obj = { Text: value };
            return $http.post(url, obj);
        }

    }
})();
