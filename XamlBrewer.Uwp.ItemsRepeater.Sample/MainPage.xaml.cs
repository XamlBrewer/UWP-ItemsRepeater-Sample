using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

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
                            PosterUrl = m.XPathSelectElement("poster/xlarge").Value,
                            TrailerUrl = m.XPathSelectElement("preview/large").Value
                        })
                        //.OrderBy(m => m.Title)
                        .ToList()
                });
            }

            GenreRepeater.ItemsSource = Genres;

            // Open the teaching tip after rendering most images.
            await Task.Delay(2000);
            ScrollTeachingTip.IsOpen = true;
        }

        private void Movie_Click(object sender, RoutedEventArgs e)
        {
            FlyoutShowOptions options = new FlyoutShowOptions();
            options.ShowMode = FlyoutShowMode.Standard;
            options.Placement = FlyoutPlacementMode.Top;

            MovieCommands.ShowAt(sender as FrameworkElement, options);
        }

        private async void Element_Click(object sender, RoutedEventArgs e)
        {
            // It stays on top of the dialog.
            MovieCommands.Hide();

            var movie = (sender as FrameworkElement)?.DataContext as Movie;
            var source = MediaSource.CreateFromUri(new Uri(movie.TrailerUrl));

            TitleText.Text = movie.Title;
            Player.Source = source;
            await MediaPlayerDialog.ShowAsync();
        }

        private void MediaPlayerDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            // Prevent the player to continue playing.
            Player.Source = null;
        }
    }
}
