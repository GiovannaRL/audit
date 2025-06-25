# AudaxWare xPlanner

The xPlanner solution has a set of projects that compose the xPlanner portal and tools.

# Getting Started

The first step is to retrieve the source to your local folder and then install the pre-requisites. You can see further details in the next sub-topics.


## Prerequisites

You must have `Visual Studio 2015` or earlier. If you install Visual Studio 2019 all the `pre-requisites` will be properly installed. You will also need to install SQL Server Express 2017 and SQL Management Studio tools. 



## Setting up a local database

The first step is to restore a recent backup from the production or development environments and restore on your local machine with the database name `audaxware`. Alternatively, you can publish an empty datbase directly from the solution, but in this case you will not have any data, including the catalog.

### Creating database backup

You must connect to the database instance running on the cloud at `audaxware.database.windows.net` with the proper credentials. When you connect, you will see two databases `audaxware` and `audaxware_dev`. The `audaxware_dev` database is for the development environment and the `audaxware` is for production. For debug, you must use the `audaxware_dev`. Please notice that some tables in the database have links to the environment (e.g.: blob storage), so if you pick the database from production it will not work unless you make some changes. See section "cloning the production database". You must follow the steps below to perform the backup:

- Login to `audaxware.database.windows.net` using SQL Management Studio
- Go into the database, expand Security->Security Policies and disable the enterpriseSecurityPolicy
- Right-Click on the Database, select tasks and then Export Data Tier application
- Click next, then in the box `Save to local disk` select a local path on your computer
- Click Next and the Finish to create the backup
- `IMPORTANT`: Re-enable the security policies to ensure we have the proper protection

### Restoring the database backup
If you have an older backup, you must delete it. Renaming the database can cause conflict on your data files, so if you would like to have a copy of your local database (in case the restore fails), you must follow the previous steps but on your local system. You must then follow these steps to restore the database on your local system:

- Make sure you have a local instace called "SQLEXPRESS". If you use a different name, the AudaxWare environment will not be able to debug locally.
- Login to (LOCAL)\SQLEXPRESS using SQL Management Studio
- Rick-Click on the `Databases` folder and select `Import Data-tier application`
- Click Next and then select the backup file in the `Import from local disk` field
- Click Next and in the field `New database name` type `audaxware`
- Click Next to complete


# Remote Debugging on DEV
1. Open the following URL the Dev Slot Deployment Center by clicking [here](https://portal.azure.com/#@lourencoteodoro1audaxware.onmicrosoft.com/resource/subscriptions/97d3b2ca-1fbd-421b-b76b-f9f2662c76f8/resourceGroups/main/providers/Microsoft.Web/sites/audaxwareCentralUS/slots/DEV/vstscd)
2. Under the tab FTPS credentials, copy the user name (it should be $audaxwareCentralUS__DEV) and the password, you will need it for publising
	IMPORTANT: Do not use the User Scope, that might also publish to production, which can lead you to mistakes.
3. Open the solution and select build debug for Any CPU and then build
4. Right click on xPlannerAPI and then select publish, then make sure the profile is set to audaxwareCentralUS-DEV - Web Deploy.pubxml
5. Click published, you will be asked for user name and password, enter the ones you pick from step #2
	IMPORTANT: You might get a user name with audaxwareCentralUS__DEV\$audaxwareCentralUS__DEV, make sure you only use the part after \ (e.g.: $audaxwareCentralUS__DEV)
6. On cloud exporer navigate to "Visual Studio Enterprise -> App Services -> audaxwareCentralUS -> Deployment Slots -> audaxwareCentralUS(DEV)", right click and select "Attach Debugger"


When you finish, navigate to the Dev Slot App Service by clicking [here](https://portal.azure.com/#@lourencoteodoro1audaxware.onmicrosoft.com/resource/subscriptions/97d3b2ca-1fbd-421b-b76b-f9f2662c76f8/resourceGroups/main/providers/Microsoft.Web/sites/audaxwareCentralUS/slots/DEV/appServices) and restart the app service. If you do not do this, the debug will fail later


# Making changes to the code
The audaxware powershell module helps you make changes to your source code. The powershell simply makes git command calls to align with AudaxWare standards and process. These are the steps to load the module:
- Start a powershell session
- Run the command
``` Powershell
# cd <RepositoryPath>
cd C:\MyUser\xPlannerRepo\PSModule
# Installs the powershell module
.\PSModule\AudaxWare\LoadAudaxWareModule.ps1

```
It will list all the available commands. The next sub-sections describe the purpose of each command, but you can use `Get-Help <Command>` (e.g.: `Get-Help New-GitBranch`) to get detailed documantation about the command.

Please notice that for all other commands in this section you must be in the repository folder.

## Starting a change (new branch)
To start a change, run the command:
``` Powershell
# This command will prompt you for the parameters
# the user parameter is optional, it will get the user currently logged in, use -user if you want to customize the user name
New-GitBranch
```
## Commiting a change
You can use the regular git command, but if you use the powershell below it will help with the following tasks:
- Ensure you do not commit to `dev` or `main` by mistake
- Ensure you use conventional commits on your comments
- Add all current modifications
- Push the branch to ADO
This will all be done in a single command.
``` Powershell
New-GitCommit
```
## Submitting a PR
Before you can submit a PR, it is important to ensure you have no conflicts with the dev branch. Therefore, you should run the following command to merge the latest dev into the branch you are currently working, resolve conflicts, and then update your branch. If no conflicts are detected, the command will properly update your branch.

``` Powershell
Get-GitDev
```


## Creating a release
To create a new release, we need to checkout the main branch, merge with dev, resolve conflicts (if any) and commit the changes. The following command executes those tasks:

``` Powershell
# This command will generate a release branch
# with the format <user>/release/<date>
New-GitReleaseBranch
```
After you run this command, go to ADO and use the `Compare Branch` feature to check for differences between the branches. If you find differences, make local changes and upload to ensure the branches are the same


# Running the tests


# Making changes to the database

# Recreating the Data Model


# Build



# References

# Authors

* **Louren�o Teodoro** - *Initial work*

