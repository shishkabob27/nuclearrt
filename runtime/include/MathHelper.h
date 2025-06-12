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

    // Forward declaration needed for operator/
    struct SafeDivision;
    
    // Intermediate result of division operation
    template<typename T>
    struct SafeDivResult {
        T value;
        
        SafeDivResult(T val) : value(val) {}
        
        // Second division operation
        T operator/(T denom) const {
            return SafeDivide(value, denom);
        }
    };
    
    // Generic SafeDiv that works with multiple types
    struct SafeDivision {
        // Handle integers
        SafeDivResult<int> operator/(int num) const {
            return SafeDivResult<int>(num);
        }
        
        // Handle floats
        SafeDivResult<float> operator/(float num) const {
            return SafeDivResult<float>(num);
        }
        
        // Handle longs
        SafeDivResult<long> operator/(long num) const {
            return SafeDivResult<long>(num);
        }
    };
    
    // First part of the division operation
    template<typename T>
    SafeDivResult<T> operator/(T num, const SafeDivision&) {
        return SafeDivResult<T>(num);
    }
    
    // Single instance that works with all types - using inline to prevent multiple definition errors
    inline const SafeDivision safeDivision{};
} 