#include "FrameFactory.h"
{{ FRAME_INCLUDES }}

std::unique_ptr<Frame> FrameFactory::CreateFrame(int index) {
    switch (index) {
{{ FRAME_CASES }}
    }
    return nullptr;
}

int FrameFactory::GetFrameCount() {
    return {{ FRAME_COUNT }};
}