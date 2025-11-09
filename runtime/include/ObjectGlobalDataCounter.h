#pragma once

#include "ObjectGlobalData.h"

class ObjectGlobalDataCounter : public ObjectGlobalData {
public:
    ObjectGlobalDataCounter(unsigned int objectInfoHandle) : ObjectGlobalData(objectInfoHandle) {}

    int value = 0;
    int minValue = 0;
    int maxValue = 0;
};