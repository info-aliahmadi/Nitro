
docker-compose up -d
http://localhost:9200
http://localhost:5601



Add-Migration dbVersion_1 -Context MigrationContext -StartupProject Nitro.Web


 Update-Database -Context MigrationContext -verbose