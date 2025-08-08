#include "MathHelper.h"

namespace MathHelper {
    const SafeDivision& GetSafeDivision() {
        static SafeDivision instance;
        return instance;
    }
}
