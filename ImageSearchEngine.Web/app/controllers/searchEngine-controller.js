(function () { ////Immediately Invoked Function Expression (IIFE).
    angular.module('searchEngineApp').controller('SearchEngineCtrl', function ($scope, searchEngineService) {

        var vm = this;
        $scope.selectedDatabase = "Preclin";
        $scope.selectedDescriptor = "1";
        $scope.selectedMetric = "1";
        $scope.showLoader = false;
        $scope.state = true;
        $scope.numberOfImages = 30;
        $scope.imagesHistory = [];
        $scope.imagesFavourites = [];
        $scope.isError = false;
        $scope.selectedRF = "1";
        $scope.selectedProcessing = "1";

        $scope.toggleState = function () {
            $scope.state = !$scope.state;
        };

        activate();


        function activate() {
            searchEngineService.getAllImages($scope.selectedDatabase, $scope.numberOfImages).then(function (res) {
                $scope.images = res.data;
                resetRelevance();
            });
            searchEngineService.getDatabases().then(function (res) {
                $scope.databases = res.data;
            }).then(function () {
                searchEngineService.getDescriptors($scope.selectedDatabase).then(function (res) {
                    $scope.descriptors = res.data;
                });
            });

            searchEngineService.getMetrics($scope.selectedDescriptor).then(function (res) {
                $scope.metrics = res.data;
            });
        }

        $scope.reloadImages = function () {
            searchEngineService.getAllImages($scope.selectedDatabase, $scope.numberOfImages)
                .then(function (res) {
                    $scope.images = res.data;
                },
                function (error) {
                    showError(error);
                }
             );
        }


        $scope.searchImage = function (imageUrl) {
            $scope.showLoader = true;
            searchEngineService.getImages(imageUrl, $scope.selectedDatabase, $scope.selectedMetric, $scope.selectedDescriptor, $scope.numberOfImages, $scope.selectedProcessing)
                .then(function (res) {
                    $scope.images = res.data;
                    $scope.showLoader = false;
                    resetRelevance();
                    saveToHistory($scope.images[0]);
                },
                function (error) {
                    showError(error);
                }
           );
        }

        

        $scope.searchImagesSameEvent = function (imageUrl) {
            $scope.showLoader = true;
            searchEngineService.searchImagesSameEvent(imageUrl, $scope.selectedDatabase, $scope.selectedMetric, $scope.selectedDescriptor, $scope.numberOfImages, $scope.selectedProcessing)
                .then(function (res) {
                    $scope.images = res.data;
                    $scope.showLoader = false;
                    resetRelevance();
                    saveToHistory($scope.images[0]);
                }, function (error) {
                    alert(error);
                }
            );
        }

        $scope.showPreviousImage = function (index) {
            if (index >= 0) {
                $scope.currentIndex = index - 1;
                $scope.currentImage = $scope.images[$scope.currentIndex];
            }
        }

        $scope.showNextImage = function (index) {
            if (index >= 0 && index < $scope.numberOfImages) {
                $scope.currentIndex = index + 1;
                $scope.currentImage = $scope.images[$scope.currentIndex];
            }
        }

        $scope.showImage = function (image, index) {
            if (index >= 0) {
                $scope.showLoader = true;
                $scope.currentImage = $scope.images[index];
                $scope.currentIndex = index;
                $scope.showLoader = false;
            }
        }

        $scope.selectImage = function (index) {
            if ($scope.images[index].selected)
                $scope.numberOfSelectedImages = $scope.numberOfSelectedImages - 1;
            else
                $scope.numberOfSelectedImages = $scope.numberOfSelectedImages + 1;

            $scope.images[index].selected = !$scope.images[index].selected;
        }

        $scope.setDatabase = function (dbName) {
            $scope.selectedDatabase = dbName;
            $scope.showLoader = true;
            searchEngineService.getAllImages(dbName, $scope.numberOfImages).then(function (res) {
                $scope.images = res.data;
                resetRelevance();
            }).then(function () {
                searchEngineService.getDescriptors($scope.selectedDatabase)
                    .then(function (res) {
                        $scope.descriptors = res.data;
                        if ($scope.descriptors.length > 0)
                            $scope.selectedDescriptor = $scope.descriptors[0].Id.toString();;
                        $scope.showLoader = false;
                    },
                        function (error) {
                            showError(error);
                        }
                );
            });
        };

        $scope.resetRelevance = function () {
            resetRelevance();
        }

        $scope.selectFavourite = function (index) {
            $scope.imagesFavourites.push($scope.images[index]);
        }
        $scope.hideAlert = function () {
            $scope.isError = false;
            $scope.ErrorMessage = "";
        }
        $scope.searchRelevanceFeedback = function () {
            var maxValues = 30;
            if ($scope.images.length < maxValues)
                maxValues = $scope.images.length;
            var images = [];
            for (i = 0; i < maxValues; i++) {
                images.push($scope.images[i]);
            }
            $scope.showLoader = true;
            for (i = 0; i < images.length; i++)
                if (images[i].selected)
                    saveToHistory(images[i]);
            searchEngineService.getImagesRF(images, $scope.selectedDatabase, $scope.selectedDescriptor, $scope.numberOfImages, $scope.selectedRF)
                .then(function (res) {
                    $scope.images = res.data;
                    $scope.showLoader = false;
                    resetRelevance();
                },
                function (error) {
                    showError(error);
                }
            );
        }
        $scope.searchImageFromLink = function () {
            searchEngineService.getImagesFromLink($scope.searchImageLink, $scope.selectedDatabase, $scope.selectedMetric, $scope.selectedDescriptor, $scope.numberOfImages, $scope.selectedProcessing)
                .then(function (res) {
                    $scope.images = res.data;
                    $scope.showLoader = false;
                },
                function (error) {
                    showError(error);
                }
           );
        }
        



        function resetRelevance() {
            var i = 0;
            for (i = 0; i < $scope.images.length; i++) {
                $scope.images[i].selected = false;
            }
            $scope.numberOfSelectedImages = 0;
            $scope.ErrorMessage = "";
            $scope.isError = false;
        }

        function showError(error) {
            $scope.ErrorMessage = error.data.ExceptionMessage;
            $scope.isError = true;
            $scope.showLoader = false;
        }

        function saveToHistory(image) {
            $scope.imagesHistory.push(image);
        }


    });


})();