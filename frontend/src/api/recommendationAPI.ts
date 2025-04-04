import { RecommendationComparison } from '../types/recommendationTypes';

const BASE_URL = 'https://localhost:5000/api/Recommendation';

export const fetchContentIds = async (): Promise<string[]> => {
  const response = await fetch(`${BASE_URL}/contentIds`);
  if (!response.ok) throw new Error('Failed to fetch contentIds');
  return response.json();
};

export const fetchRecommendations = async (contentId: string): Promise<RecommendationComparison> => {
  const response = await fetch(`${BASE_URL}/${contentId}`);
  if (!response.ok) throw new Error('Failed to fetch recommendations');
  return response.json();
};