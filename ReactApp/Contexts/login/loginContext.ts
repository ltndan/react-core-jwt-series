import React from "react";
import { UserLogin } from "../../Generated/api";

interface ILoginContext {
  login: UserLogin | null;
  setLogin: (login: UserLogin | null) => void;
  refreshToken: {
    value: string;
    expiration: number;
  } | null;
  setRefreshToken: (refreshToken: {
    value: string;
    expiration: number;
  }) => void;
}

export const LoginContext = React.createContext<ILoginContext>({
  login: null,
  setLogin: () => {},
  refreshToken: null,
  setRefreshToken: () => {}
});
