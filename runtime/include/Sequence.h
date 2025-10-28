#pragma once

#include "Direction.h"
#include <vector>

class Sequence {
public:
    Sequence(const std::vector<Direction*>& directions)
        : Directions(directions) {}

    std::vector<Direction*> Directions;
}; 