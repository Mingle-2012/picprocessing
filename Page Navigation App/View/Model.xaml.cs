using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AppApi;
using OpenCvSharp;
using Page_Navigation_App.Utilities;
using Page_Navigation_App.Windows;
using Path = System.IO.Path;

namespace Page_Navigation_App.View
{
    /// <summary>
    /// Model.xaml 的交互逻辑
    /// </summary>
    public partial class Model : UserControl
    {
        private bool _first = true;
        private bool _outputFirst = true;
        private string _imagePath = "";
        private string _outputPath = "";
        private const string TempPath = "Temp\\";
        private DispatcherTimer _timer;
        public Model()
        {
            InitializeComponent();
            InitializeTimer();
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
        
        private static string GetPath()
        {
            var cur = Directory.GetCurrentDirectory();
            cur = Directory.GetParent(cur)?.FullName;
            if (cur == null) return "";
            cur = System.IO.Path.Combine(cur, TempPath);
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
            Apis.SaveImage(img, path);
            Update(path);
        }

        private void Update(string path)
        {
            _outputPath = path;
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
        
        private void AnimateTextBox(TextBox textBox, bool flag)
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

        private void AnimateButton(Button button, bool flag)
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


        private void Output_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_outputPath))
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

        private void Click_sharpen_and_recover(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath, false);
                img = Apis.SharpenColoredImageLaplacian(img);
                SaveAndUpdate(img);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Click_clear(object sender, RoutedEventArgs e)
        {
            try
            {
                var cur = Directory.GetCurrentDirectory();
                cur = Directory.GetParent(cur)?.FullName;
                if (cur == null) return;
                cur = System.IO.Path.Combine(cur, TempPath);
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
        
        private void Click_export(object sender, RoutedEventArgs e)
        {
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

        private void Click_Sharpen_and_recover2(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var img = Apis.LoadImage(_imagePath, false);
                var window = new PopWindow("origin","double(>0.1)","sharpen","double(>0.1)");
                if (window.ShowDialog() != true)
                {
                    return;
                }

                img = Apis.SharpenColoredImageSobel(img, 
                    ToDouble(window.parameter1), 
                    ToDouble(window.parameter2));
                SaveAndUpdate(img);
            }
            catch (Exception ex)
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

        private async void Click_g1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;

                var dstDir = ModelUtil.InputPath1;
                if (!Directory.Exists(dstDir))
                {
                    Directory.CreateDirectory(dstDir);
                }
                    
                File.Copy(_imagePath, Path.Combine(dstDir,"test.jpg"), true);


                var window = new ProcessWindow();
                window.Show();
                window.SetStatus("Processing...");
                window.SetProgress(3);

                var task = Task.Run(() =>
                {
                    for (var i = 3; i < 99; i++)
                    {
                        Task.Delay(30).Wait();
                        Application.Current.Dispatcher.Invoke(() => window.SetProgress(i));
                    }
                });
                
                await Task.Run(() =>
                {
                    ModelUtil.GetRes(ModelUtil.py, ModelUtil.scriptname, ModelUtil.working_esr);
                });

                await task;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var img = Apis.LoadImage(ModelUtil.OutputPath1, false);
                    
                    window.SetStatus("Completed");
                    window.SetProgress(100);
                    
                    Thread.Sleep(2000);
                    window.Close();
                    
                    SaveAndUpdate(img);
                });
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private async void Click_g2(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var dstDir = ModelUtil.InputPath3;
                if (!Directory.Exists(dstDir))
                {
                    Directory.CreateDirectory(dstDir);
                }
                File.Copy(_imagePath, Path.Combine(dstDir,"test.jpg"), true);

                var window = new ProcessWindow();
                window.Show();
                window.SetStatus("Processing...");
                window.SetProgress(3);
                var task = Task.Run(() =>
                {
                    for (var i = 3; i < 99; i++)
                    {
                        Task.Delay(5).Wait();
                        Application.Current.Dispatcher.Invoke(() => window.SetProgress(i));
                    }
                });
                
                await Task.Run(() =>
                {
                    ModelUtil.GetRes("cmd", "/c .\\resr.exe -i test.jpg -o output.png", ModelUtil.working_exe);
                });

                await task;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var img = Apis.LoadImage(ModelUtil.OutputPath3, false);
                    window.SetStatus("Completed");
                    window.SetProgress(100);
                    window.Close();
                    SaveAndUpdate(img);
                });
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }
        
        private async void Click_g3(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckImagePath()) return;
                var dstDir = ModelUtil.InputPath3;
                if (!Directory.Exists(dstDir))
                {
                    Directory.CreateDirectory(dstDir);
                }
                File.Copy(_imagePath, Path.Combine(dstDir,"test.jpg"), true);
                
                var window = new ProcessWindow();
                window.Show();
                window.SetStatus("Processing...");
                window.SetProgress(3);
                var task = Task.Run(() =>
                {
                    for (var i = 3; i < 99; i++)
                    {
                        Task.Delay(5).Wait();
                        Application.Current.Dispatcher.Invoke(() => window.SetProgress(i));
                    }
                });
                
                await Task.Run(() =>
                {
                    ModelUtil.GetRes("cmd", "/c .\\resr.exe -i test.jpg -o output.png", ModelUtil.working_exe);
                });

                await task;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var img = Apis.LoadImage(ModelUtil.OutputPath3, false);
                    window.SetStatus("Completed");
                    window.SetProgress(100);
                    window.Close();

                    var window2 = new PopWindow("origin","double(>0.1)","sharpen","double(>0.1)");
                    if (window2.ShowDialog() != true)
                    {
                        return;
                    }
                    
                    img = Apis.SharpenColoredImageSobel(img, 
                        ToDouble(window2.parameter1 ?? 1.2), 
                        ToDouble(window2.parameter2 ?? 0.1));
                    SaveAndUpdate(img);
                });
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }
    }
}
