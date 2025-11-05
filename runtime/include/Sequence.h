#pragma once

#include "Direction.h"
#include <vector>

class Sequence {
public:
    Sequence(const std::unordered_map<int, Direction*>& directions)
        : Directions(directions) {}

    std::unordered_map<int, Direction*> Directions;
}; 