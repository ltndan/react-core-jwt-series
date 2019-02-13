import React, { useState, useContext } from "react";
import { UsersApi } from "../Generated/api";
import { LoginContext } from "../Contexts/login/loginContext";

export const SignInUser = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const loginContext = useContext(LoginContext);

  const handleSignin = async () => {
    setLoading(true);
    const api = new UsersApi({ basePath: "https://localhost:5001" });
    try {
      const result = await api.signInUser({
        emailAddress: email,
        password: password
      });
      setMessage(JSON.stringify(result));
      loginContext.setLogin(result);
    } catch (ex) {
      setMessage(JSON.stringify(ex.statusText));
      loginContext.setLogin(null);
    }
    setLoading(false);
  };

  return (
    <div>
      <h2>Signin user</h2>
      <input
        type="text"
        value={email}
        onChange={event => setEmail(event.target.value)}
      />
      <br />
      <br />
      <input
        type="password"
        value={password}
        onChange={event => setPassword(event.target.value)}
      />
      <br />
      <br />
      {loading ? (
        <span>Loading...</span>
      ) : (
        <button type="button" onClick={handleSignin}>
          Sign in!
        </button>
      )}
      <br />
      <br />
      {message}
    </div>
  );
};
