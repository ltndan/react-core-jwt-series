import React, { useContext } from "react";
import { LoginContext } from "../Contexts/login/loginContext";

export const UserInfo = () => {
  const loginContext = useContext(LoginContext);
  if (loginContext.login) {
    return (
      <div>
        <h2>Current User info</h2>
        User email: {loginContext.login.emailAddress}
        <br />
        <div>
          Token expires at{" "}
          <span>{loginContext.login.expirationDate!.toString()}</span>
        </div>
      </div>
    );
  } else {
    return <div />;
  }
};
