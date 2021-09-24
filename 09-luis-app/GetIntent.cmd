@echo off

rem Set values for your Language Understanding app
set app_id="xxxxxxxxxxxxxxxxxxxxx-9531-x-a324-x"
set endpoint="https://mycogservice2392.cognitiveservices.azure.com/"
set key="xxxxxxxxxxxxxxxxxxxxxx"

rem Get parameter and encode spaces for URL
set input=%1
set query=%input: =+%
echo %query%
rem Use cURL to call the REST API
curl -X GET "%endpoint%/luis/prediction/v3.0/apps/%app_id%/slots/production/predict?subscription-key=%key%&log=true&query=%query%"