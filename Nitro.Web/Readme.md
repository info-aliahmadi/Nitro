

Add-Migration dbVersion_1 -Context MigrationContext -StartupProject Nitro.Web


 Update-Database -Context MigrationContext -verbose