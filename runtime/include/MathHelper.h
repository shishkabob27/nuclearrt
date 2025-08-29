#pragma once
#include <iostream>

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
}