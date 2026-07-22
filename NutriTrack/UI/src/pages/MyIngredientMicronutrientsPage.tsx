import { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../api/client';
import type { IngredientMicronutrient } from '../dto';
import type { MicronutrientRef as Micronutrient } from '../dto';

export default function MyIngredientMicronutrientsPage() {
  const { ingredientId } = useParams<{ ingredientId: string }>();
  const [ingredient, setIngredient] = useState<{ id: string; name: string } | null>(null);
  const [items, setItems] = useState<IngredientMicronutrient[]>([]);
  const [micronutrients, setMicronutrients] = useState<Micronutrient[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);

  const [formMicronutrientId, setFormMicronutrientId] = useState('');
  const [formAmount, setFormAmount] = useState('');
  const [formError, setFormError] = useState<string | null>(null);
  const [formLoading, setFormLoading] = useState(false);

  const fetch = useCallback(async () => {
    if (!ingredientId) return;
    setLoading(true);
    setError(null);
    const [ingRes, micRes, itemsRes] = await Promise.all([
      api.get<{ id: string; name: string }>(`/ingredients/${ingredientId}`),
      api.get<Micronutrient[]>('/micronutrients'),
      api.get<IngredientMicronutrient[]>(`/ingredients/${ingredientId}/micronutrients`),
    ]);
    if (ingRes.data) setIngredient(ingRes.data);
    if (micRes.data) setMicronutrients(micRes.data);
    if (itemsRes.data) setItems(itemsRes.data);
    else setError(itemsRes.error);
    setLoading(false);
  }, [ingredientId]);

  useEffect(() => { fetch(); }, [fetch]);

  const handleAdd = async () => {
    if (!formMicronutrientId || !formAmount) {
      setFormError('Select a micronutrient and enter amount');
      return;
    }
    setFormLoading(true);
    const res = await api.post<IngredientMicronutrient>(`/ingredients/${ingredientId}/micronutrients`, {
      micronutrientId: formMicronutrientId,
      amountPer100g: parseFloat(formAmount),
    });
    setFormLoading(false);
    if (res.data) {
      setItems((prev) => [...prev, res.data!]);
      setShowForm(false);
      setFormMicronutrientId(''); setFormAmount(''); setFormError(null);
    } else {
      setFormError(res.error);
    }
  };

  const handleDelete = async (micronutrientId: string) => {
    if (!window.confirm('Remove this micronutrient from the ingredient?')) return;
    const res = await api.delete(`/ingredients/${ingredientId}/micronutrients/${micronutrientId}`);
    if (res.error) setError(res.error);
    else setItems((prev) => prev.filter((i) => i.micronutrientId !== micronutrientId));
  };

  if (loading) return <div className="spinner">Loading ingredient details...</div>;
  if (!ingredient) return <div className="alert alert-error">Ingredient not found</div>;

  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <div>
          <Link to="/my-ingredients" className="text-sm text-muted" style={{ display: 'block', marginBottom: '0.25rem' }}>← Back to My Ingredients</Link>
          <h1>{ingredient.name}</h1>
          <p className="text-muted text-sm">Manage micronutrients for this ingredient (per 100g).</p>
        </div>
        <button className="btn btn-primary" onClick={() => { setShowForm(true); setFormError(null); }}>+ Add Micronutrient</button>
      </div>

      {error && <div className="alert alert-error mb-2">{error}<button className="btn btn-ghost btn-sm" onClick={fetch} style={{marginLeft:'auto'}}>Retry</button></div>}

      {!loading && items.length === 0 && !showForm && (
        <div className="empty-state">
          <h3>No micronutrients for this ingredient</h3>
          <p>Add micronutrient data to this ingredient.</p>
          <button className="btn btn-primary mt-1" onClick={() => { setShowForm(true); setFormError(null); }}>Add Micronutrient</button>
        </div>
      )}

      {showForm && (
        <div className="card mb-2">
          <h3 className="mb-1">Add Micronutrient</h3>
          {formError && <div className="alert alert-error mb-1">{formError}</div>}
          <div className="flex gap-2 items-center">
            <div className="form-group" style={{ flex: 2 }}>
              <label htmlFor="my-im-micronutrient">Micronutrient</label>
              <select id="my-im-micronutrient" className="form-select" value={formMicronutrientId} onChange={(e) => setFormMicronutrientId(e.target.value)}>
                <option value="">Select...</option>
                {micronutrients.map((m) => (
                  <option key={m.id} value={m.id}>{m.name} ({m.unit})</option>
                ))}
              </select>
            </div>
            <div className="form-group" style={{ flex: 1 }}>
              <label htmlFor="my-im-amount">Per 100g</label>
              <input id="my-im-amount" type="number" className="form-input" value={formAmount} onChange={(e) => setFormAmount(e.target.value)} min="0" step="0.01" placeholder="mg" />
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
                <th>Micronutrient</th>
                <th>Amount per 100g</th>
                <th style={{ width: 80 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.micronutrientId}>
                  <td><strong>{item.micronutrientName}</strong></td>
                  <td>{item.amountPer100g}</td>
                  <td>
                    <button className="btn btn-ghost btn-sm" onClick={() => handleDelete(item.micronutrientId)}>🗑️</button>
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