# Intruduction

Payment gateway to process payments with a fake acquirer.



## Functinonalities
It is prosible to create payments and retrieve payments. See Testing API for more details.

**Swagger**: Swagger has been use to facilitate the exploration and testing of the API

**HMac signature**: Implemented HMac signature message authentication. In order for a Merchant to be able to send payment request, the merchant has to be registered in the sistem and have a SigningKey assinged. Payment request have to be singed with the assinged SigningKey.

**Api key Authentication**: Authentication based on API key. In order to use the API the merchant has to be registered in the system and have an API Key assinged. The API Key has to be sent in all requests.

**CQS** (not CQRS): Commands and queries has been separated following CQS principle. Commands and Queries are in the Application folder.

**Database**: There is no real database. I've implemented in-memory repositories. 

**Acquirer bank**: I'm using [mocky](http://www.mocky.io/) to fake the acquirer bank.

**Mediatr**: Is used to dispatch Command and Queriues.

**FluentValidations**: In conguntion whith Mediator pipeline, fluentValidations is used to validate Command and Queries request.

**FluentAssertions**: To create assetion in unit and integration tests.

**Unit test**: Due to time restriction I coudn't write all the necessary unit tests. For example filters and Mediatr behaviours are not unit tested. 

**Integration test**: Some integration tests have been implemented.


# Run
Execute command from PaymentGateway project folder:

dotnet run


# Unit and integration Tests

Execute command from root folder:

dotnet test

# Testing the API
This project uses swagger to help exploring the API. Suagger should be displayed in the root url https://localhost:5001/index.html

It is required for a merchant to exist in the database and have an apiKey and SigningKey assinged in order to be able to call the API.

NOTE: There is an existing test merchant in the database, use the ApiKey and SigningKey for your tests

```javascript
Merchant
{
    Id : "7fb6f154-9869-4917-8be4-f0767b12cd37",
    ApiKey : "testkey",
    Code : "TestMerchant",
    SigningKey : "DFB1EB5485895CFA84146406857104ABB4CBCABDC8AAF103A624C8F6A3EAAB00"
};
```

### Generate message signatures Hmac
There is a utility endpoint to facilitate the generation of signatures (only for testing purposes)

/api/v1/signatures

Provide all the required fields (apiKey header is not required) and the signature will be returned.

## Process Payment 
End point for request new payment.

/api/v1/Payments


Example request:
```javascript
{
  "merchantTransactionId": "12345678901",
  "amount": 10,
  "cardNumber": "1234123412341234",
  "expiryMonth": 10,
  "expiryYear": 2022,
  "cvv": "123",
  "currency": "EUR",
  "signature": "j6Ze59AWImB/ka9AySkvCxbOhvdX0P9yqiojz3vfVlE="
}
```


That call will return a result similar to 

```javascript
{
  "transactionId": "f388feb3-6285-4cf0-addf-ad431163ab0a",
  "resultCode": "OK",
  "resultDescription": "Payment processed on 03/14/2020 12:01:36"
}
```

## Get Payment 
End point to get an existing payment

/api/v1/Payments/{transactionId}

Fetch the transactionId from the Process payment response and using to call this end point.

example resonse
```javascript
{
  "transactionId": "7fb6f154-9869-4917-8be4-f0767b12cd37",
  "amount": 122,
  "cardNumber": "****2112",
  "expiryMonth": 34,
  "expiryDay": 0,
  "cvv": "123",
  "currency": "EUR",
  "createdDate": null,
  "statusCode": "S100"
}
```

Alternatively there is an existing transaction in the in-memory repository with id: 7fb6f154-9869-4917-8be4-f0767b12cd37 with ApiKey header value: testkey



