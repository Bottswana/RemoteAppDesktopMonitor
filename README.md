## RemoteApp Monitor Service
Since 2012 and 2012 R2, Microsoft have removed the ability to allow a Remote Desktop Session Host server to publish both a Desktop and RemoteApp.
Publishing a RemoteApp in a collection will remove the Desktop icon from the RemoteApp feed and website.

**What does it do**

This service makes it so that a Collection can have both a Desktop and RemoteApp Icon.

**The Problem**

When a Collection switches from Desktop to RemoteApp, a registry key is set from 1 to 0, which hides the icon. One solution is to set this back, which makes it available again. However, Windows regularly changes this back whenever the Broker servers are restarted, or changes are made to the RemoteApp's published or the collection settings itself.
It is possible to set permissions to prevent changing of the registry key, but this results in errors when publishing RemoteApps and other problems when regenerating the Desktop icon's settings.

Instead, This service simply monitors the key and changes it back whenever Windows disables it, thus allowing changes and preventing errors, but keeping both the Desktop and RemoteApp icons published correctly.

**The Registry Key**

The registry key that controls the Remote Desktop Icon is the following:
HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Terminal Server\CentralPublishedResources\PublishedFarms\<collection>\RemoteDesktops\<collection>\ShowInPortal


## How to use the service
- Install the Service using InstallUtil.exe RemoteDesktopService.exe
- Create the Registry Key 'ForcePortal' DWORD value 1 in the same location as the ShowInPortal entry for the entry you would like to remain published
- The service will now ensure ShowInPortal is always set to 1

