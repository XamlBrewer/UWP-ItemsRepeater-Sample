using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XamlBrewer.Uwp.ItemsRepeater.Sample
{
    public sealed partial class MainPage : Page
    {
        private List<Genre> _genres = new List<Genre>();

        public List<Genre> Genres => _genres;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            string xml;

            using (var client = new HttpClient())
            {
                xml = await client.GetStringAsync("http://trailers.apple.com/trailers/home/xml/current.xml");
            }

            var movies = XDocument.Parse(xml);

            var genreNames = movies.XPathSelectElements("//genre/name")
                          .Select(m => m.Value)
                          .OrderBy(m => m)
                          .Distinct()
                          .ToList();

            foreach (var genreName in genreNames)
            {
                _genres.Add(new Genre()
                {
                    Name = genreName,
                    Movies = movies.XPathSelectElements("//genre[name='" + genreName + "']")
                        .Ancestors("movieinfo")
                        .Select(m => new Movie()
                        {
                            Title = m.XPathSelectElement("info/title").Value,
                            PosterUrl = m.XPathSelectElement("poster/xlarge").Value
                        })
                        //.OrderBy(m => m.Title)
                        .ToList()
                });
            }

            GenreRepeater.ItemsSource = Genres;
        }

        private void Movie_Click(object sender, RoutedEventArgs e)
        {
            var movie = (sender as FrameworkElement)?.DataContext as Movie;
        }
    }
}
