using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BlocInBloc.Trame {
    public struct LLQTrame {
        public readonly TRAMETYPE trametype;
        public readonly string utcTime;
        public readonly string utcDate;
        public readonly double longitude;
        public readonly string longitudeUnits;
        public readonly double latitude;
        public readonly string latitudeUnits;
        public readonly int positionQuality;
        public readonly int numberOfSatellites;
        public readonly double coordinatesQuality;
        public readonly double altitude;
        public readonly string altitudeUnits;
        public readonly GPSQuality gpsQuality;

        public Vector3d position => new Vector3d (longitude, latitude, altitude);

        public LLQTrame (string[] tokens) {
            if (tokens.Length != 12 || !tokens[0].Contains ("LLQ")) {
                throw new InvalidDataException ("trame is not well formed");
            }
            trametype = TRAMETYPE.LLQ;
            utcTime = tokens[1];
            utcDate = tokens[2];
            longitude = tokens[3] == "" ? 0 : Convert.ToDouble (tokens[3]);
            longitudeUnits = tokens[4];
            latitude = tokens[5] == "" ? 0 : Convert.ToDouble (tokens[5]);
            latitudeUnits = tokens[6];
            positionQuality = Convert.ToInt32 (tokens[7]);
            numberOfSatellites = Convert.ToInt32 (tokens[8]);
            coordinatesQuality = tokens[9] == "" ? 0 : Convert.ToDouble (tokens[9]);
            altitude = tokens[10] == "" ? 0 : Convert.ToDouble (tokens[10]);
            altitudeUnits = Regex.Match (tokens[11], @"/^.[^\*]*").Value;
            gpsQuality = ConvertitQuality (positionQuality);
        }

        public static GPSQuality ConvertitQuality (int quality) {
            GPSQuality gpsQuality = GPSQuality.UNKNOWN;
            try {
                if (quality == 4)
                    quality = 3;
                if (quality == 5)
                    quality = 2;

                gpsQuality = (GPSQuality) quality;
            } catch {
                Debug.LogError ("couldn't convert quality " + quality);
            }
            return gpsQuality;
        }

        public override string ToString () {
            return
                "\n UTC TIME : " + utcTime +
                "\n UTC Date : " + utcDate +
                "\n LONGITUDE : " + longitude +
                "\n LONGITUDE UNTIS : " + longitudeUnits +
                "\n LATITUDE : " + latitude +
                "\n LATITUDE UNITS : " + latitudeUnits +
                "\n POSITION QUALITY : " + positionQuality +
                "\n SATELLITES NUMBER : " + numberOfSatellites +
                "\n COORDS QUALITY : " + coordinatesQuality +
                "\n ALTITUDE : " + altitude +
                "\n ALTITUDE UNITS : " + altitudeUnits
                ;
        }
    }

    public enum TRAMETYPE {
        GGA, GGL, GSA, GSV, RMC, GGQ, LLQ, LLK, GST, VTG
    }

    public enum TRAMETALKER {
        GN, GP, GL, GA, BD
    }

    public enum GPSQuality {
        UNKNOWN = -1,
        INVALID = 0,
        NATURAL = 1,
        DGPS = 2,
        RTK = 3,
        xRTK = 10
    }
}