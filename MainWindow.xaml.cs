using System;
using System.Collections.Generic;
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
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace downloader
{
    public partial class MainWindow : Window
    {
        bool hd, best;
        string previewurl, PreviewURLdoZamiany;
        public MainWindow()
        {
        }



        //RUCH OKNA 
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        //ZAMYKANIE APLIKACJI
        private void zamknij_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //MINIMALIZOWANIE APLIKACJI
        private void minimalizuj_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void ApiPobieranie_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Pobieranie zakończone", "Wiadomość", MessageBoxButton.OK, MessageBoxImage.Information);
            PDownload.Visibility = Visibility.Hidden;
            Download.Visibility = Visibility.Visible;

            preview.Width = 0;
            preview.Height = 0;

            preview.Visibility = Visibility.Hidden;
            preview.Close();

            preview.Height = 500;
            preview.Width = 1000;

            bplay.Visibility = Visibility.Hidden;
            bpause.Visibility = Visibility.Hidden;
            volumeSlider.Visibility = Visibility.Hidden;
        }

        Brush Bprzed = new SolidColorBrush(Color.FromRgb(11, 94, 215));
        Brush Bpo = new SolidColorBrush(Color.FromRgb(8, 63, 140));

        private void bhd_Click(object sender, RoutedEventArgs e)
        {
            hd = true;
            best = false;

            bhd.Background = Bpo;
            bbest.Background = Bprzed;
        }

        private void bbest_Click(object sender, RoutedEventArgs e)
        {
            best = true;
            hd = false;

            bbest.Background = Bpo;
            bhd.Background = Bprzed;
        }

        private void URLin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (URLin.Text.Contains("youtube") == true)
            {
                bhd.Visibility = Visibility.Visible;
                bbest.Visibility = Visibility.Visible;

            }
            else
            {
                bhd.Visibility = Visibility.Hidden;
                bbest.Visibility = Visibility.Hidden;
            }
        }

        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            preview.Volume = (double)volumeSlider.Value;
        }

        private void bplay_Click(object sender, RoutedEventArgs e)
        {
            preview.Play();
        }

        private void bpause_Click(object sender, RoutedEventArgs e)
        {
            preview.Pause();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            InicjalizacjaPobierania();
        }

        async void InicjalizacjaPobierania()
        {

            //textbox.text
            string URL = URLin.Text;

            //szurag
            Regex tik = new Regex(@"(@[a-zA-z0-9]*|.*)(\/.*\/|trending.?shareId=)([\d]*)");
            Regex inst = new Regex(@"((?:https?:\/\/)?(?:www\.)?instagram\.com\/(?:p|reel)\/([^/?#&]+)).*");

            Match czytik = tik.Match(URL);
            Match czyinst = inst.Match(URL);
            bool btik = URL.Contains("tiktok");
            bool binst = URL.Contains("instagram");

            //szuragV2
            Regex yt = new Regex(@"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$");
            Regex por = new Regex(@"^((?:https?:)?\/\/)?((?:pl|com)\.)?((?:pornhub\.com))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$");

            Match czyyt = yt.Match(URL);
            Match czypor = por.Match(URL);
            bool byt = URL.Contains("youtube");
            bool bpor = URL.Contains("pornhub");

            //szurag
            if (btik == true || binst == true)
            {
                //json
                var data = new StringContent("{\"url\":\"" + URL + "\"}", Encoding.UTF8, "application/json");

                //url
                var url = "https://api.sagin.pl/szurag";

                using var client = new HttpClient();

                var response = await client.PostAsync(url, data);

                var result = await response.Content.ReadAsStringAsync();
                var temp = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

                var arr = temp.Values.ToArray();

                if (arr[0].Contains("https://api.sagin.pl/"))
                {
                    response = await client.GetAsync(arr[0]);
                    result = await response.Content.ReadAsStringAsync();

                    async Task<string> OtrzymanieURL()
                    {
                        while (JsonConvert.DeserializeObject<Dictionary<string, string>>(result).Values.ToArray()[0].Length <= 3)
                        {
                            response = await client.GetAsync(arr[0]);
                            result = await response.Content.ReadAsStringAsync();
                            Thread.Sleep(250);
                        }

                        Download.Visibility = Visibility.Hidden;
                        bhd.Visibility = Visibility.Hidden;
                        bbest.Visibility = Visibility.Hidden;

                        PDownload.Visibility = Visibility.Visible;

                        PreviewURLdoZamiany = JsonConvert.DeserializeObject<Dictionary<string, string>>(result).Values.ToArray()[0];

                        previewurl = PreviewURLdoZamiany;

                        preview.Visibility = Visibility.Visible;

                        preview.Source = new Uri(previewurl);
                        preview.Volume = 0;

                        preview.Height = 500;
                        preview.Width = 1000;

                        bplay.Visibility = Visibility.Visible;
                        bpause.Visibility = Visibility.Visible;
                        volumeSlider.Visibility = Visibility.Visible;

                        preview.Play();

                        return JsonConvert.DeserializeObject<Dictionary<string, string>>(result).Values.ToArray()[0];
                    }


                    using (var ApiPobieranie = new WebClient())
                    {

                        string kox = await OtrzymanieURL();

                        Uri uri = new Uri(kox);

                        SaveFileDialog dialog = new SaveFileDialog();
                        dialog.Filter = "MP4 Video (*.mp4)|*.mp4|Wszystkie pliki (*.*)|*.*";

                        var result1 = dialog.ShowDialog();

                        ApiPobieranie.DownloadProgressChanged += (s, e) => { PDownload.Value = e.ProgressPercentage; };

                        if (result1 == true)
                        {

                            ApiPobieranie.DownloadFileAsync(uri, dialog.FileName);

                            ApiPobieranie.DownloadFileCompleted += ApiPobieranie_DownloadFileCompleted;
                        }
                        else
                        {
                            PDownload.Visibility = Visibility.Hidden;
                            Download.Visibility = Visibility.Visible;

                            //preview.Width = 0;
                            //preview.Height = 0;

                            preview.Visibility = Visibility.Hidden;

                            bplay.Visibility = Visibility.Hidden;
                            bpause.Visibility = Visibility.Hidden;
                            volumeSlider.Visibility = Visibility.Hidden;

                            preview.Close();
                        }
                        //ApiPobieranie.DownloadFileCompleted += ApiPobieranie_DownloadFileCompleted;



                    }


                }

            }
            else if (byt == true || bpor == true)
            {
                StringContent data;
                data = new StringContent("{\"settings\":\"hd\", \"url\":\"" + URL + "\"}", Encoding.UTF8, "application/json");

                if (best == true)
                {
                    data = new StringContent("{\"settings\":\"best\", \"url\":\"" + URL + "\"}", Encoding.UTF8, "application/json");
                }
                else if (hd == true)
                {

                    data = new StringContent("{\"settings\":\"hd\", \"url\":\"" + URL + "\"}", Encoding.UTF8, "application/json");
                }
                else
                {
                    data = new StringContent("{\"settings\":\"hd\", \"url\":\"" + URL + "\"}", Encoding.UTF8, "application/json");
                }

                //url
                var url = "https://api.sagin.pl/szuragV2";

                using var client = new HttpClient();

                var response = await client.PostAsync(url, data);

                var result = await response.Content.ReadAsStringAsync();
                var temp = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

                var arr = temp.Values.ToArray();

                if (arr[0].Contains("https://api.sagin.pl/"))
                {
                    response = await client.GetAsync(arr[0]);
                    result = await response.Content.ReadAsStringAsync();

                    async Task<string> OtrzymanieURL()
                    {
                        while (JsonConvert.DeserializeObject<Dictionary<string, string>>(result).Values.ToArray()[0].Length <= 3)
                        {
                            response = await client.GetAsync(arr[0]);
                            result = await response.Content.ReadAsStringAsync();
                            Thread.Sleep(250);
                        }

                        Download.Visibility = Visibility.Hidden;
                        bhd.Visibility = Visibility.Hidden;
                        bbest.Visibility = Visibility.Hidden;

                        PDownload.Visibility = Visibility.Visible;

                        PreviewURLdoZamiany = JsonConvert.DeserializeObject<Dictionary<string, string>>(result).Values.ToArray()[0];

                        previewurl = PreviewURLdoZamiany;

                        preview.Visibility = Visibility.Visible;

                        preview.Source = new Uri(previewurl);
                        preview.Volume = 0;

                        preview.Height = 500;
                        preview.Width = 1000;

                        bplay.Visibility = Visibility.Visible;
                        bpause.Visibility = Visibility.Visible;
                        volumeSlider.Visibility = Visibility.Visible;

                        preview.Play();

                        return JsonConvert.DeserializeObject<Dictionary<string, string>>(result).Values.ToArray()[0];
                    }


                    using (var ApiPobieranie = new WebClient())
                    {

                        string kox = await OtrzymanieURL();

                        Uri uri = new Uri(kox);

                        SaveFileDialog dialog = new SaveFileDialog();
                        dialog.Filter = "MP4 Video (*.mp4)|*.mp4|Wszystkie pliki (*.*)|*.*";

                        var result1 = dialog.ShowDialog();

                        ApiPobieranie.DownloadProgressChanged += (s, e) => { PDownload.Value = e.ProgressPercentage; };

                        if (result1 == true)
                        {

                            ApiPobieranie.DownloadFileAsync(uri, dialog.FileName);

                            ApiPobieranie.DownloadFileCompleted += ApiPobieranie_DownloadFileCompleted;
                        }
                        else
                        {
                            PDownload.Visibility = Visibility.Hidden;
                            Download.Visibility = Visibility.Visible;

                            //preview.Width = 0;
                            //preview.Height = 0;

                            preview.Visibility = Visibility.Hidden;

                            bplay.Visibility = Visibility.Hidden;
                            bpause.Visibility = Visibility.Hidden;
                            volumeSlider.Visibility = Visibility.Hidden;

                            preview.Close();
                        }
                        //ApiPobieranie.DownloadFileCompleted += ApiPobieranie_DownloadFileCompleted;



                    }


                }
            }
            else
            {
                MessageBox.Show("Wprowadź poprawny URL", "Ostrzeżenie", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
    }

}