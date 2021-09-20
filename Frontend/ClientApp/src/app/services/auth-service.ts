import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import jwt_decode from 'jwt-decode';


interface TokenData {
  token: string
}

@Injectable()
export class AuthService {

  constructor() {

  }

  public logout(): void {
    localStorage.removeItem(environment.storage.token);
  }

  public isLoggedIn(): boolean {
    const token: string = localStorage.getItem(environment.storage.token);
    if (token !== null) {
      return true;
    }

    return false;
  }

  public getToken(): string {
    const token = localStorage.getItem(environment.storage.token);
    return token;
  }

  public setSession(authResult): void {
    let tokenData: TokenData = authResult.body;
    localStorage.setItem(environment.storage.token, tokenData.token);
  }

  public getUserId(): string {
    const token = localStorage.getItem(environment.storage.token);
    let id: string;
    if (token) {
      const res = jwt_decode(token);
      id = res[environment.userIdTokenKey];
    }

    return id;
  }

  public getUserRole(): string {
    const token = localStorage.getItem(environment.storage.token);
    let role = '';
    if (token) {
      const res = jwt_decode(token);
      role = res[environment.userRolesTokenKey];
    }

    return role;
  }
}
