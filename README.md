# ParanoidOneDriveBackup
Backup your OneDrive cloud files to any (Linux) Server

## Motivation

It happened two times to me that I deleted my whole OneDrive cloud files by making a mistake while using some OneDrive client on Linux. Altough you can recover your cloud files from the trash in OneDrive it's very annoying to do that file by file and if you recover all files you have a lot of old trash files in your OneDrive that you deleted before. Another problem is that deleted OneNote notebooks don't go to the trash if you delete them by using a third party sync client. This application tries to prevent such issues by giving the possibility to backup your cloud files (including your notebooks) any time you want to some (Linux) server. 

## Prerequisites

- Linux or Windows 10 (64 bit)
- [.NET Core](https://dotnet.microsoft.com/download) x64 runtime

## Setup

The application is designed to run as a service on both platforms, but you should have no problem using it as a normal console application. So just running it without the following setup steps should work totally fine, but then you have to manage running the application at specific times yourself.

### Linux

Download the latest release files and extract them to `~/ParanoidOneDriveBackup` (or another folder of your choice). 

Run the app for the first time: `~/ParanoidOneDriveBackup/ParanoidOneDriveBackup`

The app copies the default config file to your application data folder. Edit it to your needs: `nano ~/.config/ParanoidOneDriveBackup/.config.json`. You can find more information to the MS graph client id at the MS graph section of this readme.

Now run the app another time. If your config file is valid the app prints instructions on how to authorize in the console. Once you have authorized the app to read your OneDrive files it should start the first backup directly. (You can cancel it if you want)

##### Registering the app to systemd:

Copy the two systemd files (.service, .timer) to `/etc/system/` and modify them to your needs. The default files specify that the app should do a backup every day at 4 pm.

Start the service with `systemctl start ParanoidOneDriveBackup.timer` and check that there are no errors in the log with `journalctl -u ParanoidOneDriveBackup`. 

If you want your service to start on startup of your system you can call `systemctl enable ParanoidOneDriveBackup.timer`.

### Windows

currently not supported as a service (coming soon)



## MS Graph

The application uses the Microsoft Graph API to backup your files. 

You can try using my registered app with the client id: ------------------------------------------------------------------------------

If this client id does not work for you, you can register your own app following [this article](https://docs.microsoft.com/de-de/graph/auth-register-app-v2). Make sure that `https://login.microsoftonline.com/common/oauth2/nativeclient` is activated as redirect URI and that the checkbox "Default client type" is on "Yes" (both settings can be found at the "Authentication" tab).

Additionally you need to specify the necessary scopes of your the application. You can do that at the "API permissions" tab. Search for the scopes that are specified in the default config file and add them to your app's permissions. Finally copy the client id at the "Overview" tab and insert it in your config file.

## Implementation Notes

- The config file is stored at:
  - `~/.config/ParanoidOneDriveBackup/config.json` (Linux)
  - `%localappdata%/ParanoidOneDriveBackup/config.json` (Windows)
- The token cache file is stored at:
  - `~/.cache/ParanoidOneDriveBackup/token_cache.bin3` (Linux)
  - `%localappdata%/ParanoidOneDriveBackup/token_cache.bin3` (Windows)