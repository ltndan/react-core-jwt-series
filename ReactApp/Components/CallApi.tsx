import React, { useState } from "react";
import { ValuesApi } from "../Generated/api";

export const CallApi = () => {
  const [token, setToken] = useState("");
  const [isLoading, setLoading] = useState(false);
  const [result, setResult] = useState("");

  const callApi = async () => {
    setLoading(true);

    try {
      const api = new ValuesApi({
        basePath: "https://localhost:5001",
        apiKey: `Bearer ${token}`
      });
      const result = await api.get();
      setResult(JSON.stringify(result));
    } catch (ex) {
      setResult(ex.statusText);
    }
    setLoading(false);
  };

  return (
    <div>
      <h2>Authenticated API</h2>
      <input
        type="text"
        value={token}
        onChange={event => setToken(event.target.value)}
      />
      <br />
      <br />
      {isLoading ? (
        <span>Loading...</span>
      ) : (
        <button type="button" onClick={callApi}>
          Call API
        </button>
      )}

      <br />
      <br />
      {result}
    </div>
  );
};
