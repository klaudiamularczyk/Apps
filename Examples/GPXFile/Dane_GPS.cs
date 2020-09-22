using System;
using System.Collections.Generic;
using System.Text;

namespace pos_project
{
    public class Dane_GPS
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string SeaLevel { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
        public string Speed { get; set; }
        public string Azymuth { get; set; }


        public Dane_GPS(string Latitude, string longitude, string seaLevel, string time, string date, string speed, string azymuth)
        {
            this.Latitude = Latitude;
            this.Longitude = longitude;
            this.SeaLevel = seaLevel;
            this.Time = time;
            this.Date = date;
            this.Speed = speed;
            this.Azymuth = azymuth;

        }
    }
}
