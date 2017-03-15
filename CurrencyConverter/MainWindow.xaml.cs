using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace CurrencyConverter
{
    public partial class MainWindow
    {
        public static readonly DependencyProperty IsAlwaysOnTopProperty =
            DependencyProperty.Register
            ("IsAlwaysOnTop", typeof(bool), typeof(MainWindow),
            new PropertyMetadata(false));

        private bool m_isConverting;

        public bool IsAlwaysOnTop
        {
            get { return (bool)GetValue(IsAlwaysOnTopProperty); }
            set { SetValue(IsAlwaysOnTopProperty, value); }
        }

        public MainWindow()
        {
            DataContext = this;

            InitializeComponent();

            this.Left = Properties.Settings.Default.Left;
            this.Top = Properties.Settings.Default.Top;

            Loaded += (sender, args) => LoadPreferences();

            FetchCurrencyRates().Wait();
        }

        public class CurrencyRateResponse
        {
            public string to { get; set; }
            public double rate { get; set; }
            public string from { get; set; }
        }

        private async Task FetchCurrencyRates()
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var content = await httpClient.GetStringAsync("http://rate-exchange-1.appspot.com/currency?from=ISK&to=EUR").ConfigureAwait(false);
                    var response = JsonConvert.DeserializeObject<CurrencyRateResponse>(content);
                    Properties.Settings.Default.IskToEur = response.rate;


                    content = await httpClient.GetStringAsync("http://rate-exchange-1.appspot.com/currency?from=ISK&to=CAD").ConfigureAwait(false);
                    response = JsonConvert.DeserializeObject<CurrencyRateResponse>(content);
                    Properties.Settings.Default.IsktoCad = response.rate;


                    content = await httpClient.GetStringAsync("http://rate-exchange-1.appspot.com/currency?from=EUR&to=CAD").ConfigureAwait(false);
                    response = JsonConvert.DeserializeObject<CurrencyRateResponse>(content);
                    Properties.Settings.Default.EurToCad = response.rate;
                }
                catch (Exception ex) { }
            }
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            SavePreferences();
        }

        private void SavePreferences()
        {
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.IsAlwaysOnTop = IsAlwaysOnTop;

            Properties.Settings.Default.Save();
        }


        private void LoadPreferences()
        {
            IsAlwaysOnTop = Properties.Settings.Default.IsAlwaysOnTop;
        }

        private void OnIskChanged(object sender, TextChangedEventArgs e)
        {
            if (m_isConverting)
                return;

            m_isConverting = true;

            double isk;
            if (double.TryParse(m_isk.Text, out isk))
            {
                m_cad.Text = (isk * Properties.Settings.Default.IsktoCad).ToString("N2");
                m_eur.Text = (isk * Properties.Settings.Default.IskToEur).ToString("N2");
            }
            else
            {
                m_cad.Text = "-";
                m_eur.Text = "-";
            }

            m_isConverting = false;
        }

        private void OnEurChanged(object sender, TextChangedEventArgs e)
        {
            if (m_isConverting)
                return;

            m_isConverting = true;

            double eur;
            if (double.TryParse(m_eur.Text, out eur))
            {
                m_cad.Text = (eur * Properties.Settings.Default.EurToCad).ToString("N2");
                m_isk.Text = (eur * 1 / Properties.Settings.Default.IskToEur).ToString("N2");
            }
            else
            {
                m_cad.Text = "-";
                m_eur.Text = "-";
            }

            m_isConverting = false;
        }

        private void OnCadChanged(object sender, TextChangedEventArgs e)
        {
            if (m_isConverting)
                return;

            m_isConverting = true;

            double cad;
            if (double.TryParse(m_isk.Text, out cad))
            {
                m_isk.Text = (cad * 1 / Properties.Settings.Default.IsktoCad).ToString("N2");
                m_eur.Text = (cad * 1 / Properties.Settings.Default.EurToCad).ToString("N2");
            }
            else
            {
                m_cad.Text = "-";
                m_eur.Text = "-";
            }

            m_isConverting = false;
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
