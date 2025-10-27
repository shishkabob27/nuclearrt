#pragma once

#include "ObjectInstance.h"
#include <vector>
#include <memory>
#include <algorithm>
#include <iterator>

class ObjectIterator;

class ObjectSelector {
public:
	using iterator = std::vector<ObjectInstance*>::iterator;
	using const_iterator = std::vector<ObjectInstance*>::const_iterator;
	using value_type = ObjectInstance*;
	using reference = ObjectInstance*&;
	using const_reference = const ObjectInstance*&;
	using size_type = size_t;
	
	// Constructor that references an external collection
	ObjectSelector(const std::unordered_map<unsigned int, ObjectInstance*>& allInstances, unsigned int objectInfoId, bool isQualifier = false) // used after object creation
		: AllInstances(allInstances), ObjectInfoId(objectInfoId), IsQualifier(isQualifier) {
		Initialize(allInstances);
	}

	// called after all object instances are created on frame initialization and when a new object is created that needs to be added to the selector
	void Initialize(const std::unordered_map<unsigned int, ObjectInstance*>& allInstances) {
		AllSelectorObjectInstances.clear();
		for (const auto& [handle, instance] : allInstances) {
			if (IsQualifier) {
				for (int i = 0; i < 8; i++) {
					if (instance->Qualifiers[i] == ObjectInfoId) {
						AllSelectorObjectInstances.push_back(instance->Handle);
						break;
					}
				}
			}
			else if (instance->ObjectInfoHandle == ObjectInfoId) {
				AllSelectorObjectInstances.push_back(instance->Handle);
			}
		}
	}

	// Called during a event to scope all objects that are relevant to the event
	void Reset() {
		SelectedInstances.clear();
		for (const auto& handle : AllSelectorObjectInstances) {
			if (AllInstances.find(handle) != AllInstances.end()) {
				SelectedInstances.push_back(AllInstances.at(handle));
			}
			else {
				// instance has been destroyed
				RemoveInstance(handle);
			}
		}
	}

	// Called when an object is destroyed
	void RemoveInstance(unsigned int handle) {
		AllSelectorObjectInstances.erase(std::remove(AllSelectorObjectInstances.begin(), AllSelectorObjectInstances.end(), handle), AllSelectorObjectInstances.end());
	}

	void AddInstance(ObjectInstance* instance) {
		if (std::find(AllSelectorObjectInstances.begin(), AllSelectorObjectInstances.end(), instance->Handle) == AllSelectorObjectInstances.end()) {
			AllSelectorObjectInstances.push_back(instance->Handle);
			if (std::find(SelectedInstances.begin(), SelectedInstances.end(), instance) == SelectedInstances.end()) {
				SelectedInstances.push_back(instance);
			}
		}
	}

	// Deselect an instance by index in the selected array
	void DeselectAt(size_t index) {
		if (index < SelectedInstances.size()) {
			SelectedInstances.erase(SelectedInstances.begin() + index);
		}
	}

	// Add an instance back to selection
	void Select(ObjectInstance* instance) {
		if (instance->ObjectInfoHandle == ObjectInfoId && 
			std::find(SelectedInstances.begin(), SelectedInstances.end(), instance) == SelectedInstances.end()) {
			SelectedInstances.push_back(instance);
		}
	}

	// Get number of selected instances
	size_t Count() const {
		return SelectedInstances.size();
	}

	// Access instance by index
	ObjectInstance* At(size_t index) {
		if (index < SelectedInstances.size()) {
			return SelectedInstances[index];
		}
		return nullptr;
	}

	// Iterator methods with improved signatures
	iterator begin() { return SelectedInstances.begin(); }
	iterator end() { return SelectedInstances.end(); }
	const_iterator begin() const { return SelectedInstances.begin(); }
	const_iterator end() const { return SelectedInstances.end(); }
	
	// Additional iterator methods for better compatibility
	const_iterator cbegin() const { return SelectedInstances.cbegin(); }
	const_iterator cend() const { return SelectedInstances.cend(); }

	// Friend declaration for the iterator
	friend class ObjectIterator;

private:
	const std::unordered_map<unsigned int, ObjectInstance*>& AllInstances;
	std::vector<ObjectInstance*> SelectedInstances;
	std::vector<int> AllSelectorObjectInstances; // list of all object instance handles for this selector
	unsigned int ObjectInfoId;
	bool IsQualifier = false;
};

// Add ObjectIterator class to support the requested iteration pattern
class ObjectIterator {
public:
	ObjectIterator(ObjectSelector& selector) 
		: selector_(selector), currentIndex_(0), skipIncrement_(false) {
	}
	
	// Check if we've reached the end of iteration
	bool end() const {
		return currentIndex_ >= selector_.SelectedInstances.size();
	}
	
	// Increment operator to move to next object
	ObjectIterator& operator++() {
		if (!end()) {
			// Only increment if we're not supposed to skip
			if (!skipIncrement_) {
				currentIndex_++;
			} else {
				// Reset the flag after using it
				skipIncrement_ = false;
			}
		}
		return *this;
	}
	
	// Dereference operator to get the current object
	ObjectInstance* operator*() {
		if (!end()) {
			return selector_.SelectedInstances[currentIndex_];
		}
		return nullptr;
	}
	
	// Deselect the current object during iteration
	void deselect() {
		if (!end()) {
			selector_.SelectedInstances.erase(selector_.SelectedInstances.begin() + currentIndex_);
			skipIncrement_ = true;
		}
	}

	size_t index() const {
		return currentIndex_;
	}
	
private:
	ObjectSelector& selector_;
	size_t currentIndex_;
	bool skipIncrement_; // Flag to track if we should skip the next increment
};