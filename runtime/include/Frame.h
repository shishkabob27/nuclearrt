#pragma once

#include "ObjectInstance.h"
#include "ObjectFactory.h"
#include "ObjectSelector.h"
#include "Timer.h"
#include <vector>
#include <memory>
#include "Layer.h"
#include "CounterBase.h"

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

	std::unordered_map<unsigned int, ObjectInstance*> ObjectInstances;
	unsigned int MaxObjectInstanceHandle = 0;
	
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
			for (auto& layer : Layers) {
				auto it = std::find_if(layer.instances.begin(), layer.instances.end(), [handle](ObjectInstance* instance) {
					return instance->Handle == handle;
				});
				if (it != layer.instances.end()) {
					layer.instances.erase(it);
					break;
				}
			}
			ObjectInstances.erase(handle);
		}
		instancesMarkedForDeletion.clear();
	}

	void DrawLayer(Layer& layer);
	void DrawCounterNumbers(CounterBase *counter, int value, int x, int y);

	std::vector<unsigned int> GetImagesUsed();
	std::vector<unsigned int> GetFontsUsed();

	ObjectInstance* CreateInstance(ObjectInstance* createdInstance, short x, short y, unsigned int layer, short instanceValue, unsigned int objectInfoHandle, short angle, ObjectInstance* parentInstance = nullptr);

	std::vector<ObjectGlobalData*> GetGlobalObjectData();
	void ApplyGlobalObjectData(std::vector<ObjectGlobalData*> globalData);

	void MoveObjectToLayer(ObjectInstance* instance, unsigned int layer);
	void MoveObjectToFront(ObjectInstance* instance);
	void MoveObjectToBack(ObjectInstance* instance);
	void MoveObjectInFrontOf(ObjectInstance* instance, unsigned int oiHandle);
	void MoveObjectBehindOf(ObjectInstance* instance, unsigned int oiHandle);

	int GetMouseX();
	int GetMouseY();

	int StringLength(std::string str) {
		return (int)str.length();
	}

	std::string StringLeft(std::string str, int length) {
		return str.substr(0, length);
	}

	std::string StringRight(std::string str, int length) {
		return str.substr(str.length() - length);
	}

	int Loopindex(std::string loopName) {
		//TODO: loopindex will return 0 untul the loop system can support expressions
		return 0;
	}

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
