# w3champions-identification-service

This is the Authorization Backend for the https://github.com/w3champions/w3champions-ui Project.

## Running the service locally
If you want to use the service locally, you need a rsa public and private key. This is being used in the `AuthorizationController` to create the JWTs. There is a function that can generate strings for you in the Tests called `SecretGeneration`. Generate private and public key there and use it to run the service.