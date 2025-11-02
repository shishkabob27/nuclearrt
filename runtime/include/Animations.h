#pragma once

#include <unordered_map>
#include <memory>
#include <algorithm>

#include "Application.h"
#include "Sequence.h"

class Animations {
public:
	Animations() = default;
	Animations(const std::unordered_map<int, Sequence*> sequences) {
		Sequences = sequences;

		//set current sequence to the first one in the map
		if (!Sequences.empty()) {
			CurrentSequenceIndex = Sequences.begin()->first;
		}
		
		CurrentDirection = Sequences.begin()->second->Directions.begin()->second->Index;
	}

	bool IsSequenceOver(int sequence) const {
		auto it = SequenceOverEvents.find(sequence);
		if (it != SequenceOverEvents.end()) {
			SequenceOverEvents.erase(it);
			return true;
		}
		return false;
	}

	std::vector<unsigned int> GetImagesUsed() const {
		std::vector<unsigned int> imagesUsed;
		for (const auto& sequencePair : Sequences) {
			const auto& sequence = sequencePair.second;
			for (const auto& directionPair : sequence->Directions) {
				for (const auto& frame : directionPair.second->Frames) {
					if (std::find(imagesUsed.begin(), imagesUsed.end(), frame) == imagesUsed.end()) {
						imagesUsed.push_back(frame);
					}
				}
			}
		}
		return imagesUsed;
	}

	unsigned int GetCurrentImageHandle() const {
		int displaySequence = forcedSequence != -1 ? forcedSequence : CurrentSequenceIndex;
		int displayDirection = forcedDirection != -1 ? forcedDirection : CurrentDirection;
		int displayFrame = forcedFrame != -1 ? forcedFrame : CurrentFrameIndex;

		if (Sequences.at(displaySequence)->Directions.find(displayDirection) == Sequences.at(displaySequence)->Directions.end() || AutomaticRotation) {
			displayDirection = Sequences.at(displaySequence)->Directions.begin()->second->Index;
		}
		
		return Sequences.at(displaySequence)->Directions.at(displayDirection)->Frames.at(displayFrame);
	}

	unsigned int GetCurrentSequenceIndex() const {
		return CurrentSequenceIndex;
	}

	unsigned int GetCurrentDirection() const {
		return CurrentDirection;
	}

	unsigned int GetCurrentFrameIndex() const {
		return CurrentFrameIndex;
	}

	int GetAutomaticRotationDirection() const {
		return automaticRotationDirection;
	}

	void Start() {
		started = true;
	}
	void Stop() {
		started = false;
	}

	void SetCurrentSequenceIndex(int index) {
		if (index == CurrentSequenceIndex) {
			return;
		}

		if (Sequences.find(index) == Sequences.end()) {
			return;
		}

		CurrentSequenceIndex = index;
		CurrentDirection = Sequences.at(index)->Directions.begin()->second->Index;
		CurrentFrameIndex = 0;
		CurrentFrameTime = 0.0f;
	}

	void SetCurrentDirection(int index) {
		if (index == CurrentDirection) {
			return;
		}

		auto& currentSequence = Sequences.at(CurrentSequenceIndex);
		if (index >= 0 && currentSequence->Directions.find(index) != currentSequence->Directions.end()) {
			CurrentDirection = index;
			CurrentFrameIndex = 0; // Reset frame when changing direction
			CurrentFrameTime = 0.0f; // Reset frame time
		}
	}

	void SetCurrentFrameIndex(int index) {
		if (index == CurrentFrameIndex) {
			return;
		}

		auto& currentDirection = Sequences.at(CurrentSequenceIndex)->Directions.find(CurrentDirection);
		if (index >= 0 && currentDirection != Sequences.at(CurrentSequenceIndex)->Directions.end() && index < static_cast<int>(currentDirection->second->Frames.size())) {
			CurrentFrameIndex = index;
			CurrentFrameTime = 0.0f; // Reset frame time
		}
	}
	
	void SetForcedSequence(int sequence) {
		forcedSequence = sequence;
	}

	void SetForcedFrame(int frame) {
		forcedFrame = frame;
	}

	void SetForcedDirection(int directionMask) {
		int newDirection = -1;
		auto& sequence = Sequences.at(CurrentSequenceIndex);

		std::vector<int> validDirections;
		if (directionMask == -1) // set to any valid direction
		{
			//set to all
			for (int i = 0; i < 32; ++i) {
				validDirections.push_back(i);
			}
		}
		else
		{
			for (int i = 0; i < 32; ++i) {
				if (directionMask & (1 << i)) {
					validDirections.push_back(i);
				}
			}
		}

		//remove any directions that don't exist
		validDirections.erase(std::remove_if(validDirections.begin(), validDirections.end(), [sequence](int direction) {
			return sequence->Directions.find(direction) == sequence->Directions.end();
		}), validDirections.end());

		if (validDirections.empty()) {
			return;
		}

		//get a random valid direction from the list
		int index = Application::Instance().RandomRange(0, static_cast<short>(validDirections.size() - 1));
		newDirection = validDirections.at(index);
		automaticRotationDirection = newDirection;

		forcedDirection = newDirection;
	}

	void RestoreForcedSequence() {
		forcedSequence = -1;
	}
	
	void RestoreForcedDirection() {
		forcedDirection = -1;
	}
	
	void RestoreForcedFrame() {
		CurrentFrameIndex = forcedFrame;
		forcedFrame = -1;
	}

	bool IsDirectionForced() const {
		return forcedDirection != -1;
	}

	void Update(float deltaTime) {
		CurrentFrameTime += deltaTime;

		int displayFrame = CurrentFrameIndex;
		int displaySequence = CurrentSequenceIndex;
		int displayDirection = CurrentDirection;
		if (forcedFrame != -1) {
			displayFrame = forcedFrame;
		}
		if (forcedDirection != -1) {
			displayDirection = forcedDirection;
		}
		if (forcedSequence != -1) {
			displaySequence = forcedSequence;
		}

		auto& currentSequence = Sequences.at(displaySequence);

		//if the direction doesn't exist, set to the first direction
		if (currentSequence->Directions.find(displayDirection) == currentSequence->Directions.end()) {
			forcedDirection = -1;
			displayDirection = currentSequence->Directions.begin()->second->Index;
			CurrentDirection = displayDirection;
			CurrentFrameIndex = 0;
			CurrentFrameTime = 0.0f;
			return;
		}
		auto& currentDirection = currentSequence->Directions.at(displayDirection);

		int animSpeed = currentDirection->MaximumSpeed; // TODO: look at minimum speed too

		if (animSpeed == 0) {
			return;
		}

		if (!started) {
			// If the animation is stopped, we don't update the frame time or index
			return;
		}

		if (CurrentFrameTime >= 1.0f / animSpeed) {

			//TODO: verify if this is correct
			if (forcedFrame != -1) return; // If a forced frame is set, don't update the index

			CurrentFrameTime = 0.0f;
			CurrentFrameIndex++;

			if (CurrentFrameIndex >= static_cast<int>(currentDirection->Frames.size())) // reached end of animation
			{
				if (currentDirection->Repeat) {
					CurrentFrameIndex = currentDirection->RepeatFrame;
				} else {
					
					// If animation is not the first one (stopped), change to that animation
					int firstSequenceIndex = Sequences.begin()->first;
					if (CurrentSequenceIndex != firstSequenceIndex) {
						//set sequence over event
						SequenceOverEvents[CurrentSequenceIndex] = true;

						//Change to the first sequence
						SetCurrentSequenceIndex(firstSequenceIndex);
					}
					else // if it is the first one, stay on the last frame
					{
						CurrentFrameIndex -= 1;

						//TODO: this shouldn't activate during certian conditions but idk what they are
						//set sequence over event
						SequenceOverEvents[CurrentSequenceIndex] = true;
					}
				}
			}
		}
	}
	
	bool AutomaticRotation = false;
private:
	std::unordered_map<int, Sequence*> Sequences;
	float CurrentFrameTime = 0.0f;

	int CurrentSequenceIndex = 0;
	int CurrentDirection = 0;
	int CurrentFrameIndex = 0;

	bool started = true;

	int forcedFrame = -1;
	int forcedDirection = -1;
	int forcedSequence = -1;

	int automaticRotationDirection = -1;

	mutable std::unordered_map<int, bool> SequenceOverEvents;
};