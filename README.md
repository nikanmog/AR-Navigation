# Azure Spatial Anchors Navigation Demo

Welcome to the ASA Navigation repository on GitHub. Here you can find an indoor navigation that was build using the ASA service. The app is capable of guiding a user on a predefined path (an animated dog is the "guide") and includes multi-platform support (iOS/Android/HoloLens).

Azure Spatial Anchors is a managed cloud service and platform that enables multi-user, spatially aware mixed reality and augmented reality (MR/AR) apps for HoloLens, iOS devices with ARKit, and Android devices with ARCore. For more information, see [Azure Spatial Anchors documentation](https://docs.microsoft.com/azure/spatial-anchors/overview "Azure Spatial Anchors Documentation")

![Implementation System Overview](Unity/SystemOverview.jpg "Implementation System Overview")

# First time setup
This tutorial is a modified version of the original documentation found [here](https://docs.microsoft.com/en-us/azure/spatial-anchors/tutorials/tutorial-share-anchors-across-devices?tabs=VS%2CUnity).

## Create a Spatial Anchors resource

Go to the  [Azure portal](https://portal.azure.com/).

In the left navigation pane in the Azure portal, select  **Create a resource**.

Use the search box to search for  **Spatial Anchors**.

![Search for Spatial Anchors](https://docs.microsoft.com/en-us/azure/includes/media/spatial-anchors-get-started-create-resource/portal-search.png)

Select  **Spatial Anchors**. In the dialog box, select  **Create**.

In the  **Spatial Anchors Account**  dialog box:

-   Enter a unique resource name, using regular alphanumeric characters.
    
-   Select the subscription that you want to attach the resource to.
    
-   Create a resource group by selecting  **Create new**. Name it  **myResourceGroup**  and select  **OK**. A  [resource group](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/overview#terminology)  is a logical container into which Azure resources like web apps, databases, and storage accounts are deployed and managed. For example, you can choose to delete the entire resource group in one simple step later.
    
-   Select a location (region) in which to place the resource.
    
-   Select  **New**  to begin creating the resource.
    
    ![Create a resource](https://docs.microsoft.com/en-us/azure/includes/media/spatial-anchors-get-started-create-resource/create-resource-form.png)
    

After the resource is created, Azure Portal will show that your deployment is complete. Click  **Go to resource**.

![Deployment complete](https://docs.microsoft.com/en-us/azure/includes/media/spatial-anchors-get-started-create-resource/deployment-complete.png)

Then, you can view the resource properties. Copy the resource's  **Account ID**  value into a text editor because you'll need it later.

![Resource properties](https://docs.microsoft.com/en-us/azure/includes/media/spatial-anchors-get-started-create-resource/view-resource-properties.png)

Under  **Settings**, select  **Key**. Copy the  **Primary key**  value into a text editor. This value is the  `Account Key`. You'll need it later.

![Account key](https://docs.microsoft.com/en-us/azure/includes/media/spatial-anchors-get-started-create-resource/view-account-key.png)



## Create and publish app backend
Please create the app backend as described [here](Webservice/readme.md)

## Android Instructions
1. Copy the .apk file to your android device and install it.
2. Enter your webservice url, ASA Account ID and ASA Primary key in the app. [Microsoft Internal Instruction Video](https://msit.microsoftstream.com/video/c6b55dfe-5fc7-4563-bf5b-d53f6ae8a11c)

# How to use the demo
## AR Navigation Demo
The workflow for the navigation demo is simple. There is a "Creator Mode" and a "Visitor Mode".
On app launch the app checks whether you have already created anchors in a previous session or not (by contacting the webservice & CosmosDB). If no anchor is found, the creator mode is launched. If an anchor is found, the visitor mode is launched and all anchors are retrieved from the server. 
- Creator Mode: Place anchors for the guide to follow. The guide will use the same sequence in which the anchors were created. 
	- Important: Please make sure your anchors are not too far apart (especially on older devices). Otherwise the app will get stuck in the "saving" step (no connection can be made between two anchors). To fix this you have to delete all anchors (via the REST API) and start over :(
- Visitor Mode: The app will download all anchors on app launch. Please click next to initiate anchor search in your environment.
## Conference Room Demo
- Open the AR-Navigation scene and set the guide object to null (in the AzureSpatialAnchors object)
- Set the prefabs 1-8 to "Conference room busy.prefab" or "Conference room available.prefab"
- The app will have the same workflow as in the navigation demo above (creator/visitor mode)
- The prefab in visitor mode is set by the prefab ID which is downloaded at app start from the CosmosDB. To modify the prefab type at a specific position (e.g. change conference room monitor from free to busy) edit the number in the AnchorType column with the CosmosDB Data Explorer on portal.azure.com)

## Delete or modify anchors
- Anchors can be delete via the REST API of your webservice:
	- Go to https://your-webserivce.azurewebsites.net/index.html in your browser
	- Click "/api/anchors/delete" and then execute
- You can set the prefab type remotely via the CosmosDB
	- Open the AR-Navigation scene and assign prefabs (1-8) (in the AzureSpatialAnchors object) 
	- After creating your objects you can modify the prefab type via the CosmosDB Data Explorer on portal.azure.com

# More information about the Azure Spatial Anchors Service 

[Create and locate anchors](https://docs.microsoft.com/azure/spatial-anchors/concepts/create-locate-anchors-unity "Create/locate anchors")

[MR/AR sharing across devices and sessions](https://docs.microsoft.com/azure/spatial-anchors/tutorials/tutorial-use-cosmos-db-to-store-anchors "Sharing across sessions")

[Anchor Relationships](https://docs.microsoft.com/azure/spatial-anchors/concepts/anchor-relationships-way-finding "Anchor Relationships")

[Experience Guidelines](https://docs.microsoft.com/en-us/azure/spatial-anchors/concepts/guidelines-effective-anchor-experiences "Experience Guidelines")

[Frequently Asked Questions (FAQ)](https://docs.microsoft.com/azure/spatial-anchors/spatial-anchor-faq "FAQ")

## CC Attributions
Many thanks to Shiba Inu. for his Dog model!
