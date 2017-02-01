using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Accord.Imaging;
using Accord.MachineLearning;
using Accord.Math;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using ImageSearchEngine.DTO;
using ImageSearchEngine.DTO.Enum;
using ImageSearchEngine.Core;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using AForge.Video.FFMPEG;
using System.Xml;
using Accord.Statistics.Analysis;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Configuration;
using ImageSearchEngine.DTO.Interfaces;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace Test
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
        public static string ROOT = @"";
        static string dbName = "";
        static string featureNames = "";
        static string fileExtension = "";
        static bool setDefaultValues = true;
        static bool computePCA = true;
        static bool computeFaces = true;
        static string db = ConfigurationManager.AppSettings["db"].ToString();
        static string connectionString = ConfigurationManager.AppSettings["dbIp"].ToString();

        public MainProgram(IDescriptorManager descriptorManager)
        {
            _descriptorManager = descriptorManager;
        }

        static void SetDefaultParametersValues()
        {
            ROOT = ConfigurationManager.AppSettings["ROOT"].ToString();
            dbName = ConfigurationManager.AppSettings["dbName"].ToString();
            featureNames = ConfigurationManager.AppSettings["featureNames"].ToString();
            computePCA = Convert.ToBoolean(ConfigurationManager.AppSettings["computePCA"].ToString());
            fileExtension = ConfigurationManager.AppSettings["fileExtension"].ToString();
            computeFaces = Convert.ToBoolean(ConfigurationManager.AppSettings["computeFaces"].ToString());
        }

        public void Run()
        {
            Console.WriteLine("Options: ");
            Console.WriteLine("1 - Generate database: ");
            Console.WriteLine("0 - Exit: ");

            var option = Console.ReadLine();

            if (option == "1")
            {
                if (!setDefaultValues)
                {
                    Console.WriteLine("Image database folder: ");
                    ROOT = Console.ReadLine();
                    Console.WriteLine("Image database name: ");
                    dbName = Console.ReadLine();
                    Console.WriteLine("Name of the features (comma separated): ");
                    featureNames = Console.ReadLine();
                    Console.WriteLine("Image extension: ");
                    fileExtension = Console.ReadLine();
                }
                else
                {
                    SetDefaultParametersValues();
                }

                GenerateFeatures();
            }
        }

        #region GenerateFeatures
        public static void GenerateFeatures()
        {
            Console.WriteLine("Start process");
            string[] lstDescriptors = featureNames.Split(',');
            var files = GetImageFilesList();

            if (files == null || files.Count == 0)
            {
                Console.WriteLine("Start process");
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
                    DocumentName = file.Name,
                    Root = ROOT,
                };

                if (computeFaces)
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

            if (computePCA)
            {
                Console.WriteLine("PCA compute... ");

                var descriptorName = lstDescriptors[0];
                int featureSize = lstImages[0].LstDescriptors[descriptorName].Count();
                double[,] data = new double[lstImages.Count, featureSize];
                for (int i = 0; i < lstImages.Count; i++)
                {
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

            _descriptorManager.WriteImageDescriptor(dbName, lstImages);
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


        private static List<FileInfo> GetImageFilesList()
        {
            //Get all images from a folder
            DirectoryInfo directory = new DirectoryInfo(ROOT);
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

    }

}
