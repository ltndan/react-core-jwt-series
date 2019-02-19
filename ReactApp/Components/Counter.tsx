import React, { useState, useEffect, useContext } from "react";
import { LoginContext } from "../Contexts/login/loginContext";
import { useWebservice } from "../webservice";

export const Counter = () => {
  const [minutes, setMinutes] = useState(0);
  const [seconds, setSeconds] = useState(0);
  const loginContext = useContext(LoginContext);
  const [usersApi] = useWebservice();

  useEffect(() => {
    if (loginContext.login) {
      setTimeout(() => {
        var diff =
          loginContext.login!.expirationDate!.getTime() - new Date().getTime();

        var seconds = diff / 1000;

        var minutes = Math.floor(seconds / 60);
        seconds = Math.floor(seconds % 60);

        setMinutes(minutes);
        setSeconds(seconds);
      }, 1000);
    }
  });

  useEffect(() => {
    if (loginContext.login) {
      setTimeout(async () => {
        const newLogin = await usersApi.refreshLogin({
          oldJwtToken: loginContext.login!.jwtToken,
          refreshToken: loginContext.login!.refreshToken
        });
        loginContext.setLogin(newLogin);
      }, loginContext.login.expirationDate!.getTime() - new Date().getTime() - 1000);
    }
  }, [loginContext.login]);

  return (
    <div>
      <h1>Counter</h1>
      {loginContext.login ? (
        <span>
          Remaining time: {seconds < 0 ? "expired" : `${minutes}:${seconds}`}
        </span>
      ) : (
        <span>not logged in</span>
      )}
    </div>
  );
};
