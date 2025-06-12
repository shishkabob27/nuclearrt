
#include "Application.h"

int main(int argc, char *argv[])
{
	Application& app = Application::Instance();
	app.Initialize();
	app.Run();
	return 0;
}
