#include "Application.h"
#include "CommonProperties.h"
#include "UltimateFullscreenExtension.h"
void UltimateFullscreenExtension::Initialize() {

}
void UltimateFullscreenExtension::Update(float deltaTime) {

}
void UltimateFullscreenExtension::GoFullscreen() {
    //Application::Instance().GetBackend()->Fullscreen(true);
    //isFullscreen = true;
}
void UltimateFullscreenExtension::GoWindowed() {
    //Application::Instance().GetBackend()->Windowed();
    //isFullscreen = false;
}
UltimateFullscreenExtension::UltimateFullscreenExtension(short flags) {
    
}