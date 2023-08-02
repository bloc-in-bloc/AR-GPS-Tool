using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BlocInBloc.Trame {
    public class GGATrame {
        public readonly TRAMETYPE trametype;
        public readonly string utcTime;
        public double longitude { get; set; }
        public readonly string longitudeUnits;
        public double latitude { get; set; }
        public readonly string latitudeUnits;
        public readonly int positionQuality;
        public readonly int numberOfSatellites;
        public readonly double HDOP;
        public double altitude { get; set; }

        public readonly string altitudeUnits;
        public readonly double geodialSeparation;
        public readonly string geodialSeparationUnits;
        public readonly double diffTimeGNSS;
        public readonly string baseStationID;

        public double coordinatesQuality { get; set; } = -1;

        public GGATrame (string[] tokens) {
            if (tokens.Length != 15 && tokens[0] != "$GPGGA") {
                throw new InvalidDataException ("trame is not well formed");
            }

            trametype = TRAMETYPE.GGA;
            utcTime = tokens[1];
            latitude = tokens[2] == "" ? 0 : Convert.ToDouble (tokens[2]);
            latitudeUnits = tokens[3];
            longitude = tokens[4] == "" ? 0 : Convert.ToDouble (tokens[4]);
            longitudeUnits = tokens[5];
            positionQuality = Convert.ToInt32 (tokens[6]);
            numberOfSatellites = Convert.ToInt32 (tokens[7]);
            HDOP = tokens[8] == "" ? 0 : Convert.ToDouble (tokens[8]);
            altitude = tokens[9] == "" ? 0 : Convert.ToDouble (tokens[9]);
            altitudeUnits = tokens[10];
            geodialSeparation = tokens[11] == "" ? 0 : Convert.ToDouble (tokens[11]);
            geodialSeparationUnits = tokens[12];
            diffTimeGNSS = tokens[13] == "" ? 0 : Convert.ToDouble (tokens[13]);
            baseStationID = tokens[14];

            longitude = ConvertDMSToDD (longitude, longitudeUnits);
            latitude = ConvertDMSToDD (latitude, latitudeUnits);
        }

        private double ConvertDMSToDD (double value, string unit) {
            bool positive = unit is "N" or "E";
            int degree = Mathf.FloorToInt (Convert.ToSingle (value / 100f));
            value = value - degree * 100f;
            int minutes = Mathf.FloorToInt (Convert.ToSingle (value));
            value = value - minutes;
            double secondes = value * 60;
            return (degree + minutes / 60f + secondes / 3600f) * (positive ? 1 : -1);
        }

        public override string ToString () {
            return
                "\n UTC TIME : " + utcTime +
                "\n LONGITUDE : " + longitude +
                "\n LONGITUDE UNTIS : " + longitudeUnits +
                "\n LATITUDE : " + latitude +
                "\n LATITUDE UNITS : " + latitudeUnits +
                "\n POSITION QUALITY : " + positionQuality +
                "\n SATELLITES NUMBER : " + numberOfSatellites +
                "\n HDOP : " + HDOP +
                "\n ALTITUDE : " + altitude +
                "\n ALTITUDE UNITS : " + altitudeUnits +
                "\n GEODIAL SEPARATION : " + geodialSeparation +
                "\n GEODIAL SEPARATION UNITS: " + geodialSeparationUnits +
                "\n DIFF TIME : " + diffTimeGNSS +
                "\n BASE STATION ID : " + baseStationID
                ;
        }

        public bool CheckAccuracy (double minimumAccuracyToStart) {
            return true;
        }
    }
}