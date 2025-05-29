# Magic API 
The Magic API gives API access to various tables in both the Magic and Exenta DBs

Pulled to its own application from Peeps 2.
## Env variables needed
CONNECTIONSTRINGS__DATABASE__MAGIC

CONNECTIONSTRINGS__DATABASE__EXENTA

CONNECTIONSTRINGS__DATABASE__SERILOG

AUTHSETTINGS__PRIVATEKEY

AUTHSETTINGS__AUDIENCE

AUTHSETTINGS__TIMEOUT

AUTHSETTINGS__ISSUER


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
