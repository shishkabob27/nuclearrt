#pragma once

#include <memory>
#include <unordered_map>
#include "ObjectInfo.h"
#include "ObjectInstance.h"
#include "BackdropProperties.h"
#include "QuickBackdropProperties.h"
#include "CommonProperties.h"

#include "MouseMovement.h"
#include "EightDirectionsMovement.h"

{{ EXTENSION_INCLUDES }}

class ObjectFactory {
public:
    static ObjectFactory& Instance() {
        static ObjectFactory instance;
        return instance;
    }

    std::shared_ptr<ObjectInfo> GetObjectInfo(unsigned int handle);

    std::shared_ptr<ObjectInstance> CreateInstance(unsigned int handle, unsigned int objectInfoHandle, int x, int y, unsigned int layer, short instanceValue) {
        auto objectInfo = GetObjectInfo(objectInfoHandle);
        if (!objectInfo) {
            return nullptr;
        }
        return std::make_shared<ObjectInstance>(handle, objectInfo, x, y, layer, instanceValue);
    }

private:
    ObjectFactory() = default;
    std::unordered_map<unsigned int, std::shared_ptr<ObjectInfo>> objectInfos;

    {{ OBJECT_INFO_FUNCTIONS_DEFINITIONS }}
}; 