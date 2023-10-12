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

namespace WyvLauncher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    

    partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            Start();
            TextBox1.Text = Properties.Settings.Default.Username;
        }

        private void folder(object sender, RoutedEventArgs e)
        {
            var path1 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Console.WriteLine(path1 + ".minecraft");
            var launcher = new CMLauncher(new MinecraftPath());
            Process.Start("explorer.exe", $"{path1}\\.minecraft");
        }

        private async void launch(object sender, RoutedEventArgs e)
        {
            




            image.Source = new BitmapImage(new Uri("images/logoonwindo2.png", UriKind.Relative));

            if (comboBox.SelectedItem != null)
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
                    image.Source = new BitmapImage(new Uri("images/logoonwindo4.png", UriKind.Relative));
                    await launcher.CheckAndDownloadAsync(selectedMVersion);
                    var text = TextBox1.Text;


                    

                    MSession session = new MSession(text, "plain", Guid.NewGuid().ToString());

                    

                    var process = launcher.CreateProcess(selectedMVersion, new MLaunchOption()
                    {
                        StartVersion = selectedMVersion,
                        Session = session,

                        Path = new MinecraftPath(),
                        MaximumRamMb = 4096,
                        JavaPath = "javaw.exe",
                        JVMArguments = new string[] { },



                        ScreenWidth = 1600,
                        ScreenHeight = 900,

                        VersionType = "Wyvern 1.0",
                        GameLauncherName = "WyvernLauncher",
                        GameLauncherVersion = "2",

                        FullScreen = false,


                    });
                    image.Source = new BitmapImage(new Uri("images/logoonwindo5.png", UriKind.Relative));
                    process.Start();

                    await Task.Delay(500);

                    comboBox.Items.Clear();

                    
                    foreach (MVersionMetadata ver in versions)
                    {

                        comboBox.Items.Add(ver.Name);
                    }

                    
                    Console.WriteLine(versions.LatestReleaseVersion.Name);

                    
                    Console.WriteLine(versions.LatestSnapshotVersion.Name);
                    comboBox.SelectedIndex = 0;

                    ;

                    comboBox.IsEnabled = true;
                    
                    btn.IsEnabled = true;
                }


                image.Source = new BitmapImage(new Uri("images/logoonwindo3.png", UriKind.Relative));

            }
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


            comboBox.SelectedIndex = 0;




        }

        
    }
}
