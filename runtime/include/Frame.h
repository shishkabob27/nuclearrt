#pragma once

#include "ObjectInstance.h"
#include "ObjectFactory.h"
#include "ObjectSelector.h"
#include "Timer.h"
#include <vector>
#include <memory>
#include "Layer.h"

class Frame {
public:
	Frame() = default;
    virtual ~Frame() = default;

	int Index = -1;
	std::string Name = "";

	int Width = 0;
	int Height = 0;

	int BackgroundColor = 0;

	std::vector<Layer> Layers;

	std::vector<std::shared_ptr<ObjectInstance>> ObjectInstances;
	Timer GameTimer;

	bool IsGroupActive(unsigned int groupId) {
		if (groupId < ActiveGroups.size()) {
			return ActiveGroups[groupId];
		}
		return false;
	}

	void SetGroupActive(unsigned int groupId, bool active) {
		if (groupId >= ActiveGroups.size()) {
			ActiveGroups.resize(groupId + 1, false);
		}
		ActiveGroups[groupId] = active;
	}

	virtual void Initialize();
	void PostInitialize();
	virtual void Update();
	virtual void Draw();

	void SetScroll(int x, int y, int layer = -1);
	void SetScrollX(int x);
	void SetScrollY(int y);

	//mark an instance for deletion
	void MarkForDeletion(ObjectInstance* instance) {
		if (instance) {
			instancesMarkedForDeletion.push_back(instance->Handle);
		}
	}

	void DeleteMarkedInstances() {
		for (auto& handle : instancesMarkedForDeletion) {
			auto it = std::remove_if(ObjectInstances.begin(), ObjectInstances.end(),
				[handle](const std::shared_ptr<ObjectInstance>& instance) { return instance->Handle == handle; });
			ObjectInstances.erase(it, ObjectInstances.end());
		}
		instancesMarkedForDeletion.clear();
	}

	virtual void DrawLayer(Layer& layer, unsigned int index);
	virtual void DrawCounterNumbers(Counter& counter, int value, int x, int y);

	std::vector<unsigned int> GetImagesUsed();
	std::vector<unsigned int> GetFontsUsed();

	void CreateInstance(short x, short y, unsigned int layer, unsigned int objectInfoHandle, short angle, ObjectInstance* parentInstance = nullptr);

	int GetMouseX();
	int GetMouseY();

	//Collision detection
	bool IsCollidingWithBackground(ObjectInstance* instance);
	bool IsColliding(ObjectInstance* instance1, ObjectInstance* instance2);
	bool IsColliding(ObjectInstance* instance, int x, int y);
	bool IsPointInPolygon(int x, int y, int polygon[][2], int numPoints);
	bool DoLinesIntersect(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4);

	void RotatePoints(int& x1, int& y1, int& x2, int& y2, int& x3, int& y3, int& x4, int& y4, int offsetX, int offsetY, float angle);
	void RotatePoint(int& x, int& y, float angle);
	bool IsPointOnLine(int x, int y, int x1, int y1, int x2, int y2);

private:
	std::vector<unsigned int> instancesMarkedForDeletion;
	std::vector<bool> ActiveGroups;

	int scrollX = 0;
	int scrollY = 0;
};
