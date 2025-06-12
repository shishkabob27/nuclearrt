#pragma once

#include "ObjectInfoProperties.h"
#include "Shape.h"
#include <memory>

class QuickBackdropProperties : public ObjectInfoProperties {
public:
    QuickBackdropProperties(unsigned int obstacleType, unsigned int collisionType, 
                       int width, int height, std::shared_ptr<Shape> shape) 
        : ObstacleType(obstacleType), CollisionType(collisionType),
          Width(width), Height(height), oShape(shape) {}
    
    unsigned int ObstacleType;
    unsigned int CollisionType;
    int Width;
    int Height;
    std::shared_ptr<Shape> oShape;
}; 