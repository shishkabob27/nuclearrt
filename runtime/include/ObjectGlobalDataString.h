#pragma once

#include <string>

#include "ObjectGlobalData.h"

class ObjectGlobalDataString : public ObjectGlobalData {
public:
    ObjectGlobalDataString(unsigned int objectInfoHandle) : ObjectGlobalData(objectInfoHandle) {}

    std::string alterableText = "";
    int currentParagraph = 0;
};