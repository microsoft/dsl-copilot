#!/bin/bash

echo "Setting up environment variables for Azurite"
LanguageBlobService__AccountName="accountname"
accesskey="accesskey"
LanguageBlobService__AccessKey=$(echo $accesskey | base64)
echo $LanguageBlobService__AccessKey
AZURITE_ACCOUNTS="$LanguageBlobService__AccountName:$LanguageBlobService__AccessKey"
export AZURITE_ACCOUNTS
echo $AZURITE_ACCOUNTS
