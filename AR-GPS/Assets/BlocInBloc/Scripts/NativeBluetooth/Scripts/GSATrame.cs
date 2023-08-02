using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BlocInBloc.Trame {
    
    public struct GSATrame {

        public readonly TRAMETYPE trametype;
        public readonly string mode;
        public readonly string dimension;
        public readonly double PDOP;
        public readonly double HDOP;
        public readonly double VDOP;

        public GSATrame (string trame) {
            string[] tokens = trame.Split (',');
            if (!tokens[0].Contains ("GSA")) {
                throw new InvalidDataException ("trame is not well formed - no GSA in first talker");
            }
            //if (tokens.Length != 24) {
            //    throw new InvalidDataException ("trame is not well formed - not enough characters");
            //}

            trametype = TRAMETYPE.GSA;
            mode = tokens[1];
            dimension = tokens[2];
            PDOP = tokens[tokens.Length - 3] == "" ? 0 :Convert.ToDouble(tokens[tokens.Length - 3]);
            HDOP = tokens[tokens.Length - 2] == "" ? 0 :Convert.ToDouble(tokens[tokens.Length - 2]);
            string lastValue = Regex.Match (tokens[11], @"/^.[^\*]*").Value;
            VDOP = lastValue == "" ? 0 :Convert.ToDouble(lastValue);
        }

        public override string ToString () {
            return 
                "\n mode : " + mode +
                "\n dimension : " + dimension +
                "\n PDOP : " + PDOP +
                "\n HDOP : " + HDOP +
                "\n VDOP : " + VDOP
                ;
        }
    }
}