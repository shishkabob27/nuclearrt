#include "ObjectFactory.h"

std::shared_ptr<ObjectInfo> ObjectFactory::GetObjectInfo(unsigned int handle) {
    switch (handle) {
        {{ OBJECT_INFO_CASES }}
    }
    return nullptr;
}