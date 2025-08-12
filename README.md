# Recipe API

This is the Recipe Lambda function that provides recipe management capabilities.

## API Endpoints

### Create Recipe

**POST** `/recipes`

Creates a new recipe with ingredients and steps.

#### Request Body

```json
{
  "title": "Spaghetti Carbonara",
  "description": "A classic Italian pasta dish with eggs, cheese, and pancetta",
  "category": "Italian",
  "photoUrl": "https://example.com/carbonara.jpg",
  "isFavorite": false,
  "ingredients": [
    {
      "name": "Spaghetti",
      "quantity": "400",
      "unit": "g"
    },
    {
      "name": "Eggs",
      "quantity": "4",
      "unit": "large"
    },
    {
      "name": "Pecorino Romano",
      "quantity": "100",
      "unit": "g"
    }
  ],
  "steps": [
    {
      "stepNumber": 1,
      "instructionText": "Bring a large pot of salted water to boil and cook spaghetti according to package directions"
    },
    {
      "stepNumber": 2,
      "instructionText": "In a large skillet, cook pancetta until crispy"
    },
    {
      "stepNumber": 3,
      "instructionText": "Beat eggs and cheese in a bowl, then toss with hot pasta and pancetta"
    }
  ]
}
```

#### Response

**Status:** 201 Created

```json
{
  "id": "12345678-1234-1234-1234-123456789012",
  "title": "Spaghetti Carbonara",
  "description": "A classic Italian pasta dish with eggs, cheese, and pancetta",
  "category": "Italian",
  "photoUrl": "https://example.com/carbonara.jpg",
  "isFavorite": false,
  "createdAt": "2024-01-15T10:30:00Z",
  "createdByUserId": "11111111-1111-1111-1111-111111111111",
  "ingredients": [
    {
      "id": "87654321-4321-4321-4321-210987654321",
      "name": "Spaghetti",
      "quantity": "400",
      "unit": "g"
    }
  ],
  "steps": [
    {
      "id": "11223344-3322-3322-3322-443322112233",
      "stepNumber": 1,
      "instructionText": "Bring a large pot of salted water to boil and cook spaghetti according to package directions"
    }
  ]
}
```

#### Validation

- `title`: Required, max 200 characters
- `description`: Required, max 1000 characters
- `category`: Required, max 100 characters
- `photoUrl`: Optional, max 500 characters
- `ingredients`: Required, must contain at least one ingredient
- `steps`: Required, must contain at least one step
- Each ingredient must have `name`, `quantity`, and `unit` (all required)
- Each step must have `stepNumber` and `instructionText` (both required)

### Get Recipes

**GET** `/recipes`

Returns a list of recipe names.

### Get Welcome

**GET** `/welcome`

Returns a welcome message from the recipe service.

## Development

### Running Locally

```bash
dotnet run
```

The API will be available at `http://localhost:5000` with Swagger documentation at `/swagger`.

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

## Architecture

This Lambda function follows a clean architecture pattern:

- **DTOs**: Data transfer objects for API requests and responses
- **Services**: Business logic layer
- **Domain Entities**: Core business entities (Recipe, Ingredient, Step)
- **Infrastructure**: Database context and persistence layer

## Dependencies

- ASP.NET Core 8.0
- Entity Framework Core
- AWS Lambda hosting
- Serilog for structured logging
- Swagger/OpenAPI for documentation
