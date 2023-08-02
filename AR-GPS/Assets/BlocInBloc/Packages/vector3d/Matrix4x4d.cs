using System;
using System.Globalization;
using UnityEngine;

// A standard 4x4d transformation matrix.
// Inspired by https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Math/Matrix4x4.cs
public struct Matrix4x4d {
    // memory layout:
    //
    //                row no (=vertical)
    //               |  0   1   2   3
    //            ---+----------------
    //            0  | m00 m10 m20 m30
    // column no  1  | m01 m11 m21 m31
    // (=horiz)   2  | m02 m12 m22 m32
    //            3  | m03 m13 m23 m33

    public double m00;
    public double m10;
    public double m20;
    public double m30;

    public double m01;
    public double m11;
    public double m21;
    public double m31;

    public double m02;
    public double m12;
    public double m22;
    public double m32;

    public double m03;
    public double m13;
    public double m23;
    public double m33;

    public Matrix4x4d (params double[] values) {
        this.m00 = values[0];
        this.m01 = values[1];
        this.m02 = values[2];
        this.m03 = values[3];
        this.m10 = values[4];
        this.m11 = values[5];
        this.m12 = values[6];
        this.m13 = values[7];
        this.m20 = values[8];
        this.m21 = values[9];
        this.m22 = values[10];
        this.m23 = values[11];
        this.m30 = values[12];
        this.m31 = values[13];
        this.m32 = values[14];
        this.m33 = values[15];
    }

    public Matrix4x4d (Matrix4x4 matrix) {
        this.m00 = matrix.m00;
        this.m01 = matrix.m01;
        this.m02 = matrix.m02;
        this.m03 = matrix.m03;
        this.m10 = matrix.m10;
        this.m11 = matrix.m11;
        this.m12 = matrix.m12;
        this.m13 = matrix.m13;
        this.m20 = matrix.m20;
        this.m21 = matrix.m21;
        this.m22 = matrix.m22;
        this.m23 = matrix.m23;
        this.m30 = matrix.m30;
        this.m31 = matrix.m31;
        this.m32 = matrix.m32;
        this.m33 = matrix.m33;
    }

    // Access element at [row, column].
    public double this[int row, int column] {
        get {
            return this[row + column * 4];
        }

        set {
            this[row + column * 4] = value;
        }
    }

    // Access element at sequential index (0..15 inclusive).
    public double this[int index] {
        get {
            switch (index) {
                case 0:
                    return m00;
                case 1:
                    return m10;
                case 2:
                    return m20;
                case 3:
                    return m30;
                case 4:
                    return m01;
                case 5:
                    return m11;
                case 6:
                    return m21;
                case 7:
                    return m31;
                case 8:
                    return m02;
                case 9:
                    return m12;
                case 10:
                    return m22;
                case 11:
                    return m32;
                case 12:
                    return m03;
                case 13:
                    return m13;
                case 14:
                    return m23;
                case 15:
                    return m33;
                default:
                    throw new IndexOutOfRangeException ("Invalid matrix index!");
            }
        }
        set {
            switch (index) {
                case 0:
                    m00 = value;
                    break;
                case 1:
                    m10 = value;
                    break;
                case 2:
                    m20 = value;
                    break;
                case 3:
                    m30 = value;
                    break;
                case 4:
                    m01 = value;
                    break;
                case 5:
                    m11 = value;
                    break;
                case 6:
                    m21 = value;
                    break;
                case 7:
                    m31 = value;
                    break;
                case 8:
                    m02 = value;
                    break;
                case 9:
                    m12 = value;
                    break;
                case 10:
                    m22 = value;
                    break;
                case 11:
                    m32 = value;
                    break;
                case 12:
                    m03 = value;
                    break;
                case 13:
                    m13 = value;
                    break;
                case 14:
                    m23 = value;
                    break;
                case 15:
                    m33 = value;
                    break;

                default:
                    throw new IndexOutOfRangeException ("Invalid matrix index!");
            }
        }
    }

    // Transforms a position by this matrix, with a perspective divide. (generic)
    public Vector3d MultiplyPoint (Vector3d point) {
        Vector3d res = new Vector3d ();
        double w;
        res.x = this.m00 * point.x + this.m01 * point.y + this.m02 * point.z + this.m03;
        res.y = this.m10 * point.x + this.m11 * point.y + this.m12 * point.z + this.m13;
        res.z = this.m20 * point.x + this.m21 * point.y + this.m22 * point.z + this.m23;
        w = this.m30 * point.x + this.m31 * point.y + this.m32 * point.z + this.m33;

        w = 1F / w;
        res.x *= w;
        res.y *= w;
        res.z *= w;
        return res;
    }

    // Transforms a position by this matrix, without a perspective divide. (fast)
    public Vector3d MultiplyPoint3x4 (Vector3d point) {
        return new Vector3d () {
            x = this.m00 * point.x + this.m01 * point.y + this.m02 * point.z + this.m03,
            y = this.m10 * point.x + this.m11 * point.y + this.m12 * point.z + this.m13,
            z = this.m20 * point.x + this.m21 * point.y + this.m22 * point.z + this.m23
        };
    }

    // Transforms a direction by this matrix.
    public Vector3d MultiplyVector (Vector3d vector) {
        return new Vector3d () {
            x = this.m00 * vector.x + this.m01 * vector.y + this.m02 * vector.z,
            y = this.m10 * vector.x + this.m11 * vector.y + this.m12 * vector.z,
            z = this.m20 * vector.x + this.m21 * vector.y + this.m22 * vector.z
        };
    }
}