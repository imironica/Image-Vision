(function () { ////Immediately Invoked Function Expression (IIFE).
    angular.module('searchEngineApp').controller('TextEngineCtrl', function ($scope, textEngineService) {

        $scope.selectedTopic = "Computers";
        $scope.option = '1'; //1 = random; 2 = most positives; 3 = most negatives
        $scope.clasify = false;
        $scope.response = '';
        activate();


        function activate() {

            textEngineService.getTopics().then(function (res) {
                $scope.topics = res.data;
            });

            textEngineService.getAllTexts($scope.selectedTopic, '1').then(function (res) {
                $scope.texts = res.data;
                for (var i = 0; i < $scope.texts.length; i++) {
                    if ($scope.texts[i].Sentiment == 1) {
                        $scope.texts[i].src = 'Images/happy.jpg';
                    }
                    if ($scope.texts[i].Sentiment == 2) {
                        $scope.texts[i].src = 'Images/angry.jpg';
                    }
                    if ($scope.texts[i].Sentiment == 3) {
                        $scope.texts[i].src = 'Images/neutral.jpg';
                    }
                }
            });

        }


        $scope.reloadTopics = function () {
            getPosts('1');
        };
        $scope.getMostPositive = function () {
            getPosts('2');
        };
        $scope.getMostNegative = function () {
            getPosts('3');
        };

        $scope.setTopic = function (topic) {
            $scope.selectedTopic = topic;
            getPosts('1');
        };

        $scope.clasifyText = function () {
            textEngineService.clasifyText($scope.search).then(function (res) {
                var sentiment = res.data;
                if (sentiment == 1) {
                    $scope.response = 'Images/happy.jpg';
                }
                if (sentiment == 2) {
                    $scope.response = 'Images/angry.jpg';
                }
                if (sentiment == 3) {
                    $scope.response = 'Images/neutral.jpg';
                }
                $("#inputMessageImage").attr("src", $scope.response);
                $scope.clasify = true;
            });
        };

        function getPosts(option) {
            textEngineService.getAllTexts($scope.selectedTopic, option).then(function (res) {
                $scope.texts = res.data;
                for (var i = 0; i < $scope.texts.length; i++) {
                    if ($scope.texts[i].Sentiment == 1) {
                        $scope.texts[i].src = 'Images/happy.jpg';
                    }
                    if ($scope.texts[i].Sentiment == 2) {
                        $scope.texts[i].src = 'Images/angry.jpg';
                    }
                    if ($scope.texts[i].Sentiment == 3) {
                        $scope.texts[i].src = 'Images/neutral.jpg';
                    }
                }
            });
        }

    });


})();