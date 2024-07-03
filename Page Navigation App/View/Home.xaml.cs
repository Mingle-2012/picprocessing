using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AppApi;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using OpenCvSharp;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using Page_Navigation_App.Windows;

namespace Page_Navigation_App.View
{
    /// <summary>
    /// Interaction logic for Customers.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        private bool _first = true;
        private bool _outputFirst = true;
        private string _imagePath = "";
        private string _outputPath = "";
        private const string Path = "Temp\\";
        private DispatcherTimer _timer;
        public Home()
        {
            InitializeComponent();
            InitializeTimer();

            DispatcherUnhandledExceptionEventHandler handler = (sender, e) =>
            {
                ErrorCloseButton.Visibility = Visibility.Visible;
                Error.Visibility = Visibility.Visible;
                Error.Text = e.Exception.Message;
                e.Handled = true;
            };
        }
        
        private void InitializeTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _timer.Tick += Timer_Tick;
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            CloseError_Click(null, null);
        }
        
        private (TransformedBitmap, string) OpenImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true) return (null, null);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(openFileDialog.FileName);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            var resizedBitmap = new TransformedBitmap(bitmap, new ScaleTransform(250.0 / bitmap.PixelWidth, 250.0 / bitmap.PixelHeight));

            return (resizedBitmap, openFileDialog.FileName);
        } 

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var (display, path) = OpenImage();
            if(display == null || string.IsNullOrEmpty(path)) return;
            Display.Source = display;
            _imagePath = path;
            Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

            if (!_first) return;
            Display.Height = 250;
            Display.Width = 250;
            _first = false;
            Border.Opacity = 1;
            BorderCopy.InvalidateVisual();

            Border.Style = (Style)FindResource("BorderStyleWithNoTrigger");
        }
        
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _imagePath = _outputPath;
            Display.Source = Output.Source;
            Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
        }

        
        private void ButtonBase_OnClick1(object sender, RoutedEventArgs e)
        {
            try
            {
                var cur = Directory.GetCurrentDirectory();
                cur = Directory.GetParent(cur)?.FullName;
                if (cur == null) return;
                cur = System.IO.Path.Combine(cur, Path);
                if (Directory.Exists(cur) == false) return;
                foreach (var file in Directory.GetFiles(cur))
                {
                    File.Delete(file);
                }
                ShowMessage("Cleaned up the temp folder successfully.");
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_outputPath))
            {
                ShowMessage("No output image to save.");
                return;
            }
            string imageName;
            try
            {
                if(string.IsNullOrEmpty(_outputPath))return;
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*"
                };
                if (saveFileDialog.ShowDialog() != true) return;
                File.Copy(_outputPath, saveFileDialog.FileName, true);
                imageName = System.IO.Path.GetFileName(saveFileDialog.FileName);
            }catch(Exception ex)
            {
                ShowError(ex);
                return;
            }
            ShowMessage($"Saved {imageName} successfully.");
        }

        private void ShowError(Exception message)
        {
            Console.WriteLine(message.ToString());
            Error.Text = message.Message.Length > 60 ? string.Concat(message.Message.AsSpan(0, 60), "...") : message.Message;
            AnimateTextBox(Error, false);
            AnimateButton(ErrorCloseButton, false);
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Start();
        }

        private void ShowMessage(string msg)
        {
            Error.Text = msg;
            AnimateTextBox(Error, false);
            AnimateButton(ErrorCloseButton, false);
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Start();
        }

        private bool CheckImagePath()
        {
            if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath)) return true;
            ShowMessage("Please select an image first.");
            return false;
        }
        
        private static void AnimateTextBox(TextBox textBox, bool flag)
        {
            DoubleAnimation animation;
            if (flag)
            {
                animation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3))
                };
            }
            else
            {
                animation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3))
                };
            }
            textBox.BeginAnimation(OpacityProperty, animation);
        }

        private static void AnimateButton(Button button, bool flag)
        {
            DoubleAnimation animation;
            if (flag)
            {
                animation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3))
                };
            }
            else
            {
                animation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3))
                };
            }
            button.BeginAnimation(OpacityProperty, animation);
        }

        private void CloseError_Click(object sender, RoutedEventArgs e)
        {
            if(_timer.IsEnabled) _timer.Stop();
            AnimateTextBox(Error, true);
            AnimateButton(ErrorCloseButton, true);
        }

        private static string GetPath()
        {
            var cur = Directory.GetCurrentDirectory();
            cur = System.IO.Path.Combine(cur, Path);
            if (Directory.Exists(cur) == false)
            {
                Directory.CreateDirectory(cur);
            }

            return cur + Guid.NewGuid() + ".png";
        }

        private void SaveAndUpdate(Mat img)
        {
            var path = GetPath();
            Console.WriteLine(path);
            _outputPath = path;
            Apis.SaveImage(img, path);
            if (_outputFirst)
            {
                _outputFirst = false;
                BorderCopy.Opacity = 1;
                BorderCopy.InvalidateVisual();
                BorderCopy.Style = (Style)FindResource("BorderStyleWithNoTrigger");
            }
            var bitmap = new BitmapImage(new Uri(path));
            var transformedBitmap = new TransformedBitmap(bitmap, new ScaleTransform(250.0 / bitmap.PixelWidth, 250.0 / bitmap.PixelHeight));
            Output.Source = transformedBitmap;
            Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
        }

        

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.ToHsv(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.ToGray(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Add_Self(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.Add(img, img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Add_Other(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,true);
                var img2 = OpenImage();
                var img2Mat = Apis.LoadImage(img2.Item2,true);
                img = Apis.Add(img, img2Mat);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
            
            
        }

        private void Output_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_imagePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _outputPath,
                        UseShellExecute = true
                    });
                }
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Multiply_Self(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.Mul(img, img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Multiply_Other(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,true);
                var img2 = OpenImage();
                var img2Mat = Apis.LoadImage(img2.Item2,true);
                img = Apis.Mul(img, img2Mat);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Divide_Self(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                Apis.Div(img, img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Divide_Other(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,true);
                var img2 = OpenImage();
                var img2Mat = Apis.LoadImage(img2.Item2,true);
                img = Apis.Div(img, img2Mat);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Subtract_Self(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                Apis.Sub(img, img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Subtract_Other(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,true);
                var img2 = OpenImage();
                var img2Mat = Apis.LoadImage(img2.Item2,true);
                img = Apis.Sub(img, img2Mat);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_And_Self(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.And(img, img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_And_Other(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,true);
                var img2 = OpenImage();
                var img2Mat = Apis.LoadImage(img2.Item2,true);
                img = Apis.And(img, img2Mat);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Or_Self(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                Apis.Or(img, img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Or_Other(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,true);
                var img2 = OpenImage();
                var img2Mat = Apis.LoadImage(img2.Item2,true);
                img = Apis.Or(img, img2Mat);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Flip_x(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.Flip(img, FlipMode.X);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Flip_y(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.Flip(img, FlipMode.Y);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Flip_xy(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.Flip(img, FlipMode.XY);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Dft(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.RealDft(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Log_Transformation(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                var temp = Apis.LogTransform(img);
                img = temp[1];
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private int ToInt(object o)
        {
            if(o is string s)
            {
                return int.TryParse(s, out var i) ? i : 0;
            }
            return (int)o;
        }
        
        private double ToDouble(object o)
        {
            if(o is string s)
            {
                return double.TryParse(s, out var i) ? i : 0;
            }
            return (double)o;
        }
        
        private void Click_Linear_Transformation(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);

                var window = new PopWindow("a", "b", "c", "d");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                var temp = Apis.LinearTransform(img, ToInt(window.parameter1), ToInt(window.parameter2), ToInt(window.parameter3), ToInt(window.parameter4));
                img = temp[1];
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Hist_Normalization(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,true);
                var img2 = OpenImage();
                var img2Mat = Apis.LoadImage(img2.Item2,true);
                var temp = Apis.HistNormalize(img, img2Mat);
                img = temp[1];
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }


        private void Click_Hist_Equalization(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,true);
                var temp = Apis.EqualizeHist(img);
                img = temp[1];
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_ed_roberts(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.EdgeDetectRoberts(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_ed_sobel(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.EdgeDetectSobel(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_ed_laplacian(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.EdgeDetectLaplacian(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_ed_canny(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.EdgeDetectCanny(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_ed_logG(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.EdgeDetectLoG(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Hough_Lines(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.HoughLines(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Hough_LinesP(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.HoughLinesP(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Spatial_Mean_Filter(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.MeanFilter(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Frequency_Ideal_LowPass(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                
                var window = new PopWindow("cutoff");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.IdealLowPassFilter(img, ToDouble(window.parameter1));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }


        private void Click_Frequency_Butterworth_LowPass(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                
                var window = new PopWindow("cutoff", "order");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.ButterworthLowPassFilter(img, ToDouble(window.parameter1), ToInt(window.parameter2));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Frequency_Gaussian_LowPass(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                var window = new PopWindow("cutoff");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.GaussianLowPassFilter(img, ToDouble(window.parameter1));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void click_sharpen_roberts(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.SharpenRobert(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void click_sharpen_sobel(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.SharpenSobel(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void click_sharpen_laplacian(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.SharpenLaplacian(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void click_sharpen_prewitt(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.SharpenPrewitt(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Frequency_Ideal_HighPass(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                
                var window = new PopWindow("cutoff");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.HighPassFilter(img, Apis.FilterType.Ideal, ToDouble(window.parameter1));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Frequency_Butterworth_HighPass(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                
                var window = new PopWindow("cutoff");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.HighPassFilter(img, Apis.FilterType.Butterworth, ToDouble(window.parameter1));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Frequency_Gaussian_HighPass(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                
                var window = new PopWindow("cutoff");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.HighPassFilter(img, Apis.FilterType.Gaussian, ToDouble(window.parameter1));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Morphological_Erosion(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                var window = new PopWindow("kernel");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.Erode(img, ToInt(window.parameter1));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Morphological_Dilation(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                
                var window = new PopWindow("kernel");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.Dilate(img, ToInt(window.parameter1));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Morphological_Opening(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                
                var window = new PopWindow("kernel");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                
                img = Apis.Open(img, ToInt(window.parameter1));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private void Click_Morphological_Closing(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                
                var window = new PopWindow("kernel");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.Close(img, ToInt(window.parameter1));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_add_noise(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);

                var window = new PopWindow("prob");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.SaltPepperNoise(img, ToDouble(window.parameter1));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }


        private void Click_recover_mean(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.MeanRecoverRGB(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }


        private void Click_recover_median(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.MedianRecover(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_recover_selective(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                img = Apis.SelectiveRecover(img);
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_resize(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);
                
                var window = new PopWindow("height", "width");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.Resize(img, ToInt(window.parameter1), ToInt(window.parameter2));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_rotate(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);

                var window = new PopWindow("angle");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.Rotate(img, ToInt(window.parameter1));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_translate(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);

                var window = new PopWindow("x", "y");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.Translate(img, ToInt(window.parameter1), ToInt(window.parameter2));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        static Point2f[] StringToIntArray(string input)
        {
            var stringArray = input.Split(',');
            var result = new Point2f[stringArray.Length / 2];
            for (var i = 0; i < stringArray.Length; i += 2)
            {
                result[i / 2] = new Point2f(int.Parse(stringArray[i]), int.Parse(stringArray[i + 1]));
            }

            return result;
        }
        
        private void Click_affine_transform(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);

                var window = new PopWindow("src", "dst");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                img = Apis.AffineTransform(img, StringToIntArray((string)window.parameter1), 
                    StringToIntArray((string)window.parameter2));
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Threshold_Triangle(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,false);

                var window = new PopWindow("threshold");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                var temp = Apis.Threshold(img, ToInt(window.parameter1), ThresholdTypes.Triangle);
                img = temp[1];
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Otsu_Thresholding(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,true);
                var window = new PopWindow("threshold");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                var temp = Apis.Threshold(img, ToInt(window.parameter1), ThresholdTypes.Otsu);
                img = temp[1];
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Trunc_Threshold(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,true);
                var window = new PopWindow("threshold");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                var temp = Apis.Threshold(img, ToInt(window.parameter1), ThresholdTypes.Trunc);
                img = temp[1];
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_Threshold_Tozero(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath,true);
                var window = new PopWindow("threshold");
                if (window.ShowDialog() != true)
                {
                    return;
                }
                var temp = Apis.Threshold(img, ToInt(window.parameter1), ThresholdTypes.Tozero);
                img = temp[1];
                SaveAndUpdate(img);
            }catch(Exception ex)
            {
                ShowError(ex);
            }
        }
    }
}
