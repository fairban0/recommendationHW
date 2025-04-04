import { useEffect, useState } from 'react';
import {
  fetchContentIds,
  fetchRecommendations,
} from '../api/recommendationAPI';
import { RecommendationComparison } from '../types/recommendationTypes';
import Select from 'react-select';

interface Option {
  value: string;
  label: string;
}

const RecommendationView = () => {
  const [contentIds, setContentIds] = useState<string[]>([]);
  const [options, setOptions] = useState<Option[]>([]);
  const [selectedOption, setSelectedOption] = useState<Option | null>(null);
  const [recommendation, setRecommendation] = useState<RecommendationComparison | null>(null);
  const [error, setError] = useState('');

  // Load content IDs
  useEffect(() => {
    fetchContentIds()
      .then((ids) => {
        setContentIds(ids);
        const opts = ids.map((id) => ({
          value: id,
          label: `${id}...`, // or just `id` if you want full
        }));
        setOptions(opts);
      })
      .catch(() => setError('Failed to load content IDs'));
  }, []);

  // Load recommendations
  useEffect(() => {
    if (!selectedOption) return;
    fetchRecommendations(selectedOption.value)
      .then(setRecommendation)
      .catch(() => setError('Failed to load recommendations'));
  }, [selectedOption]);

  return (
    <div style={{ padding: '1rem' }}>
      <h2>Explore Recommendations</h2>

      <label htmlFor="contentSelect">Select Content ID:</label>
      <Select
        inputId="contentSelect"
        options={options}
        value={selectedOption}
        onChange={(option) => setSelectedOption(option as Option)}
        placeholder="Select a content ID..."
        styles={{
          control: (base) => ({
            ...base,
            fontSize: '16px',
            padding: '2px',
          }),
          option: (base) => ({
            ...base,
            fontSize: '16px',
          }),
        }}
      />

      {error && <p style={{ color: 'red' }}>{error}</p>}

      {recommendation && (
        <div style={{ marginTop: '2rem' }}>
          <h3>If you liked:</h3>
          <p>{recommendation.ifYouLiked}</p>

          <div style={{ display: 'flex', gap: '3rem' }}>
            <div>
              <h4>Collaborative Filtering</h4>
              <ul>
                {recommendation.collaborativeRecommendations.map((rec, i) => (
                  <li key={i}>{rec}</li>
                ))}
              </ul>
            </div>

            <div>
              <h4>Content-Based Filtering</h4>
              <ul>
                {recommendation.contentRecommendations.map((rec, i) => (
                  <li key={i}>{rec}</li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default RecommendationView;