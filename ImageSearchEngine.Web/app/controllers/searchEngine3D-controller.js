(function () { ////Immediately Invoked Function Expression (IIFE).
    angular.module('searchEngineApp').controller('SearchEngine3DCtrl', function ($scope, searchEngineService) {

        var vm = this;
        $scope.selectedDatabase = "medicalDb";
        $scope.numberOfImages = 300;
        $scope.showLoader = false;
        $scope.engine = null;
        $scope.isError = false;
        $scope.selectedProcessing = "1";
        $scope.state = true;
        $scope.selectedVizualizationType = "1";

        $scope.toggleState = function () {
            $scope.state = !$scope.state;
        };

        activate();

        function activate() {
            $scope.showLoader = true;
            searchEngineService.getDatabases().then(function (res) {
                $scope.databases = res.data;
                setDatabase($scope.databases[0].Code);
                $scope.showLoader = false;
            });
        }

        $scope.setVisualization = function () {
            setDatabase($scope.selectedDatabase);
        };

        $scope.setDatabase = function (dbName) {
            setDatabase(dbName);
        };

        function setDatabase(dbName)
        {
            $scope.selectedDatabase = dbName;
            $scope.showLoader = true;
            searchEngineService.getAllImages3D(dbName, $scope.numberOfImages).then(function (res) {
                $scope.images = res.data;
                generateGUI($scope.images);
                $scope.showLoader = false;
            });
        }

        function generateGUI(images) {
            var canvas = document.getElementById("renderCanvas");
            if ($scope.engine !== null) {
                console.log("Scrapping engine...");
                $scope.engine.stopRenderLoop();
                $scope.engine.dispose();
                delete $scope.engine;
                $scope.engine = null;
            }
            $scope.engine = new BABYLON.Engine(canvas, true);

            var lstImages = {};
            var createScene = function () {
                var scene = new BABYLON.Scene($scope.engine);

                var light1 = new BABYLON.PointLight("Point", new BABYLON.Vector3(-1000, -1000, -1000), scene);
                var light2 = new BABYLON.PointLight("Point", new BABYLON.Vector3(-1000, 1000, -1000), scene);
                var light3 = new BABYLON.PointLight("Point", new BABYLON.Vector3(-1000, -1000, 1000), scene);
                var light4 = new BABYLON.PointLight("Point", new BABYLON.Vector3(1000, 1000, -1000), scene);
                var light5 = new BABYLON.PointLight("Point", new BABYLON.Vector3(-1000, -1000, -1000), scene);

                var camera = new BABYLON.ArcRotateCamera("Camera", -1.2, 1.2, 28, new BABYLON.Vector3(0, 0, 0), scene);
                camera.attachControl(canvas, true);

                var x = 0;
                var y = 0;
                var z = 0;

                for (var i = 1; i < images.length; i++) {

                    x = images[i].x * 300;
                    y = images[i].y * 300;
                    z = images[i].z * 300;
                    imageRoot = images[i].image;

                    var mat = new BABYLON.StandardMaterial(imageRoot, scene);
                    mat.alpha = 1.0;
                    mat.diffuseColor = new BABYLON.Color3(2.5, 2.5, 2.5);
                    var texture = new BABYLON.Texture(imageRoot, scene);
                    mat.diffuseTexture = texture;


                    var box = {};

                    if($scope.selectedVizualizationType == "1")
                        box = BABYLON.Mesh.CreatePlane("plane", 1, scene);
                    if ($scope.selectedVizualizationType == "3")
                        box = BABYLON.Mesh.CreateSphere("sphere", 16, 1.3, scene);
                    if ($scope.selectedVizualizationType == "2")
                        box = BABYLON.Mesh.CreateBox("box", 1.3, scene);

                    box.position.x = x;
                    box.position.y = y;
                    box.position.z = z;
                    box.material = mat;

                    box.actionManager = new BABYLON.ActionManager(scene);

                    box.actionManager.registerAction(new BABYLON.ExecuteCodeAction(BABYLON.ActionManager.OnPickUpTrigger, function (param) {
                        console.log(param.source.material.name);
                        $("#imgDetails").attr("src", param.source.material.name);
                        dialog = $("#dialog").dialog({
                            height: 600,
                            width: 650,
                            modal: true,
                            close: function () {
                            }
                        });
                    }));
                }

                var sphere = BABYLON.Mesh.CreateSphere("sphere1", 16, 0.2, scene);
                sphere.position.x = 2;
                sphere.actionManager = new BABYLON.ActionManager(scene);
                sphere.actionManager.registerAction(new BABYLON.ExecuteCodeAction(BABYLON.ActionManager.OnPickUpTrigger, function () {
                    alert('Demo created by Ionut Mironica :)! \n ionut.mironica.ro');
                }));
                return scene;
            }

            var scene = createScene();
            $scope.engine.runRenderLoop(function () {
                scene.render();
            });

            // Resize
            window.addEventListener("resize", function () {
                $scope.engine.resize();
            });
        }
    });

})();