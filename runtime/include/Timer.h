#pragma once

#include <vector>
#include <algorithm>

enum class TimerEventType {
    Every,
    Equals,
    GreaterThan,
    LessThan
};

class TimerEvent {
public:
    int id; // ID of the event
    int time; // Total Time in milliseconds of this event
    int checkTime; // Time to check for the event
    TimerEventType type; // Type of event
    bool triggered; // Whether the condition has been met
    bool alreadyChecked; // Whether the event has already been checked this frame

    TimerEvent(int id, int checkTime, TimerEventType type) 
        : id(id), time(0), checkTime(checkTime), type(type), 
          triggered(false), alreadyChecked(false) {}

    void Update(int deltaTime) {
        time += deltaTime;
    }

    void Reset() {
        time = 0;
        triggered = false;
        alreadyChecked = false;
    }
};

class Timer {
private:
    double time;
    std::vector<TimerEvent> events;

public:
    Timer() : time(0) {}

    void Update(double deltaTime) {
        time += deltaTime;

        // Update event times
        for (auto& evt : events) {
            evt.Update(static_cast<int>(deltaTime * 1000));
        }
    }

    void SetTime(int time) {
        this->time = time / 1000.0;
    }

    int GetTime() const {
        return static_cast<int>(time * 1000);
    }

    int GetHundreds() const {
        return static_cast<int>(time * 100);
    }

    int GetSeconds() const {
        return static_cast<int>(time);
    }

    int GetMinutes() const {
        return static_cast<int>(time / 60);
    }

    int GetHours() const {
        return static_cast<int>(time / 3600);
    }

    bool CheckEvent(int evtID, int checkTime, TimerEventType type) {
        // Check if event exists, if it does not, add it
        auto it = std::find_if(events.begin(), events.end(), 
            [evtID](const TimerEvent& evt) { return evt.id == evtID; });

        if (it == events.end()) {
            events.emplace_back(evtID, checkTime, type);
            return false;
        }

        TimerEvent& evt = *it;

        if (evt.type == TimerEventType::Equals) {
            if (time * 1000 < evt.checkTime) {
                evt.triggered = false;
                evt.alreadyChecked = false;
            }
        }

        // Check if the event has already been checked this frame
        if (evt.alreadyChecked) {
            return false;
        }

        // Check conditions
        switch (type) {
            case TimerEventType::Every:
                if (evt.time >= evt.checkTime) {
                    evt.triggered = true;
                    evt.alreadyChecked = true;
                    evt.Reset();
                    return true;
                }
                break;
            case TimerEventType::Equals:
                if (time * 1000 >= evt.checkTime && !evt.triggered) {
                    evt.triggered = true;
                    evt.alreadyChecked = true;
                    return true;
                }
                break;
            case TimerEventType::GreaterThan:
                if (time * 1000 > evt.checkTime) {
                    evt.triggered = true;
                    return true;
                }
                break;
            case TimerEventType::LessThan:
                if (time * 1000 < evt.checkTime) {
                    evt.triggered = true;
                    return true;
                }
                break;
        }

        return false;
    }
}; 