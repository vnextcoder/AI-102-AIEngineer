@echo off

rem Set values for your Search service
set url=https://ai102srch2647032753.search.windows.net
set admin_key=xxxxxxxxxxxxxxxxx

echo -----
echo Creating the data source...
call curl -X POST %url%/datasources?api-version=2020-06-30 -H "Content-Type: application/json" -H "api-key: %admin_key%" -d @data_source.json

echo -----
echo Creating the skillset...
call curl -X PUT %url%/skillsets/margies-knowledge-skillset?api-version=2020-06-30 -H "Content-Type: application/json" -H "api-key: %admin_key%" -d @skillset.json

echo -----
echo Creating the index...
call curl -X PUT %url%/indexes/margies-knowledge-index?api-version=2020-06-30 -H "Content-Type: application/json" -H "api-key: %admin_key%" -d @index.json

rem wait
timeout /t 3 /nobreak

echo -----
echo Creating the indexer...
call curl -X PUT %url%/indexers/margies-knowledge-indexer?api-version=2020-06-30 -H "Content-Type: application/json" -H "api-key: %admin_key%" -d @indexer.json
