import React, { useEffect, useState } from "react";
import { RecommendationRow } from "../types/ColabRecRow";
import axios from "axios";

const CollabRecommendationViewer = () => {
  const [data, setData] = useState<RecommendationRow[]>([]);
  const [selectedId, setSelectedId] = useState<string>("");
  const [selectedRow, setSelectedRow] = useState<RecommendationRow | null>(null);

  useEffect(() => {
    axios
      .get<RecommendationRow[]>("https://localhost:5000/api/CollaborativeFiltering") // Update port if needed
      .then((res) => setData(res.data))
      .catch((err) => console.error("API error:", err));
  }, []);

  useEffect(() => {
    const row = data.find((d) => d.contentId === selectedId) ?? null;
    setSelectedRow(row);
  }, [selectedId, data]);

  return (
    <div className="container" style={{ padding: "2rem" }}>
      <h2>Collaborative Filtering</h2>

      <select
        className="dropdown"
        value={selectedId}
        onChange={(e) => setSelectedId(e.target.value)}
      >
        <option value="">Select a Content ID</option>
        {data.map((row) => (
          <option key={row.contentId} value={row.contentId}>
            {row.contentId}
          </option>
        ))}
      </select>

      {selectedRow && (
        <div style={{ marginTop: "1rem" }}>
          <h4>If you liked:</h4>
          <p>{selectedRow.ifYouLiked}</p>

          <h5>Recommendations:</h5>
          <ul>
            {selectedRow.recommendations.map((rec, idx) => (
              <li key={idx}>{rec}</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};

export default CollabRecommendationViewer;
