## Prerequisites

To complete this tutorial, make sure you have:
- Download and install GitHub Desktop
- Download install UnityHub and create a unity account
- Download and install the latest version of Unity in Unity Hub. **Important:**
- Make sure Visual Studio is selected in the installer **and**
- Depending on your platform, select Universal Windows Platform, iOS or Android Build Support (make sure to open the dropdown menu and select Android SDK as well)

  
## Download this repository
Clone this repository with GitHub Desktop or git clone (console).

## Deploy the sample app
In UnityHub (Projects-> Add), open the the  `Unity`  folder. If the 'Unity Version' column is empty, select your current unity version. Unity might prompt you about a different Unity version between the project and the one you've installed on your machine. This warning is okay, as long as your version of Unity Editor is newer than the one the project was created with. In that case, just click  **Continue**. If your Unity Editor version is older than the one the project needs, click  **Quit**, and upgrade your Unity Editor.

![Unity window](https://docs.microsoft.com/en-us/azure/includes/media/spatial-anchors-unity/unity-window.png)



Open  **Build Settings**  by selecting  **File**  >  **Build Settings**.

In the  **Platform**  section, select  **Android/iOS/Universal Windows Platform**. Select  **Switch Platform**  to change the platform. Unity might prompt you to install Android/iOS/UWP support components if they're missing.

![Unity Build Settings window](https://docs.microsoft.com/en-us/azure/includes/media/spatial-anchors-unity/unity-android-build-settings.png)

Close the  **Build Settings**  window.

## Configure the account identifier and key

In the  **Project**  pane, navigate to  `Assets/AzureSpatialAnchors.Examples/Scenes`  and open the  `AR-Navigation.unity`  scene file.

The next step is to configure the app to use your account identifier and account key. You copied them into a text editor when  setting up the Spatial Anchors resource.

In the  **Project**  pane, navigate to  `Assets\AzureSpatialAnchors.SDK\Resources`. Select  `SpatialAnchorConfig`. Then, in the  **Inspector**  pane, enter the  `Account Key`  as the value for  `Spatial Anchors Account Key`  and the  `Account ID`  as the value for  `Spatial Anchors Account Id`.

In the  **Project**  pane, navigate to  `Assets\AzureSpatialAnchors.Examples\Resources`. Select  `SpatialAnchorSamplesConfig`. Then, in the  **Inspector**  pane, enter the  `Sharing Anchors Service url`  (from your ASP.NET web app Azure deployment) as the value for  `Base Sharing Url`, replacing  `index.html`  with  `api/anchors`. It should look like this:  `https://<app_name>.azurewebsites.net/api/anchors`.

Save the scene by selecting  **File**  >  **Save**.

## Deploy to your device

### Deploy to Android device

Sign in on your Android device and connect it to your computer by using a USB cable.

Open  **Build Settings**  by selecting  **File**  >  **Build Settings**.

Under  **Scenes In Build**, ensure all the scenes have a check mark next to them.

Make sure  **Export Project**  doesn't have a check mark. Select  **Build And Run**. You'll be prompted to save your  `.apk`  file. You can pick any name for it.

### Deploy to HoloLens

Open  **Build Settings**  by selecting  **File**  >  **Build Settings**.

Under  **Scenes In Build**, ensure all the scenes have a check mark next to them.

Select  **Build**. In the dialog box, select a folder in which to export the HoloLens Visual Studio project.

When the export is complete, a folder containing the exported HoloLens project will appear.

In the folder, double-click  **AR-Navigation.sln**  to open the project in Visual Studio.

Change the  **Solution Configuration**  to  **Release**, change the  **Solution Platform**  to  **x86 (HoloLens 1)**, and select  **Device**  from the deployment target options.

If using HoloLens 2, use  **ARM64**  as the  **Solution Platform**, instead of  **x86**.

![Visual Studio configuration](https://docs.microsoft.com/en-us/azure/spatial-anchors/quickstarts/media/get-started-unity-hololens/visual-studio-configuration.png)

Turn on the HoloLens device, sign in, and connect the device to the PC by using a USB cable.

Select  **Debug**  >  **Start debugging**  to deploy your app and start debugging.

Follow the instructions in the app to place and recall an anchor.

In Visual Studio, stop the app by selecting either  **Stop Debugging**  or Shift+F5.


### Deploy to an iOS device

Open  **Build Settings**  by selecting  **File**  >  **Build Settings**.

Under  **Scenes In Build**, ensure all the scenes have a check mark next to them.

Select  **Build**. In the dialog box that opens, select a folder to export the Xcode project to.

When the export is complete, a folder that contains the exported Xcode project will appear.

Note: 
If a window asking you if you want to replace or append appears, we recommend that you select  **Append**  because it's faster. You should only need to select  **Replace**  if you're changing assets in your scene. (For example, if you're adding, removing, or changing parent/child relationships, or if you're adding, removing, or changing properties.) If you're only making source code changes,  **Append**  should be enough.

#### Convert the Xcode project to xcworkspace containing Azure Spatial Anchors references
In the exported Xcode project folder, run this command in the Terminal to install the necessary CocoaPods for the project:

bashCopy

```
pod install --repo-update

```

Now you can open  `Unity-iPhone.xcworkspace`  to open the project in Xcode:

bashCopy

```
open ./Unity-iPhone.xcworkspace

```

Note

See the troubleshooting steps  [here](https://docs.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-ios#cocoapods-issues-on-macos-catalina-1015)  if you're having CocoaPod issues after upgrading to macOS Catalina (10.15).

Select the root  **Unity-iPhone**  node to view the project settings, and then select the  **General**  tab.

Under  **Signing**, make sure  **Automatically manage signing**  is enabled. If it's not, enable it, and then select  **Enable Automatic**  in the dialog box that appears to reset the build settings.

Under  **Deployment Info**, make sure the  **Deployment Target**  is set to  `11.0`.

#### Deploy the app to your iOS device
Connect the iOS device to the Mac and set the  **active scheme**  to your iOS device.

![Select the device](https://docs.microsoft.com/en-us/azure/includes/media/spatial-anchors-unity/select-device.png)

Select  **Build and then run the current scheme**.

![Deploy and run](https://docs.microsoft.com/en-us/azure/includes/media/spatial-anchors-unity/deploy-run.png)

Note

If you see a  `library not found for -lPods-Unity-iPhone`  error, you probably opened the  `.xcodeproj`  file instead of the  `.xcworkspace`  file.

start a new session, and then locate it.

In Xcode, stop the app by selecting  **Stop**.
