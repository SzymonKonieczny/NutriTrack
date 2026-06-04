import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import AdminRoute from './components/AdminRoute';
import AdminLayout from './components/AdminLayout';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashboardPage';
import MealLogsPage from './pages/MealLogsPage';
import AdminDashboard from './pages/admin/AdminDashboard';
import IngredientsPage from './pages/admin/IngredientsPage';
import MicronutrientsPage from './pages/admin/MicronutrientsPage';
import RecipesPage from './pages/admin/RecipesPage';
import RecipeDetailPage from './pages/admin/RecipeDetailPage';
import IngredientDetailPage from './pages/admin/IngredientDetailPage';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Root redirect */}
          <Route path="/" element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route index element={<DashboardPage />} />
              <Route path="meal-logs" element={<MealLogsPage />} />

              {/* Admin nested routes */}
              <Route path="admin" element={<AdminRoute />}>
                <Route element={<AdminLayout />}>
                  <Route index element={<AdminDashboard />} />
                  <Route path="ingredients" element={<IngredientsPage />} />
                  <Route path="ingredients/:ingredientId" element={<IngredientDetailPage />} />
                  <Route path="micronutrients" element={<MicronutrientsPage />} />
                  <Route path="recipes" element={<RecipesPage />} />
                  <Route path="recipes/:recipeId" element={<RecipeDetailPage />} />
                </Route>
              </Route>
            </Route>
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;