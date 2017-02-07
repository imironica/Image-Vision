using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Accord.Math;
using ImageSearchEngine.DTO;
using ImageSearchEngine.Core;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using Accord.Statistics.Analysis;
using MongoDB.Driver;
using System.Configuration;
using ImageSearchEngine.DTO.Interfaces;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace FeatureGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer();
            //container.RegisterType<IDescriptorManager, DescriptorManagerMongoDb>();
            container.LoadConfiguration();

            var program = container.Resolve<MainProgram>();
            program.Run();
        }
    }

    class MainProgram
    {

        static IDescriptorManager _descriptorManager;
        static string _rootFolder = string.Empty;
        static string _dbName = string.Empty;
        static string _featureNames = string.Empty;
        static string _fileExtension = string.Empty;
        static bool _setDefaultValues = true;
        static bool _computePCA = true;
        static bool _computeFaces = false;
        static bool _includeConcepts = false;
        static string _db = ConfigurationManager.AppSettings["db"].ToString();
        static string _connectionString = ConfigurationManager.AppSettings["dbIp"].ToString();

        public MainProgram(IDescriptorManager descriptorManager)
        {
            _descriptorManager = descriptorManager;
        }

        static void SetDefaultParametersValues()
        {
            _rootFolder = ConfigurationManager.AppSettings["ROOT"].ToString();
            _dbName = ConfigurationManager.AppSettings["dbName"].ToString();
            _featureNames = ConfigurationManager.AppSettings["featureNames"].ToString();
            _computePCA = Convert.ToBoolean(ConfigurationManager.AppSettings["computePCA"].ToString());
            _fileExtension = ConfigurationManager.AppSettings["fileExtension"].ToString();
            _computeFaces = Convert.ToBoolean(ConfigurationManager.AppSettings["computeFaces"].ToString());
            _includeConcepts = Convert.ToBoolean(ConfigurationManager.AppSettings["includeConcepts"].ToString());

        }


        public void Run()
        {
            Console.WriteLine("Options: ");
            Console.WriteLine("1 - Generate database: ");
            Console.WriteLine("0 - Exit: ");

            var option = Console.ReadLine();
            if (option == "1")
            {
                if (!_setDefaultValues)
                {
                    Console.WriteLine("Image database folder: ");
                    _rootFolder = Console.ReadLine();
                    Console.WriteLine("Image database name: ");
                    _dbName = Console.ReadLine();
                    Console.WriteLine("Name of the features (comma separated): ");
                    _featureNames = Console.ReadLine();
                    Console.WriteLine("Image extension: ");
                    _fileExtension = Console.ReadLine();
                }
                else
                {
                    SetDefaultParametersValues();
                }

                GenerateFeatures();
            }
        }

        #region GenerateFeatures
        private static void GenerateFeatures()
        {
            Console.WriteLine("Start process");
            string[] lstDescriptors = _featureNames.Split(',');
            var files = GetImageFilesList();

            if (files == null || files.Count == 0)
            {
                Console.WriteLine("The folder does not contain any document");
                return;
            }

            List<DocumentInfo> lstImages = new List<DocumentInfo>();
            var index = 0;
            var detector = GetFaceDetector();

            foreach (var file in files)
            {
                Console.WriteLine((++index).ToString() + " " + file.Name);

                Bitmap image = (Bitmap)Image.FromFile(file.FullName, true);
                var imageInfo = new DocumentInfo()
                {
                    ImageUrl = file.FullName,
                    Description = file.Name,
                    DocumentName = file.FullName.Replace(_rootFolder, string.Empty),
                    Root = _rootFolder
                };

                if (_computeFaces)
                {
                    FaceDetection(detector, image, ref imageInfo);
                }

                foreach (var descriptor in lstDescriptors)
                {
                    var flags = image.Palette.Entries.Count();
                    if (flags == 0)
                    {
                        double[] descriptors = DescriptorComputation.ComputeDescriptor(descriptor, image);
                        imageInfo.LstDescriptors.Add(descriptor, descriptors);
                    }
                }

                lstImages.Add(imageInfo);
            }

            if (_computePCA)
            {
                Console.WriteLine("PCA compute... ");

                var descriptorName = lstDescriptors[0];
                int featureSize = lstImages[0].LstDescriptors[descriptorName].Count();
                double[,] data = new double[lstImages.Count, featureSize];
                for (int i = 0; i < lstImages.Count; i++)
                {
                    if(lstImages[i].LstDescriptors.ContainsKey(descriptorName))
                        for (int j = 0; j < featureSize; j++)
                        {
                            data[i, j] = lstImages[i].LstDescriptors[descriptorName][j];
                        }
                }

                PrincipalComponentAnalysis pca = ComputePCA(data);

                double[,] pcaValues = pca.Transform(data);
                index = 0;
                for (int i = 0; i < lstImages.Count; i++)
                {
                    double[] desc = { pcaValues[i, 0], pcaValues[i, 1], pcaValues[i, 2] };
                    lstImages[i].LstDescriptors.Add("PCA", desc);
                }
            }

            if (_includeConcepts)
            {
                ProcessConceptFile(ref lstImages);
            }

            _descriptorManager.WriteImageDescriptor(_dbName, lstImages);
        }

        private static void ProcessConceptFile(ref List<DocumentInfo> lstImages)
        {
            var filename = ConfigurationManager.AppSettings["fileConcepts"];

            if (!string.IsNullOrEmpty(filename))
            {
                using (StreamReader sr = File.OpenText(filename))
                {
                    string line = String.Empty;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var name = line.Substring(0, line.IndexOf("##"));
                        var conceptsStr = line.Substring(line.IndexOf("##") + 2);
                        var concepts = conceptsStr.Split('#');
                        var imagePath = lstImages.Where(x => x.ImageUrl == name);
                        if (imagePath != null)
                        {
                            var doc = imagePath.FirstOrDefault();
                            if (doc != null)
                            {
                                int index = 0;
                                int indexConcept = 0;
                                var lstConcepts = new List<Concept>();
                                doc.Concepts = new Concept[concepts.Length / 2];
                                while (index < concepts.Length / 2)
                                {
                                    var concept = new Concept
                                    {
                                        Name = concepts[index],
                                        Percentage = Convert.ToDouble(concepts[index + 1])
                                    };

                                    doc.Concepts[indexConcept++] = concept;
                                    index = index + 2;
                                }
                            }
                             
                        }
                        
                    }
                }
            }
        }

        private static void FaceDetection(HaarObjectDetector detector, Bitmap image, ref DocumentInfo imageInfo)
        {
            Rectangle[] objects = detector.ProcessFrame(image);
            if (objects != null && objects.Length > 0)
            {
                imageInfo.Faces = new Face[objects.Length];
                int iFace = 0;
                foreach (var faceObj in objects)
                {
                    imageInfo.Faces[iFace++] = new Face()
                    {
                        X = faceObj.X,
                        Y = faceObj.Y,
                        Height = faceObj.Height,
                        Width = faceObj.Width
                    };
                }
            }
        }
        #endregion

        #region PrivateFunctions
        private static List<FileInfo> GetImageFilesList()
        {
            //Get all images from a folder
            DirectoryInfo directory = new DirectoryInfo(_rootFolder);
            var filesDir = directory.GetFiles();
            var files = new List<FileInfo>();
            if (filesDir != null)
                files = filesDir.ToList();
            DirectoryInfo[] directories = directory.GetDirectories();
            foreach (var dir in directories)
            {
                FileInfo[] filesTemp = dir.GetFiles();
                foreach (var file in filesTemp)
                {
                    files.Add(file);
                }
            }
            return files;
        }

        private static PrincipalComponentAnalysis ComputePCA(double[,] data)
        {
            AnalysisMethod method = AnalysisMethod.Center;
            var pca = new PrincipalComponentAnalysis(data, method);
            pca.Compute();
            return pca;
        }

        private static HaarObjectDetector GetFaceDetector()
        {
            //Viola-Jones face detector initialization
            HaarCascade cascade = new FaceHaarCascade();
            HaarObjectDetector detector = new HaarObjectDetector(cascade, 30);
            detector.SearchMode = ObjectDetectorSearchMode.Average;
            detector.ScalingMode = ObjectDetectorScalingMode.SmallerToGreater;
            detector.ScalingFactor = 1.5f;
            detector.UseParallelProcessing = false;
            detector.Suppression = 2;
            return detector;
        }

        #endregion
    }

}
