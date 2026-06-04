// ─── Dashboard ────────────────────────────────────────────────────────────────

export interface Counts {
  ingredients: number;
  micronutrients: number;
  recipes: number;
  mealLogs: number;
}

export interface RecipeProposal {
  id: number;
  title: string;
  description: string;
  reason: string;
  emoji: string;
}

// ─── Micronutrients ───────────────────────────────────────────────────────────

export interface Micronutrient {
  id: string;
  name: string;
  dailyReferenceAmount: number;
  unit: string;
  note: string | null;
}

export interface MicronutrientRef {
  id: string;
  name: string;
  unit: string;
}

// ─── Ingredients ───────────────────────────────────────────────────────────────

export interface Ingredient {
  id: string;
  name: string;
  note: string | null;
}

export interface IngredientMicronutrient {
  ingredientId: string;
  micronutrientId: string;
  micronutrientName: string;
  amountPer100g: number;
}

// ─── Recipes ───────────────────────────────────────────────────────────────────

export interface Recipe {
  id: string;
  name: string;
  prepNote: string | null;
  youTubeUrl: string | null;
}

export interface RecipeIngredient {
  recipeId: string;
  ingredientId: string;
  ingredientName: string;
  amountInGrams: number;
}

// ─── Meal Logs ─────────────────────────────────────────────────────────────────

export interface MealLog {
  id: string;
  eatenAt: string;
  eatenByUserId: string;
  recipeId: string | null;
  ingredientId: string | null;
  amountInGrams: number | null;
  servings: number | null;
  note: string | null;
}

export interface CreateMealLogRequest {
  eatenAt: string;
  recipeId: string | null;
  ingredientId: string | null;
  amountInGrams: number | null;
  servings: number | null;
  note: string | null;
}