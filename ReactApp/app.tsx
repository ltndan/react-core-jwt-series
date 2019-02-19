import React from "react";
import { render } from "react-dom";
import { CreateUser } from "./Components/CreateUser";
import { SignInUser } from "./Components/SigninUser";
import { CallApi } from "./Components/CallApi";
import { UserInfo } from "./Components/UserInfo";
import { withLogin } from "./Contexts/login/loginWrapper";
import { RefreshToken } from "./Components/RefreshToken";
import { CallAuthentificatedApi } from "./Components/CallAuthentificatedApi";
import { Counter } from "./Components/Counter";

const App = () => {
  return (
    <React.Fragment>
      <div style={{ wordBreak: "break-all" }}>
        <CreateUser />
        <SignInUser />
        <CallApi />
        <RefreshToken />
        <CallAuthentificatedApi />
        <Counter />
      </div>
      <UserInfo />
    </React.Fragment>
  );
};

render(withLogin(<App />), document.getElementById("root"));
