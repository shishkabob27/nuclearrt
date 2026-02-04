#pragma once
#include <iostream>
#include <cmath>
#include <string>
#include <algorithm>

namespace MathHelper {
    template<typename T>
    T SafeDivide(T numerator, T denominator) {
        if (denominator == 0) {
            return 0;
        }
        T result = numerator / denominator;
        return result;
    }

    struct SafeDivision;

    template<typename T>
    struct SafeDivResult {
        T value;

        explicit SafeDivResult(T val) : value(val) {}

        T operator/(T denom) const {
            return SafeDivide(value, denom);
        }
    };

    struct SafeDivision {
        SafeDivResult<int> operator/(int num) const {
            return SafeDivResult<int>(num);
        }

        SafeDivResult<float> operator/(float num) const {
            return SafeDivResult<float>(num);
        }

        SafeDivResult<long> operator/(long num) const {
            return SafeDivResult<long>(num);
        }
    };

    template<typename T>
    SafeDivResult<T> operator/(T num, const SafeDivision&) {
        return SafeDivResult<T>(num);
    }

    const SafeDivision& GetSafeDivision();

    // trig/math helpers (NOTE: ctf uses degrees, C++ uses radians)
    inline double ToRadians(double degrees) { return degrees * (3.14159265358979323846 / 180.0); }
    inline double ToDegrees(double radians) { return radians * (180.0 / 3.14159265358979323846); }

    inline double Sin(double degrees) { return std::sin(ToRadians(degrees)); }
    inline double Cos(double degrees) { return std::cos(ToRadians(degrees)); }
    inline double Tan(double degrees) { return std::tan(ToRadians(degrees)); }
    inline double ASin(double v) { return ToDegrees(std::asin(v)); }
    inline double ACos(double v) { return ToDegrees(std::acos(v)); }
    inline double ATan(double v) { return ToDegrees(std::atan(v)); }
    inline double ATan2(double y, double x) { return ToDegrees(std::atan2(y, x)); }

    // color helpers
    inline int GetRGB(int r, int g, int b) { return (r << 16) | (g << 8) | b; }
    inline int GetRed(int rgb) { return (rgb >> 16) & 0xFF; }
    inline int GetGreen(int rgb) { return (rgb >> 8) & 0xFF; }
    inline int GetBlue(int rgb) { return rgb & 0xFF; }

    // math utility helpers
    inline double Distance(double x1, double y1, double x2, double y2) { return std::sqrt(std::pow(x2 - x1, 2) + std::pow(y2 - y1, 2)); }
    inline double VectorAngle(double x1, double y1, double x2, double y2) { return ToDegrees(std::atan2(y1 - y2, x2 - x1)); }
    inline double Range(double v, double minVal, double maxVal) { return (v < minVal) ? minVal : (v > maxVal ? maxVal : v); }
}