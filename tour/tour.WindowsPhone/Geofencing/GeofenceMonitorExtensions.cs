﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;

namespace tour.Geofencing
{
    public static class GeofenceMonitorExtensions
    {
        public static IList<IList<Geopoint>> GetFenceGeometries(this GeofenceMonitor monitor)
        {
            return monitor.Geofences.Select(p => p.ToCirclePoints()).ToList();
        }
    }
}
