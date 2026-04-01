#pragma once

#include "ObjectInstance.h"
#include <vector>
#include <algorithm>

class ObjectIterator;

class ObjectSelector {
public:
	using iterator = std::vector<ObjectInstance*>::iterator;
	using const_iterator = std::vector<ObjectInstance*>::const_iterator;

	// Constructor that references an external collection
	ObjectSelector(const std::unordered_map<unsigned int, ObjectInstance*>& allInstances, unsigned int objectInfoId, bool isQualifier = false)
		: AllInstances(allInstances), ObjectInfoId(objectInfoId), IsQualifier(isQualifier), IsDirty(false) {
		Initialize(allInstances);
	}

	// called after all object instances are created on frame initialization and when a new object is created that needs to be added to the selector
	void Initialize(const std::unordered_map<unsigned int, ObjectInstance*>& allInstances) {
		AllSelectorObjectInstances.clear();
		for (const auto& [handle, instance] : allInstances) {
			instance->isSelected = false; 
			if (IsQualifier) {
				for (int i = 0; i < 8; i++) {
					if (instance->Qualifiers[i] == ObjectInfoId) {
						AllSelectorObjectInstances.push_back(instance);
						break;
					}
				}
			} else if (instance->ObjectInfoHandle == ObjectInfoId) {
				AllSelectorObjectInstances.push_back(instance);
			}
		}
		std::sort(AllSelectorObjectInstances.begin(), AllSelectorObjectInstances.end(), 
			[](ObjectInstance* a, ObjectInstance* b) { return a->Handle < b->Handle; });
	}

	// Called during a event to scope all objects that are relevant to the event
	void Reset() {
		for (auto* obj : SelectedInstances) {
			obj->isSelected = false;
		}
		SelectedInstances = AllSelectorObjectInstances;
		for (auto* obj : SelectedInstances) {
			obj->isSelected = true;
		}
		IsDirty = false; 
	}

	// Called when an object is destroyed
	void RemoveInstance(unsigned int handle) {
		auto it = std::find_if(AllSelectorObjectInstances.begin(), AllSelectorObjectInstances.end(),
			[handle](ObjectInstance* p) { return p->Handle == handle; });
		
		if (it != AllSelectorObjectInstances.end()) {
			(*it)->isSelected = false;
			AllSelectorObjectInstances.erase(it);
		}

		auto itSel = std::find_if(SelectedInstances.begin(), SelectedInstances.end(),
			[handle](ObjectInstance* p) { return p->Handle == handle; });
		
		if (itSel != SelectedInstances.end()) {
			*itSel = SelectedInstances.back();
			SelectedInstances.pop_back();
			IsDirty = true;
		}
	}

	void AddInstance(ObjectInstance* instance) {
		if (!instance->isSelected) {
			instance->isSelected = true;
			SelectedInstances.push_back(instance);
			IsDirty = true;
			
			auto it = std::lower_bound(AllSelectorObjectInstances.begin(), AllSelectorObjectInstances.end(), instance,
				[](ObjectInstance* a, ObjectInstance* b) { return a->Handle < b->Handle; });
			
			if (it == AllSelectorObjectInstances.end() || (*it)->Handle != instance->Handle) {
				AllSelectorObjectInstances.insert(it, instance);
			}
		}
	}

	// Deselect an instance by index in the selected array
	void DeselectAt(size_t index) {
		if (index < SelectedInstances.size()) {
			SelectedInstances[index]->isSelected = false;
			SelectedInstances[index] = SelectedInstances.back();
			SelectedInstances.pop_back();
			IsDirty = true;
		}
	}

	// Add an instance back to selection
	void Select(ObjectInstance* instance) {
		if (instance->ObjectInfoHandle == ObjectInfoId && !instance->isSelected) {
			instance->isSelected = true;
			SelectedInstances.push_back(instance);
			IsDirty = true;
		}
	}

	void SelectOnly(ObjectInstance* instance) {
		for (auto* obj : SelectedInstances) obj->isSelected = false;
		SelectedInstances.clear();
		
		instance->isSelected = true;
		SelectedInstances.push_back(instance);
		IsDirty = false; 
	}

	void SelectRandom() {
		if (!SelectedInstances.empty()) {
			size_t randomIndex = Application::Instance().RandomRange(0, static_cast<short>(SelectedInstances.size() - 1));
			ObjectInstance* instance = SelectedInstances[randomIndex];
			SelectOnly(instance);
		}
	}

	// Get number of selected instances
	size_t Count() const { return SelectedInstances.size(); }
	
	// Get number of all object instances in the selector
	size_t Size() const { return AllSelectorObjectInstances.size(); }

	// Access instance by index
	ObjectInstance* At(size_t index) {
		if (index < SelectedInstances.size()) {
			EnsureSorted();
			return SelectedInstances[index];
		}
		return nullptr;
	}

	// Iterator methods with improved signatures
	iterator begin() { EnsureSorted(); return SelectedInstances.begin(); }
	iterator end() { return SelectedInstances.end(); }

	const_iterator begin() const { 
		const_cast<ObjectSelector*>(this)->EnsureSorted(); 
		return SelectedInstances.begin(); 
	}
	const_iterator end() const { return SelectedInstances.end(); }

	// Friend declaration for the iterator
	friend class ObjectIterator;

private:
	void EnsureSorted() {
		if (IsDirty) {
			std::sort(SelectedInstances.begin(), SelectedInstances.end(), 
				[](ObjectInstance* a, ObjectInstance* b) { return a->Handle < b->Handle; });
			IsDirty = false;
		}
	}

	const std::unordered_map<unsigned int, ObjectInstance*>& AllInstances;
	std::vector<ObjectInstance*> SelectedInstances;	  
	std::vector<ObjectInstance*> AllSelectorObjectInstances; // list of all object instance handles for this selector
	unsigned int ObjectInfoId;
	bool IsQualifier;
	bool IsDirty; 
};

// Add ObjectIterator class to support the requested iteration pattern
class ObjectIterator {
public:
	ObjectIterator(ObjectSelector& selector) 
		: selector_(selector), currentIndex_(0), skipIncrement_(false) {
		selector_.EnsureSorted();
	}
	
	// Check if we've reached the end of iteration
	bool end() const { return currentIndex_ >= selector_.SelectedInstances.size(); }
	
	// Increment operator to move to next object
	ObjectIterator& operator++() {
		if (!end()) {
			// Only increment if we're not supposed to skip
			if (!skipIncrement_) currentIndex_++;
			else skipIncrement_ = false; // Reset the flag after using it
		}
		return *this;
	}
	
	// Dereference operator to get the current object
	ObjectInstance* operator*() {
		return end() ? nullptr : selector_.SelectedInstances[currentIndex_];
	}
	
	// Deselect the current object during iteration
	void deselect() {
		if (!end()) {
			selector_.DeselectAt(currentIndex_);
			skipIncrement_ = true;
		}
	}

	size_t index() const { return currentIndex_; }
	
private:
	ObjectSelector& selector_;
	size_t currentIndex_;
	bool skipIncrement_; // Flag to track if we should skip the next increment
};