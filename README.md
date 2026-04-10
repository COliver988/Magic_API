# NOTE
The company this was written for decided "to go in a different direction". As this souce code is not going to be used, I figured I would preserve it here.
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
