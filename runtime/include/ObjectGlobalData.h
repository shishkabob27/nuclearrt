#pragma once

#include "AlterableFlags.h"
#include "AlterableValues.h"
#include "AlterableStrings.h"

class ObjectGlobalData {
public:
    ObjectGlobalData(unsigned int objectInfoHandle) : objectInfoHandle(objectInfoHandle) {}

    unsigned int objectInfoHandle = 0;

    AlterableFlags flags;
    AlterableValues values;
    AlterableStrings strings;
};