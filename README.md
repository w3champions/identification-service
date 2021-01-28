# w3champions-identification-service

This is the Authorization Backend for the https://github.com/w3champions/w3champions-ui Project.

## Create new certs
As the key and pub file will be replaced during the deployment, here a quick reminder on how to generate those:

```
ssh-keygen -t rsa -b 4096 -m PEM -f jwtRS256.key
# Don't add passphrase
openssl rsa -in jwtRS256.key -pubout -outform PEM -out jwtRS256.key.pub
```

Use the files and add `\n` after each line to make it parsable in `docker-compose`

