#include "MathHelper.h"

namespace MathHelper {
    const SafeDivision& GetSafeDivision() {
        static SafeDivision instance;
        return instance;
    }
    
    inline const Power& GetPower() {
        static Power p;
        return p;
    }
}

