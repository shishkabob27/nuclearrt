#pragma once

#include <memory>
#include <unordered_map>
#include "ObjectInstance.h"

#include "QuickBackdrop.h"
#include "Backdrop.h"
#include "Active.h"
#include "StringObject.h"
#include "Score.h"
#include "Lives.h"
#include "Counter.h"
#include "Extension.h"

#include "MouseMovement.h"
#include "EightDirectionsMovement.h"

{{ EXTENSION_INCLUDES }}

class ObjectFactory {
public:
    static ObjectFactory& Instance() {
        static ObjectFactory instance;
        return instance;
    }

    ObjectInstance* CreateInstance(unsigned int handle);

    {{ OBJECT_INFO_FUNCTIONS_DEFINITIONS }}
}; 