import { UsersApi, ValuesApi } from "./Generated/api";
import { useContext, useEffect, useState } from "react";
import { LoginContext } from "./Contexts/login/loginContext";
import { Configuration } from "./Generated/index";

const basePath = "https://localhost:5001";

export const useWebservice = (): [UsersApi, ValuesApi] => {
  const loginContext = useContext(LoginContext);

  const [usersApi, setUsersApi] = useState(new UsersApi({ basePath }));
  const [valuesApi, setValuesApi] = useState(new ValuesApi({ basePath }));

  useEffect(() => {
    const configuration: Configuration = {
      basePath
    };

    if (loginContext.login) {
      configuration.apiKey = `Bearer ${loginContext.login.jwtToken}`;
    } else {
      configuration.apiKey = "";
    }

    setUsersApi(new UsersApi(configuration, basePath, fetch));
    setValuesApi(new ValuesApi(configuration, basePath, fetch));
  }, [loginContext.login]);

  return [usersApi, valuesApi];
};
