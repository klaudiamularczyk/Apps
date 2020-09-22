using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pos_project
{
    class Program
    {
        static string path = @"GPS_messages.gps";
        static void Main(string[] args)
        {

            ZapisNaglowka();
            int p = 7;
            var lineCount = System.IO.File.ReadLines(path).Count();

            string Latitude = String.Empty;
            string NS = string.Empty;
            string EW = string.Empty;
            string Longitude = String.Empty;
            string SeaLevel = String.Empty;
            string Time = String.Empty;
            string Date = String.Empty;
            string Speed = String.Empty;
            string Azymuth = String.Empty;

            Dane_GPS dane_GPS = new Dane_GPS(Latitude, Longitude, SeaLevel, Time, Date, Speed, Azymuth);
            for (int n = 0; n < lineCount; n = n + 7)
            {
                var lines = System.IO.File.ReadLines(path).Skip(n).Take(p).ToArray();
                foreach (var item in lines)
                {
                    string[] split = item.Split(',').ToArray();
                    string header = item.Substring(0, 6);

                    switch (header)
                    {
                        case "$GPGGA":

                            Latitude = split[2];
                            NS = split[3];
                            Longitude = split[4];
                            EW = split[5];
                            SeaLevel = split[9].Replace(".",",");
                            break;
                        case "$GPRMC":
                            Time = split[1].Substring(0, 2) + ":" + split[1].Substring(2, 2) + ":" + split[1].Substring(4, 2) + "." + split[1].Substring(7, 2);
                            Date = "20" + split[9].Substring(4, 2) + "-" + split[9].Substring(2, 2) + "-" + split[9].Substring(0, 2);
                            break;
                        case "$GPVTG":
                            Speed = split[7].Replace(".", ",");
                            Azymuth = split[1].Replace(".", ",");                         
                            break;
                        default:
                            break;
                    }              
                }
                  if (Latitude!="" && Longitude !="" && SeaLevel != "" && Time != "" && Date != "" && Azymuth != "" && Speed != "")
                     { 
                    dane_GPS.Latitude = Przeliczanie(Latitude, NS);
                    dane_GPS.Longitude = Przeliczanie(Longitude, EW);
                    dane_GPS.SeaLevel = SeaLevel;
                    dane_GPS.Time = Time;
                    dane_GPS.Date = Date;
                    dane_GPS.Azymuth = Azymuth;
                    dane_GPS.Speed = Speed;
                    ZapisTresci(dane_GPS);
                     }
            }
            ZapisKonca();
        }
        public static void ZapisNaglowka()
        {
            string path = @"Wiadomosc_gps.gpx";
            StreamWriter naglowek = new StreamWriter(path, false);
            naglowek.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?> \n" +
                " <gpx version=\"1.0\" \n" +
                "creator=\"SymDVR - https://sites.google.com/site/symdvr/en\"" +
                " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                " xmlns=\"http://www.topografix.com/GPX/1/0\"" +
                " xsi:schemaLocation=\"http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd\">" +
                " <trk><name>111111112</name>" +
                " <trkseg>");
            naglowek.Close();
        }

        public static void ZapisTresci(Dane_GPS dane_GPS)
        {
            string path = @"Wiadomosc_gps.gpx";
            StreamWriter tresc = new StreamWriter(path, true);
            tresc.WriteLine("<trkpt lat=\"{0}\" lon=\"{1}\">"+
                "<time>{2}T{3}Z</time>"+
                "<course>{4}</course>"+
                "<speed>{5}</speed>"+
                "<heading>{6}</heading> \n"+
                "</trkpt>", dane_GPS.Latitude, dane_GPS.Longitude, dane_GPS.Date, dane_GPS.Time, dane_GPS.Azymuth,dane_GPS.Speed,dane_GPS.SeaLevel);
            tresc.Close();
        }

        public static void ZapisKonca()
        {
            string path = @"Wiadomosc_gps.gpx";
            StreamWriter koncowka = new StreamWriter(path, true);
            koncowka.WriteLine("</trkseg>");
            koncowka.WriteLine("</trk>");
            koncowka.WriteLine("</gpx>");
            koncowka.Close();
        }

        public static string Przeliczanie(string wartosc, string NS)
        {
            double wartosc_ = double.Parse(wartosc, CultureInfo.InvariantCulture);
            double wartosc_stopnie = Math.Truncate(wartosc_/100);
            double wartosc_minuty = (int)((wartosc_/100 - (int)wartosc_/100) * 100);
            double wartosc_ulamek_minuty = wartosc_ - Math.Truncate(wartosc_);
            double wartosc_wynik = wartosc_stopnie + wartosc_minuty/60 + wartosc_ulamek_minuty* 60 /3600;

            if (NS == "S" || NS=="W")
            {
                wartosc_wynik = -wartosc_wynik;
            }

            string wynik= wartosc_wynik.ToString().Replace(",",".") ;
            return wynik;
        }
    }
}

