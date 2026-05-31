#include "main.h"

#include <keychain/keychain.h>

keychain::Error* lastError = nullptr;

extern "C" {

EXPORTED bool setPassword(const char* package, const char* service, const char* user, const char* password)
{
	keychain::Error* error = new keychain::Error();
	keychain::setPassword(package, service, user, password, *error);

	if (error->type != keychain::ErrorType::NoError)
	{
		lastError = error;
		return false;
	}

	delete error;

	return true;
}

EXPORTED const char* getPassword(const char* package, const char* service, const char* user)
{
	keychain::Error* error = new keychain::Error();
	auto password = keychain::getPassword(package, service, user, *error);

	if (error->type != keychain::ErrorType::NoError)
	{
		lastError = error;
		return nullptr;
	}

	char* writable = new char[password.size() + 1];
	std::copy(password.begin(), password.end(), writable);
	writable[password.size()] = '\0';

	delete error;

	return writable;
}

EXPORTED bool deletePassword(const char* package, const char* service, const char* user)
{
	keychain::Error* error = new keychain::Error();
	keychain::deletePassword(package, service, user, *error);

	if (error->type != keychain::ErrorType::NoError)
	{
		lastError = error;
		return false;
	}

	delete error;

	return true;
}

EXPORTED const char* getLastErrorMessage()
{
	if (!lastError)
		return nullptr;

	char* writable = new char[lastError->message.size() + 1];
	std::copy(lastError->message.begin(), lastError->message.end(), writable);
	writable[lastError->message.size()] = '\0';

	return writable;
}

EXPORTED keychain::ErrorType getLastError()
{
	if (!lastError)
		return keychain::ErrorType::NoError;

	return lastError->type;
}

// Returns tab/newline-delimited entries: "service\tuser\n..." or nullptr on error.
// Each line is one entry: service TAB user.
EXPORTED const char* listPasswords(const char* package)
{
	keychain::Error* error = new keychain::Error();
	auto entries = keychain::listPasswords(package, *error);

	if (error->type != keychain::ErrorType::NoError)
	{
		lastError = error;
		return nullptr;
	}

	delete error;

	std::string result;
	for (const auto& entry : entries)
	{
		result += entry.service + "\t" + entry.user + "\n";
	}

	char* writable = new char[result.size() + 1];
	std::copy(result.begin(), result.end(), writable);
	writable[result.size()] = '\0';

	return writable;
}
}
