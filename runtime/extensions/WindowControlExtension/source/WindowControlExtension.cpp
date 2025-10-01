#include "Application.h"
#include "CommonProperties.h"
#include "WindowControlExtension.h"
void WindowControlExtension::Initialize() {

}
WindowControlExtension::WindowControlExtension(short flags) {

}
void WindowControlExtension::Update(float deltatime) {
    
}
void WindowControlExtension::SetWindowPosition(int param) {
    switch (param) {
        case 4: // Middle/Middle
            Application::Instance().GetBackend()->ChangeWindowPosX(1920 / 2);
            Application::Instance().GetBackend()->ChangeWindowPosY(1080 / 2);
            break;
        default:
            std::cout << "Parameter out of reach";
    }
}