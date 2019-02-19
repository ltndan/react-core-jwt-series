import React from "react";
import { LoginContext } from "./loginContext";
import { UserLogin } from "../../Generated/api";

class LoginWrapper extends React.Component {
  state = {
    login: null,
    setLogin: (login: UserLogin) => {
      var loginWithDate = login;
      if (loginWithDate) {
        loginWithDate.expirationDate = loginWithDate.expirationDate
          ? new Date(loginWithDate.expirationDate.toString())
          : login.expirationDate;
      }
      this.setState({
        login: loginWithDate
      });
    },
    refreshToken: null,
    setRefreshToken: refreshToken => {
      this.setState({ refreshToken });
    }
  };
  render() {
    return (
      <LoginContext.Provider value={this.state}>
        {this.props.children}
      </LoginContext.Provider>
    );
  }
}

export const withLogin = children => {
  return <LoginWrapper>{children}</LoginWrapper>;
};
