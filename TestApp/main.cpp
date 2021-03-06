#include <windows.h>
#include <stdio.h>

void MyMagicFunction(int i) {
	if (i == 0)
		SetConsoleTitleA("ApiHooker TestApp 2");
	else
		MyMagicFunction(i - 1);
}

int main(void)
{
	SetConsoleTitleA("ApiHooker TestApp");
	MyMagicFunction(10);

	auto consoleWnd = GetStdHandle(STD_OUTPUT_HANDLE);

	SMALL_RECT windowSize = { 0, 0, 69, 34 };
	SetConsoleWindowInfo(consoleWnd, TRUE, &windowSize);

	SetConsoleScreenBufferSize(consoleWnd, { 70, 35 });

	SMALL_RECT writeRegion;
	CHAR_INFO charInfo;
	
	writeRegion = { 0, 0, 10, 10 };
	charInfo = { 'A', FOREGROUND_RED | FOREGROUND_INTENSITY };
	WriteConsoleOutputA(consoleWnd, &charInfo, { 1, 1 }, { 0, 0 }, &writeRegion);

	writeRegion = { 1, 0, 10, 10 };
	charInfo = { 'B', FOREGROUND_GREEN | FOREGROUND_INTENSITY };
	WriteConsoleOutputA(consoleWnd, &charInfo, { 1, 1 }, { 0, 0 }, &writeRegion);

	char consoleTitle[256];
	auto consoleTitleLen = GetConsoleTitleA(consoleTitle, sizeof(consoleTitle));

	printf("Console title: %s, length: %d\n", consoleTitle, consoleTitleLen);

	//getchar();

	Sleep(5000);

	return 42;
}