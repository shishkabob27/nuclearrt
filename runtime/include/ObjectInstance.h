#pragma once

#include <algorithm>
#include <memory>
#include <string>
#include <vector>

class ObjectInstance {
public:
    ObjectInstance(unsigned int objectInfoHandle, int type, std::string name)
        : ObjectInfoHandle(objectInfoHandle), Type(type), Name(name) {}
    virtual ~ObjectInstance() = default;
	
	unsigned int Handle = 0;
	unsigned int Type = 0;
	unsigned int ObjectInfoHandle = 0;
	std::string Name = "";
	
	int X = 0;
	int Y = 0;

	unsigned int Layer = 0;

	short InstanceValue = 0;
	std::vector<short> Qualifiers = { -1, -1, -1, -1, -1, -1, -1, -1 };

	int RGBCoefficient = 0xFFFFFF;
	int Effect = 0;
	unsigned int EffectParameter = 0; 

	unsigned char GetBlendCoefficient() const {
		return BlendCoefficient;
	}
	
	void SetBlendCoefficient(int blendCoefficient) {
		BlendCoefficient = static_cast<unsigned char>(std::clamp(blendCoefficient, 0, 255));
	}
	
	unsigned int GetAngle() const {
		return Angle;
	}

	void SetAngle(int angle) {
		//limit to 0-359 degrees
		while (angle < 0) {
			angle += 360;
		}
		while (angle >= 360) {
			angle -= 360;
		}
		
		Angle = angle;
	}

	virtual std::vector<unsigned int> GetImagesUsed() { return std::vector<unsigned int>(); };
	virtual std::vector<unsigned int> GetFontsUsed() { return std::vector<unsigned int>(); };

private:
	unsigned int Angle = 0;
	unsigned char BlendCoefficient = 0; // Alpha
};
