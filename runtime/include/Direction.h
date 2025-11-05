#pragma once

#include <vector>

class Direction {
public:
    Direction(int index, int minimumSpeed, int maximumSpeed, bool repeat, int repeatFrame, std::vector<unsigned int> frames)
        : Index(index), MinimumSpeed(minimumSpeed), MaximumSpeed(maximumSpeed), 
          Repeat(repeat), RepeatFrame(repeatFrame), Frames(frames) {}

    int Index;
    int MinimumSpeed;
    int MaximumSpeed;
    bool Repeat;
    int RepeatFrame;
    std::vector<unsigned int> Frames;
}; 