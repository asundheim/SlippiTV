#include <iostream>
#include <filesystem>
#include <thread>
#include <chrono>
#include <windows.h>
#include <tlhelp32.h>

namespace fs = std::filesystem;

bool KillProcessByName(const std::wstring& processName) {
    bool killed = false;
    HANDLE hSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnap == INVALID_HANDLE_VALUE) return false;

    PROCESSENTRY32W pe;
    pe.dwSize = sizeof(PROCESSENTRY32W);
    if (Process32FirstW(hSnap, &pe)) {
        do {
            if (processName == pe.szExeFile) {
                HANDLE hProc = OpenProcess(PROCESS_TERMINATE, FALSE, pe.th32ProcessID);
                if (hProc) {
                    if (TerminateProcess(hProc, 0)) killed = true;
                    CloseHandle(hProc);
                }
            }
        } while (Process32NextW(hSnap, &pe));
    }
    CloseHandle(hSnap);
    return killed;
}

int main(int argc, char* argv[])
{
    if (argc < 3) {
        std::cerr << "Usage: SlippiTV.Updater <zipExtractDir> <zipDestination>\n";
        return 1;
    }

    std::string zipExtractDir = argv[1];
    std::string zipDestination = argv[2];
    std::string exeName = "SlippiTV.exe";
    fs::path exePath = fs::current_path() / exeName;
    fs::path newExePath = fs::path(zipExtractDir) / exeName;

    std::cout << "====================================================\n";
    std::cout << "  ____    _   _                   _   _______     __\n";
    std::cout << " / ___|  | | (_)  _ __    _ __   (_) |_   _\\ \\   / /\n";
    std::cout << " \\___ \\  | | | | |  _ \\  |  _ \\  | |   | |  \\ \\ / / \n";
    std::cout << "  ___) | | | | | | |_) | | |_) | | |   | |   \\ V /  \n";
    std::cout << " |____/  |_| |_| | .__/  | .__/  |_|   |_|    \\_/   \n";
    std::cout << "                 |_|     |_|                        \n";
    std::cout << "====================================================\n";

    // Wait for parent process to exit
    std::this_thread::sleep_for(std::chrono::seconds(2));

    // Verify SlippiTV.exe exists
    if (!fs::exists(exePath)) {
        std::cerr << "[ERROR] SlippiTV.exe not found in the current directory.\n";
        return 1;
    }

    // Kill all active instances of SlippiTV.exe
    std::cout << "[INFO] Checking for running instances of SlippiTV.exe...\n";
    if (KillProcessByName(L"SlippiTV.exe")) {
        std::cout << "[INFO] All instances of SlippiTV.exe terminated.\n";
    } else {
        std::cout << "[INFO] No active instances of SlippiTV.exe found.\n";
    }

    // Replace the old .exe with the new .exe
    std::cout << "[INFO] Replacing the old SlippiTV.exe with the new version...\n";
    try {
        fs::remove(exePath);
        fs::rename(newExePath, exePath);
        std::cout << "[INFO] Replacement successful.\n";
    } catch (const std::exception& e) {
        std::cerr << "[ERROR] Failed to replace SlippiTV.exe: " << e.what() << "\n";
        return 1;
    }

    // Delete the zip
    try {
        fs::remove(zipDestination);
        std::cout << "[INFO] ZIP file removed.\n";
    } catch (...) {
        std::cout << "[WARNING] Could not delete ZIP file. You may remove it manually.\n";
    }

    // Delete the extracted directory
    try {
        fs::remove_all(zipExtractDir);
        std::cout << "[INFO] Extracted directory removed.\n";
    } catch (...) {
        std::cout << "[WARNING] Could not delete extracted directory. You may remove it manually.\n";
    }

    // Relaunch SlippiTV.exe
    std::cout << "[INFO] Relaunching SlippiTV.exe...\n";
    STARTUPINFOW si = { sizeof(si) };
    PROCESS_INFORMATION pi;
    if (CreateProcessW(
        exePath.wstring().c_str(),
        NULL,
        NULL,
        NULL,
        FALSE,
        0,
        NULL,
        exePath.parent_path().wstring().c_str(),
        &si,
        &pi
    )) {
        std::cout << "[INFO] SlippiTV.exe relaunched successfully.\n";
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    } else {
        std::cerr << "[ERROR] Failed to relaunch SlippiTV.exe.\n";
        return 1;
    }

    return 0;
}
