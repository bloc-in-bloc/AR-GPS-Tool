using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BlocInBloc.Trame {
    public class GSTTrame {
        public readonly TRAMETYPE trametype;
        public readonly string utcTime;
        public readonly double pseudoRangeResidualRMS;
        public readonly double errorEllipseSemiMajor;
        public readonly double errorEllipseSemiMinor;
        public readonly double errorEllipseOrientation;
        public readonly double latitudeError;
        public readonly double longitudeError;
        public readonly double altitudeError;

        public GSTTrame (string[] tokens) {
            if (tokens.Length < 9 && tokens[0] != "$GPGST") {
                throw new InvalidDataException ("trame is not well formed");
            }
            // 0- $GPGST
            // 1- 142510.00
            // 2- 17
            // 3- 2.1
            // 4- 0.67
            // 5- 48
            // 6- 0.62
            // 7- 0.67
            // 8- 0.99*59
            trametype = TRAMETYPE.GST;
            utcTime = tokens[1];
            pseudoRangeResidualRMS = tokens[2] == "" ? 0 : Convert.ToDouble (tokens[2]);
            errorEllipseSemiMajor = tokens[3] == "" ? 0 : Convert.ToDouble (tokens[3]);
            errorEllipseSemiMinor = tokens[4] == "" ? 0 : Convert.ToDouble (tokens[4]);
            errorEllipseOrientation = tokens[5] == "" ? 0 : Convert.ToDouble (tokens[5]);
            latitudeError = tokens[6] == "" ? 0 : Convert.ToDouble (tokens[6]);
            longitudeError = tokens[7] == "" ? 0 : Convert.ToDouble (tokens[7]);
            altitudeError = tokens[8] == "" ? 0 : Convert.ToDouble (tokens[8]);
        }

        public override string ToString () {
            return
                "\n UTC TIME : " + utcTime +
                "\n PSEUDO RANGE RESIDUAL RMS : " + pseudoRangeResidualRMS +
                "\n ERROR ELLIPSE SEMI MAJPR : " + errorEllipseSemiMajor +
                "\n ERROR ELLIPSE SEMI MINOR : " + errorEllipseSemiMinor +
                "\n ERROR ELLIPSE ORIENTATION : " + errorEllipseOrientation +
                "\n LATITUDE ERROR : " + latitudeError +
                "\n LONGITUDE ERROR : " + longitudeError +
                "\n ALTITUDE : " + altitudeError
                ;
        }
    }
}