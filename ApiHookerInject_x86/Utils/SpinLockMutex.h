#pragma once
#include <windows.h>

struct SpinLockMutex {
	volatile LONG isLocked = FALSE;

	void Enter()
	{
		SIZE_T spinCount = 0;

		// Wait until the flag is FALSE.
		while (InterlockedCompareExchange(&isLocked, TRUE, FALSE) != FALSE)
		{
			// No need to generate a memory barrier here, since InterlockedCompareExchange()
			// generates a full memory barrier itself.

			// Prevent the loop from being too busy.
			if (spinCount < 32)
				Sleep(0);
			else
				Sleep(1);

			spinCount++;
		}
	}

	void Leave()
	{
		// No need to generate a memory barrier here, since InterlockedExchange()
		// generates a full memory barrier itself.

		InterlockedExchange(&isLocked, FALSE);
	}
};