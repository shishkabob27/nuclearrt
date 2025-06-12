#pragma once

#include "ObjectInfoProperties.h"

class BackdropProperties : public ObjectInfoProperties {
public:
    BackdropProperties(unsigned int obstacleType, unsigned int collisionType, 
                       int width, int height, unsigned int image) 
        : ObstacleType(obstacleType), CollisionType(collisionType),
          Width(width), Height(height), Image(image) {}
    
    unsigned int ObstacleType;
    unsigned int CollisionType;
    int Width;
    int Height;
    unsigned int Image;
}; 