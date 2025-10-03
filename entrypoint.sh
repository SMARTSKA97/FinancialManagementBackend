# entrypoint.sh
#!/bin/sh
: "${PORT:=10000}"
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"
exec dotnet FinancialPlanner.API.dll
