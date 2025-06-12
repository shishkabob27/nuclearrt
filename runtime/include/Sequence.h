#pragma once

#include "Direction.h"
#include <vector>
#include <memory>

class Sequence {
public:
    Sequence(const std::vector<std::shared_ptr<Direction>>& directions)
        : Directions(directions) {}

    std::vector<std::shared_ptr<Direction>> Directions;
}; 