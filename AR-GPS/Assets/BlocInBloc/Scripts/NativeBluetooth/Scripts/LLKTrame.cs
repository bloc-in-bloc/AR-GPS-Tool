using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BlocInBloc.Trame {
    
    public struct LLKTrame {

        public readonly TRAMETYPE trametype;
        public readonly string utcTime;
        public readonly string utcDate;
        public readonly double longitude;
        public readonly string longitudeUnits;
        public readonly double latitude;
        public readonly string latitudeUnits;
        public readonly int positionQuality;
        public readonly int numberOfSatellites;
        public readonly double GDOP;
        public readonly double altitude;
        public readonly string altitudeUnits;
        
        public LLKTrame (string trame) {
            string[] tokens = trame.Split (',');
            if (tokens.Length != 12 || !tokens[0].Contains ("LLK")) {
                throw new InvalidDataException ("trame is not well formed");
            }

            trametype = TRAMETYPE.LLK;
            utcTime = tokens[1];
            utcDate = tokens[2];
            longitude = tokens[3] == "" ? 0 :Convert.ToDouble(tokens[3]);
            longitudeUnits = tokens[4];
            latitude = tokens[5] == "" ? 0 :Convert.ToDouble(tokens[5]);
            latitudeUnits = tokens[6];
            positionQuality = Convert.ToInt32(tokens[7]);
            numberOfSatellites = Convert.ToInt32(tokens[8]);
            GDOP = tokens[9] == "" ? 0 :Convert.ToDouble(tokens[9]);
            altitude = tokens[10] == "" ? 0 :Convert.ToDouble(tokens[10]);
            altitudeUnits = Regex.Match (tokens[11], @"/^.[^\*]*").Value;
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
                "\n GDOP : " + GDOP +
                "\n ALTITUDE : " + altitude +
                "\n ALTITUDE UNITS : " + altitudeUnits
                ;
        }
    }
}