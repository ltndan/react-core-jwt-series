import React, { useState, useContext } from "react";
import { useWebservice } from "../webservice";
import { LoginContext } from "../Contexts/login/loginContext";

export const CallAuthentificatedApi = () => {
  const [isLoading, setLoading] = useState(false);
  const [result, setResult] = useState("");
  const [, ValuesApi] = useWebservice();
  const loginContext = useContext(LoginContext);

  const callApi = async () => {
    setLoading(true);

    try {
      const result = await ValuesApi.get();
      setResult(JSON.stringify(result));
    } catch (ex) {
      setResult(ex.statusText);
    }
    setLoading(false);
  };

  return (
    <div>
      <h2>Context Authenticated API</h2>
      {loginContext && loginContext.login ? (
        isLoading ? (
          <span>Loading...</span>
        ) : (
          <button type="button" onClick={callApi}>
            Call Context Authenticated API
          </button>
        )
      ) : (
        <span>not signed in</span>
      )}

      <br />
      <br />
      {result}
    </div>
  );
};
