# Search Recipes by Text API Implementation

## Overview
This document describes the implementation of the search recipes by text API endpoint in the Recipe Lambda service.

## Features Implemented

### 1. Search API Endpoint
- **Endpoint**: `POST /recipes/search`
- **Method**: POST
- **Content-Type**: application/json

### 2. Search Functionality
The search API searches recipes across multiple fields:
- Recipe title
- Recipe description  
- Recipe category
- Ingredient names
- Step instructions

### 3. Pagination Support
- Configurable page size (1-100)
- Page number tracking
- Total count and page information
- Next/previous page indicators

### 4. Search Request DTO (`SearchRecipeRequest`)
```csharp
public class SearchRecipeRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string SearchText { get; set; }
    
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
    
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;
}
```

### 5. Search Response DTO (`SearchRecipeResponse`)
```csharp
public class SearchRecipeResponse
{
    public List<RecipeSearchResult> Recipes { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
```

### 6. Recipe Search Result DTO (`RecipeSearchResult`)
```csharp
public class RecipeSearchResult
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserName { get; set; }
    public int IngredientCount { get; set; }
    public int StepCount { get; set; }
}
```

## Implementation Details

### 1. Service Layer
- Added `SearchRecipesAsync` method to `IRecipeService` interface
- Implemented search logic in `RecipeService` class
- Uses Entity Framework Core with LINQ for efficient database queries
- Includes proper error handling and logging

### 2. Search Logic
- Case-insensitive text search using `StringComparison.OrdinalIgnoreCase`
- Searches across multiple related entities (Recipe, Ingredients, Steps)
- Orders results by creation date (newest first)
- Efficient pagination using `Skip()` and `Take()`

### 3. Database Query Optimization
- Uses `Include()` to load related entities in a single query
- Applies search filters before pagination for accurate count
- Selects only necessary fields for search results

### 4. API Endpoint
- RESTful design following ASP.NET Core conventions
- Proper HTTP status codes (200 for success, 500 for errors)
- Comprehensive Swagger/OpenAPI documentation
- Input validation using Data Annotations

## Usage Examples

### Basic Search
```json
POST /recipes/search
{
    "searchText": "chicken",
    "pageSize": 10,
    "pageNumber": 1
}
```

### Search with Pagination
```json
POST /recipes/search
{
    "searchText": "pasta",
    "pageSize": 5,
    "pageNumber": 2
}
```

## Testing
- Created unit tests for DTOs and validation
- All tests pass successfully
- Test coverage includes:
  - Search request validation
  - Search response initialization
  - Recipe search result properties

## Error Handling
- Comprehensive exception handling in service layer
- Proper logging of errors and search operations
- Returns appropriate HTTP status codes
- User-friendly error messages

## Performance Considerations
- Efficient database queries with proper indexing
- Pagination to handle large result sets
- Case-insensitive search for better user experience
- Minimal data transfer (only necessary fields in response)

## Future Enhancements
- Full-text search capabilities
- Search result highlighting
- Advanced filtering options
- Search result ranking/scoring
- Caching for frequently searched terms
