import { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../../api/client';
import type { RecipeIngredient } from '../../dto';
import type { Ingredient } from '../../dto';

export default function RecipeDetailPage() {
  const { recipeId } = useParams<{ recipeId: string }>();
  const [recipe, setRecipe] = useState<{ id: string; name: string } | null>(null);
  const [items, setItems] = useState<RecipeIngredient[]>([]);
  const [ingredients, setIngredients] = useState<Ingredient[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);

  const [formIngredientId, setFormIngredientId] = useState('');
  const [formAmount, setFormAmount] = useState('');
  const [formError, setFormError] = useState<string | null>(null);
  const [formLoading, setFormLoading] = useState(false);

  const fetch = useCallback(async () => {
    if (!recipeId) return;
    setLoading(true);
    setError(null);
    const [recRes, ingRes, itemsRes] = await Promise.all([
      api.get<{ id: string; name: string }>(`/recipes/${recipeId}`),
      api.get<Ingredient[]>('/ingredients'),
      api.get<RecipeIngredient[]>(`/recipes/${recipeId}/ingredients`),
    ]);
    if (recRes.data) setRecipe(recRes.data);
    if (ingRes.data) setIngredients(ingRes.data);
    if (itemsRes.data) setItems(itemsRes.data);
    else setError(itemsRes.error);
    setLoading(false);
  }, [recipeId]);

  useEffect(() => { fetch(); }, [fetch]);

  const handleAdd = async () => {
    if (!formIngredientId || !formAmount) {
      setFormError('Select an ingredient and enter amount');
      return;
    }
    setFormLoading(true);
    const res = await api.post<RecipeIngredient>(`/recipes/${recipeId}/ingredients`, {
      ingredientId: formIngredientId,
      amountInGrams: parseFloat(formAmount),
    });
    setFormLoading(false);
    if (res.data) {
      setItems((prev) => [...prev, res.data!]);
      setShowForm(false);
      setFormIngredientId(''); setFormAmount(''); setFormError(null);
    } else {
      setFormError(res.error);
    }
  };

  const handleDelete = async (ingredientId: string) => {
    if (!window.confirm('Remove this ingredient from the recipe?')) return;
    const res = await api.delete(`/recipes/${recipeId}/ingredients/${ingredientId}`);
    if (res.error) setError(res.error);
    else setItems((prev) => prev.filter((i) => i.ingredientId !== ingredientId));
  };

  if (loading) return <div className="spinner">Loading recipe details...</div>;
  if (!recipe) return <div className="alert alert-error">Recipe not found</div>;

  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <div>
          <Link to="/admin/recipes" className="text-sm text-muted" style={{ display: 'block', marginBottom: '0.25rem' }}>← Back to Recipes</Link>
          <h1>{recipe.name}</h1>
          <p className="text-muted text-sm">Manage ingredients in this recipe.</p>
        </div>
        <button className="btn btn-primary" onClick={() => { setShowForm(true); setFormError(null); }}>+ Add Ingredient</button>
      </div>

      {error && <div className="alert alert-error mb-2">{error}<button className="btn btn-ghost btn-sm" onClick={fetch} style={{marginLeft:'auto'}}>Retry</button></div>}

      {!loading && items.length === 0 && !showForm && (
        <div className="empty-state">
          <h3>No ingredients in this recipe</h3>
          <p>Add ingredients to this recipe.</p>
          <button className="btn btn-primary mt-1" onClick={() => { setShowForm(true); setFormError(null); }}>Add Ingredient</button>
        </div>
      )}

      {showForm && (
        <div className="card mb-2">
          <h3 className="mb-1">Add Ingredient</h3>
          {formError && <div className="alert alert-error mb-1">{formError}</div>}
          <div className="flex gap-2 items-center">
            <div className="form-group" style={{ flex: 2 }}>
              <label htmlFor="ri-ingredient">Ingredient</label>
              <select id="ri-ingredient" className="form-select" value={formIngredientId} onChange={(e) => setFormIngredientId(e.target.value)}>
                <option value="">Select...</option>
                {ingredients.map((i) => (
                  <option key={i.id} value={i.id}>{i.name}</option>
                ))}
              </select>
            </div>
            <div className="form-group" style={{ flex: 1 }}>
              <label htmlFor="ri-amount">Amount (g)</label>
              <input id="ri-amount" type="number" className="form-input" value={formAmount} onChange={(e) => setFormAmount(e.target.value)} min="0" step="0.1" placeholder="g" />
            </div>
            <div className="flex gap-1" style={{ marginTop: '1.4rem' }}>
              <button className="btn btn-primary btn-sm" disabled={formLoading} onClick={handleAdd}>
                {formLoading ? '...' : 'Add'}
              </button>
              <button className="btn btn-secondary btn-sm" onClick={() => { setShowForm(false); setFormError(null); }}>Cancel</button>
            </div>
          </div>
        </div>
      )}

      <div className="card">
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Ingredient</th>
                <th>Amount (g)</th>
                <th style={{ width: 80 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.ingredientId}>
                  <td><strong>{item.ingredientName}</strong></td>
                  <td>{item.amountInGrams}</td>
                  <td>
                    <button className="btn btn-ghost btn-sm" onClick={() => handleDelete(item.ingredientId)}>🗑️</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}