#pragma once

class PathNode {
public:
	PathNode(unsigned char speed, unsigned char direction, short destinationX, short destinationY)
		: Speed(speed), Direction(direction), DestinationX(destinationX), DestinationY(destinationY) {}

	unsigned char Speed;
	unsigned char Direction; //determines what angle the sprite should be pointing

	//offset from the last node
	short DestinationX;
	short DestinationY;
};