# Magic API 
The Magic API gives API access to various tables in both the Magic and Exenta DBs

Pulled to its own application from Peeps 2.
## Env variables needed
* CONNECTIONSTRINGS__DATABASE__MAGIC - SQL connection string for the magic DB
* CONNECTIONSTRINGS__DATABASE__EXENTA - SQL connection string for Exenta DB
* CONNECTIONSTRINGS__DATABASE__SERILOG - Postgres connection string - not used?
* CONNECTIONSTRINGS__DATABASE__<xx>SHOPFLOOR - SQL connection strings for the various shop floor instances (MWW, SP, GM, TJ)
* JWT settings
  * AUTHSETTINGS__PRIVATEKEY - used to set up JWT tokens for authentication
  * AUTHSETTINGS__AUDIENCE - used to set up JWT tokens for authentication
  * AUTHSETTINGS__TIMEOUT
  * AUTHSETTINGS__ISSUER

## Deploying
Currently a manual process. On your local machine:
1. verify you have the current version of the `main` branch
2. at the application directory, on the command line: `dotnet publish -c Release`
3. copy the bin/Release/net8.0/publish directory
4. on the server, go to IIS and turn off the magic API site
5. replace the files in c:\inetpub\MWWApplications\MWWMagic_API with the new publish contents
6. restart the web service
7. verify it is working

Long term we do want to automate this process. So small now it really does not matter.

## Endpoints
### Exenta
* CustomerBOLShipment
* OrderHeader
* InvoiceOrderHeader
### Magic
* DAP Partners
* MWW_Applications
* ProductOverrides
* StuckProductionOrders
### Services
* ResetWorkOrder
### Reports
* OrderReport
