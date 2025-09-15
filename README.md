https://trello.com/invite/b/68b587d9b35cfb46a46d5f54/ATTI8beba2bb019d1c5094afa2ed52d44dfd2461662E/project-1

# EpicQrizz
## QuestionAPI

### GetAll
https://localhost:7112/api/Quetion/GetAll

Dit geeft je alle vragen zonder het antwoord:

```json
[
  {
    "id": 1,
    "questionText": "Wat is de hoofdstad van Nederland?",
    "optionA": "Amsterdam",
    "optionB": "Rotterdam",
    "optionC": "Den Haag",
    "optionD": "Utrecht"
  },
  {
    "id": 2,
    "questionText": "Welke kleur krijg je als je blauw en geel mengt?",
    "optionA": "Paars",
    "optionB": "Groen",
    "optionC": "Oranje",
    "optionD": "Bruin"
  }
]
```

### GetById
https://localhost:7112/api/Quetion/GetById/{id}

Dit geeft je een specievieke vraag

```json
{
  "id": 1,
  "questionText": "Wat is de hoofdstad van Nederland?",
  "optionA": "Amsterdam",
  "optionB": "Rotterdam",
  "optionC": "Den Haag",
  "optionD": "Utrecht"
}
```

### CheckAwnser
https://localhost:7112/api/Quetion/GetById/{id}](https://localhost:7112/api/Quetion/CheckAwnser/{id}/{choice})

Controllert of het antwoord van een vraag goed is

```json
true
```
Dit is een test

### CreateAccount
http://localhost:8080/api/user/CreateAccount

Dit zorgt ervoor dat je een account kan maken

```json
{
  "id": "automatisch gegenereerde uuid",
  "name": "username",
  "password": "hashedPassword"
}
```

### Login
http://localhost:8080/api/user/Login/{username}

Deze call zorgt ervoor dat je kan inloggen

``````
