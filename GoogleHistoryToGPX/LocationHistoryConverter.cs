using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo.Gps;

namespace GoogleHistoryToGPX
{
    public class LocationHistoryConverter
    {
        public void ConvertRecordsJSON(string path)
        {
            using (StreamReader file = File.OpenText(path))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject locations = (JObject)JToken.ReadFrom(reader);

                Track track = new Track();
                TrackSegment segment = new TrackSegment();

                foreach (JToken location in locations["locations"])
                {
                    // "timestamp": "2019-11-08T19:33:42.156Z"

                    Waypoint waypoint = new Waypoint(Convert.ToDouble(location["latitudeE7"]) / 10000000, Convert.ToDouble(location["longitudeE7"]) / 10000000);
                    waypoint.TimeUtc = DateTime.Parse(location["timestamp"].ToString());

                    //if (waypoint.TimeUtc.Value.Year == 2023 && waypoint.TimeUtc.Value.Month == 4 && waypoint.TimeUtc.Value.Day == 10)
                        segment.Waypoints.Add(waypoint);
                }

                track.Segments.Add(segment);

                GpsData gpsData = new GpsData();
                gpsData.Tracks.Add(track);

                string directory = new FileInfo(path).Directory.FullName;
                File.WriteAllText(directory + "\\aaa.gpx", gpsData.ToGpx());
            }
        }

        internal void ConvertJSON(string path)
        {
            using (StreamReader file = File.OpenText(path))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject timelineHistory = (JObject)JToken.ReadFrom(reader);

                Track track = new Track();

                foreach (JToken timelineObject in timelineHistory["timelineObjects"])
                {
                    if (timelineObject["activitySegment"] != null)
                    {
                        TrackSegment segment = new TrackSegment();

                        Waypoint startWaypoint = new Waypoint(Convert.ToDouble(timelineObject["activitySegment"]["startLocation"]["latitudeE7"]) / 10000000,
                            Convert.ToDouble(timelineObject["activitySegment"]["startLocation"]["longitudeE7"]) / 10000000);
                        segment.Waypoints.Add(startWaypoint);

                        if (timelineObject["activitySegment"] != null && timelineObject["activitySegment"]["waypointPath"] != null)
                        {
                            var points = timelineObject["activitySegment"]["waypointPath"]["waypoints"];
                            foreach (var point in points)
                            {
                                Waypoint waypoint = new Waypoint(Convert.ToDouble(point["latE7"]) / 10000000, Convert.ToDouble(point["lngE7"]) / 10000000);
                                segment.Waypoints.Add(waypoint);
                            }
                        }

                        Waypoint endWaypoint = new Waypoint(Convert.ToDouble(timelineObject["activitySegment"]["endLocation"]["latitudeE7"]) / 10000000,
                            Convert.ToDouble(timelineObject["activitySegment"]["endLocation"]["longitudeE7"]) / 10000000);
                        segment.Waypoints.Add(endWaypoint);

                        track.Segments.Add(segment);
                    }
                }

                GpsData gpsData = new GpsData();
                gpsData.Tracks.Add(track);

                string directory = new FileInfo(path).Directory.FullName;
                File.WriteAllText(directory + "\\aaa.gpx", gpsData.ToGpx());
            }

            //LocationHistory timelineHistory = JsonConvert.DeserializeObject<LocationHistory>(File.ReadAllText(path));

            //foreach (JObject timelineObject in timelineHistory.TimelineObjects)
            //{
            //    if (timelineObject.ToString().Contains("activitySegment"))
            //    {
            //        var ff = JsonConvert.DeserializeObject<ActivitySegment>(timelineObject.ToString());
            //    }
            //}
        }
    }
}
