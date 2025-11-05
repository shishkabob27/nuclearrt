#include "ObjectFactory.h"

ObjectInstance* ObjectFactory::CreateInstance(unsigned int handle) {
    switch (handle) {
        {{ OBJECT_INFO_CASES }}
        default:
            return nullptr;
    }
}

{{ OBJECT_INFO_FUNCTIONS }}