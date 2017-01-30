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

namespace Test
{
    class Program
    {
        static DescriptorManagerSimple descriptorManager;
        public static string ROOT = @"D:\Proiecte\Practica_.Net\ImageSearchEngine\ImageSearchEngine\ImageSearchEngine.Web\";
        static void Main(string[] args)
        {
            descriptorManager = new DescriptorManagerSimple();
           
        }

        static void GenerateFaceFeatures(string featureFile, string saveFile)
        {
            TextReader trFiles = new StreamReader(ROOT + "paths.csv");
            TextReader trFeature = new StreamReader(ROOT + featureFile);
            string line = null;
            List<DocumentInfo> lstImages = new List<DocumentInfo>();

            while ((line = trFiles.ReadLine()) != null)
            {
                List<string> features = trFeature.ReadLine().Split(',').ToList();
                double[] descriptors = features.Select(x => double.Parse(x)).ToList().ToArray();
                descriptors[descriptors.Length - 1] = 0;

                var sum = descriptors.Sum();
                if (sum > 0)
                    for (int k = 0; k < descriptors.Length; k++)
                        descriptors[k] /= sum;

                var imageInfo = new DocumentInfo()
                {

                    ImageUrl = line,
                    Description = line,
                    Root = "db.FaceRecognition",

                };

                imageInfo.LstDescriptors.Add("DESC", descriptors);
                lstImages.Add(imageInfo);
            }
            descriptorManager.WriteImageDescriptor(ROOT + saveFile, lstImages);

        }

        public static void GeneratePCADescriptors(string imageRoot, string saveFeaturePCA)
        {
            List<DocumentInfo> images = descriptorManager.GetImageDescriptor(ROOT + imageRoot);
            double[,] data = new double[images.Count, images[0].LstDescriptors["DESC"].Count()];
            int count = images[0].LstDescriptors["DESC"].Count();
            for (int i = 0; i < images.Count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    data[i, j] = images[i].LstDescriptors["DESC"][j];
                }
            }

            AnalysisMethod method = AnalysisMethod.Center;  
            var pca = new PrincipalComponentAnalysis(data, method);
            pca.Compute();

            double[,] pcaValues = pca.Transform(data);
 
            var lstDocs = new List<DocumentInfo>();
            for (int i = 0; i < images.Count; i++)
            {
                var img = images[i];
                 
                double[] desc = { pcaValues[i, 0], pcaValues[i, 1], pcaValues[i, 2]};
                Dictionary<string, double[]> dic = new Dictionary<string, double[]>();
                dic.Add("DESC", desc);
                DocumentInfo doc = new DocumentInfo()
                {
         
                    Description = img.Description,
                    ImageUrl = img.ImageUrl,
                    Root = img.Root,
                    LstDescriptors = dic
                };
                lstDocs.Add(doc);
            }
            descriptorManager.WriteImageDescriptor(ROOT + saveFeaturePCA, lstDocs);
        }

        public static void ConvertImageToDocument(string saveFile)
        {
            List<ImageInfo> lstImages =   descriptorManager.GetImagesDescriptor(ROOT + saveFile );
            var lstDocs = new List<DocumentInfo>();
            foreach (var img in lstImages)
            {
                DocumentInfo doc = new DocumentInfo()
                {
                    Comments = img.Comments,
                    Description = img.Description,
                    ImageUrl = img.ImageUrl,
                    Root = img.Root,
                    LstDescriptors = img.LstDescriptors,
                    
                };
                lstDocs.Add(doc);
            }
            descriptorManager.WriteImageDescriptor(ROOT + saveFile, lstDocs);
        }

        public static void generatePCA()
        {
            List<DocumentInfo> images = descriptorManager.GetImageDescriptor(ROOT + "PRECLIN_CSD.txt");
            double[,] data = new double[images.Count, images[0].LstDescriptors["DESC"].Count()];
            int count = images[0].LstDescriptors["DESC"].Count();
            for (int i = 0; i < images.Count; i++)
            {
                for (int j = 0; j <count; j++)
                {
                    data[i, j] = images[i].LstDescriptors["DESC"][j];
                }
            }  
 
            AnalysisMethod method = AnalysisMethod.Center; // AnalysisMethod.Standardize
  
            var pca = new PrincipalComponentAnalysis(data, method);
 
            pca.Compute();
 
            double[,] pcaValues = pca.Transform(data);
            TextWriter txtW = new StreamWriter("D:\\testPCA.txt");
            for (int i = 0; i < images.Count; i++)
            {
                string url = images[i].Root + "/" + images[i].ImageUrl;
                string imageJSON = string.Format("{{ image:'..{0}', x:{1}, y:{2}, z:{3}}},", url, pcaValues[i,0], pcaValues[i, 1], pcaValues[i, 2]);
                txtW.WriteLine(imageJSON);
            }
            txtW.Close();
        }

   

        public static void GenerateGlobalFeaturesDb(string directoryName, string fileExtension, string saveFile, DescriptorTypeEnum descriptorType)
        {
            DirectoryInfo directory = new DirectoryInfo(ROOT + directoryName);
            var filesDir = directory.GetFiles(fileExtension);
            List<FileInfo> files = new List<FileInfo>();
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

            List<DocumentInfo> lstImages = new List<DocumentInfo>();
            var index = 0;
            foreach (var file in files)
            {
                Console.WriteLine((index++).ToString() + " " + file.Name);

                Bitmap image = (Bitmap)Image.FromFile(file.FullName, true);
                var flags = image.Palette.Entries.Count();
                if (flags == 0)
                {
                    double[] descriptors = DescriptorComputation.ComputeDescriptor(descriptorType, file.FullName);

                    var sum = descriptors.Sum();
                    if (sum > 0)
                        for (int k = 0; k < descriptors.Length; k++)
                            descriptors[k] /= sum;

                    var imageInfo = new DocumentInfo()
                    {

                        ImageUrl = file.FullName.Replace(ROOT + directoryName.Replace("/", ""), ""),
                        Description = file.Name,
                        Root = directoryName,

                    };

                    imageInfo.LstDescriptors.Add("DESC", descriptors);
                    lstImages.Add(imageInfo);
                }
                else
                {

                }

            }
            descriptorManager.WriteImageDescriptor(ROOT + saveFile, lstImages);
        }


        public static void GenerateFaceDetectionViolaJonesTest(string directoryName, string directoryDestination, string fileExtension)
        {
            DirectoryInfo directory = new DirectoryInfo(directoryName);
            var filesDir = directory.GetFiles(fileExtension);
            List<FileInfo> files = new List<FileInfo>();
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

            HaarObjectDetector detector;
            HaarCascade cascade = new FaceHaarCascade();
            detector = new HaarObjectDetector(cascade, 30);

            List<DocumentInfo> lstImages = new List<DocumentInfo>();
            var index = 0;
            foreach (var file in files)
            {

                Console.WriteLine(file.FullName);
                Bitmap image = (Bitmap)Image.FromFile(file.FullName, true);



                detector.SearchMode = ObjectDetectorSearchMode.Average;
                detector.ScalingMode = ObjectDetectorScalingMode.SmallerToGreater;
                detector.ScalingFactor = 1.5f;
                detector.UseParallelProcessing = false;
                detector.Suppression = 2;

                Stopwatch sw = Stopwatch.StartNew();


                // Process frame to detect objects
                Rectangle[] objects = detector.ProcessFrame(image);

                Pen pen = new Pen(Color.Red, 2);
                Graphics g = Graphics.FromImage(image);
                for (int i = 0; i < objects.Length; i++)

                    g.DrawRectangle(pen, objects[i].X, objects[i].Y, objects[i].Width, objects[i].Height);

                image.Save(directoryDestination + file.Name);

            }

        }

        public static void GenerateBOW(string directoryName, string fileExtension, string saveFile, int numberOfWords)
        {
            var originalTrainImages = new Dictionary<string, Bitmap>();


            DirectoryInfo directory = new DirectoryInfo(ROOT + directoryName);
            List<FileInfo> files = new List<FileInfo>();
            var filesDir = directory.GetFiles(fileExtension);
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
            List<DocumentInfo> lstImages = new List<DocumentInfo>();

            for (int i = 0; i < files.Count; i += 100)
            {
                Bitmap image = (Bitmap)Image.FromFile(files[i].FullName, true);
                var flags = image.Palette.Entries.Count();
                if (flags == 0)
                {
                    originalTrainImages.Add(files[i].FullName.Replace(ROOT + directoryName, "").Replace("/", ""), image);
                }
            }

            // Create a Binary-Split clustering algorithm
            BinarySplit binarySplit = new BinarySplit(numberOfWords);
            Stopwatch sw1 = Stopwatch.StartNew();
            IBagOfWords<Bitmap> bow;

            // Create bag-of-words (BoW) with the given algorithm
            var surfBow = new BagOfVisualWords(binarySplit);

            // Compute the BoW codebook using training images only
            surfBow.Compute(originalTrainImages.Values.ToArray());
            bow = surfBow;
            sw1.Stop();

            Stopwatch sw2 = Stopwatch.StartNew();

            // Extract features for all images
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                Bitmap image = (Bitmap)Image.FromFile(files[i].FullName, true);
                try
                {
                    if (image.PixelFormat != PixelFormat.Format24bppRgb)
                        throw new Exception();
                    // Get item image
                    Console.WriteLine(file.Name);
                    // Process image
                    double[] descriptors = bow.GetFeatureVector(image);
                    var fileName = files[i].FullName.Replace(ROOT + directoryName.Replace("/", ""), "");

                    var imageInfo = new DocumentInfo()
                    {
                        ImageUrl = fileName.Substring(1),
                        Description = file.Name,
                        Root = directoryName,
                    };

                    imageInfo.LstDescriptors.Add("DESC", descriptors);
                    lstImages.Add(imageInfo);
                }
                catch (Exception)
                {

                }
            }

            sw2.Stop();
            descriptorManager.WriteImageDescriptor(ROOT + saveFile, lstImages);
        }


        public static void GenerateFeatureHOG()
        {
            string directoryName = @"/images.db";
            DirectoryInfo directory = new DirectoryInfo(@"C:\Users\imironica\Downloads\image.vary.jpg\image.vary.jpg\");
            FileInfo[] files = directory.GetFiles("*.jpg");
            List<DocumentInfo> lstImages = new List<DocumentInfo>();

            foreach (var file in files)
            {
                Bitmap image = (Bitmap)Image.FromFile(file.FullName, true);

                int size = Math.Min(image.Height - 1, image.Width - 1) / 3;

                HistogramsOfOrientedGradients histogram = new HistogramsOfOrientedGradients(9, 3, size);

                var descriptors = histogram.ProcessImage(image);

                var imageInfo = new DocumentInfo()
                {

                    ImageUrl = file.Name,
                    Description = file.Name,
                    Root = directoryName,

                };

                imageInfo.LstDescriptors.Add("HOG", descriptors[0]);
                lstImages.Add(imageInfo);

            }

            descriptorManager.WriteImageDescriptor(@"D:\Proiecte\Practica_.Net\HOG.txt", lstImages);



        }

    }

}
