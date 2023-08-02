using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BlocInBloc.Trame {
    
    public struct RMCTrame {

        public readonly TRAMETYPE trametype;
        public readonly string utcTime;
        public readonly string status;
        public readonly double latitude;
        public readonly string NorthOrSouth;
        public readonly double longitude;
        public readonly string WestOrEast;
        public readonly double speedOverGrounds;
        public readonly double courseOverGrounds;
        public readonly string date;
        public readonly double magneticVariation;
        public readonly string EastOrWestMagnetic;
        public readonly string modeIndicator;

        public RMCTrame (string trame) {
            string[] tokens = trame.Split (',');
            if (tokens.Length != 13 || !tokens[0].Contains ("RMC")) {
                throw new InvalidDataException ("trame is not well formed");
            }

            trametype = TRAMETYPE.RMC;
            utcTime = tokens[1];
            status = tokens[2];
            latitude = tokens[3] == "" ? 0 :Convert.ToDouble(tokens[3]);
            NorthOrSouth = tokens[4];
            longitude = tokens[5] == "" ? 0 :Convert.ToDouble(tokens[5]);
            WestOrEast = tokens[6];
            speedOverGrounds = tokens[7] == "" ? 0 :Convert.ToDouble(tokens[7]);
            courseOverGrounds = tokens[8] == "" ? 0 :Convert.ToDouble(tokens[8]);
            date = tokens[9];
            magneticVariation = tokens[10] == "" ? 0 :Convert.ToDouble(tokens[10]);
            EastOrWestMagnetic = tokens[11];
            modeIndicator = tokens[12];
        }

        public override string ToString () {
            return 
                "\n UTC TIME : " + utcTime +
                "\n STATUS : " + status +
                "\n LATITUDE : " + latitude +
                "\n LATITUDE DIRECTION : " + NorthOrSouth +
                "\n LONGITUDE : " + longitude +
                "\n LONGITUDE DIRECTION : " + WestOrEast +
                "\n SPEED OVER GROUND : " + speedOverGrounds +
                "\n COURSE OVER GROUND : " + courseOverGrounds + 
                "\n DATE : " + date + 
                "\n MAGNETIC VARIATION : " + magneticVariation + 
                "\n MAGNETIC DIRECTION : " + EastOrWestMagnetic + 
                "\n MODE INDICATOR : " + modeIndicator 
                ;
        }
    }
}