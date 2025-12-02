#pragma once

#include <memory>

class Frame;

class FrameFactory {
public:
	static std::unique_ptr<Frame> CreateFrame(int index);
	static int GetFrameCount();
};