// TrueCryptNativeTestLib.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "TrueCryptNativeTestLib.h"
#include "crc.h"
#include "Tcdefs.h"

//
//// This is an example of an exported variable
//TRUECRYPTNATIVETESTLIB_API int nTrueCryptNativeTestLib=0;
//
//// This is an example of an exported function.
//TRUECRYPTNATIVETESTLIB_API int fnTrueCryptNativeTestLib(void)
//{
//    return 42;
//}

// This is the constructor of a class that has been exported.
// see TrueCryptNativeTestLib.h for the class definition

TRUECRYPTNATIVETESTLIB_API bool MixKeyFile(Password *password, const char* fileName)
{
	static unsigned __int8 keyPool[KEYFILE_POOL_SIZE];
	size_t i;

	VirtualLock(keyPool, sizeof(keyPool));
	memset(keyPool, 0, sizeof(keyPool));

	KeyFileProcess(keyPool, fileName);

	/* Mix the keyfile pool contents into the password */
	for (i = 0; i < sizeof(keyPool); i++)
	{
		if (i < password->Length)
			password->Text[i] += keyPool[i];
		else
			password->Text[i] = keyPool[i];
	}

	if (password->Length < (int)sizeof(keyPool))
		password->Length = sizeof(keyPool);

	return true;
};

TRUECRYPTNATIVETESTLIB_API bool KeyFileProcess(unsigned __int8 *keyPool, const char*  fileName)
{
	FILE *f;
	unsigned __int8 buffer[64 * 1024];
	unsigned __int32 crc = 0xffffffff;
	int writePos = 0;
	size_t bytesRead, totalRead = 0;
	int status = TRUE;

	HANDLE src;
	FILETIME ftCreationTime;
	FILETIME ftLastWriteTime;
	FILETIME ftLastAccessTime;

	BOOL bTimeStampValid = FALSE;

	/* Remember the last access time of the keyfile. It will be preserved in order to prevent
	an adversary from determining which file may have been used as keyfile. */
	src = CreateFileA(fileName,
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);

	if (src != INVALID_HANDLE_VALUE)
	{
		if (GetFileTime((HANDLE)src, &ftCreationTime, &ftLastAccessTime, &ftLastWriteTime))
			bTimeStampValid = TRUE;
	}


	f = fopen(fileName, "rb");
	if (f == NULL) return FALSE;

	while ((bytesRead = fread(buffer, 1, sizeof(buffer), f)) > 0)
	{
		size_t i;

		if (ferror(f))
		{
			status = FALSE;
			goto close;
		}

		for (i = 0; i < bytesRead; i++)
		{
			crc = UPDC32(buffer[i], crc);

			keyPool[writePos++] += (unsigned __int8)(crc >> 24);
			keyPool[writePos++] += (unsigned __int8)(crc >> 16);
			keyPool[writePos++] += (unsigned __int8)(crc >> 8);
			keyPool[writePos++] += (unsigned __int8)crc;

			if (writePos >= KEYFILE_POOL_SIZE)
				writePos = 0;

			if (++totalRead >= KEYFILE_MAX_READ_LEN)
				goto close;
		}
	}

	if (ferror(f))
	{
		status = FALSE;
	}
	else if (totalRead == 0)
	{
		status = FALSE;
		SetLastError(ERROR_HANDLE_EOF);
	}

close:
	DWORD err = GetLastError();
	fclose(f);

	if (bTimeStampValid)//&& !IsFileOnReadOnlyFilesystem (keyFile->FileName))
	{
		// Restore the keyfile timestamp
		SetFileTime(src, &ftCreationTime, &ftLastAccessTime, &ftLastWriteTime);
	}

	SetLastError(err);
	return status;
}