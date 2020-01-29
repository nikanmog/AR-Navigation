
## Create a database account

Add an Azure Cosmos Database to the resource group.

1.  In a new browser window, sign in to the  [Azure portal](https://portal.azure.com/).
    
2.  In the left navigation pane, select  **Create a resource**. Select  **Databases**  and then select  **Azure Cosmos DB**.
    
    ![Screenshot of the Azure portal, highlighting More Services, and Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/includes/media/cosmos-db-create-dbaccount-table/create-nosql-db-databases-json-tutorial-1.png)
    
3.  On the  **Create Azure Cosmos DB Account**  page, enter the settings for the new Azure Cosmos DB account:

	Subscription: Your subscription
Resource Group: Use the same RG
Account Name: Enter a unique name
API: Azure Table
Location: Select the region closest to your users

You can leave the  **Geo-Redundancy**  and  **Multi-region Writes**  options at their default values (**Disable**) to avoid additional RU charges. You can skip the  **Network**  and  **Tags**  sections.
    
4.  Select  **Review+Create**. After the validation is complete, select  **Create**  to create the account.
    
    ![The new account page for Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/includes/media/cosmos-db-create-dbaccount-table/azure-cosmos-db-create-new-account.png)
    
5.  It takes a few minutes to create the account. You'll see a message that states  **Your deployment is underway**. Wait for the deployment to finish and then select  **Go to resource**.
    
    ![The Azure portal notifications pane](https://docs.microsoft.com/en-us/azure/includes/media/cosmos-db-create-dbaccount-table/azure-cosmos-db-account-created.png)
    

Copy the  `Connection String`  because you'll need it.

## Make minor changes to the SharingService files
In  **Solution Explorer**, open  `Cosmos-Backend\appsettings.json`.

Locate the  `StorageConnectionString`  property, and set the value to be the same as the  `Connection String`  value that you copied in the  create a database account step. Save the file.


## Deploy your Sharing Anchors Service
Open Visual Studio, and open the project at the  `Cosmos-Backend\SharingService.sln`.

### Open the publish wizard

In  **Solution Explorer**, right-click the  **SharingService**  project and select  **Publish**.

The Publish Wizard starts. Select  **App Service**  >  **Publish**  to open the  **Create App Service**  dialog box.

### Sign in to Azure

In the  **Create App Service**  dialog box, select  **Add an account**  and sign in to your Azure subscription. If you're already signed in, select the account you want from the drop-down list.

Note: If you're already signed in, don't select  **Create**  yet.

### Use existing resource group

### Create an App Service plan

An  [App Service plan](https://docs.microsoft.com/en-us/azure/app-service/overview-hosting-plans)  specifies the location, size, and features of the web server farm that hosts your app. You can save money when hosting multiple apps by configuring the web apps to share a single App Service plan.

App Service plans define:

-   Region (for example: North Europe, East US, or Southeast Asia)
-   Instance size (small, medium, or large)
-   Scale count (1 to 20 instances)
-   SKU (Free, Shared, Basic, Standard, or Premium)

Next to  **Hosting Plan**, select  **New**.

In the  **Configure Hosting Plan**  dialog box, use these settings:

App Service Plan: "MySharingServicePlan", 
Location: "West US",
Size: "Free"

Select  **OK**.

### Create and publish the web app

In  **App Name**, enter a unique app name (valid characters are  `a-z`,  `0-9`, and  `-`), or accept the automatically generated unique name. The URL of the web app is  `https://<app_name>.azurewebsites.net`, where  `<app_name>`  is your app name.

Select  **Create**  to start creating the Azure resources.

After the wizard finishes, it publishes the ASP.NET Core web app to Azure and then opens the app in your default browser.

![Published ASP.NET web app in Azure](https://docs.microsoft.com/en-us/azure/includes/media/spatial-anchors-azure/web-app-running-live.png)

The app name you used in this section is used as the URL prefix in the format  `https://<app_name>.azurewebsites.net`. Take note of this URL because you'll need it.