
Micronutrient
  - Id (guid)
  - Name (string, required)
  - DailyReferenceAmount (decimal)
  - Unit (enum: Mg, Mcg, IU)
  - Note (string?)

Ingredient
  - Id (guid)
  - Name (string, required)
  - Note (string?)

IngredientMicronutrient          ← composite PK (IngredientId, MicronutrientId)
  - IngredientId (FK)
  - MicronutrientId (FK)
  - AmountPer100g (decimal)

Recipe                           ← renamed from MealType, clearer intent
  - Id (guid)
  - Name (string, required)
  - PrepNote (string?)
  - YouTubeUrl (string?)

RecipeIngredient                 ← composite PK (RecipeId, IngredientId)
  - RecipeId (FK)
  - IngredientId (FK)
  - AmountInGrams (decimal)

MealLog
  - Id (guid)
  - EatenAt (DateTimeOffset)     ← not just Date; preserves timezone
  - EatenByUserId (string)       ← FK to AspNetUsers
  - RecipeId (int?, FK)          ← null if raw ingredient
  - IngredientId (int?, FK)      ← null if recipe
  - AmountInGrams (decimal?)     ← used for raw ingredient logs
  - Servings (decimal?)          ← used for recipe logs
  - Note (string?)