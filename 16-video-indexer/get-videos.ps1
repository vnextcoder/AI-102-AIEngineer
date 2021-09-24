$account_id="xxxxxxxxxxxxxxxx"
# get it at https://www.videoindexer.ai/account/a81bb724-8776-4531-a9ca-4e4a04e9561b/settings
$api_key="xxxxxxxxxxxxxx"
# get it at https://api-portal.videoindexer.ai/profile
$location="trial"

# Call the AccessToken method with the API key in the header to get an access token
$token = Invoke-RestMethod -Method "Get" -Uri "https://api.videoindexer.ai/auth/$location/Accounts/$account_id/AccessToken" -Headers @{'Ocp-Apim-Subscription-Key' = $api_key}

# Use the access token to make an authenticated call to the Videos method to get a list of videos in the account
Invoke-RestMethod -Method "Get" -Uri "https://api.videoindexer.ai/$location/Accounts/$account_id/Videos?accessToken=$token" | ConvertTo-Json
