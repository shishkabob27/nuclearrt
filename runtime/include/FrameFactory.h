#pragma once

#include "Frame.h"
#include <memory>

class FrameFactory {
public:
    static std::unique_ptr<Frame> CreateFrame(int index);
};