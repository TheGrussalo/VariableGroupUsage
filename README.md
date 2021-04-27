# VariableGroupUsage

This is a simple web app that allows you to select an Azure Variable Group and it will then show you what projects it is used in.

I use this to help determine the impact of any variable groups that I need to update.

## Setup

Update the `appsettings.json` file with the following values:

    "AzureDevOps": {
        "PersonalAccessToken": "",
        "Organisation": "",
        "ProjectName": ""
    }

## PersonalAccessToken

Create a [PAT token](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate) with the following permissions:

- Build (Read)
- Release (Read)
- Variable Groups (Read)

## Organisation

The organisation can be found in the URL that you use to connect to Azure DevOps.
It will be either organisation.visualstudio.com or dev.azure.com/organisation

## ProjectName

This is the project within the organisation that is used.
It can be found in the url organisation.visualstudio.com/projectname or dev.azure.com/organisation/projectname.

## Proxy Server

If the connection uses a proxy server enter the url of the server (along with the port).
