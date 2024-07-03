using AppApi;
using OpenCvSharp;
using System;
using System.Diagnostics;
using System.Drawing; 
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.IO;
using System.Drawing.Imaging;
using Size = System.Drawing.Size;

namespace Test
{
    class Program
    {
        private const string Path1 = @"D:\\Data\\cs\\CV2\\Test\\images\\5.jpg";
        private const string Path2 = @"D:\\Data\\cs\\CV2\\Test\\images\\2.jpg";
        private const string Path3 = @"D:\\Data\\cs\\CV2\\Test\\images\\image.png";
        private const string Path4 = @"D:\\Data\\cs\\CV2\\Test\\images\\target.png";
        private const string Path5 = @"D:\\Data\\cs\\CV2\\Test\\images\\a.png";
        
        public const string Path6 = @"esr\results\test_output.png";
        public const string Path7 = @"resr\results\input_output.png";

        private static void Main(string[] args)
        {
            string script = "test.py";
            string working = "resr";
            
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "python";
            psi.Arguments = script;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = working; 
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
        
            using (Process process = new Process())
            {
                process.StartInfo = psi;
                process.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
                process.ErrorDataReceived += (sender, data) => Console.WriteLine(data.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
        
            var img = Apis.LoadImage(Path7,false);
            Apis.ShowImage(img);
        }
        
        private static void Main1(string[] args)
        {

            var img1 = Apis.LoadImage(Path1,false);
            var img2 = Apis.LoadImage(Path2,false);
            var img3 = Apis.LoadImage(Path3,false);
            var img4 = Apis.LoadImage(Path4,false);
            var img5 = Apis.LoadImage(Path5,false);

            dynamic res = Apis.GetSRImage(Path1);
            switch (res)
            {
                case null:
                    break;  
                case Mat[] rs:
                {
                    Apis.ShowImageNoWait(rs);
                    break;
                }
                case Mat re:
                    Apis.ShowImage(re);
                    break;
            }
        }
        
        
        
         static async Task Main2(string[] args)
        {
            string modelPath = "D:\\Data\\cs\\CV2\\Test\\model\\model.onnx";
            string imagePath = Path2;
            string outputPath = "D:\\Data\\cs\\CV2\\Test\\output.png";
            var inputData = await Task.Run(() => PreprocessImage(imagePath));
            var result = await RunInferenceAsync(modelPath, inputData);
            SaveResultAsImage(result, outputPath);
        }

        private static float[] PreprocessImage(string imagePath)
        {
            using (var bitmap = new Bitmap(imagePath))
            {
                using (var resized = new Bitmap(bitmap, new Size(64, 64)))
                {
                    float[] inputData = new float[3 * 64 * 64];
                    for (int y = 0; y < 64; y++)
                    {
                        for (int x = 0; x < 64; x++)
                        {
                            var pixel = resized.GetPixel(x, y);
                            int index = (y * 64 + x) * 3;
                            inputData[index] = pixel.R / 255.0f;
                            inputData[index + 1] = pixel.G / 255.0f;
                            inputData[index + 2] = pixel.B / 255.0f;
                        }
                    }
                    return inputData;
                }
            }
        }

        private static async Task<float[]> RunInferenceAsync(string modelPath, float[] inputData)
        {
            return await Task.Run(() =>
            {
                using (var session = new InferenceSession(modelPath))
                {
                    var inputMeta = session.InputMetadata;
                    var inputName = inputMeta.Keys.First();
                    var tensor = new DenseTensor<float>(inputData, new[] { 1, 3, 64, 64 });

                    var inputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor(inputName, tensor)
                    };

                    using (var results = session.Run(inputs))
                    {
                        var output = results.First().AsTensor<float>().ToArray();
                        return output;
                    }
                }
            });
        }

        private static void SaveResultAsImage(float[] result, string outputPath)
        {
            int newWidth = 256; 
            int newHeight = 256;

            using (var resultBitmap = new Bitmap(newWidth, newHeight))
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        int index = (y * newWidth + x) * 3;

                        int r = Math.Min(255, Math.Max(0, (int)(result[index] * 255)));
                        int g = Math.Min(255, Math.Max(0, (int)(result[index + 1] * 255)));
                        int b = Math.Min(255, Math.Max(0, (int)(result[index + 2] * 255)));

                        Color color = Color.FromArgb(r, g, b);
                        resultBitmap.SetPixel(x, y, color);
                    }
                }

                resultBitmap.Save(outputPath, ImageFormat.Png);
            }
        }
    }
}

