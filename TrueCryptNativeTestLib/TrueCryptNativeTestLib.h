// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the TRUECRYPTNATIVETESTLIB_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// TRUECRYPTNATIVETESTLIB_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef TRUECRYPTNATIVETESTLIB_EXPORTS
#define TRUECRYPTNATIVETESTLIB_API __declspec(dllexport)
#else
#define TRUECRYPTNATIVETESTLIB_API __declspec(dllimport)
#endif

#define KEYFILE_POOL_SIZE	64
#define	KEYFILE_MAX_READ_LEN	(1024*1024)

#define MIN_PASSWORD			1		// Minimum possible password length
#define MAX_PASSWORD			64		// Maximum possible password length

#define PASSWORD_LEN_WARNING	20		// Display a warning when a password is shorter than this

typedef struct
{
	// Modifying this structure can introduce incompatibility with previous versions
	unsigned __int32 Length;
	unsigned char Text[MAX_PASSWORD + 1];
	char Pad[3]; // keep 64-bit alignment
} Password;

// This class is exported from the TrueCryptNativeTestLib.dll
class TRUECRYPTNATIVETESTLIB_API CTrueCryptNativeTestLib {
public:

	static bool MixKeyFile(Password *password, const char* fileName);
private:
	static bool KeyFileProcess(unsigned __int8 *keyPool, const char* fileName);
//public:
//	CTrueCryptNativeTestLib(void);
//	// TODO: add your methods here.
};
//
//extern TRUECRYPTNATIVETESTLIB_API int nTrueCryptNativeTestLib;

extern "C" {
	TRUECRYPTNATIVETESTLIB_API bool MixKeyFile(Password *password, const char* fileName);
	TRUECRYPTNATIVETESTLIB_API bool KeyFileProcess(unsigned __int8 *keyPool, const char* fileName);
}
