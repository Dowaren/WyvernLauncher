using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Version;
using System.Data;
using System.Data.SqlClient;
using CmlLib.Core.VersionMetadata;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Win32;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ProjectWyvern
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Start();

            TextBox1.Text = Properties.Settings.Default.Username;
            
            if (Properties.Settings.Default.access != null)
            {

                string accessToken = Properties.Settings.Default.access;
                
                try
                {
                    TcpClient client = new TcpClient("localhost", 8888); // Замените на IP-адрес и порт вашего сервера

                    NetworkStream stream = client.GetStream();

                    byte[] data = Encoding.ASCII.GetBytes(accessToken);
                    stream.Write(data, 0, data.Length);

                    data = new byte[256];
                    string response = string.Empty;
                    int bytes = stream.Read(data, 0, data.Length);
                    response = Encoding.ASCII.GetString(data, 0, bytes);

                    string[] responseParts = response.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string username = responseParts[0].Trim().Substring(10);
                    string prefix = responseParts[1].Trim().Substring(8);
                    string imgid = responseParts[2].Trim().Substring(7);

                    ussername.Text = username;
                    prsefix.Text = prefix;

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imgid);
                    bitmap.EndInit();

                    img.ImageSource = bitmap;


                    stream.Close();
                    client.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }




            }




        }

        private bool isDragging = false;
        private Point startPoint;

        private async void launch(object sender, RoutedEventArgs e)
        {






            if (comboBox.SelectedItem != null)
            {

                static bool IsJavaInstalled()
                {
                    // Проверка наличия Java по пути к исполняемому файлу Java
                    string javaPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Java";
                    if (Directory.Exists(javaPath))
                    {
                        string[] versions = Directory.GetDirectories(javaPath);
                        foreach (string version in versions)
                        {
                            string javaExe = version + "\\bin\\java.exe";
                            if (File.Exists(javaExe))
                            {
                                return true;
                            }
                        }
                    }

                    // Если Java не найдена в Program Files, проверяем другие пути
                    string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
                    if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
                    {
                        string javaExe = javaHome + "\\bin\\java.exe";
                        if (File.Exists(javaExe))
                        {
                            return true;
                        }
                    }

                    // Java не установлена
                    return false;
                }







                if (IsJavaInstalled())
                {
                    btn.IsEnabled = false;
                    comboBox.IsEnabled = false;
                    var selectedVersion = comboBox.SelectedItem.ToString();
                    Properties.Settings.Default.Username = TextBox1.Text;
                    Properties.Settings.Default.Save();
                    var launcher = new CMLauncher(new MinecraftPath());



                    MVersionCollection versions = await launcher.GetAllVersionsAsync();

                    MVersion selectedMVersion = versions.GetVersion(selectedVersion);




                    if (selectedMVersion != null)
                    {
                        STATUS.Text = "WYVERN - Начинаю проверку/установку...";
                        await launcher.CheckAndDownloadAsync(selectedMVersion);
                        var text = TextBox1.Text;




                        MSession session = new MSession(text, "plain", Guid.NewGuid().ToString());








                        var process = launcher.CreateProcess(selectedMVersion, new MLaunchOption()
                        {
                            StartVersion = selectedMVersion,
                            Session = session,

                            Path = new MinecraftPath(),
                            MaximumRamMb = 4096,
                            
                            



                            ScreenWidth = 800,
                            ScreenHeight = 800,

                            VersionType = "Wyvern 1.0",
                            GameLauncherName = "WyvernLauncher",
                            GameLauncherVersion = "2",

                            FullScreen = false,


                        }); ;
                        STATUS.Text = "WYVERN - Запускаем майнкрафт...";
                        process.Start();

                        await Task.Delay(500);

                        comboBox.Items.Clear();


                        foreach (MVersionMetadata ver in versions)
                        {

                            comboBox.Items.Add(ver.Name);
                        }

                        for (int i = comboBox.Items.Count - 1; i >= 0; i--)
                        {
                            string item = comboBox.Items[i].ToString();
                            if (item.Contains("1.16") || item.Contains("w") || item.Contains("pre") || item.Contains("a") || item.Contains("b") || item.Contains("Pre") || item.Contains("rc") || item.Contains("RC") || item.Contains("rd") || item.Contains("c") || item.Contains("rd") || item.Contains("inf"))
                            {
                                comboBox.Items.RemoveAt(i);
                            }
                        }


                        Console.WriteLine(versions.LatestReleaseVersion.Name);


                        Console.WriteLine(versions.LatestSnapshotVersion.Name);
                        comboBox.SelectedIndex = 0;

                        ;

                        comboBox.IsEnabled = true;

                        btn.IsEnabled = true;



                        STATUS.Text = "WYVERN";
                    }
                }

                else
                {
                    MessageBox.Show("Не найдено java.", "ERROR HANDLE: JDKdoesntexists");
                }

                
            
                   
               




            }
        }


        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            startPoint = e.GetPosition(null);
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(null);
                Vector offset = startPoint - currentPoint;

                if (Math.Abs(offset.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(offset.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Начать перетаскивание окна
                    DragMove();
                }
            }
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeApp(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Login_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        async void Start()
        {
            InitializeComponent();

            var launcher = new CMLauncher(new MinecraftPath());

            // CMLauncher class create DefaultVersionLoader instance automatically
            // MVersionCollection versions = launcher.VersionLoader.GetVersionMetadatasAsync();
            MVersionCollection versions = await launcher.GetAllVersionsAsync(); // shortcut

            // show all versions
            foreach (MVersionMetadata ver in versions)
            {

                comboBox.Items.Add(ver.Name);
            }

            // Get latest release version name:
            Console.WriteLine(versions.LatestReleaseVersion.Name);

            // Get latest snapshot version name:
            Console.WriteLine(versions.LatestSnapshotVersion.Name);

            for (int i = comboBox.Items.Count - 1; i >= 0; i--)
            {
                string item = comboBox.Items[i].ToString();
                if (item.Contains("1.16") || item.Contains("w") || item.Contains("pre") || item.Contains("a") || item.Contains("b") || item.Contains("Pre") || item.Contains("rc") || item.Contains("RC") || item.Contains("rd") || item.Contains("c") || item.Contains("rd") || item.Contains("inf"))
                {
                    comboBox.Items.RemoveAt(i);
                }
            }



            comboBox.SelectedIndex = 0;






        }

        private async void Access(object sender, RoutedEventArgs e)
        {


            if (comboBox.Items.Count == 0)
            {

            }
            string accessToken = accesstoken.Text;

            if (accessToken != "Токен")
            {
                
                try
                {
                    TcpClient client = new TcpClient("localhost", 8888); // Замените на IP-адрес и порт вашего сервера

                    NetworkStream stream = client.GetStream();

                    byte[] data = Encoding.ASCII.GetBytes(accessToken);
                    stream.Write(data, 0, data.Length);

                    data = new byte[256];
                    string response = string.Empty;
                    int bytes = stream.Read(data, 0, data.Length);
                    response = Encoding.ASCII.GetString(data, 0, bytes);

                    string[] responseParts = response.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string username = responseParts[0].Trim().Substring(10);
                    string prefix = responseParts[1].Trim().Substring(8);
                    string imgid = responseParts[2].Trim().Substring(7);

                    if (response != "Token not found")
                    {
                        

                        stream.Close();
                        client.Close();

                        Properties.Settings.Default.access = accessToken;
                        Properties.Settings.Default.Save();

                        ussername.Text = username;
                        prsefix.Text = prefix;
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imgid);
                        bitmap.EndInit();

                        img.ImageSource = bitmap;


                    }

                    else
                    {
                        MessageBox.Show("Token no found", "Deny");
                    }
                           

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                Console.ReadLine();

            }
        }

    }
}
            

    