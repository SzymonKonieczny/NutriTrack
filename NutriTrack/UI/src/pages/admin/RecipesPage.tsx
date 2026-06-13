import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../../api/client';
import type { Recipe, RecipeNutrition } from '../../dto';
import type { Ingredient } from '../../dto';

type ModalMode = 'create' | 'edit' | null;

export default function RecipesPage() {
  const [items, setItems] = useState<Recipe[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [editing, setEditing] = useState<Recipe | null>(null);

  const [formName, setFormName] = useState('');
  const [formPrepNote, setFormPrepNote] = useState('');
  const [formYoutubeUrl, setFormYoutubeUrl] = useState('');
  const [formError, setFormError] = useState<string | null>(null);
  const [formLoading, setFormLoading] = useState(false);

  // Ingredient multi-select state (create only)
  const [ingredients, setIngredients] = useState<Ingredient[]>([]);
  const [selectedIngs, setSelectedIngs] = useState<Record<string, string>>({}); // ingredientId → amount string
  const [ingSearch, setIngSearch] = useState('');

  // Nutrition modal state
  const [nutritionModal, setNutritionModal] = useState<{ recipeName: string; data: RecipeNutrition | null; loading: boolean; error: string | null } | null>(null);

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    const res = await api.get<Recipe[]>('/recipes');
    if (res.data) setItems(res.data);
    else setError(res.error);
    setLoading(false);
  }, []);

  useEffect(() => { fetch(); }, [fetch]);

  const openCreate = async () => {
    setEditing(null);
    setFormName(''); setFormPrepNote(''); setFormYoutubeUrl('');
    setFormError(null); setSelectedIngs({});
    setIngSearch('');
    // Fetch ingredients for the multi-select
    const res = await api.get<Ingredient[]>('/ingredients');
    if (res.data) setIngredients(res.data);
    else setIngredients([]);
    setModalMode('create');
  };

  const openEdit = (item: Recipe) => {
    setEditing(item);
    setFormName(item.name); setFormPrepNote(item.prepNote || '');
    setFormYoutubeUrl(item.youTubeUrl || ''); setFormError(null); setSelectedIngs({});
    setModalMode('edit');
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Delete this recipe?')) return;
    const res = await api.delete(`/recipes/${id}`);
    if (res.error) setError(res.error);
    else setItems((prev) => prev.filter((i) => i.id !== id));
  };

  const openNutrition = async (item: Recipe) => {
    setNutritionModal({ recipeName: item.name, data: null, loading: true, error: null });
    const res = await api.get<RecipeNutrition>(`/recipes/${item.id}/nutrition`);
    if (res.data) {
      setNutritionModal({ recipeName: item.name, data: res.data, loading: false, error: null });
    } else {
      setNutritionModal({ recipeName: item.name, data: null, loading: false, error: res.error });
    }
  };

  const handleCreate = async () => {
    if (!formName.trim()) { setFormError('Name is required'); return; }
    setFormLoading(true);

    const ingredientsPayload = Object.entries(selectedIngs)
      .filter(([, amount]) => amount && parseFloat(amount) > 0)
      .map(([id, amount]) => ({
        ingredientId: id,
        amountInGrams: parseFloat(amount),
      }));

    const res = await api.post<Recipe>('/recipes', {
      name: formName.trim(),
      prepNote: formPrepNote || null,
      youTubeUrl: formYoutubeUrl || null,
      ingredients: ingredientsPayload.length > 0 ? ingredientsPayload : undefined,
    });
    setFormLoading(false);
    if (res.data) {
      setItems((prev) => [...prev, res.data!]);
      setModalMode(null);
    } else {
      setFormError(res.error);
    }
  };

  const handleUpdate = async () => {
    if (!editing || !formName.trim()) { setFormError('Name is required'); return; }
    setFormLoading(true);
    const res = await api.put<Recipe>(`/recipes/${editing.id}`, {
      name: formName.trim(),
      prepNote: formPrepNote || null,
      youTubeUrl: formYoutubeUrl || null,
    });
    setFormLoading(false);
    if (res.data) {
      setItems((prev) => prev.map((i) => (i.id === editing.id ? res.data! : i)));
      setModalMode(null);
    } else {
      setFormError(res.error);
    }
  };

  const handleIngAmountChange = (id: string, value: string) => {
    setSelectedIngs((prev) => ({ ...prev, [id]: value }));
  };

  if (loading) return <div className="spinner">Loading recipes...</div>;

  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <div>
          <h1>Recipes</h1>
          <p className="text-muted text-sm">Manage recipes and their ingredients.</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>+ New Recipe</button>
      </div>

      {error && <div className="alert alert-error mb-2">{error}<button className="btn btn-ghost btn-sm" onClick={fetch} style={{marginLeft:'auto'}}>Retry</button></div>}

      {!loading && !error && items.length === 0 && (
        <div className="empty-state">
          <h3>No recipes yet</h3>
          <p>Create your first recipe to get started.</p>
          <button className="btn btn-primary mt-1" onClick={openCreate}>Create Recipe</button>
        </div>
      )}

      <div className="card">
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Prep Note</th>
                <th>YouTube</th>
                <th style={{ width: 160 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td><strong>{item.name}</strong></td>
                  <td className="text-muted text-sm">{item.prepNote || '—'}</td>
                  <td className="text-sm">{item.youTubeUrl ? <a href={item.youTubeUrl} target="_blank" rel="noopener noreferrer">Watch</a> : '—'}</td>
                  <td>
                    <div className="flex gap-1">
                      <button className="btn btn-ghost btn-sm" onClick={() => openNutrition(item)} title="View nutrition">📊</button>
                      <Link to={`/admin/recipes/${item.id}`} className="btn btn-ghost btn-sm">🥘</Link>
                      <button className="btn btn-ghost btn-sm" onClick={() => openEdit(item)}>✏️</button>
                      <button className="btn btn-ghost btn-sm" onClick={() => handleDelete(item.id)}>🗑️</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {modalMode && (
        <div className="modal-overlay">
          <div className="modal modal-lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{modalMode === 'create' ? 'New Recipe' : 'Edit Recipe'}</h2>
              <button className="btn btn-ghost" onClick={() => setModalMode(null)}>✕</button>
            </div>
            <div className="modal-body">
              {formError && <div className="alert alert-error">{formError}</div>}
              <div className="form-group">
                <label htmlFor="rec-name">Name</label>
                <input id="rec-name" type="text" className="form-input" value={formName} onChange={(e) => setFormName(e.target.value)} placeholder="e.g. Chicken Salad" required />
              </div>
              <div className="form-group">
                <label htmlFor="rec-prep-note">Prep Note</label>
                <input id="rec-prep-note" type="text" className="form-input" value={formPrepNote} onChange={(e) => setFormPrepNote(e.target.value)} placeholder="Optional preparation note" />
              </div>
              <div className="form-group">
                <label htmlFor="rec-youtube">YouTube URL</label>
                <input id="rec-youtube" type="url" className="form-input" value={formYoutubeUrl} onChange={(e) => setFormYoutubeUrl(e.target.value)} placeholder="https://youtube.com/..." />
              </div>

              {modalMode === 'create' && ingredients.length > 0 && (
                <div className="form-group">
                  <label>Ingredients</label>
                  <p className="text-muted text-sm mb-1">Optionally add ingredients to this recipe with amounts.</p>
                  <input
                    type="text"
                    className="form-input mb-1"
                    placeholder="Search ingredients..."
                    value={ingSearch}
                    onChange={(e) => setIngSearch(e.target.value)}
                  />
                  <div className="association-table">
                    <table>
                      <thead>
                        <tr>
                          <th>Ingredient</th>
                          <th style={{ width: 160 }}>Amount (g)</th>
                        </tr>
                      </thead>
                      <tbody>
                        {ingredients
                          .filter((ing) => ing.name.toLowerCase().includes(ingSearch.toLowerCase()))
                          .map((ing) => (
                          <tr key={ing.id}>
                            <td><strong>{ing.name}</strong></td>
                            <td>
                              <input
                                type="number"
                                className="form-input"
                                placeholder="g"
                                min="0"
                                step="0.1"
                                value={selectedIngs[ing.id] ?? ''}
                                onChange={(e) => handleIngAmountChange(ing.id, e.target.value)}
                              />
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}
            </div>
            <div className="modal-actions">
              <button className="btn btn-secondary" onClick={() => setModalMode(null)}>Cancel</button>
              <button className="btn btn-primary" disabled={formLoading} onClick={modalMode === 'create' ? handleCreate : handleUpdate}>
                {formLoading ? 'Saving...' : modalMode === 'create' ? 'Create' : 'Save'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ─── Nutrition Modal ─── */}
      {nutritionModal && (
        <div className="modal-overlay">
          <div className="modal modal-lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>📊 {nutritionModal.recipeName} — Nutrition</h2>
              <button className="btn btn-ghost" onClick={() => setNutritionModal(null)}>✕</button>
            </div>
            <div className="modal-body">
              {nutritionModal.loading && <p className="text-muted">Loading nutrition data...</p>}
              {nutritionModal.error && <div className="alert alert-error">{nutritionModal.error}</div>}
              {nutritionModal.data && nutritionModal.data.micronutrients.length === 0 && (
                <div className="empty-state">
                  <h3>No nutrition data</h3>
                  <p>This recipe has no micronutrient data associated with its ingredients.</p>
                </div>
              )}
              {nutritionModal.data && nutritionModal.data.micronutrients.length > 0 && (
                <div className="table-container">
                  <table>
                    <thead>
                      <tr>
                        <th>Micronutrient</th>
                        <th style={{ width: 160, textAlign: 'right' }}>Amount</th>
                      </tr>
                    </thead>
                    <tbody>
                      {nutritionModal.data.micronutrients.map((m) => (
                        <tr key={m.micronutrientId}>
                          <td><strong>{m.micronutrientName}</strong></td>
                          <td style={{ textAlign: 'right' }}>{m.totalAmount} {m.unit}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
            <div className="modal-actions">
              <button className="btn btn-secondary" onClick={() => setNutritionModal(null)}>Close</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}