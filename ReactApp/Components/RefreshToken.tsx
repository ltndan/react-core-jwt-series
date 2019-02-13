import React, { useState, useContext } from "react";
import { LoginContext } from "../Contexts/login/loginContext";
import { UsersApi } from "../Generated/api";

export const RefreshToken = () => {
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const loginContext = useContext(LoginContext);

  const handleClick = async () => {
    setLoading(true);
    const api = new UsersApi({ basePath: "https://localhost:5001" });
    try {
      const newLogin = await api.refreshLogin({
        oldJwtToken: loginContext.login!.jwtToken,
        refreshToken: loginContext.login!.refreshToken
      });

      loginContext.setLogin(newLogin);
    } catch (ex) {
      setMessage(ex.message);
    }
    setLoading(false);
  };

  return (
    <div>
      <h2>Refresh Token</h2>
      {loading ? (
        <span>Loading ...</span>
      ) : (
        <button onClick={handleClick} disabled={loginContext.login == null}>
          {loginContext.login ? "Refresh" : "First sign in"}
        </button>
      )}
      <div>{message}</div>
    </div>
  );
};
