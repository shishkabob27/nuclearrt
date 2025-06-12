#pragma once

#include "ObjectInstance.h"
#include <vector>
#include <memory>
#include <algorithm>
#include <iterator>

// Forward declaration of ObjectIterator for friendship
class ObjectIterator;

class ObjectSelector {
public:
    // Iterator type aliases for better usability
    using iterator = std::vector<std::shared_ptr<ObjectInstance>>::iterator;
    using const_iterator = std::vector<std::shared_ptr<ObjectInstance>>::const_iterator;
    using value_type = std::shared_ptr<ObjectInstance>;
    using reference = std::shared_ptr<ObjectInstance>&;
    using const_reference = const std::shared_ptr<ObjectInstance>&;
    using size_type = size_t;

    ObjectSelector() = default; // Used on frame initialization
    
    // Constructor that references an external collection
    ObjectSelector(const std::vector<std::shared_ptr<ObjectInstance>>& allInstances, unsigned int objectInfoId, bool isQualifier = false) // used after object creation
        : AllInstances(allInstances), ObjectInfoId(objectInfoId), IsQualifier(isQualifier), OwnsInstances(false) {
        Reset();
    }
    
    // Constructor that creates and owns its own instances collection
    ObjectSelector(unsigned int objectInfoId, bool isQualifier = false)
        : AllInstances(InternalInstances), ObjectInfoId(objectInfoId), IsQualifier(isQualifier), OwnsInstances(true) {
        // No need to call Reset() as InternalInstances is empty
    }

    // Reset to include all instances with matching ObjectInfo ID
    void Reset() {
        SelectedInstances.clear();
        for (const auto& instance : AllInstances) {
            if (IsQualifier) {
                auto commonProps = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties); // If it has a qualifier, it must have common properties
                if (commonProps) {
                    for (int i = 0; i < 8; i++) {
                        if (commonProps->Qualifiers[i] == ObjectInfoId) {
                            SelectedInstances.push_back(instance);
                            break;
                        }
                    }
                }
            } else if (instance->OI->Handle == ObjectInfoId) {
                SelectedInstances.push_back(instance);
            }
        }
    }

    // Deselect an instance by pointer
    void Deselect(std::shared_ptr<ObjectInstance> instance) {
        auto it = std::find(SelectedInstances.begin(), SelectedInstances.end(), instance);
        if (it != SelectedInstances.end()) {
            SelectedInstances.erase(it);
        }
    }

    // Deselect an instance by index in the selected array
    void DeselectAt(size_t index) {
        if (index < SelectedInstances.size()) {
            SelectedInstances.erase(SelectedInstances.begin() + index);
        }
    }

    // Add an instance back to selection
    void Select(std::shared_ptr<ObjectInstance> instance) {
        if (instance->OI->Handle == ObjectInfoId && 
            std::find(SelectedInstances.begin(), SelectedInstances.end(), instance) == SelectedInstances.end()) {
            SelectedInstances.push_back(instance);
        }
    }

    // Add an external instance that may not be in AllInstances
    void AddExternalInstance(std::shared_ptr<ObjectInstance> instance) {
        // Check if the instance already exists in the selection
        if (std::find(SelectedInstances.begin(), SelectedInstances.end(), instance) == SelectedInstances.end()) {
            // Add to selected instances regardless of its ObjectInfoId
            SelectedInstances.push_back(instance);
            
            // If we own the instances collection, we can add to AllInstances too
            if (OwnsInstances && std::find(InternalInstances.begin(), InternalInstances.end(), instance) == InternalInstances.end()) {
                InternalInstances.push_back(instance);
            }
        }
    }
    
    // Permanently remove an instance from SelectedInstances and AllInstances if we own it
    void RemoveInstance(std::shared_ptr<ObjectInstance> instance) {
        // Remove from SelectedInstances
        auto it = std::find(SelectedInstances.begin(), SelectedInstances.end(), instance);
        if (it != SelectedInstances.end()) {
            SelectedInstances.erase(it);
        }
        
        // If we own the instances collection, we can remove from AllInstances too
        if (OwnsInstances) {
            auto allIt = std::find(InternalInstances.begin(), InternalInstances.end(), instance);
            if (allIt != InternalInstances.end()) {
                InternalInstances.erase(allIt);
            }
        }
    }

    // Get number of selected instances
    size_t Count() const {
        return SelectedInstances.size();
    }

    // Access instance by index
    std::shared_ptr<ObjectInstance> At(size_t index) {
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
    
    // Add a FilterInPlace method for convenience
    template<typename Predicate>
    void FilterInPlace(Predicate pred) {
        for (auto it = SelectedInstances.begin(); it != SelectedInstances.end();) {
            if (!pred(*it)) {
                it = SelectedInstances.erase(it);
            } else {
                ++it;
            }
        }
    }

private:
    const std::vector<std::shared_ptr<ObjectInstance>>& AllInstances;
    std::vector<std::shared_ptr<ObjectInstance>> SelectedInstances;
    unsigned int ObjectInfoId;
    bool IsQualifier = false;
    bool OwnsInstances = false;
    std::vector<std::shared_ptr<ObjectInstance>> InternalInstances;
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
    std::shared_ptr<ObjectInstance> operator*() {
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