using System;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using System.IO;


namespace GeneradorDePoligonos
{
    class Program
    {
        static void Main(string[] args)
        {


            var puntosUTM = JsonConvert.DeserializeObject<Collection<PuntoUtm>>(File.ReadAllText(args[0]));
            var poligono = string.Empty;

            //double AjusteLon = 0.008507294;
            //double AjusteLat = -0.009599376

            double AjusteLon = -0.000346706;
            double AjusteLat = -0.000666376;

            foreach (var puntoUtm in puntosUTM)
            {
                var puntoGps = UtmToLatLon(puntoUtm.X, puntoUtm.Y, puntoUtm.UtmZone);
                poligono = $"{poligono},{puntoGps.Longitud- AjusteLon},{puntoGps.Latitud- AjusteLat},0";
            }

            var puntoCierre = UtmToLatLon(puntosUTM[0].X, puntosUTM[0].Y, puntosUTM[0].UtmZone);

            poligono = $"{poligono},{puntoCierre.Longitud - AjusteLon},{puntoCierre.Latitud - AjusteLat},0";

            Console.WriteLine(poligono);



        }


        public static PuntoGps UtmToLatLon(double utmX, double utmY, string utmZone)
        {
            var isNorthHemisphere = utmZone.Last() >= 'N';

            const double diflat = -0.00066286966871111111111111111111111111;
            const double diflon = -0.0003868060578;

            var zone = int.Parse(utmZone.Remove(utmZone.Length - 1));
            const double cSa = 6378137.000000;
            const double cSb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(cSa, 2) - Math.Pow(cSb, 2)), 0.5) / cSb;
            var e2Cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(cSa, 2) / cSb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (cSa * 0.9996);
            var v = (c / Math.Pow(1 + (e2Cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2Cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2Cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));
            var puntoGps = new PuntoGps();

            puntoGps.Longitud = ((delt * (180.0 / Math.PI)) + s) + diflon;
            puntoGps.Latitud = ((lat + (1 + e2Cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2Cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;


            return puntoGps;
        }


        

    }
}
