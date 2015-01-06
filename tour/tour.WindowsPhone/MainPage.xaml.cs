using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Storage.Streams;
using Windows.UI;
using MappingUtilities;
using tour.Geofencing;
using Windows.Services.Maps;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Microsoft.WindowsAzure.MobileServices;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace tour
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Geolocator geolocator;
        Geopoint currentGeoPoint = null;
        CancellationTokenSource cts;
        private int radius = 50;
        private TimeSpan dwellTime = TimeSpan.FromSeconds(0);
        Dictionary<String, List<Geopoint>> tourCollection = new Dictionary<String, List<Geopoint>>();
        
        private List<Fence> fences = new List<Fence>();
        private IMobileServiceTable<Tour> tourTable = App.MobileService.GetTable<Tour>();
        private IMobileServiceTable<Fence> fenceTable = App.MobileService.GetTable<Fence>();
        String selectedTour = null;


        public MainPage()
        {
           
           
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
           
           
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
          

            GeofenceMonitor.Current.GeofenceStateChanged += OnGeofenceStateChanged;
          
           
           
            if (e.Parameter.ToString() == "")
            {
                cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;
                if (geolocator == null)
                {
                    geolocator = new Geolocator
                    {
                        DesiredAccuracy = PositionAccuracy.High,
                        MovementThreshold = 1
                    };

                   
                }

              
                var location = await geolocator.GetGeopositionAsync().AsTask();
              
                Geopoint geopoint = new Geopoint(new BasicGeoposition()
                {
                    Latitude = location.Coordinate.Point.Position.Latitude,
                    Longitude = location.Coordinate.Point.Position.Longitude
                });

                Geopoint geopoint2 = new Geopoint(new BasicGeoposition()
                {
                    Latitude = 53.2737969,
                    Longitude = -9.051779899999929
                });

                if (location.Coordinate.Point.Position.Latitude.ToString() == "47.6785619")
                {
                   

                    MapControl1.Center = geopoint2;
                }
                else
                {
                    MapControl1.Center = geopoint;
                }

                MapControl1.ZoomLevel = 16;
               
               
                
            }
            else
            {
                showToast(e.Parameter.ToString());
                number = 0;
                geofenceID = 0;
                GeofenceMonitor.Current.Geofences.Clear();
                selectedTour = e.Parameter.ToString();
                selectedTourTxt.Text = selectedTour;
                RemoveDrawFence();              
                CreateAllTourGeofences(e.Parameter.ToString());  
               

            }
            
        }

        private async void GeolocatorPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                GeolocatorPositionChanged(args.Position); // calls back to not shared portion
            });
        }
        private void GeolocatorPositionChanged(Geoposition point)
        {
         
            var pos = new Geopoint(new BasicGeoposition { Latitude = point.Coordinate.Point.Position.Latitude, Longitude = point.Coordinate.Point.Position.Longitude });

            DrawCarIcon(pos);           
            MapControl1.TrySetViewAsync(pos, MapControl1.ZoomLevel, MapControl1.Heading, MapControl1.Pitch, MapAnimationKind.Linear);
        }
       
        

        public async void OnGeofenceStateChanged(GeofenceMonitor sender, object e)
        {
            var fences = await fenceTable.Where(f => f.TourName == selectedTour).ToListAsync();
          
            if (sender.Geofences.Any() && fences.Count()!=0)
            {
                var reports = sender.ReadReports();
                foreach (var report in reports)
                {
                    switch (report.NewState)
                    {
                        case GeofenceState.Entered:
                            {
                                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    int fenceID = Int32.Parse(report.Geofence.Id)-1;
                                   
                                        showToast(fences[fenceID].FenceDescription);
                                 

                                    
                                });
                                break;
                            }
                        //case GeofenceState.Exited:
                        //    {
                        //        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        //        {
                        //            showToast("exited");
                        //        });
                        //        break;
                        //    }
                    }
                }
            }
           }


        public void showToast(string message)
        {

            ToastTemplateType toastType = ToastTemplateType.ToastText02;

            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastType);

            XmlNodeList toastTextElement = toastXml.GetElementsByTagName("text");
            toastTextElement[0].AppendChild(toastXml.CreateTextNode(message));
           toastTextElement[1].AppendChild(toastXml.CreateTextNode(message));

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "short");

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast); 
        }

        const int zind = 4;
        private MapPolygon DrawOneGeofence(Geopoint point)
        {
           
            var color = Colors.DarkBlue;
            color.A = 70;
            var pointlist = GeopointExtensions.GetCirclePoints(point, radius);
        

            MapPolygon shape = new MapPolygon
            {
                FillColor = color,
                StrokeColor = color,
                Path = new Geopath(pointlist.Select(p => p.Position)),
                ZIndex = zind
            };
            MapControl1.MapElements.Add(shape);
            DrawIcon(point);
            return shape;

        }

        
        const int carZIndewxz = 5;
        int number = 0;
        private void DrawIcon(Geopoint pos)
        {
            number += 1;
           
            MapIcon carIcon = new MapIcon
            {
                NormalizedAnchorPoint = new Point(0.5, 0.5),
                ZIndex = carZIndewxz
            };
            carIcon.Location = pos;
            carIcon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Numbers-" + number.ToString() + "-icon.png"));
            MapControl1.MapElements.Add(carIcon);
           

        }

        const int carIndex = 6;
        private void DrawCarIcon(Geopoint pos)
        {
            var carIcon = MapControl1.MapElements.OfType<MapIcon>().FirstOrDefault(p => p.ZIndex == carIndex);
            if (carIcon == null)
            {
                carIcon = new MapIcon
                {
                    NormalizedAnchorPoint = new Point(0.5, 0.5),
                    ZIndex = carIndex
                };
                carIcon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/person.png"));
                MapControl1.MapElements.Add(carIcon);
            }
            carIcon.Location = pos;

        }
          


        
        private void MapControl1_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            if (selectedTour == null)
            {
                MessageBox("Create Tour First, Please");

            }
            else if (fences.Count()!=9)
            {
                Geopoint geopoint = new Geopoint(args.Location.Position);
                DrawOneGeofence(geopoint);
                currentGeoPoint = geopoint;
                FenceGrid.Visibility = Visibility.Visible;
              
              
            }
            else
            {
                MessageBox("Maximum Number of Fences Added");
            }
        }
        private void AddFence(object sender, RoutedEventArgs e)
        {
            Fence fence = new Fence()
            {
               
                FenceName = FenceName.Text,
                FenceDescription = FenceDescription.Text,
                Latitude = currentGeoPoint.Position.Latitude,
                Longitude = currentGeoPoint.Position.Longitude,
                TourName=selectedTour
            };
            fences.Add(fence);
            FenceGrid.Visibility = Visibility.Collapsed;

        }
        
      
        private async void MessageBox(string message)
        {
            var dialog = new MessageDialog(message.ToString());
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => await dialog.ShowAsync());
        }

      
        private async void CreateAllTourGeofences(string selectedTour)
        {
            int count = 0;
           
            var fences = await fenceTable.Where(f => f.TourName == selectedTour).ToListAsync();
          
            foreach (var fence in fences)
            {
               
                Geopoint geopoint = new Geopoint(new BasicGeoposition()
                {
                    Latitude = fence.Latitude,
                    Longitude = fence.Longitude
                });
                if (count == 0)
                {
                    
                    MapControl1.Center = geopoint;
                }

                var shape = DrawOneGeofence(geopoint);
                MapControl1.MapElements.Add(shape);
               
                CreateGeofence(geopoint);
                count++;

            }
        }

        private List<Geopoint> GetAllFences(string selectedTour)
        {

            List<Geopoint> tourCol = tourCollection[selectedTour];
            return tourCol;
        }


        int geofenceID = 0;
        private void CreateGeofence(Geopoint point)
        {
            geofenceID += 1;
          
            //DrawCarIcon(point);
            // Sets the center of the Geofence.
            var position = new BasicGeoposition
            {
                Latitude = point.Position.Latitude,
                Longitude = point.Position.Longitude,

            };

            // The Geofence is a circular area centered at (latitude, longitude) point, with the
            // radius in meter.
            var geocircle = new Geocircle(position, radius);

            // Sets the events that we want to handle: in this case, the entrace and the exit
            // from an area of intereset.
            var mask = MonitoredGeofenceStates.Entered | MonitoredGeofenceStates.Exited;

            // Specifies for how much time the user must have entered/exited the area before 
            // receiving the notification.
            

            // Creates the Geofence and adds it to the GeofenceMonitor.
            var geofence = new Geofence(geofenceID.ToString(), geocircle, mask, false, dwellTime);
           
            GeofenceMonitor.Current.Geofences.Add(geofence);
        }

        private void ZoomOut(object sender, RoutedEventArgs e)
        {

        }

        private void CreateTour(object sender, RoutedEventArgs e)
        {
            GeofenceMonitor.Current.Geofences.Clear();
            RemoveDrawFence();
            number = 0;

            TourGrid.Visibility = Visibility.Visible;
        }


        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (MapControl1 != null)
                MapControl1.ZoomLevel = e.NewValue;
        }
        
        private async void DrawTour(string selected)
        {

            var fences = await fenceTable.Where(f => f.TourName == selected).ToListAsync();
          
           
            foreach (var fence in fences)
            {
               
                Geopoint geopoint = new Geopoint(new BasicGeoposition()
                {
                    Latitude = fence.Latitude,
                    Longitude = fence.Longitude
                });
                 var shape = DrawOneGeofence(geopoint);

                MapControl1.MapElements.Add(shape);
                
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

         
                string tour = TourName.Text;
                List<Geopoint> tourGeopoints = new List<Geopoint>();
                tourCollection.Add(tour, tourGeopoints);
                selectedTourTxt.Text = tour;
                selectedTour = tour;
                TourGrid.Visibility = Visibility.Collapsed;
                NewTour.Visibility = Visibility.Collapsed;
                SaveTourBtn.Visibility = Visibility.Visible;        


        }
        private void RemoveDrawFence()
        {
            var routeFences = MapControl1.MapElements.Where(p => p.ZIndex == carZIndewxz).ToList();
            foreach (var fence in routeFences)
            {
                MapControl1.MapElements.Remove(fence);
            }
            var routeFences2 = MapControl1.MapElements.Where(p => p.ZIndex == zind).ToList();
            foreach (var fence in routeFences2)
            {
                MapControl1.MapElements.Remove(fence);
            }
        }

        private void SelectTour(object sender, RoutedEventArgs e)
        {
            geolocator.PositionChanged += GeolocatorPositionChanged;
            Frame.Navigate(typeof(TourList));
        }
       
        private async void SaveTour(object sender, RoutedEventArgs e)
        {

            Tour tour = new Tour() { TourName=selectedTour, TourDescription="Some Tour" };
            foreach (var fence in fences)
            {
                await fenceTable.InsertAsync(fence);
            }           
            await tourTable.InsertAsync(tour);
            SaveTourBtn.Visibility = Visibility.Collapsed;
            NewTour.Visibility = Visibility.Visible;

        }

        private void Settings(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Visibility = Visibility.Visible;
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            if (RadiusTxt.Text == "" )
            {
                showToast("Please Set Radius in meters ");
            }
            else if(DwellTimeTxt.Text == "")
            {
                 showToast("Please SetDwell Time in ms");         

            }
           
            else
            {
                radius = Int32.Parse(RadiusTxt.Text);
                dwellTime = TimeSpan.Parse(DwellTimeTxt.Text);
                SettingsGrid.Visibility = Visibility.Collapsed;
            }       
           


        }

       

       
        
    }
}
